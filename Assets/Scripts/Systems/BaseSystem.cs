using Cysharp.Threading.Tasks;
using Helpers;
using Orchestrator;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
    public class BaseSystem : ISystem
    {
        private const string PlayerBaseConfigAddress = "Assets/Settings/Game/PlayerBase.asset";
        private const string EnemyBaseConfigAddress = "Assets/Settings/Game/EnemyBase.asset";

        private UnitDataScriptableObject _playerBaseConfig;
        private UnitDataScriptableObject _enemyBaseConfig;

        private const ushort MinimumBaseSpacing = 3;
        private const ushort NumBases = 2;
        private const int BaseRadius = 2;

        private MapSystem _mapSystem;

        public async UniTask Init()
        {
            _mapSystem = await Orchestrator.Orchestrator.GetSystemAsync<MapSystem>();

            _playerBaseConfig = await Addressables.LoadAssetAsync<UnitDataScriptableObject>(PlayerBaseConfigAddress);
            _enemyBaseConfig = await Addressables.LoadAssetAsync<UnitDataScriptableObject>(EnemyBaseConfigAddress);
            
            await PlaceBases();
        }

        private async UniTask PlaceBases()
        {
            ushort playerBases = 0;
            ushort enemyBases = 0;

            // Player bases
            for (int x = 2; x < MapSystem.SizeX && playerBases < NumBases; x++)
            {
                for (int y = 2; y < MapSystem.SizeY / 2 - 1 && playerBases < NumBases; y++)
                {
                    if (!_mapSystem.IsTileOpen((x, y), BaseRadius)) continue;

                    var spawnPos = MapSystem.TileToWorldSpace((x, y));
                    await CreateBase(spawnPos, true);

                    playerBases++;

                    if (playerBases < NumBases)
                    {
                        x += MinimumBaseSpacing;
                    }
                }
            }

            // Enemy bases
            for (int x = MapSystem.SizeX - 1; x > 0 && enemyBases < NumBases; x--)
            {
                for (int y = MapSystem.SizeY - 1; y > 0 && enemyBases < NumBases; y--)
                {
                    if (!_mapSystem.IsTileOpen((x, y), BaseRadius)) continue;

                    var spawnPos = MapSystem.TileToWorldSpace((x, y));
                    await CreateBase(spawnPos, false);

                    enemyBases++;

                    if (enemyBases < NumBases)
                    {
                        x -= MinimumBaseSpacing;
                    }
                }
            }
        }

        private async UniTask CreateBase(Vector3 pos, bool isPlayerOwned)
        {
            var option = isPlayerOwned ? _playerBaseConfig : _enemyBaseConfig;
            var visual = await SpawnUnitHelper.SpawnVisual(option, pos);
            _mapSystem.CreateUnit(visual, isPlayerOwned, option);
        }
    }
}