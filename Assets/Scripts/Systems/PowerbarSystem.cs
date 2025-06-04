using Cysharp.Threading.Tasks;
using Orchestrator;
using UI;
using UnityEngine;

namespace Systems
{
  public class PowerbarSystem: ISystem
  {
    private const float FillRate = 0.04f;
    
    private CanvasReferences _ui;

    public async UniTask Init()
    {
      var uiSystem = await Orchestrator.Orchestrator.GetSystemAsync<UISystem>();
      _ui = uiSystem.CanvasReferences;
    }

    public void Update() => FillPowerbarContinuous();

    private void FillPowerbarContinuous() => _ui.PowerBar.value += Mathf.Min(FillRate * Time.deltaTime, 1f);

    public bool UsePower(float amount)
    {
      var adjustedAmount = amount / 100;

      if (_ui.PowerBar.value - adjustedAmount < 0 || _ui.PowerBar.value - adjustedAmount > 1)
      {
        return false;
      }

      _ui.PowerBar.value -= adjustedAmount;
      
      return true;
    }
  }
}