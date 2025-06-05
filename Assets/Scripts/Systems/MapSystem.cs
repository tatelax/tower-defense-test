using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Orchestrator;
using ScriptableObjects;
using Types;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Visuals;

namespace Systems
{
    public class MapSystem : ISystem
    {
        public const int SizeX = 9;
        public const int SizeY = 15;

        public HashSet<Unit> Units { get; private set; }
        public Tile[,] Map { get; } = new Tile[SizeX, SizeY];

        private const string BlockingSpriteAddress = "Assets/Sprites/A_2D_digital_illustration_showcases_a_square_pool_.png";
        private const float BlockedChance = 0.2f;

        private readonly System.Random rng = new();

        private AudioSystem _audioSystem;
        
        public async UniTask Init()
        {
            Debug.Assert(SizeX % 2 != 0 && SizeY % 2 != 0, "Width and height should be odd numbers");
            
            _audioSystem = await Orchestrator.Orchestrator.GetSystemAsync<AudioSystem>();
            
            Units = new ();
            await GenerateMap();
        }

        private async UniTask GenerateMap()
        {
            var blockingSprite = await Addressables.LoadAssetAsync<Sprite>(BlockingSpriteAddress);

            var parent = new GameObject($"Map [{SizeX}, {SizeY}]").transform;
        
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    bool isWalkable;
                    var pos = new Vector3(x - SizeX / 2, 0, y - SizeY / 2);

                    // Make border unwalkable
                    if (x == 0 || y == 0 || x == SizeX - 1 || y == SizeY - 1)
                    {
                        // var edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        // edge.transform.position = pos;
                        isWalkable = false;
                    }
                    else
                    {
                        isWalkable = rng.NextDouble() > BlockedChance;

                        if (!isWalkable && y is > 3 and < SizeY - 3)
                        {
                            var detailGo = new GameObject($"{x},{y}", typeof(SpriteRenderer));
                            detailGo.transform.parent = parent;
                            var detailRenderer = detailGo.GetComponent<SpriteRenderer>();
                            var rot = new Vector3(90, 0, 0);
                            var quat = Quaternion.Euler(rot);
                            detailGo.transform.SetPositionAndRotation(pos, quat);
                            detailRenderer.sprite = blockingSprite;
                            detailRenderer.sortingOrder = 1;
                        }
                    }
                
                    Map[x, y] = new Tile(x, y, isWalkable);
                }
            }
        
            Addressables.Release(blockingSprite);
        }

#if UNITY_EDITOR
        public void Update()
        {
            for (int x = 0; x < SizeX; x++)
            {
                Debug.DrawLine(TileToWorldSpace((x, 0)), TileToWorldSpace((x, SizeY - 1)), Color.cyan);
            }
            
            for (int y = 0; y < SizeY; y++)
            {
                Debug.DrawLine(TileToWorldSpace((0, y)), TileToWorldSpace((SizeX - 1, y)), Color.cyan);
            }
        }     
#endif

        public List<(int x, int y)> GetTilesCovered((int x, int y) center, int radius)
        {
            var tiles = new List<(int x, int y)>();
            int r2 = radius * radius;
            for (int dx = -radius + 1; dx < radius; dx++)
            {
                for (int dy = -radius + 1; dy < radius; dy++)
                {
                    int tx = center.x + dx;
                    int ty = center.y + dy;
                    if (tx >= 0 && tx < SizeX && ty >= 0 && ty < SizeY)
                    {
                        if (dx * dx + dy * dy < r2)
                            tiles.Add((tx, ty));
                    }
                }
            }
            return tiles;
        }

        public void PlaceUnit(Unit unit, (int x, int y) pos)
        {
            var newTiles = GetTilesCovered(pos, unit.Data.Radius);
            var oldTiles = GetTilesCovered(unit.CurrTile, unit.Data.Radius);

            foreach (var tile in oldTiles)
            {
                if (Map[tile.x, tile.y].Unit == unit)
                    Map[tile.x, tile.y].Unit = null;
            }

            foreach (var tile in newTiles)
            {
                if (!IsWalkable(tile) || (Map[tile.x, tile.y].Unit != null && Map[tile.x, tile.y].Unit != unit))
                {
                    Debug.LogError($"Failed to place unit {unit.GetHashCode()} at {tile} (radius {unit.Data.Radius})");
                    return;
                }
            }

            foreach (var tile in newTiles)
                Map[tile.x, tile.y].Unit = unit;

            unit.CurrTile = pos;
            Debug.Log($"placed at {pos}. IsWalkable = {Map[pos.x, pos.y].IsWalkable}");
        }

        public Unit CreateUnit(UnitVisual visual, bool isPlayerOwned, UnitDataScriptableObject stats)
        {
            var pos = WorldToTileSpace(visual.transform.position);

            if (!IsTileOpen(pos, stats.Radius))
            {
                Debug.LogError($"Failed to create unit of type {stats.UnitType} at {pos} because tile was occupied");
                return null;
            }

            var newUnit = new Unit(visual, isPlayerOwned, pos, stats);
            Units.Add(newUnit);

            visual.gameObject.name = $"{stats.UnitType} Unit ({newUnit.GetHashCode()})";
            PlaceUnit(newUnit, newUnit.CurrTile);

            return newUnit;
        }

        public void AttackTarget(Unit unit)
        {
            if (unit.Target is null)
            {
                Debug.LogError("Tried to attack target when there was none.");
                return;
            }

            unit.Target.Damage(unit.Data.Strength);

            if (unit.Target.CurrHealth <= 0)
            {
                foreach (var tile in GetTilesCovered(unit.Target.CurrTile, unit.Target.Data.Radius))
                {
                    if (Map[tile.x, tile.y].Unit == unit.Target)
                        Map[tile.x, tile.y].Unit = null;
                }

                // TODO: Pooling
                Addressables.Release(unit.Target.Visual.gameObject);

                _audioSystem.Play(Sound.Die);
                
                Run().Forget();
                async UniTask Run()
                {
                    await UniTask.Yield();
                    Units.Remove(unit.Target);
                }
            }
        }

        public bool IsTileOpen((int x, int y) pos, int radius = 1)
        {
            var tiles = GetTilesCovered(pos, radius);
            foreach (var tile in tiles)
            {
                if (tile.x < 0 || tile.x >= SizeX || tile.y < 0 || tile.y >= SizeY)
                    return false;
                if (!IsWalkable(tile) || Map[tile.x, tile.y].Unit != null)
                    return false;
            }
            return true;
        }

        public static (int, int) WorldToTileSpace(Vector3 world)
        {
            int tileX = Mathf.RoundToInt(world.x) + (SizeX - 1) / 2;
            int tileY = Mathf.RoundToInt(world.z) + (SizeY - 1) / 2;
            return (tileX, tileY);
        }

        public static Vector3 TileToWorldSpace((int x, int y) pos, float worldY = 1f)
        {
            float worldX = pos.x - (SizeX - 1) / 2f;
            float worldZ = pos.y - (SizeY - 1) / 2f;
            return new Vector3(worldX, worldY, worldZ);
        }

        public static int DistanceBetween_TileSpace_Squared((int x, int y) a, (int x, int y) b)
        {
            int dx = a.x - b.x;
            int dy = a.y - b.y;
            return dx * dx + dy * dy;
        }

        public static int DistanceBetween_TileSpace((int x, int y) a, (int x, int y) b)
        {
            int dx = a.x - b.x;
            int dy = a.y - b.y;
            return Mathf.RoundToInt(Mathf.Sqrt(dx * dx + dy * dy));
        }

        public bool IsWalkable((int x, int y) pos) => Map[pos.x, pos.y].IsWalkable;
    }
}