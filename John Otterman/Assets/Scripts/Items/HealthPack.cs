using UnityEngine;

[CreateAssetMenu(fileName = "Health Pack", menuName = "Items/Health Pack")]
public class HealthPack : Item
{
    [SerializeField] private int m_healthToRestore;
    
    public override void UseItem()
    {
        PlayerController.instance.RestoreHealth(m_healthToRestore);
        PlayerController.instance.inventory.RemoveItem(this);
    }
}
