using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour, IDimensionHandler
{
    [SerializeField] private Dimension m_currentDimension;
    //[SerializeField] 
    private SpriteRenderer spriteRenderer;
    public Dimension CurrentDimension => m_currentDimension;

    private float dimensionSwapCooldown = 0.5f;
    private float lastDimensionSwap;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        PlayerController.instance.onPlayerDimensionChange += OnPlayerSwitchDimension;
        
    }

    public Dimension GetDimension()
    {
        return m_currentDimension;
    }

    private void OnDestroy()
    {
        PlayerController.instance.onPlayerDimensionChange -= OnPlayerSwitchDimension;
    }

    public void SetDimension(Dimension dimension)
    {
        if (lastDimensionSwap > Time.time) return;

        m_currentDimension = dimension;
        int newLayer = LayerMask.NameToLayer(dimension.ToString());
        gameObject.layer = newLayer;
        OnPlayerSwitchDimension(PlayerController.instance.CurrentDimension);

        lastDimensionSwap = Time.time + dimensionSwapCooldown;
    }

    public void OnPlayerSwitchDimension(Dimension dimension)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        Color newColor = Color.white;
        if (dimension == m_currentDimension) newColor.a = 1f;
        else newColor.a = 0.25f;

        spriteRenderer.color = newColor;
    }
}
