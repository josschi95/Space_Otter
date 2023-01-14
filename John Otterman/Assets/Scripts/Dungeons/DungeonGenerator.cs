using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    private const int ROOM_HEIGHT = 10;
    private const int ROOM_WIDTH = 18;

    public bool dungeonCreated { get; private set; }
    [SerializeField] private int m_dungeonSize; //The soft-cap to number of rooms in the dungeon before end caps are placed
    [Space]
    [SerializeField] private DungeonRoom fourWayRoom; //Dungeon room which has an entrance on all 4 sides
    [SerializeField] private DungeonRoom[] threeWayRooms; //Dungeon rooms which has 3 entrances
    [SerializeField] private DungeonRoom[] twoWayRooms; //Dungeon rooms which has 2 entrances
    [SerializeField] private DungeonRoom[] endCapRooms; //Dungeon rooms which have only one entrance

    public List<DungeonRoom> dungeonRooms { get; private set; }
    private List<HallwayNode> openNodes; //the list of hallways which do not yet lead to another room
    [SerializeField] private List<HallwayNode> closedNodes; //the list of nodes which have been connected with another

    private List<DungeonRoom> allRoomPrefabs; //A combined list of all dungeon rooms, to cycle through looking for a good fit
    private List<HallwayNode> nodesToRemove = new List<HallwayNode>(); //list to collect and remove overlapping nodes
    private Coroutine dungeonGenerationCoroutine;

    private Vector2 bottomLeft;
    private Vector2 topRight;

    public void GenerateDungeon(int size)
    {
        dungeonCreated = false;

        m_dungeonSize = size;

        dungeonRooms = new List<DungeonRoom>();
        openNodes = new List<HallwayNode>();
        closedNodes = new List<HallwayNode>();

        allRoomPrefabs = new List<DungeonRoom>();
        allRoomPrefabs.Add(fourWayRoom);
        allRoomPrefabs.AddRange(threeWayRooms);
        allRoomPrefabs.AddRange(twoWayRooms);
        allRoomPrefabs.AddRange(endCapRooms);

        //Generate the first room in the dungeon
        AddRoom(fourWayRoom, transform.position);

        dungeonGenerationCoroutine = StartCoroutine(BuildOutRooms());
    }

    //Procedurally generate rooms until the min size has been reached
    private IEnumerator BuildOutRooms()
    {
        //Generate rooms until the desired size has been reached
        while (dungeonRooms.Count < m_dungeonSize + 1)
        {
            //Find an open nodes to connect the next room to
            var newNode = FindOpenNode();

            //There are no open nodes... I honestly don't see this being an issue
            if (newNode == null) yield break;

            //First check that where the next room would spawn is open and not occupied by a room
            Vector3 nextRoomSpawnLocation = GetAdjustedPosition(newNode.nodePosition, newNode.direction);

            if (CheckForRoomMisplacement(nextRoomSpawnLocation)) Debug.LogError("Should not have gotten here");

            //Check if there are any additional nodes surrounding the next spawn location 
            var neighborNodes = FindNeighborNodes(nextRoomSpawnLocation);

            //Find all rooms which would neighbor the new room
            var neighborRooms = FindNeighborRooms(nextRoomSpawnLocation);

            //Get a room that will connec to all surrounding nodes
            var nextRoom = GetNewDungeonRoom(neighborNodes, neighborRooms);

            //Instantiate it and add it to the list
            AddRoom(nextRoom, nextRoomSpawnLocation);

            //Check if there are any new hallway connections after adding this room
            yield return CheckForConnectingHallways();
            yield return null;
        }
        
        //Dungeon has reached its minimum size. Now close off all open nodes
        while (openNodes.Count > 0)
        {
            //Find an open nodes to connect the next room to
            var newNode = FindOpenNode();

            //Now I want to do the opposite of this, this would find the position of a connecting room, I want to find the position of the room that contains this node
            Vector3 replacementRoomPosition = GetAdjustedPosition(newNode.nodePosition, GetOpposite(newNode.direction)); //Ok so we have a room at this location

            //Remove old room
            DungeonRoom roomAtThisPosition = null;
            for (int i = 0; i < dungeonRooms.Count; i++)
            {
                if (dungeonRooms[i].transform.position == replacementRoomPosition)
                    roomAtThisPosition = dungeonRooms[i];
            }

            //Remove the old room
            dungeonRooms.Remove(roomAtThisPosition);
            Destroy(roomAtThisPosition.gameObject);

            //I also want to make sure to remove the open nodes 
            var nodesToRemove = FindNeighborNodes(replacementRoomPosition);

            //This will return a list of all closed nodes from this room
            var nodesToClose = FindNeighborNodes(replacementRoomPosition, false);

            //In this case, I want to pass neightbor rooms as a list of all directions which do not already have a connection
            var closedDirections = new List<Direction>();
            closedDirections.Add(Direction.North);
            closedDirections.Add(Direction.South);
            closedDirections.Add(Direction.East);
            closedDirections.Add(Direction.West);

            for (int i = 0; i < nodesToClose.Count; i++)
            {
                closedDirections.Remove(nodesToClose[i]);
            }

            //Add in new room
            var replacementRoom = GetNewDungeonRoom(nodesToClose, closedDirections, true);

            AddRoom(replacementRoom, replacementRoomPosition, false);

            //Check if there are any new hallway connections after adding this room
            yield return CheckForConnectingHallways();

            //Remove the open nodes that were associated with the previous room
            for (int i = 0; i < nodesToRemove.Count; i++)
            {
                //I need to get positions, not directions
                var nodePosition = GetAdjustedPosition(replacementRoomPosition, nodesToRemove[i]);
                for (int n = openNodes.Count - 1; n >= 0; n--)
                {
                    if (openNodes[n].nodePosition == nodePosition)
                    {
                        //Debug.Log("Removing Open Node located at " + openNodes[n].nodePosition);
                        openNodes.RemoveAt(n);
                    }
                }
            }

            yield return null;
            //yield return new WaitForSeconds(2);
        }

        dungeonCreated = true;

        //nodesToRemove.Clear();
        //closedNodes.Clear();
        Debug.Log("Dungeon Generation Complete");

        for (int i = 0; i < dungeonRooms.Count; i++)
        {
            if (dungeonRooms[i].transform.position.x < bottomLeft.x)
                bottomLeft.x = dungeonRooms[i].transform.position.x;
            if (dungeonRooms[i].transform.position.y < bottomLeft.y)
                bottomLeft.y = dungeonRooms[i].transform.position.y;

            if (dungeonRooms[i].transform.position.x > topRight.x)
                topRight.x = dungeonRooms[i].transform.position.x;
            if (dungeonRooms[i].transform.position.y > topRight.y)
                topRight.y = dungeonRooms[i].transform.position.y;
        }

        bottomLeft.x -= ROOM_WIDTH;
        bottomLeft.y -= ROOM_HEIGHT;

        topRight.x += ROOM_WIDTH;
        topRight.y += ROOM_HEIGHT;
        //Debug.Log(bottomLeft);
        //Debug.Log(topRight);

        //Pathfinding.instance.CreateGrid(bottomLeft, bottomLeft, topRight);
    }

    //Instantiate a room prefab and add it and its nodes to the lists
    private void AddRoom(DungeonRoom room, Vector3 position, bool generateNewNodes = true)
    {
        var go = Instantiate(room.gameObject, position, Quaternion.identity);
        dungeonRooms.Add(go.GetComponent<DungeonRoom>());
        go.transform.SetParent(transform);

        if (!generateNewNodes) return;

        for (int i = 0; i < room.Entrances.Length; i++)
        {
            // get the position of the node from the gameObject
            Vector3 newPos = GetAdjustedPosition(go.transform.position, room.Entrances[i].direction);
            openNodes.Add(new HallwayNode(room.Entrances[i].direction, newPos));
        }
    }

    //Returns a random node from the list of open nodes
    private HallwayNode FindOpenNode()
    {
        if (openNodes.Count == 0) return null;
        return openNodes[Random.Range(0, openNodes.Count)];
    }

    //Returns a list of all surrounding nodes given a world spawn position for a room
    private List<Direction> FindNeighborNodes(Vector3 center, bool findOpenNodes = true)
    {
        var nodeList = new List<Direction>();
        
        Vector3 northPos = center + (Vector3.up * ROOM_HEIGHT);
        Vector3 southPos = center + (Vector3.down * ROOM_HEIGHT);
        Vector3 eastPos = center + (Vector3.right * ROOM_WIDTH);
        Vector3 westPos = center + (Vector3.left * ROOM_WIDTH);

        var existingList = openNodes;
        if (!findOpenNodes) existingList = closedNodes;

        for (int i = 0; i < existingList.Count; i++)
        {
            var roomPos = existingList[i].nodePosition;
            if (roomPos == northPos)
            {
                //Debug.Log("Neighboring node found at " + roomPos);
                nodeList.Add(Direction.North);
            }
            else if (roomPos == southPos)
            {
                //Debug.Log("Neighboring node found at " + roomPos);
                nodeList.Add(Direction.South);
            }
            else if (roomPos == eastPos)
            {
                //Debug.Log("Neighboring node found at " + roomPos);
                nodeList.Add(Direction.East);
            }
            else if (roomPos == westPos)
            {
                //Debug.Log("Neighboring node found at " + roomPos);
                nodeList.Add(Direction.West);
            }
        }

        return nodeList;
    }

    //Returns a list of all surrounding rooms given a world spawn position for a room
    private List<Direction> FindNeighborRooms(Vector3 center)
    {
        var roomList = new List<Direction>();

        Vector3 northPos = center + (Vector3.up * ROOM_HEIGHT * 2);
        Vector3 southPos = center + (Vector3.down * ROOM_HEIGHT * 2);
        Vector3 eastPos = center + (Vector3.right * ROOM_WIDTH * 2);
        Vector3 westPos = center + (Vector3.left * ROOM_WIDTH * 2);

        //for (int i = 0; i < existingRooms.Count; i++)
        for (int i = 0; i < dungeonRooms.Count; i++)
        {
            //var roomPos = existingRooms[i].transform.position;
            var roomPos = dungeonRooms[i].transform.position;
            if (roomPos == northPos)
            {
                //Debug.Log("Neighboring room found at " + roomPos);
                roomList.Add(Direction.North);
            }
            if (roomPos == southPos)
            {
                //Debug.Log("Neighboring room found at " + roomPos);
                roomList.Add(Direction.South);
            }
            if (roomPos == eastPos)
            {
                //Debug.Log("Neighboring room found at " + roomPos);
                roomList.Add(Direction.East);
            }
            if (roomPos == westPos)
            {
                //Debug.Log("Neighboring room found at " + roomPos);
                roomList.Add(Direction.West);
            }
        }

        string text = "Room number " + dungeonRooms.Count + " has neighbors to the: ";
        for (int i = 0; i < roomList.Count; i++)
        {
            text += roomList[i].ToString() + ", ";
        }
        if (roomList.Count == 0) text = "Room number " + dungeonRooms.Count + " has no neighbors";
        //Debug.Log(text);

        return roomList;
    }

    private DungeonRoom GetNewDungeonRoom(List<Direction> neighborNodes, List<Direction> neighborRooms, bool getEndCap = false)
    {
        //Generate new list
        var roomList = new List<DungeonRoom>();

        //Add all rooms which have entrances that line up with the given nodes
        roomList.AddRange(GetRoomsListFromNodes(neighborNodes.ToArray()));

        //Create a new list of directions
        var blockedDirections = new List<Direction>();

        for (int i = 0; i < neighborRooms.Count; i++)
        {
            if (!neighborNodes.Contains(neighborRooms[i]))
            {
                //Debug.Log("neighbor nodes list does not contain direction: " + neighborRooms[i].ToString());
                //There is a room on a side where there is no node, so it's a wall
                blockedDirections.Add(neighborRooms[i]);
            }
        }

        for (int i = roomList.Count - 1; i >= 0; i--)
        {
            if (roomList[i].HasAnyEntranceFromDirections(blockedDirections.ToArray()))
                roomList.RemoveAt(i);
        }
        
        //Find the piece that closes off all surrounding nodes
        if (getEndCap) return GetClosingRoom(roomList, neighborNodes);

        int index = Random.Range(0, roomList.Count);
        return roomList[index];
    }

    private DungeonRoom GetClosingRoom(List<DungeonRoom> roomList, List<Direction> neighborNodes)
    {
        //if the room has any entrances from directions which are not included in the neighborNodes list
        List<Direction> emptyDirections = new List<Direction>();
        for (int i = 0; i < System.Enum.GetNames(typeof(Direction)).Length; i++)
        {
            if (!neighborNodes.Contains((Direction)i))
                emptyDirections.Add((Direction)i);
        }

        for (int i = roomList.Count - 1; i >= 0; i--)
        {
            if (roomList[i].HasAnyEntranceFromDirections(emptyDirections.ToArray()))
                roomList.RemoveAt(i);
        }
        return roomList[0];
    }

    //Returns all dungeon rooms which have hallways in the directions of the given nodes
    private List<DungeonRoom> GetRoomsListFromNodes(Direction[] nodes)
    {
        //Generate new list
        var tempList = new List<DungeonRoom>();

        //if the room has entrances in all given directions, add to list
        for (int i = 0; i < allRoomPrefabs.Count; i++)
        {
            if (allRoomPrefabs[i].HasAllEntrances(nodes))
                tempList.Add(allRoomPrefabs[i]);
        }

        return tempList;
    }

    //Returns the position that a new room should be placed based on direction of an open node
    private Vector3 GetAdjustedPosition(Vector3 fromPos, Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                //The hallway leads north
                fromPos.y += ROOM_HEIGHT;
                break;
            case Direction.South:
                fromPos.y -= ROOM_HEIGHT;
                break;
            case Direction.East:
                fromPos.x += ROOM_WIDTH;
                break;
            case Direction.West:
                fromPos.x -= ROOM_WIDTH;
                break;
        }
        return fromPos;
    }

    //Checks for nodes that have the same locations and closes them off
    private bool HallwayCheck()
    {
        nodesToRemove.Clear();

        for (int i = 0; i < openNodes.Count; i++)
        {
            for (int x = 0; x < openNodes.Count; x++)
            {
                //ignore matching hallways
                if (openNodes[i] == openNodes[x]) continue;

                //A hallway connection has been found! remove them both and return true
                //may have to change this to a Vector2.Distance chack 
                if (openNodes[i].nodePosition == openNodes[x].nodePosition)
                {
                    //Debug.Log("Hallway Connection Found!" + openNodes[i].nodePosition + " and " + openNodes[x].nodePosition);
                    nodesToRemove.Add(openNodes[i]);
                    nodesToRemove.Add(openNodes[x]);
                }
            }
        }

        if (nodesToRemove.Count > 0)
        {
            for (int i = 0; i < nodesToRemove.Count; i++)
            {
                closedNodes.Add(nodesToRemove[i]);
                openNodes.Remove(nodesToRemove[i]);
            }

            return true;
        }

        return false;
    }

    //Check for hallway connections recursively
    private IEnumerator CheckForConnectingHallways()
    {
        //Keep running this method until it returns false
        while (HallwayCheck() == true)
        {
            //Debug.Log("Checking for hallway connections");
            yield return null;
        }
    }

    //Check to make sure that the next room location is not on top of another one
    private bool CheckForRoomMisplacement(Vector3 nextRoomSpawnLocation)
    {
        bool occupiedSpaceFound = false;
        //Check existing rooms to see if they occupy the same location as the next spawn location
        for (int i = 0; i < dungeonRooms.Count; i++)
        {
            if (Vector2.Distance(dungeonRooms[i].transform.position, nextRoomSpawnLocation) < 1)
            {
                //There is already a room in this location....
                //I'll have to add some method later to either close off that hallway, or swap out that room with one to connect it
                Debug.LogWarning("Space is occupied at " + nextRoomSpawnLocation + ". Retrying");
                occupiedSpaceFound = true;
                RestartCoroutine();
            }
        }
        return occupiedSpaceFound;
    }

    //Restarts the Dungeon Generation coroutine because it tried to place a new room over an existing room
    //This should not happen anymore with the new update
    private void RestartCoroutine()
    {
        if (dungeonGenerationCoroutine != null)
            StopCoroutine(dungeonGenerationCoroutine);
        Debug.Log("Restarting Coroutine");
        dungeonGenerationCoroutine = StartCoroutine(BuildOutRooms());
    }

    //Returns the opposite of the direction given
    private Direction GetOpposite(Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return Direction.South;
            case Direction.South:
                return Direction.North;
            case Direction.East:
                return Direction.West;
            case Direction.West:
                return Direction.East;
            default:
                throw new System.Exception("No other possibility exists");
        }
    }
}