namespace Types
{
  public class Stats
  {
    public float CurrHealth;
    public readonly float Defense;
    public readonly float AttackSpeed;
    public readonly float Strength;

    public Stats(float currHealth, float defense, float attackSpeed, float strength)
    {
      CurrHealth = currHealth;
      Defense = defense;
      AttackSpeed = attackSpeed;
      Strength = strength;
    }

  }
}