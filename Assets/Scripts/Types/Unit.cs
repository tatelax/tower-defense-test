using System.Collections.Generic;
using UnityEngine;

namespace Types
{
  public class Unit
  {
    public (int x, int y) CurrTile;
    public float CurrAttackTimer;
    public Unit Target;
    
    public UnitState State { get; private set; }
    public GameObject Visual { get; }
    public bool IsPlayerOwned { get; }
    public UnitType UnitType { get; }
    public Stats Stats { get; }

    public List<Vector3> CurrentPath { get; set; }
    public int CurrentPathIndex { get; set; }
    public Unit(GameObject visual, bool isPlayerOwned, UnitType unitType, (int x, int y) pos, float defense = 1, float attackSpeed = 1, float strength = 25)
    {
      Visual = visual;
      IsPlayerOwned = isPlayerOwned;
      UnitType = unitType;
      CurrTile = pos;

      Stats = new Stats(100, defense, attackSpeed, strength);
      State = UnitState.Idle;
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