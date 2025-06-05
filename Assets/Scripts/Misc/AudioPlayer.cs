using AYellowpaper.SerializedCollections;
using Types;
using UnityEngine;

namespace Misc
{
  [RequireComponent(typeof(AudioSource))]
  public class AudioPlayer: MonoBehaviour
  {
    [Header("Settings")]
    [SerializedDictionary("Type", "Clip")]
    public SerializedDictionary<Sound, AudioClip> _sounds;

    private AudioSource _audioSource;
    
    private void Awake() => _audioSource = GetComponent<AudioSource>();

    public void Play(Sound sound) => _audioSource.PlayOneShot(_sounds[sound]);
  }
}