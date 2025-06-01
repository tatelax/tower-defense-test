using System;
using Cysharp.Threading.Tasks;
using Orchestrator;
using Types;
using UnityEngine;

namespace Systems
{
  public class UnitAISystem : ISystem
  {
    private MapSystem _mapSystem;
    
    public async UniTask Init()
    {
      _mapSystem = await Orchestrator.Orchestrator.GetSystemAsync<MapSystem>();
    }

    public void Update()
    {
      if (_mapSystem.Units is null || _mapSystem.Units.Count == 0)
        return;

      foreach (Unit unit in _mapSystem.Units)
      {
        if (unit.Target is null)
        {
          var foundTarget = FindTarget(unit);
          
          if(foundTarget)
            Debug.Log("found target!");
        }
      }
    }

    private bool FindTarget(Unit unit)
    {
      Unit closestTarget = null;
      int shortestDistance = Int32.MaxValue;
      
      for (var i = 0; i < _mapSystem.Units.Count; i++)
      {
        var comparisonUnit = _mapSystem.Units[i];
        
        if(unit == comparisonUnit || comparisonUnit.IsPlayerOwned != !unit.IsPlayerOwned || unit.UnitType != UnitType.Character)
          continue;

        int dist = MapSystem.DistanceBetween_TileSpace(unit.CurrTile, comparisonUnit.CurrTile);

        if (dist < shortestDistance)
        {
          shortestDistance = dist;
          closestTarget = _mapSystem.Units[i];
        }
      }

      unit.Target = closestTarget;
      return closestTarget != null;
    }
  }
}
