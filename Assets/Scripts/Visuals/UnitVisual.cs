using UnityEngine;
using UnityEngine.UI;

namespace Visuals
{
    public class UnitVisual : MonoBehaviour
    {
        [SerializeField] private Slider _healthBar;
        
        public Slider HealthBar => _healthBar;
    }
}
