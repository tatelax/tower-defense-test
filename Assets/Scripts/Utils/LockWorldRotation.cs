using UnityEngine;

namespace Utils
{
  public class LockWorldRotation : MonoBehaviour
  {
    private static readonly Quaternion LockedRotation = Quaternion.Euler(90f, 0f, 0f);

    void LateUpdate()
    {
      transform.rotation = LockedRotation;
    }
  }
}