using System;
using Cysharp.Threading.Tasks;
using Helpers;
using Orchestrator;
using Types;
using UnityEngine;

namespace Systems
{
    public class UnitAISystem : ISystem
    {
        private const float MoveSpeed = 5.0f;
        
        private MapSystem _mapSystem;

        public async UniTask Init()
        {
            _mapSystem = await Orchestrator.Orchestrator.GetSystemAsync<MapSystem>();
        }
 
        public void Update()
        {
            if (_mapSystem.Units == null || _mapSystem.Units.Count == 0)
                return;

            foreach (Unit unit in _mapSystem.Units)
            {
                if (unit.UnitType != UnitType.Character)
                    continue;

                if (!FindTarget(unit))
                    continue;

                var tileVisualIsIn = MapSystem.WorldToTileSpace(unit.Visual.transform.position);
                
                if (tileVisualIsIn != unit.CurrTile || unit.CurrentPath is null)
                {
                    if(tileVisualIsIn != unit.CurrTile)
                        _mapSystem.PlaceUnit(unit, tileVisualIsIn);
                    
                    var path = Pathfinder.FindPath(unit, _mapSystem.Map, unit.CurrTile, unit.Target.CurrTile);

                    if (path is null || path.Count < 2)
                        continue;
                    
                    Vector3[] pathWorldSpace = new Vector3[path.Count];
                    
                    for (var i = 0; i < path.Count; i++)
                    {
                        pathWorldSpace[i] = MapSystem.TileToWorldSpace(path[i]);
                    }

                    unit.CurrentPath = PathSmoothingUtil.CatmullRomSpline(pathWorldSpace, 5);
                    unit.CurrentPathIndex = 0;
                }

                if (unit.CurrentPathIndex > unit.CurrentPath.Count - 1)
                {
                    Debug.Log("Path end reached");
                    continue;
                }
                
                var distToNextWayPoint = Vector3.Distance(unit.Visual.transform.position, unit.CurrentPath[unit.CurrentPathIndex]);

                if (distToNextWayPoint < 0.001f)
                    unit.CurrentPathIndex++;

                unit.Visual.transform.position = Vector3.MoveTowards(unit.Visual.transform.position, unit.CurrentPath[unit.CurrentPathIndex], Time.deltaTime * MoveSpeed);
                    
                // check if we're in a new tile
                // if we are, reset the spline index to zero and do pathfinding again
                // otherwise just keep moving the visual
                
                for (var i = 0; i < unit.CurrentPath.Count - 1; i++)
                {
                    Debug.DrawLine(unit.CurrentPath[i], unit.CurrentPath[i + 1], Color.magenta);
                }
            }
        }

        private bool FindTarget(Unit unit)
        {
            Unit closestTarget = null;
            int shortestDistance = Int32.MaxValue;

            foreach (var comparisonUnit in _mapSystem.Units)
            {
                if (unit == comparisonUnit || comparisonUnit.IsPlayerOwned == unit.IsPlayerOwned)
                    continue;

                int dist = MapSystem.DistanceBetween_TileSpace(unit.CurrTile, comparisonUnit.CurrTile);
                if (dist < shortestDistance)
                {
                    shortestDistance = dist;
                    closestTarget = comparisonUnit;
                }
            }

            unit.Target = closestTarget;
            return closestTarget != null;
        }
    }
}