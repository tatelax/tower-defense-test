using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Helpers;
using Orchestrator;
using Types;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Visuals;

namespace Systems
{
  public class PlayerUnitPlacementSystem : ISystem
  {
    private const float DragSpeed = 20.0f;
    
    private MapSystem _mapSystem;
    private PowerbarSystem _powerbarSystem;
    private AudioSystem _audioSystem;
    private CanvasReferences _ui;
    
    private UnitVisual _currentUnitVisual;
    private Vector3 _targetPosition;
    private Vector3 _targetPositionLastFrame;
    
    public async UniTask Init()
    {
      var uiSystem = await Orchestrator.Orchestrator.GetSystemAsync<UISystem>();
      _mapSystem = await Orchestrator.Orchestrator.GetSystemAsync<MapSystem>();
      _powerbarSystem = await Orchestrator.Orchestrator.GetSystemAsync<PowerbarSystem>();
      _audioSystem = await Orchestrator.Orchestrator.GetSystemAsync<AudioSystem>();
      
      _ui = uiSystem.CanvasReferences;
      _ui.OnDragBegin += id => _ = OnDragBegin(id);
      _ui.OnDragEnd += OnDragEnd;
    }

    private async UniTask OnDragBegin(string id)
    {
      if (_currentUnitVisual is not null)
        return;

      var character = _ui.CharacterButtons.First(b => b.UnitData.Name == id).UnitData;
      _currentUnitVisual = await SpawnUnitHelper.SpawnVisual(character, GetDragPosInWorldSpace());

      _currentUnitVisual.transform.DOScale(Vector3.one * 1.4f, 0.3f);
      
      _audioSystem.Play(Sound.Create);
    }
    
    private void OnDragEnd(string id)
    {
      if (_currentUnitVisual is null)
        return;

      _currentUnitVisual.transform.DOScale(Vector3.one, 0.2f);
      _currentUnitVisual.transform.DOMoveY(1, 0.2f).SetEase(Ease.InOutElastic).onComplete += () =>
      {
        var characterData = _ui.CharacterButtons.First(b => b.UnitData.Name == id).UnitData;
        var newUnit = _mapSystem.CreateUnit(_currentUnitVisual, true, characterData);

        if (newUnit == null)
        {
          Addressables.Release(_currentUnitVisual.gameObject);
        }
        else
        {
          _powerbarSystem.UsePower(characterData.PowerRequired);
          _audioSystem.Play(Sound.Drop);
        }

        _currentUnitVisual = null;
      };
    }

    public void Update()
    {
      if (_currentUnitVisual is null)
        return;

      _targetPosition = GetDragPosInWorldSpace();
      _currentUnitVisual.transform.position = Vector3.Lerp(_currentUnitVisual.transform.position, _targetPosition, Time.deltaTime * DragSpeed);

      if (_targetPosition != _targetPositionLastFrame)
      {
        _audioSystem.Play(Sound.ChangeTile);
      }

      _targetPositionLastFrame = _targetPosition;
    }

    private Vector3 GetDragPosInWorldSpace()
    {
      Camera cam = Camera.main;
      Vector3 mouseScreen = Input.mousePosition;
    
      // Distance from camera Y to desired Y (which is 1)
      float desiredY = 4f;
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
