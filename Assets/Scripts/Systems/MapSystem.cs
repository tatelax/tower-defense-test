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
                Debug.LogError("Failed to place unit because tile was occupied");
                return;
            }
            
            _map[pos.x, pos.y].Unit = unit;
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

            PlaceUnit(newUnit, newUnit.CurrTile);
            
            return newUnit;
        }

        public bool IsTileOpen((int x, int y) pos)
        {
            if (_map is null)
                return false;

            if (pos.x > SizeX - 1 || pos.x < 0)
                return false;

            if (pos.y > SizeY - 1 || pos.y < 0)
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

        // --- Endless pathfinding/movement loop ---
        // private async UniTask FindAndFollowPath()
        // {
        //     while (true)
        //     {
        //         // Find a random walkable destination different from current
        //         do
        //         {
        //             int dx = rng.Next(SizeX);
        //             int dy = rng.Next(SizeY);
        //             endPos = (dx, dy);
        //         }
        //         while (endPos == startPos || !_map[endPos.x, endPos.y].IsWalkable);
        //
        //         // Highlight start/end
        //         ResetTileColors();
        //         _tileGos[startPos.x, startPos.y].GetComponent<SpriteRenderer>().color = Color.green;
        //         _tileGos[endPos.x, endPos.y].GetComponent<SpriteRenderer>().color = Color.cyan;
        //
        //         // Find path
        //         path = Pathfinder.FindPath(_map, startPos, endPos);
        //
        //         if (path == null || path.Count < 2)
        //         {
        //             // If no path found, pick a new destination next frame
        //             await UniTask.Delay(250);
        //             continue;
        //         }
        //
        //         // Set up spline path
        //         pathPoints = new List<Vector3>();
        //         foreach (var (x, y) in path)
        //             pathPoints.Add(new Vector3(x, 0.5f, y));
        //         pathPoints.Insert(0, pathPoints[0]);
        //         pathPoints.Add(pathPoints[pathPoints.Count - 1]);
        //         ComputeSegmentLengths();
        //         pathT = 0f;
        //
        //         // Move along the curve
        //         bool finished = false;
        //         while (!finished)
        //         {
        //             float speedThisFrame = moveSpeed * Time.deltaTime / pathLength;
        //             pathT += speedThisFrame;
        //             pathT = Mathf.Min(pathT, 1f);
        //
        //             // Find spline segment
        //             float scaledT = pathT * (pathPoints.Count - 3);
        //             int seg = Mathf.FloorToInt(scaledT);
        //             float t = scaledT - seg;
        //
        //             if (seg >= pathPoints.Count - 3)
        //             {
        //                 moverCube.transform.position = pathPoints[pathPoints.Count - 2];
        //                 finished = true;
        //                 break;
        //             }
        //
        //             Vector3 pos = CatmullRom(
        //                 pathPoints[seg],
        //                 pathPoints[seg + 1],
        //                 pathPoints[seg + 2],
        //                 pathPoints[seg + 3],
        //                 t
        //             );
        //             moverCube.transform.position = pos;
        //
        //             // Optional: rotate cube to face direction of movement
        //             if (seg < pathPoints.Count - 4)
        //             {
        //                 Vector3 next = CatmullRom(
        //                     pathPoints[seg],
        //                     pathPoints[seg + 1],
        //                     pathPoints[seg + 2],
        //                     pathPoints[seg + 3],
        //                     Mathf.Min(t + 0.01f, 1f)
        //                 );
        //                 Vector3 dir = next - pos;
        //                 if (dir.sqrMagnitude > 0.0001f)
        //                     moverCube.transform.forward = dir.normalized;
        //             }
        //
        //             await UniTask.Yield();
        //         }
        //
        //         // After arrival, make this node the new start for the next leg
        //         startPos = endPos;
        //         await UniTask.Delay(300); // Brief pause before new path
        //     }
        // }
        //
        // // --- Helper functions ---
        // private void ResetTileColors()
        // {
        //     for (int x = 0; x < SizeX; x++)
        //     for (int y = 0; y < SizeY; y++)
        //     {
        //         var r = _tileGos[x, y].GetComponent<SpriteRenderer>();
        //         r.color = _map[x, y].IsWalkable ? Color.white : Color.gray;
        //     }
        // }
        //
        // private void ComputeSegmentLengths()
        // {
        //     pathLength = 0f;
        //     segmentLengths = new List<float>();
        //     int segments = pathPoints.Count - 3;
        //     Vector3 prev = pathPoints[1];
        //     int samples = 10;
        //     for (int seg = 0; seg < segments; seg++)
        //     {
        //         float segLen = 0f;
        //         for (int i = 1; i <= samples; i++)
        //         {
        //             float t = i / (float)samples;
        //             Vector3 p = CatmullRom(
        //                 pathPoints[seg],
        //                 pathPoints[seg + 1],
        //                 pathPoints[seg + 2],
        //                 pathPoints[seg + 3],
        //                 t
        //             );
        //             segLen += Vector3.Distance(prev, p);
        //             prev = p;
        //         }
        //         segmentLengths.Add(segLen);
        //         pathLength += segLen;
        //     }
        // }

        public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * (t * t) +
                (-p0 + 3f * p1 - 3f * p2 + p3) * (t * t * t)
            );
        }
    }
}