using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Grid<TGridObject>
{
    //This event is called whenever a grid value changes
    public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
    public class OnGridValueChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;

    public bool finishedLoading { get; private set; }

    public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createdGribObject, bool showDebug = false)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createdGribObject(this, x, y);
            }
        }

        if (showDebug)
        {
            TMP_Text[,] debugTextArray = new TMP_Text[width, height];

            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    GameObject go = ObjectPooler.Spawn("testMarker", GetWorldPosition(x, y), Quaternion.identity);
                    go.transform.SetParent(null);

                    debugTextArray[x, y] = go.GetComponentInChildren<TMP_Text>();
                    debugTextArray[x, y].text = gridArray[x, y]?.ToString();

                    #region - Debug -
                    /*Debug.DrawLine(GetWorldPosition(x, y) - new Vector3(cellSize, cellSize) * 0.5f,
                        GetWorldPosition(x, y + 1) - new Vector3(cellSize, cellSize) * 0.5f, Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y) - new Vector3(cellSize, cellSize) * 0.5f,
                        GetWorldPosition(x + 1, y) - new Vector3(cellSize, cellSize) * 0.5f, Color.white, 100f); */
                    #endregion
                }
            }
            #region - Debug -
            /*Debug.DrawLine(GetWorldPosition(0, height) - new Vector3(cellSize, cellSize) * 0.5f,
                GetWorldPosition(width, height) - new Vector3(cellSize, cellSize) * 0.5f, Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0) - new Vector3(cellSize, cellSize) * 0.5f,
                GetWorldPosition(width, height) - new Vector3(cellSize, cellSize) * 0.5f, Color.white, 100f); */
            #endregion

            OnGridValueChanged += (object sender, OnGridValueChangedEventArgs eventArgs) =>
            {
                debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
                var node = Pathfinding.instance.GetNode(eventArgs.x, eventArgs.y);
                if (node.isOccupied) debugTextArray[eventArgs.x, eventArgs.y].color = Color.blue;
                else if (!node.isWalkable) debugTextArray[eventArgs.x, eventArgs.y].color = Color.blue;
                else debugTextArray[eventArgs.x, eventArgs.y].color = Color.white;
            };
        }

        finishedLoading = true;
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt(((worldPosition - originPosition).x + 0.5f) / cellSize);
        y = Mathf.FloorToInt(((worldPosition - originPosition).y + 0.5f) / cellSize);
    }

    public void SetGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            TriggerGridObjectChanged(x, y);
        }
    }

    public void TriggerGridObjectChanged(int x, int y)
    {
        if (OnGridValueChanged != null) OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y });
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetGridObject(x, y, value);
    }

    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        //Invalid, outside of array
        return default(TGridObject);
    }

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public float GetCellSize()
    {
        return cellSize;
    }
}
