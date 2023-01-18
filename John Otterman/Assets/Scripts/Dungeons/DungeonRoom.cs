using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRoom : MonoBehaviour
{
    [SerializeField] private Transform m_roomCenter;
    [SerializeField] private DungeonRoomEntrances[] m_entrances;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] dimensionalSprites;

    public Transform Center => m_roomCenter;
    public DungeonRoomEntrances[] Entrances => m_entrances;

    //Returns true if the room has an entrance the given direction
    public bool HasEntranceOnDirection(Direction direction)
    {
        for (int i = 0; i < m_entrances.Length; i++)
        {
            if (m_entrances[i].direction == direction)
                return true;
        }
        return false;
    }

    //Returns true if the room has entrances in all given directions
    public bool HasAllEntrances(Direction[] directions)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            if (!HasEntranceOnDirection(directions[i])) return false;
        }
        return true;
    }

    //Returns true if the room has an entrance in any of the given directions
    public bool HasAnyEntranceFromDirections(Direction[] directions)
    {
        for (int i = 0; i < directions.Length; i++)
        {
            if (HasEntranceOnDirection(directions[i])) return true;
        }
        return false;
    }

    public void SetDimensionDisplay(Dimension dimension)
    {
        if (spriteRenderer == null)
        {
            Debug.Log("missing. Fix this");
            for (int i = 0; i < m_entrances.Length; i++)
            {
                Debug.Log("entrance: " + m_entrances[i].direction);
            }
            return;
        }
        spriteRenderer.sprite = dimensionalSprites[(int)dimension];
    }
}

[System.Serializable]
public class DungeonRoomEntrances
{
    public Direction direction;
    public Transform edge;
}

[System.Serializable]
public class HallwayNode
{
    public Direction direction;
    public Vector3 nodePosition;

    public HallwayNode(Direction dir, Vector3 pos)
    {
        direction = dir;
        nodePosition = pos;
    }
}

public enum Direction { North, South, East, West }
