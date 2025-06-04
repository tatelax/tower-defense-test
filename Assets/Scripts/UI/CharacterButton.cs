using ScriptableObjects;
using UnityEngine;

namespace UI
{
  public class CharacterButton : ButtonWithDragEvents
  {
    [SerializeField] private CharacterScriptableObject _character;
    
    public CharacterScriptableObject Character => _character;
  }
}
