using UnityEngine;
using UnityEngine.UI;

namespace Visuals
{
    public class UnitVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider _healthBar;
        
        [Header("Optional References")]
        [SerializeField] private Animator _animator;
        
        public Slider HealthBar => _healthBar;
        public Animator Animator => _animator;
    }
}
