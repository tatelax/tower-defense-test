using Cysharp.Threading.Tasks;
using Orchestrator;
using Types;
using UnityEngine;

namespace Systems
{
  public class UnitAnimationSystem: ISystem
  {
    private static readonly int UnitState = Animator.StringToHash("UnitState");
    
    private MapSystem _mapSystem;
    
    public async UniTask Init()
    {
      _mapSystem = await Orchestrator.Orchestrator.GetSystemAsync<MapSystem>();
    }

    public void Update()
    {
      foreach (var unit in _mapSystem.Units)
      {
        if(unit.Data.UnitType != UnitType.Character)
          continue;
        
        unit.Visual.Animator.SetInteger(UnitState, (int)unit.State);
      }
    }
  }
}