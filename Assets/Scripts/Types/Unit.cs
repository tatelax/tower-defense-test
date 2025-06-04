using System.Collections.Generic;
using ScriptableObjects;
using Systems;
using UnityEngine;
using Visuals;

namespace Types
{
  public class Unit
  {
    public (int x, int y) CurrTile;
    public float CurrAttackTimer;
    public Unit Target;
    
    public readonly (int x, int y)[] CurrentPathBuffer;
    public List<Vector3> CurrentSmoothedPath;
    public int CurrentPathIndex;
    
    public UnitState State { get; private set; }
    public UnitVisual Visual { get; }
    public bool IsPlayerOwned { get; }
    public float CurrHealth { get; private set; }
    public UnitDataScriptableObject Data { get; }

    public Unit(UnitVisual visual, bool isPlayerOwned, (int x, int y) pos, UnitDataScriptableObject data)
    {
      Visual = visual;
      IsPlayerOwned = isPlayerOwned;
      CurrTile = pos;

      Data = data;
      State = UnitState.Idle;
      CurrHealth = 100f;

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
      CurrHealth -= amount / Data.Defense;
    }
  }
}