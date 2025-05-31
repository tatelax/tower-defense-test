using System;
using Cysharp.Threading.Tasks;
using Orchestrator;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
  public class UISystem: ISystem
  {
    private readonly string CanvasAddress = "Assets/Prefabs/Canvas.prefab";

    public CanvasReferences CanvasReferences { get; private set; }

    public async UniTask Init()
    {
      var canvas = await Addressables.InstantiateAsync(CanvasAddress);

      if (canvas.TryGetComponent<CanvasReferences>(out var references))
      {
        CanvasReferences = references;
      }
      else
      {
        throw new NullReferenceException("CanvasReferences not found.");
      }
    }
  }
}