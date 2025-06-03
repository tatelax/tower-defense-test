using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Orchestrator;
using Types;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
    public class MapSystem : ISystem
    {
        public const int SizeX = 11;
        public const int SizeY = 11;
        
        public List<Unit> Units { get; private set; }
        public Tile[,] Map => _map;
        
        private const string GroundSpriteAddress = "GroundTiles";
        private const string DetailSpriteAddress = "Assets/Art/kenney_tiny-town/Tiles/tile_0106.png";
    
        private const float BlockedChance = 0.11f;

        private readonly Tile[,] _map = new Tile[SizeX, SizeY];
        private readonly GameObject[,] _tileGos = new GameObject[SizeX, SizeY];

        private System.Random rng = new();

        public async UniTask Init()
        {
            Units = new ();
            await GenerateMap();
        }

        private async UniTask GenerateMap()
        {
            var groundSprites = await Addressables.LoadAssetsAsync<Sprite>(GroundSpriteAddress);
            var detailSprite = await Addressables.LoadAssetAsync<Sprite>(DetailSpriteAddress);

            var parent = new GameObject($"Map [{SizeX}, {SizeY}]").transform;
        
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    var go = new GameObject($"{x},{y}", typeof(SpriteRenderer));
                    go.transform.parent = parent;
                    var renderer = go.GetComponent<SpriteRenderer>();
                    var pos = new Vector3(x - SizeX / 2, 0, y - SizeY / 2);
                    var rot = new Vector3(90, 0, 0);
                    var quat = Quaternion.Euler(rot);
                    go.transform.SetPositionAndRotation(pos, quat);
                    renderer.sprite = groundSprites[rng.Next(0, groundSprites.Count - 1)];
                    _tileGos[x, y] = go;

                    bool isWalkable;

                    if (x == 0 || y == 0 || x == SizeX - 1 || y == SizeY - 1)
                    {
                        renderer.color = Color.magenta;
                        isWalkable = false;
                    }
                    else
                    {
                        isWalkable = rng.NextDouble() > BlockedChance;

                        if (!isWalkable)
                        {
                            var detailGo = new GameObject($"{x},{y}", typeof(SpriteRenderer));
                            detailGo.transform.parent = parent;
                            var detailRenderer = detailGo.GetComponent<SpriteRenderer>();
                            detailGo.transform.SetPositionAndRotation(pos, quat);
                            detailRenderer.sprite = detailSprite;
                            detailRenderer.sortingOrder = 1;
                        }
                    }
                
                    _map[x, y] = new Tile(x, y, isWalkable, go);
                }
            }
        
            Addressables.Release(groundSprites);
            Addressables.Release(detailSprite);
        }

        public void PlaceUnit(Unit unit, (int x, int y) pos)
        {
            if (!IsTileOpen(pos))
            {
                var tile = _map[pos.x, pos.y];
                
                if(tile.Unit is not null)
                    Debug.LogError($"Failed to place unit {unit.GetHashCode()} because tile {pos} was occupied by {tile.Unit.GetHashCode()}");
                
                if(!tile.IsWalkable)
                    Debug.LogError($"Failed to place unit {unit.GetHashCode()} because tile {pos} is not walkable");
                    
                return;
            }

            // Make wherever the unit is now null because this will become the previous location
            _map[unit.CurrTile.x, unit.CurrTile.y].Unit = null;
            
            // Make the desired location now occupied by the unit
            _map[pos.x, pos.y].Unit = unit;
            
            Debug.Log($"placed at {pos}. IsWalkable = {_map[pos.x, pos.y].IsWalkable}");
            
            // Actually update the unit to the tile it's on
            unit.CurrTile = pos;
        }

        public Unit CreateUnit(GameObject visual, bool isPlayerOwned, UnitType unitType)
        {
            var pos = WorldToTileSpace(visual.transform.position);
            
            if (!IsTileOpen(pos))
            {
                Debug.LogError($"Failed to create unit of type {unitType} at {pos} because tile was occupied");
                return null;
            }

            var newUnit = new Unit(visual, isPlayerOwned, unitType, pos);
            Units.Add(newUnit);

            visual.gameObject.name = $"{unitType} Unit ({newUnit.GetHashCode()})";
            
            PlaceUnit(newUnit, newUnit.CurrTile);
            
            return newUnit;
        }

        public bool IsTileOpen((int x, int y) pos)
        {
            if (_map is null)
                return false;

            if (pos.x is > SizeX - 1 or < 0)
                return false;

            if (pos.y is > SizeY - 1 or < 0)
                return false;

            if (!IsWalkable(pos))
                return false;
            
            return _map[pos.x, pos.y].Unit == null;
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

        public bool IsWalkable((int x, int y) pos) => _map[pos.x, pos.y].IsWalkable;
    }
}