using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public delegate void OnInventoryChangeCallback();
    public OnInventoryChangeCallback onInventoryChange;

    public List<InventorySlot> inventorySlots = new List<InventorySlot>();

    public void AddItem(Item newItem, int count = 1)
    {
        if (!newItem.CanStack)
        {
            for (int i = 0; i < count; i++)
            {
                inventorySlots.Add(new InventorySlot(newItem));
            }
        }
        else
        {
            if (!Contains(newItem))
            {
                inventorySlots.Add(new InventorySlot(newItem, count));
            }
            else
            {
                FindSlot(newItem).count += count;
            }
        }
        onInventoryChange?.Invoke();
    }

    public void RemoveItem(Item oldItem, int count = 1)
    {
        Debug.Assert(Contains(oldItem));

        var slot = FindSlot(oldItem);
        if (count >= slot.count) inventorySlots.Remove(slot);
        else slot.count -= count;

        onInventoryChange?.Invoke();
    }

    public bool Contains(Item item)
    {
        if (FindSlot(item) != null) return true;
        return false;
    }

    public InventorySlot FindSlot(Item item)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].item = item)
                return inventorySlots[i];
        }
        return null;
    }
}

[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int count;

    public InventorySlot(Item item, int count = 1)
    {
        this.item = item;
        this.count = count;
    }
}