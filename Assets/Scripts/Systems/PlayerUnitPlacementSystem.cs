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
    private const float DragHeight = 4.0f;
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

      _currentUnitVisual.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutExpo);
      _currentUnitVisual.transform.DOMoveY(0, 0.2f).SetEase(Ease.OutExpo).onComplete += () =>
      {
        var characterData = _ui.CharacterButtons.First(b => b.UnitData.Name == id).UnitData;
        var newUnit = _mapSystem.CreateUnit(_currentUnitVisual, true, characterData);

        if (newUnit == null)
        {
          Addressables.Release(_currentUnitVisual.gameObject);
          _audioSystem.Play(Sound.Delete);
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
      Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
      (int, int) tilePos = MapSystem.WorldToTileSpace(world);
      Vector3 snappedWorld = MapSystem.TileToWorldSpace(tilePos);
      snappedWorld.y = DragHeight;
      
      return snappedWorld;
    }
  }
}
