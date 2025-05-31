using Systems;
using UnityEngine;

namespace Types
{
  public class Unit
  {
    public GameObject Visual { get; set; }
    public bool IsPlayerOwned { get; }
    public Unit Target { get; set; }
    public (int x, int y) CurrTile { get; }
    public UnitType UnitType { get; }

    public Unit(GameObject visual, bool isPlayerOwned, UnitType unitType)
    {
      Visual = visual;
      IsPlayerOwned = isPlayerOwned;
      UnitType = unitType;
      CurrTile = MapSystem.WorldToTileSpace(visual.transform.position);
    }
  }
}