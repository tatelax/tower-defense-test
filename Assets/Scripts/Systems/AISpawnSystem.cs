using Cysharp.Threading.Tasks;
using Helpers;
using Orchestrator;
using ScriptableObjects;
using Types;
using UI;
using UnityEngine;

namespace Systems
{
  public class AISpawnSystem: ISystem
  {
    private const float SpawnInterval = 10f;
    
    private MapSystem _mapSystem;
    private CharacterButton[] _characters;

    private float spawnTimer;
    
    public async UniTask Init()
    {
      _mapSystem = await Orchestrator.Orchestrator.GetSystemAsync<MapSystem>();
      var uiSystem = await Orchestrator.Orchestrator.GetSystemAsync<UISystem>();

      _characters = uiSystem.CanvasReferences.CharacterButtons;
      
      spawnTimer = SpawnInterval;
    }

    public void Update()
    {
      spawnTimer -= Time.deltaTime;

      if (spawnTimer >= 0f)
        return;

      SpawnUnit().Forget();
      
      spawnTimer = SpawnInterval;
    }

    private async UniTask SpawnUnit()
    {
      var random = Random.Range(0, _characters.Length - 1);
      var pos = MapSystem.TileToWorldSpace((5, 5));
      var character = _characters[random].UnitData;
      
      var newCharacter = await SpawnUnitHelper.SpawnVisual(character, pos);
      
      _mapSystem.CreateUnit(newCharacter, false, character);
    }
  }
}