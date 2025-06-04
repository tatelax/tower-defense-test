using System.Collections.Generic;
using Systems;
using UnityEngine;

namespace Types
{
  public class Unit
  {
    public (int x, int y) CurrTile;
    public float CurrAttackTimer;
    public Unit Target;
    
    public (int x, int y)[] CurrentPathBuffer;
    public List<Vector3> CurrentSmoothedPath;
    public int CurrentPathIndex;
    
    public UnitState State { get; private set; }
    public GameObject Visual { get; }
    public bool IsPlayerOwned { get; }
    public UnitType UnitType { get; }
    public int Radius { get; }
    public Stats Stats { get; }

    public Unit(GameObject visual, bool isPlayerOwned, UnitType unitType, (int x, int y) pos, int radius = 1, float defense = 1, float attackSpeed = 1, float strength = 25)
    {
      Visual = visual;
      IsPlayerOwned = isPlayerOwned;
      UnitType = unitType;
      CurrTile = pos;
      Radius = radius;

      Stats = new Stats(100, defense, attackSpeed, strength);
      State = UnitState.Idle;

      CurrentPathBuffer = new (int x, int y)[MapSystem.SizeX * MapSystem.SizeY];
    }

    public bool SetState(UnitState newState)
    {
      if (newState == State)
        return false;
      
      State = newState;
      
      Debug.Log($"Unit {GetHashCode()} state is now {State}");
      return true;
    }

    public void Damage(float amount)
    {
      Stats.CurrHealth -= amount / Stats.Defense;
    }
  }
}