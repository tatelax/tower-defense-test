using Cysharp.Threading.Tasks;
using Orchestrator;
using Types;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
  public class BaseSystem: ISystem
  {
    private const string BaseAddress = "Assets/Prefabs/Base.prefab";
    private const ushort minimumBaseSpacing = 3;
    private const ushort numBases = 2;
    
    private MapSystem _mapSystem;
    private ushort _placedBases;
    
    public async UniTask Init()
    {
      _mapSystem = await Orchestrator.Orchestrator.GetSystemAsync<MapSystem>();

      await PlaceBases();
    }

    private async UniTask PlaceBases()
    {
      for (int x = 2; x < MapSystem.SizeX; x++) 
      {
        for (int y = 2; y < MapSystem.SizeY / 2 - 1; y++)
        {
          if (!_mapSystem.IsTileOpen((x, y))) continue;
          
          var spawnPos = MapSystem.TileToWorldSpace((x, y));
          var visual = await Addressables.InstantiateAsync(BaseAddress, spawnPos, Quaternion.identity);

          _ = _mapSystem.CreateUnit(visual, false, UnitType.Base);
            
          _placedBases++;
            
          if (_placedBases != numBases)
          {
            x += minimumBaseSpacing;
          }
          else
          {
            return;
          }
        }
      }
      
      Debug.LogError("Unable to place bases for some reason");
    }
  }
}