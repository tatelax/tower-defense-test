using UnityEngine;

namespace Types
{
  public struct Tile
  {
    public int X { get; }
    public int Y { get; }
    public bool IsWalkable { get; set; }
    public GameObject GO { get; }
    public Unit Unit { get; set; }

    public Tile(int x, int y, bool isWalkable, GameObject go)
    {
      X = x;
      Y = y;
      IsWalkable = isWalkable;
      GO = go;
      Unit = null;
    }
  }
}
