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
        private const float RotateSpeed = 10.0f;
        
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

                var closestTarget = FindClosestTarget(unit);
                
                // No target found. Do nothing
                if (closestTarget is null)
                {
                    unit.SetState(UnitState.Idle);
                    continue;
                }

                // We found a new target. Reset the path and index
                if (closestTarget != unit.Target)
                {
                    unit.CurrentSmoothedPath = null;
                    unit.CurrentPathIndex = 0;
                    unit.Target = closestTarget;
                }

                var tileVisualIsIn = MapSystem.WorldToTileSpace(unit.Visual.transform.position);
                
                if (tileVisualIsIn != unit.CurrTile || unit.CurrentSmoothedPath is null)
                {
                    if (tileVisualIsIn != unit.CurrTile)
                    {
                        _mapSystem.PlaceUnit(unit, tileVisualIsIn);
                    }

                    int pathLen = FastPathfinder.FindPath(unit.Target, _mapSystem.Map, unit.CurrTile, unit.Target.CurrTile, unit.CurrentPathBuffer);
                    
                    if (pathLen < 2)
                        continue;
                    
                    Vector3[] pathWorldSpace = new Vector3[pathLen];
                    
                    for (var i = 0; i < pathLen; i++)
                    {
                        pathWorldSpace[i] = MapSystem.TileToWorldSpace(unit.CurrentPathBuffer[i]);
                    }

                    unit.CurrentSmoothedPath = PathSmoothingUtil.CatmullRomSpline(pathWorldSpace, 5);
                    unit.CurrentPathIndex = 0;
                }

                if (unit.CurrentPathIndex + 1 >= unit.CurrentSmoothedPath.Count - 1 || unit.CurrentSmoothedPath.Count == 0)
                {
                    unit.SetState(UnitState.Attacking);
                    
                    // Snap the visual to the tile it's supposed to be in
                    var tilePos = MapSystem.WorldToTileSpace(unit.CurrentSmoothedPath[unit.CurrentPathIndex]);
                    var worldPos = MapSystem.TileToWorldSpace(tilePos);

                    var finalRot = Quaternion.LookRotation(unit.Target.Visual.transform.position - unit.Visual.transform.position);
                    var finalRotSmoothed = Quaternion.Slerp(unit.Visual.transform.rotation, finalRot, Time.deltaTime * RotateSpeed);
                    
                    unit.Visual.transform.SetPositionAndRotation(worldPos, finalRotSmoothed);
                    continue;
                }
                
                var distToNextWayPoint = Vector3.Distance(unit.Visual.transform.position, unit.CurrentSmoothedPath[unit.CurrentPathIndex + 1]);
                
                // If we're close to the next point in the smoothed path, set out target to the next point.
                // We just need to be more accurate than the distance between each vertex in the spline
                if (distToNextWayPoint < 0.001f)
                    unit.CurrentPathIndex++;

                var newPos = Vector3.MoveTowards(unit.Visual.transform.position, 
                    unit.CurrentSmoothedPath[unit.CurrentPathIndex + 1], 
                    Time.deltaTime * MoveSpeed);
                
                var newRot = Quaternion.LookRotation(unit.CurrentSmoothedPath[unit.CurrentPathIndex + 1] - unit.Visual.transform.position);
                var newRotSmoothed = Quaternion.Slerp(unit.Visual.transform.rotation, newRot, Time.deltaTime * RotateSpeed);
                
                unit.Visual.transform.SetPositionAndRotation(newPos, newRotSmoothed);
                
                unit.SetState(UnitState.Navigating);
                    
                for (var i = 0; i < unit.CurrentSmoothedPath.Count - 1; i++)
                    Debug.DrawLine(unit.CurrentSmoothedPath[i], unit.CurrentSmoothedPath[i + 1], Color.magenta);
            }
        }

        private Unit FindClosestTarget(Unit unit)
        {
            Unit closestTarget = null;
            int shortestDistance = Int32.MaxValue;

            foreach (var comparisonUnit in _mapSystem.Units)
            {
                if (unit == comparisonUnit || comparisonUnit.IsPlayerOwned == unit.IsPlayerOwned)
                    continue;

                int dist = MapSystem.DistanceBetween_TileSpace(unit.CurrTile, comparisonUnit.CurrTile);
                
                if (dist >= shortestDistance) continue;
                
                shortestDistance = dist;
                closestTarget = comparisonUnit;
            }

            return closestTarget;
        }
    }
}