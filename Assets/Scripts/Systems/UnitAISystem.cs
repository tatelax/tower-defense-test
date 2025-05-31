using System;
using System.Collections.Generic;
using Orchestrator;
using Types;
using UnityEngine;

namespace Systems
{
  public class UnitAISystem : ISystem
  {
    private readonly List<Unit> _units = new ();

    public void Update()
    {
      if (_units is null || _units.Count == 0)
        return;

      foreach (Unit unit in _units)
      {
        if (unit.Target is null)
        {
          var foundTarget = FindTarget(unit);
          
          if(foundTarget)
            Debug.Log("found target!");
        }
      }
    }

    public void AddUnit(Unit unit) => _units.Add(unit);

    private bool FindTarget(Unit unit)
    {
      Unit closestTarget = null;
      int shortestDistance = Int32.MaxValue;
      
      for (var i = 0; i < _units.Count; i++)
      {
        var comparisonUnit = _units[i];
        
        if(unit == comparisonUnit || comparisonUnit.IsPlayerOwned != !unit.IsPlayerOwned)
          continue;

        int dist = MapSystem.DistanceBetweenSquaredTileSpace(unit.CurrTile, comparisonUnit.CurrTile);
        Debug.Log(dist);
        if (dist < shortestDistance)
        {
          shortestDistance = dist;
          closestTarget = _units[i];
        }
      }

      unit.Target = closestTarget;
      return closestTarget != null;
    }
  }
}
