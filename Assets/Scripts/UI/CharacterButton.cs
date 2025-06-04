using ScriptableObjects;
using UnityEngine;

namespace UI
{
  public class CharacterButton : ButtonWithDragEvents
  {
    [SerializeField] private UnitDataScriptableObject unitData;
    
    public UnitDataScriptableObject UnitData => unitData;
  }
}
