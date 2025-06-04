using Types;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ScriptableObjects
{
  [CreateAssetMenu(fileName = "New Unit Data", menuName = "Game/Unit Data")]
  public class UnitDataScriptableObject: ScriptableObject
  {
    [SerializeField] private string _name;
    [SerializeField] private UnitType _unitType;
    [SerializeField] private AssetReference assetReference;
    [SerializeField] private Sprite _image;
    [SerializeField] private float _powerRequired;
    [SerializeField] private int _radius;
    [SerializeField] private float _defense;
    [SerializeField] private float _attackSpeed;
    [SerializeField] private float _strength;
    [SerializeField] private float _moveSpeed;
    
    public string Name => _name;
    public AssetReference AssetReference => assetReference;
    public Sprite Image => _image;
    public float PowerRequired => _powerRequired;
    public float Defense => _defense;
    public float Strength => _strength;
    public float MoveSpeed => _moveSpeed;
    public int Radius => _radius;
    public float AttackSpeed => _attackSpeed;
    public UnitType UnitType => _unitType;
  }
}