using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public static Pathfinding instance { get; private set; }

    private const int MOVE_STRAIGHT_COST = 1;
    private const int MOVE_DIAGONAL_COST = 14;

    private Grid<PathNode> grid;
    private List<PathNode> openList; //nodes to search
    private List<PathNode> closedList; //already searched

    [SerializeField] private Transform farBounds;
    //[SerializeField] private bool allowDiagonals = false;
    [SerializeField] private bool showDebug;

    private bool finishedLoading;

    private void Awake()
    {
        instance = this;
        if (transform.position != Vector3.zero)
        {
            Debug.LogWarning("ERROR: Pathfinder must be located at 0, 0, 0");
            Debug.Break();
            return;
        }
    }

    private void Start()
    {
        //CreateGrid();
    }

    public void CreateGrid(Vector3 origin, Vector2 bottomLeft, Vector2 topRight)
    {
        int w = Mathf.RoundToInt(topRight.x - bottomLeft.x);
        int h = Mathf.RoundToInt(topRight.y - bottomLeft.y);

        grid = new Grid<PathNode>(w, h, 1, origin, (Grid<PathNode> g, int x, int y) => new PathNode(g, x, y), showDebug);
    }

    private void CreateGrid()
    {
        int w = Mathf.RoundToInt(farBounds.transform.position.x + 1 - transform.position.x);
        int h = Mathf.RoundToInt(farBounds.transform.position.y + 1 - transform.position.y);
        //StartCoroutine(CreateGridCoroutine(w, h));

        if (showDebug && w + h >= 100)
        {
            Debug.LogError("You're going to crash the editor");
            return;
        }

        grid = new Grid<PathNode>(w, h, 1, transform.position, (Grid<PathNode> g, int x, int y) => new PathNode(g, x, y), showDebug);
    }

    public Pathfinding(int width, int height)
    {
        grid = new Grid<PathNode>(width, height, 1, Vector3.zero, (Grid<PathNode> g, int x, int y) => new PathNode(g, x, y));
    }

    //Use this method to quickly convert a list of pathNodes to a list of Vector3's
    public List<Vector3> FindVectorPath(Vector3 startWorldPosition, Vector3 endWorldPosition, bool ignoreEndNode = false)
    {
        grid.GetXY(startWorldPosition, out int startX, out int startY);
        grid.GetXY(endWorldPosition, out int endX, out int endY);

        List<PathNode> path = FindNodePath(startX, startY, endX, endY, ignoreEndNode);
        if (path == null) return null;
        else
        {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathNode node in path)
            {
                //vectorPath.Add(new Vector3(node.x, node.y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * 0.5f);
                vectorPath.Add(new Vector3(node.x, node.y));
            }
            return vectorPath;
        }
    }

    //Same thing as below but this way I can be lazy and use a basic Vector3 insted of having to Mathf.RoundToInt more than once
    public List<PathNode> FindNodePath(Vector3 startPos, Vector3 endPos, bool ignoreEndNode = false)
    {
        return FindNodePath(Mathf.RoundToInt(startPos.x), Mathf.RoundToInt(startPos.y), Mathf.RoundToInt(endPos.x), Mathf.RoundToInt(endPos.y), ignoreEndNode);
    }

    //Returns a list of nodes that can be travelled to reach a target destination
    public List<PathNode> FindNodePath(int startX, int startY, int endX, int endY, bool ignoreEndNode = false)
    {
        PathNode startNode = grid.GetGridObject(startX, startY);
        //Debug.Log("Start: " + startNode.x + "," + startNode.y);
        PathNode endNode = grid.GetGridObject(endX, endY);
        //Debug.Log("End: " + endNode.x + "," + endNode.y);

        openList = new List<PathNode> { startNode };
        closedList = new List<PathNode>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode pathNode = grid.GetGridObject(x, y);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);

            if (currentNode == endNode)
            {
                //Reached final node
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbour in GetNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbour)) continue;

                //if the neighbor is the endNode and choosing to ignore whether it is walkable, add it to the closed list
                if (neighbour == endNode && ignoreEndNode)
                {
                    //Do nothing here, bypass the next if statement
                    //Debug.Log("Ignoring End Node");
                }
                else if (!neighbour.isWalkable || neighbour.isOccupied)
                {
                    //Debug.Log("Removing unwalkable/occupied tile " + neighbour.x + "," + neighbour.y);
                    closedList.Add(neighbour);
                    continue;
                }

                //Adding in movement cost here of the neighbor node to account for areas that are more difficult to move through
                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbour) + neighbour.movementPenalty;

                if (tentativeGCost < neighbour.gCost)
                {
                    //If it's lower than the cost previously stored on the neightbor, update it
                    neighbour.cameFromNode = currentNode;
                    neighbour.gCost = tentativeGCost;
                    neighbour.hCost = CalculateDistanceCost(neighbour, endNode);
                    neighbour.CalculateFCost();

                    if (!openList.Contains(neighbour)) openList.Add(neighbour);
                }
            }
        }

        //Out of nodes on the openList
        Debug.Log("Path could not be found");
        return null;
    }

    //Return a list of nodes which can be reached given the available number of moves
    public List<Vector3> FindReachableNodes(Vector3 worldPosition, int moves)
    {
        List<PathNode> nodes = new List<PathNode>();
        PathNode startNode = grid.GetGridObject(worldPosition);

        //Get all nodes in the grid
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode pathNode = grid.GetGridObject(x, y);
                //The node can be walked through
                //Will likely have to change this in the future to allow allies to pass through each other's spaces
                //Which means returning a list of pathnodes instead, and then finding which ones are occupied,
                //and checking if the character there is an ally
                if (pathNode.isWalkable && !pathNode.isOccupied)
                {
                    if (Vector2.Distance(new Vector2(pathNode.x, pathNode.y), new Vector2(startNode.x, startNode.y)) <= moves)
                    {
                        //Note that this doesn't eliminate diagonals
                        nodes.Add(pathNode);
                        //Debug.Log(pathNode.x + "," + pathNode.y);
                    }
                }
            }
        }

        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            var temp = FindNodePath(startNode.x, startNode.y, nodes[i].x, nodes[i].y);

            //There is no available path to that node
            if (temp == null) nodes.RemoveAt(i);
            //The path requires more moves than are available
            else if (CalculatePathMoveCost(temp) > moves) nodes.RemoveAt(i);
        }

        List<Vector3> vectorPath = new List<Vector3>();
        foreach (PathNode node in nodes)
        {
            vectorPath.Add(new Vector3(node.x, node.y));
        }
        return vectorPath;
    }

    //Return a list of nodes which can be targeted given the range, used for attacks
    public List<PathNode> FindTargetableNodes(Vector3 worldPosition, int range)
    {
        List<PathNode> nodes = new List<PathNode>();
        PathNode startNode = grid.GetGridObject(worldPosition);

        //Get all nodes in the grid
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode pathNode = grid.GetGridObject(x, y);
                //Add all walkable nodes which can be accessed via a straight line path
                if (pathNode.isWalkable && Vector2.Distance(new Vector2(pathNode.x, pathNode.y), new Vector2(startNode.x, startNode.y)) <= range)
                {
                    //Note that this doesn't eliminate diagonals
                    nodes.Add(pathNode);
                    //Debug.Log(pathNode.x + "," + pathNode.y);
                }
            }
        }

        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            var temp = FindNodePath(startNode.x, startNode.y, nodes[i].x, nodes[i].y);

            //There is no available path to that node
            if (temp == null) nodes.RemoveAt(i);
            //The path requires more moves than are available
            else if (CalculatePathMoveCost(temp) > range) nodes.RemoveAt(i);
        }

        return nodes;
    }

    //Return a list of all neighbors, up/down/left/right
    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighborList = new List<PathNode>();

        //Up
        if (currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x, currentNode.y + 1));
        //Down
        if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x, currentNode.y - 1));
        //Left
        if (currentNode.x - 1 >= 0) neighborList.Add(GetNode(currentNode.x - 1, currentNode.y));
        //Right
        if (currentNode.x + 1 < grid.GetWidth()) neighborList.Add(GetNode(currentNode.x + 1, currentNode.y));

        if (currentNode.x - 1 >= 0)
        {
            //Left Down
            if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
            //Left Up
            if (currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
        }
        if (currentNode.x + 1 < grid.GetWidth())
        {
            //Right Down
            if (currentNode.y - 1 >= 0) neighborList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
            //Right Up
            if (currentNode.y + 1 < grid.GetHeight()) neighborList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
        }

        return neighborList;
    }

    public PathNode GetNode(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            //Start at the end and work backwards
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private int CalculatePathMoveCost(List<PathNode> nodes)
    {
        int cost = 0;
        //Skip the first node in the list, this is where the character is
        for (int i = 1; i < nodes.Count; i++)
        {
            cost += nodes[i].movementPenalty + 1;
        }
        return cost;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        //return MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];

        for (int i = 0; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
                lowestFCostNode = pathNodeList[i];
        }

        return lowestFCostNode;
    }

    public Grid<PathNode> GetGrid()
    {
        return grid;
    }

    public bool FinishedLoading()
    {
        return finishedLoading;
    }

    public int GetWidth()
    {
        return grid.GetWidth();
    }

    public int GetHeight()
    {
        return grid.GetHeight();
    }

    //Use this method when an object is occupying a node
    public void BlockNode(Vector3 worldPosition, bool isWalkable)
    {
        BlockNode(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.y), isWalkable);
    }

    private void BlockNode(int x, int y, bool isWalkable)
    {
        grid.GetGridObject(x, y).SetWalkable(isWalkable);
    }

    //Use this method when a character is occupying a node
    public void OccupyNode(Vector3 worldPosition, bool isOccupied)
    {
        OccupyNode(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.y), isOccupied);
    }

    private void OccupyNode(int x, int y, bool isOccupied)
    {
        grid.GetGridObject(x, y).SetOccupied(isOccupied);
    }

    public void SetNodePenalty(Vector3 worldPosition, int cost)
    {
        SetNodePenalty(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.y), cost);
    }

    private void SetNodePenalty(int x, int y, int cost)
    {
        grid.GetGridObject(x, y).SetMoveCost(cost);
    }

    public int GetNodePenalty(Vector3 worldPosition)
    {
        return grid.GetGridObject(worldPosition).movementPenalty;
    }

    public void DisplayRange(int x, int y, int moves)
    {
        //IDK how but use pathfinding to see every walkable tile from position x,y
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(transform.position, Vector3.one);
        if (farBounds != null) Gizmos.DrawCube(farBounds.position, Vector3.one);
    }
}
