using System;
using UnityEngine;

namespace UI
{
  public class CanvasReferences : MonoBehaviour
  {
    public Action<ushort> OnDragBegin;
    public Action<ushort> OnDragEnd;

    private ButtonWithDragEvents[] _buttons;

    private void Awake()
    {
      // Slow but only a few gameobjects so its fine
      _buttons = gameObject.GetComponentsInChildren<ButtonWithDragEvents>();
      
      for (ushort i = 0; i < _buttons.Length; i++)
      {
        var i1 = i;
        _buttons[i].OnBeginDragAction += () => OnBeginDragAction(i1);
        _buttons[i].OnEndDragAction += () => OnEndDragAction(i1);
      }
    }

    private void OnBeginDragAction(ushort id) => OnDragBegin?.Invoke(id);
    private void OnEndDragAction(ushort id) => OnDragEnd?.Invoke(id);
  }
}
