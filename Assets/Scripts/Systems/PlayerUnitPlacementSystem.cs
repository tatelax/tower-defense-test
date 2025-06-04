using Cysharp.Threading.Tasks;
using Orchestrator;
using Types;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
  public class PlayerUnitPlacementSystem : ISystem
  {
    private readonly string[] _unitAddresses = {
      "Assets/Prefabs/Units/Unit.prefab",
      "",
      "",
      ""
    };

    private MapSystem _mapSystem;
    
    private UnitVisual _currentUnitVisual;
    
    public async UniTask Init()
    {
      var uiSystem = await Orchestrator.Orchestrator.GetSystemAsync<UISystem>();
      _mapSystem = await Orchestrator.Orchestrator.GetSystemAsync<MapSystem>();
      
      uiSystem.CanvasReferences.OnDragBegin += id => _ = OnDragBegin(id);
      uiSystem.CanvasReferences.OnDragEnd += OnDragEnd;
    }

    private async UniTask OnDragBegin(ushort id)
    {
      if (_currentUnitVisual is not null)
        return;
      
      var newGameObject = await Addressables.InstantiateAsync(_unitAddresses[id], GetDragPosInWorldSpace(), Quaternion.identity).ToUniTask();

      if (newGameObject.TryGetComponent(out UnitVisual unitVisual))
      {
        _currentUnitVisual = unitVisual;
      }
      else
      {
        Debug.LogError("Can't find UnitVisual component");
      }
    }
    
    private void OnDragEnd(ushort id)
    {
      if (_currentUnitVisual is null)
        return;

      var newUnit = _mapSystem.CreateUnit(_currentUnitVisual, true, UnitType.Character);

      if (newUnit == null)
      {
        Addressables.Release(_currentUnitVisual.gameObject);
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
