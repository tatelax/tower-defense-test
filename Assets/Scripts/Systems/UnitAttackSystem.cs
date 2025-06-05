using Cysharp.Threading.Tasks;
using Orchestrator;
using Types;
using UnityEngine;

namespace Systems
{
  public class UnitAttackSystem : ISystem
  {
    private MapSystem _mapSystem;
    private AudioSystem _audioSystem;
    
    public async UniTask Init()
    {
      _mapSystem = await Orchestrator.Orchestrator.GetSystemAsync<MapSystem>();
      _audioSystem = await Orchestrator.Orchestrator.GetSystemAsync<AudioSystem>();
    }

    public void Update()
    {
      foreach (Unit unit in _mapSystem.Units)
      {
        if (unit.Target is null || unit.State != UnitState.Attacking)
          continue;

        if (unit.CurrAttackTimer <= 0)
        {
          _mapSystem.AttackTarget(unit);
          unit.CurrAttackTimer = unit.Data.AttackSpeed;
          _audioSystem.Play(Sound.Attack);
        }

        unit.CurrAttackTimer -= Time.deltaTime;
      }
    }
  }
}
