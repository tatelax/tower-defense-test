using Cysharp.Threading.Tasks;
using Orchestrator;

namespace Systems
{
  public class HealthbarSystem: ISystem
  {
    private MapSystem _mapSystem;

    public async UniTask Init()
    {
      _mapSystem = await Orchestrator.Orchestrator.GetSystemAsync<MapSystem>();
    }

    public void Update()
    {
      foreach (var unit in _mapSystem.Units)
      {
        if (unit.Visual?.HealthBar is null)
          continue;
        
        unit.Visual.HealthBar.value = unit.Stats.CurrHealth / 100;
      }
    }
  }
}