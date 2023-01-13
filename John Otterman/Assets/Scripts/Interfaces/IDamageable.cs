public interface IDamageable
{
    void OnDamage(int dmg, Dimension dimension);

    void OnDamagePlayer(int dmg, Dimension dimension);

    void OnDamageEnemy(int dmg, Dimension dimension);
}