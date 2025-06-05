using Cysharp.Threading.Tasks;
using Misc;
using Orchestrator;
using Types;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
  public class AudioSystem: ISystem
  {
    private const string AudioSourceAddress = "Assets/Prefabs/AudioPlayer.prefab";
    
    private AudioPlayer _audioPlayer;
    
    public async UniTask Init()
    {
      var obj = await Addressables.InstantiateAsync(AudioSourceAddress, Vector3.zero, Quaternion.identity);
      _audioPlayer = obj.GetComponent<AudioPlayer>();
    }

    public void Play(Sound sound) => _audioPlayer.Play(sound);
  }
}