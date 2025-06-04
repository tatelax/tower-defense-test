using System.Linq;
using Cysharp.Threading.Tasks;
using Orchestrator;
using Types;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
  public class PlayerUnitPlacementSystem : ISystem
  {
    private MapSystem _mapSystem;
    private PowerbarSystem _powerbarSystem;
    private CanvasReferences _ui;
    
    private UnitVisual _currentUnitVisual;
    
    public async UniTask Init()
    {
      var uiSystem = await Orchestrator.Orchestrator.GetSystemAsync<UISystem>();
      _mapSystem = await Orchestrator.Orchestrator.GetSystemAsync<MapSystem>();
      _powerbarSystem = await Orchestrator.Orchestrator.GetSystemAsync<PowerbarSystem>();
      
      _ui = uiSystem.CanvasReferences;
      _ui.OnDragBegin += id => _ = OnDragBegin(id);
      _ui.OnDragEnd += OnDragEnd;
    }

    private async UniTask OnDragBegin(string id)
    {
      if (_currentUnitVisual is not null)
        return;

      var button = _ui.CharacterButtons.First(b => b.Character.Name == id).Character;
      var newGameObject = await Addressables.InstantiateAsync(button.AssetReference, GetDragPosInWorldSpace(), Quaternion.identity).ToUniTask();

      if (newGameObject.TryGetComponent(out UnitVisual unitVisual))
      {
        _currentUnitVisual = unitVisual;
      }
      else
      {
        Debug.LogError("Can't find UnitVisual component");
      }
    }
    
    private void OnDragEnd(string id)
    {
      if (_currentUnitVisual is null)
        return;

      var characterData = _ui.CharacterButtons.First(b => b.Character.Name == id).Character;
      var stats = new Stats(100, characterData.Defense, characterData.AttackSpeed, characterData.Strength, characterData.MoveSpeed);
      
      var newUnit = _mapSystem.CreateUnit(_currentUnitVisual, 
        true, 
        UnitType.Character, 
        characterData.Radius,
        stats);

      if (newUnit == null)
      {
        Addressables.Release(_currentUnitVisual.gameObject);
      }
      else
      {
        _powerbarSystem.UsePower(characterData.PowerRequired);
      }

      _currentUnitVisual = null;
    }

    public void Update()
    {
      if (_currentUnitVisual is null)
        return;

      _currentUnitVisual.transform.position = GetDragPosInWorldSpace();
    }

    private Vector3 GetDragPosInWorldSpace()
    {
      Camera cam = Camera.main;
      Vector3 mouseScreen = Input.mousePosition;
    
      // Distance from camera Y to desired Y (which is 1)
      float desiredY = 1f;
      float camY = cam.transform.position.y;
      float distance = camY - desiredY; // should be positive if camera is above the plane
    
      // Use screen X and Y, but set Z to the distance above the plane at Y=1
      mouseScreen.z = distance;
      Vector3 world = cam.ScreenToWorldPoint(mouseScreen);

      // Snap or round as needed (for grid)
      return new Vector3(Mathf.RoundToInt(world.x), desiredY, Mathf.RoundToInt(world.z));
    }
  }
}
