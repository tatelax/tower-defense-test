using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
  public class CanvasReferences : MonoBehaviour
  {
    [SerializeField] private Slider powerBar;
    [SerializeField] private CharacterButton[] characterButtons;

    public Slider PowerBar => powerBar;
    public CharacterButton[] CharacterButtons => characterButtons;
    
    public Action<string> OnDragBegin;
    public Action<string> OnDragEnd;

    private void Awake()
    {
      for (ushort i = 0; i < characterButtons.Length; i++)
      {
        var button = characterButtons[i];
        
        button.OnBeginDragAction += () => OnBeginDragAction(button.UnitData.Name);
        button.OnEndDragAction += () => OnEndDragAction(button.UnitData.Name);
        button.image.sprite = button.UnitData.Image;
      }
    }

    private void OnBeginDragAction(string id) => OnDragBegin?.Invoke(id);
    private void OnEndDragAction(string id) => OnDragEnd?.Invoke(id);
  }
}
