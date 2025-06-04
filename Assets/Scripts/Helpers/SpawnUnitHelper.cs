using Cysharp.Threading.Tasks;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Visuals;

namespace Helpers
{
  public static class SpawnUnitHelper
  {
    public static async UniTask<UnitVisual> SpawnVisual(UnitDataScriptableObject data, Vector3 pos)
    {
      var newGameObject = await Addressables.InstantiateAsync(data.AssetReference, pos, Quaternion.identity);

      if (newGameObject.TryGetComponent(out UnitVisual unitVisual))
      {
        return unitVisual;
      }

      Debug.LogError("Can't find UnitVisual component");
      return null;
    }
  }
}