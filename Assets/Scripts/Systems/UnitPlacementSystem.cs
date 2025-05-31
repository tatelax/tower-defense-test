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

    private UnitAISystem _unitAISystem;
    private MapSystem _mapSystem;
    
    private GameObject _currentUnitVisual;
    private Vector3 _targetPosInWorldSpace;
    
    public async UniTask Init()
    {
      var uiSystem = await Orchestrator.Orchestrator.GetSystemAsync<UISystem>();
      _unitAISystem = await Orchestrator.Orchestrator.GetSystemAsync<UnitAISystem>();
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

      var newUnit = new Unit(_currentUnitVisual, true, UnitType.Character);
      
      _unitAISystem.AddUnit(newUnit);
      _currentUnitVisual = null;
    }

    public void Update()
    {
      if (_currentUnitVisual is null)
        return;

      Vector3 dragPos = GetDragPosInWorldSpace();
      (int, int) targetPosInTileSpace = MapSystem.WorldToTileSpace(dragPos);
      _targetPosInWorldSpace = MapSystem.TileToWorldSpace(targetPosInTileSpace);
      
      _currentUnitVisual.transform.position = Vector3.Lerp(_currentUnitVisual.transform.position, _targetPosInWorldSpace, DragSpeed * Time.deltaTime);
    }
    
    private Vector3 GetDragPosInWorldSpace()
    {
      Camera cam = Camera.main;
      Vector3 mouseScreen = Input.mousePosition;
      mouseScreen.z = 0f;
      Vector3 world = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, cam.transform.position.y - 1));
      return new Vector3(world.x, 1f, world.z);
    }
  }
}
