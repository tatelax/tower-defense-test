using Cysharp.Threading.Tasks;
using Orchestrator;
using Types;
using UnityEngine;

namespace Systems
{
  public class UnitAttackSystem : ISystem
  {
    private MapSystem _mapSystem;
    
    public async UniTask Init()
    {
      _mapSystem = await Orchestrator.Orchestrator.GetSystemAsync<MapSystem>();
    }

    public void Update()
    {
      foreach (Unit unit in _mapSystem.Units)
      {
        if (unit.Target is null || unit.State != UnitState.Attacking)
          continue;

        if (unit.CurrAttackTimer <= 0)
        {
          Debug.Log("Attacking");
          
          _mapSystem.AttackTarget(unit);
          
          unit.CurrAttackTimer = unit.Stats.AttackSpeed;
        }

        unit.CurrAttackTimer -= Time.deltaTime;
      }
    }
  }
}
