using System.Collections.Generic;
using UnityEngine;

namespace Types
{
  public class Unit
  {
    public GameObject Visual { get; }
    public bool IsPlayerOwned { get; }
    public Unit Target { get; set; }
    public (int x, int y) CurrTile { get; set; }
    public UnitType UnitType { get; }

    public List<Vector3> CurrentPath { get; set; }
    public int CurrentPathIndex { get; set; }
    public Unit(GameObject visual, bool isPlayerOwned, UnitType unitType, (int x, int y) pos)
    {
      Visual = visual;
      IsPlayerOwned = isPlayerOwned;
      UnitType = unitType;
      CurrTile = pos;
    }
  }
}