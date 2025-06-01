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
    
    public async UniTask Init()
    {
      _mapSystem = await Orchestrator.Orchestrator.GetSystemAsync<MapSystem>();

      await PlaceBases();
    }

    private async UniTask PlaceBases()
    {
      ushort playerBases = 0;
      ushort enemyBases = 0;

      // Player bases (top half)
      for (int x = 2; x < MapSystem.SizeX && playerBases < numBases; x++)
      {
        for (int y = 2; y < MapSystem.SizeY / 2 - 1 && playerBases < numBases; y++)
        {
          if (!_mapSystem.IsTileOpen((x, y))) continue;

          var spawnPos = MapSystem.TileToWorldSpace((x, y));
          var visual = await Addressables.InstantiateAsync(BaseAddress, spawnPos, Quaternion.identity);
          _ = _mapSystem.CreateUnit(visual, true, UnitType.Base);

          playerBases++;

          if (playerBases < numBases)
          {
            x += minimumBaseSpacing;
          }
        }
      }

      // Enemy bases (bottom half)
      for (int x = MapSystem.SizeX - 1; x > 0 && enemyBases < numBases; x--)
      {
        for (int y = MapSystem.SizeY - 1; y > 0 && enemyBases < numBases; y--)
        {
          if (!_mapSystem.IsTileOpen((x, y))) continue;

          var spawnPos = MapSystem.TileToWorldSpace((x, y));
          var visual = await Addressables.InstantiateAsync(BaseAddress, spawnPos, Quaternion.identity);
          _ = _mapSystem.CreateUnit(visual, false, UnitType.Base);

          enemyBases++;

          if (enemyBases < numBases)
          {
            x -= minimumBaseSpacing;
          }
        }
      }
    }
  }
}