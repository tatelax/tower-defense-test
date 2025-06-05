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
            for (int i = 0; i < 2; i++)
            {
                int x = 2;
                int y = ((MapSystem.SizeY - 4) * (i % 2)) + 2;
                var a = MapSystem.TileToWorldSpace((x, y));
                var b = MapSystem.TileToWorldSpace((MapSystem.SizeX - x, y));

                await CreateBase(a, i % 2 == 0);
                await CreateBase(b, i % 2 == 0);
            }
        }

        private async UniTask CreateBase(Vector3 pos, bool isPlayerOwned)
        {
            var option = isPlayerOwned ? _playerBaseConfig : _enemyBaseConfig;
            var visual = await SpawnUnitHelper.SpawnVisual(option, pos);
            _ = _mapSystem.CreateUnit(visual, isPlayerOwned, option);
        }
    }
}