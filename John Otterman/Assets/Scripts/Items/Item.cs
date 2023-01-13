using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : ScriptableObject
{
    [SerializeField] private string m_itemName;
    [SerializeField] private int m_cost;
    [SerializeField] private bool m_canStack;
    [Space]
    [SerializeField] private Sprite m_icon;
    [SerializeField] private GameObject m_prefab;

    public string ItemName => m_itemName;
    public int Cost => m_cost;
    public bool CanStack => m_canStack;
    public Sprite Icon => m_icon;
    public GameObject Prefab => m_prefab;

    public virtual void UseItem()
    {
        Debug.Log("Using " + m_itemName);
    }
}
