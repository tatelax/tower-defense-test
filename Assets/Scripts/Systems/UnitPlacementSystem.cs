using Cysharp.Threading.Tasks;
using Orchestrator;
using Types;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
  public class UnitPlacementSystem : ISystem
  {
    private readonly string[] _unitAddresses = {
      "Assets/Prefabs/Units/Unit.prefab",
      "",
      "",
      ""
    };

    private const float DragSpeed = 15.0f;

    private MapSystem _mapSystem;
    
    private GameObject _currentUnitVisual;
    private Vector3 _targetPosInWorldSpace;
    
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
      
      _currentUnitVisual = await Addressables.InstantiateAsync(_unitAddresses[id], GetDragPosInWorldSpace(), Quaternion.identity).ToUniTask();
    }
    
    private void OnDragEnd(ushort id)
    {
      if (_currentUnitVisual is null)
        return;

      var newUnit = _mapSystem.CreateUnit(_currentUnitVisual, true, UnitType.Character);

      if (newUnit == null)
      {
        Addressables.Release(_currentUnitVisual);
      }

      _currentUnitVisual = null;
    }

    public void Update()
    {
      if (_currentUnitVisual is null)
        return;

      Vector3 dragPos = GetDragPosInWorldSpace();
      _targetPosInWorldSpace = new Vector3(Mathf.RoundToInt(dragPos.x), 1, Mathf.RoundToInt(dragPos.y));
      
      //_currentUnitVisual.transform.position = Vector3.Lerp(_currentUnitVisual.transform.position, _targetPosInWorldSpace, DragSpeed * Time.deltaTime);
      _currentUnitVisual.transform.position = dragPos;
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
