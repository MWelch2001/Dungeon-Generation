using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using JetBrains.Annotations;
using Unity.Collections.LowLevel.Unsafe;
using static DungeonGen;
using UnityEditor;

public class DungeonGen : MonoBehaviour
{
    public List<Rect> completeRooms = new List<Rect>();
    [SerializeField]
    private GameObject floor;
    [SerializeField]
    private GameObject background;
    [SerializeField]
    private GameObject triggerPrefab;

    public int rMaxSize, rMinSize;
    public int rows, cols;

    public GameObject[] cardinalWalls = new GameObject[4];
    public GameObject[] cornerWalls = new GameObject[4];
    public GameObject[,] floorTiles;
    private GameObject[,] backgroundTiles;
    private GameObject[,] wallTiles;
    public class Room
    {
        public List<Rect> corridors = new List<Rect>();
        public Room left, right;
        public Rect rect;
        public Rect room = new Rect(-1, -1, 0, 0);

        public Room(Rect mrect)
        {
            rect = mrect;
        }

        public bool IsLeaf()
        {
            return left == null && right == null;
        }

        private bool GetSplitDirection()
        {
            if (rect.width > rect.height && rect.width / rect.height >= 1.25)
            {
                return true;
            }
            else if (rect.height > rect.width && rect.height / rect.width >= 1.25)
            {
                return false;
            }
            else
            {
                return Random.Range(0.0f, 1.0f) > 0.5;
            }
        }

        public Rect GetRoom()
        {
            if (IsLeaf())
            {
                return room;
            }
            if (left != null)
            {
                Rect lRoom = left.GetRoom();
                if (lRoom.x != -1)
                {
                    return lRoom;
                }
            }
            if (right != null)
            {
                Rect rRoom = right.GetRoom();
                if (rRoom.x != -1)
                {
                    return rRoom;
                }
            }

            return new Rect(-1, -1, 0, 0);
        }
        public void CreateCorridor(Room left, Room right)
        {
            Rect l = left.GetRoom();
            Rect r = right.GetRoom();
            Vector2 lPoint = new Vector2((int)Random.Range(l.x + 3, l.xMax - 3), (int)Random.Range(l.y + 3, l.yMax - 3));
            Vector2 rPoint = new Vector2((int)Random.Range(r.x + 3, r.xMax - 3), (int)Random.Range(r.y + 3, r.yMax - 3));

            if (lPoint.x > rPoint.x)
            {
                Vector2 temp = lPoint;
                lPoint = rPoint;
                rPoint = temp;
            }
            int w = (int)(lPoint.x - rPoint.x);
            int h = (int)(lPoint.y - rPoint.y);

            if (w != 0)
            {
                if (Random.Range(0, 1) > 2)
                {

                    corridors.Add(new Rect(lPoint.x, lPoint.y, Mathf.Abs(w) + 1, 1));

                    if (h < 0)
                    {
                        corridors.Add(new Rect(rPoint.x, lPoint.y, 1, Mathf.Abs(h)));
                    }
                    else
                    {
                        corridors.Add(new Rect(rPoint.x, lPoint.y, 1, -Mathf.Abs(h)));
                    }
                }
                else
                {
                    if (h < 0)
                    {
                        corridors.Add(new Rect(lPoint.x, lPoint.y, 1, Mathf.Abs(h)));
                    }
                    else
                    {
                        corridors.Add(new Rect(lPoint.x, rPoint.y, 1, Mathf.Abs(h)));
                    }
                    corridors.Add(new Rect(lPoint.x, rPoint.y, Mathf.Abs(w) + 1, 1));
                }
            }
            else
            {
                if (h < 0)
                {
                    corridors.Add(new Rect((int)lPoint.x, (int)lPoint.y, 1, Mathf.Abs(h)));
                }
                else
                {
                    corridors.Add(new Rect((int)rPoint.x, (int)rPoint.y, 1, Mathf.Abs(h)));
                }
            }
        }
        public void CreateRoom()
        {
            if (left != null)
            {
                left.CreateRoom();
            }

            if (right != null)
            {
                right.CreateRoom();
            }

            if (left != null && right != null)
            {
                CreateCorridor(left, right);
            }

            if (IsLeaf())
            {
                int rWidth = (int)Random.Range(rect.width / 2, rect.width - 2);
                int rHeight = (int)Random.Range(rect.height / 2, rect.height - 2);
                int rX = (int)Random.Range(1, rect.width - rWidth - 1);
                int rY = (int)Random.Range(1, rect.height - rHeight - 1);
                room = new Rect(rect.x + rX, rect.y + rY, rWidth, rHeight);
            }
        }

        public bool Split(int rMinSize, int rMaxSize)
        {
            bool splitVertical = GetSplitDirection();

            if (!IsLeaf())
            {
                return false;
            }

            if (Mathf.Min(rect.height, rect.width) / 2 < rMinSize)
            {
                return false;
            }

            if (splitVertical)
            {
                int split = Random.Range(rMinSize, (int)(rect.height - rMinSize));

                left = new Room(new Rect(rect.x, rect.y, split, rect.height));
                right = new Room(new Rect(rect.x + split, rect.y, rect.width - split, rect.height));

            }
            else
            {
                int split = Random.Range(rMinSize, (int)(rect.width - rMinSize));

                left = new Room(new Rect(rect.x, rect.y, rect.width, split));
                right = new Room(new Rect(rect.x, rect.y + split, rect.width, rect.height - split));
            }
            return true;
        }
    }
    public void DrawRoom(Room currentRoom)
    {
        if (currentRoom == null)
        {
            return;
        }
        if (currentRoom.IsLeaf())
        {
            for (int x = (int)currentRoom.room.x; x < currentRoom.room.xMax; x++)
            {
                for (int y = (int)currentRoom.room.y; y < currentRoom.room.yMax; y++)
                {
                    Destroy(backgroundTiles[x, y]);
                    GameObject instance = Instantiate(floor, new Vector3(x, y, 0f), Quaternion.identity);
                    instance.transform.SetParent(transform);
                    instance.GetComponent<Renderer>().sortingLayerName = "Dungeon";
                    floorTiles[x, y] = instance;
                    DrawWalls(currentRoom, x, y);
                }
            }
            completeRooms.Add(currentRoom.room);
        }
        else
        {
            DrawRoom(currentRoom.left);
            DrawRoom(currentRoom.right);
        }
    }


    public void DrawWalls(Room currentRoom, int x, int y)
    {
        if (x == currentRoom.room.xMin && y != currentRoom.room.yMin)
        {
            ClearOverlap(x, y, 2);
            InstantiateWall(x, y, 0);
        }
        else if (x == currentRoom.room.xMax - 1 && y != currentRoom.room.yMin)
        {
            ClearOverlap(x, y, 2);
            InstantiateWall(x, y, 1);
        }

        if (y == currentRoom.room.yMin)
        {
            ClearOverlap(x, y, 2);
            InstantiateWall(x, y, 3);

        }
        else if (y == currentRoom.room.yMax - 1)
        {
            ClearOverlap(x, y, 2);
            InstantiateWall(x, y, 2);
        }

        if (y == currentRoom.room.yMax - 1 && x == currentRoom.room.xMin)
        {
            ClearOverlap(x, y, 3);
            InstantiateWall(x, y, 0);
        }
        if (y == currentRoom.room.yMin && x == currentRoom.room.xMin)
        {
            ClearOverlap(x, y, 3);
            InstantiateCorner(x, y, 0);
        }

        if (y == currentRoom.room.yMax - 1 && x == currentRoom.room.xMax - 1)
        {
            ClearOverlap(x, y, 3);
            InstantiateWall(x, y, 1);
        }
        if (y == currentRoom.room.yMin && x == currentRoom.room.xMax - 1)
        {
            ClearOverlap(x, y, 3);
            InstantiateCorner(x, y, 1);
        }
    }

    public void ClearOverlap(int x, int y, int clrNum)
    {
        switch (clrNum)
        {
            case 1:
                Destroy(backgroundTiles[x, y]);
                backgroundTiles[x, y] = null;
                break;
            case 2:
                Destroy(backgroundTiles[x, y]);
                Destroy(floorTiles[x, y]);
                floorTiles[x, y] = null;
                backgroundTiles[x, y] = null;
                break;
            case 3:
                Destroy(backgroundTiles[x, y]);
                Destroy(floorTiles[x, y]);
                Destroy(wallTiles[x, y]);
                wallTiles[x, y] = null;
                backgroundTiles[x, y] = null;
                floorTiles[x, y] = null;
                break;
        }
    }

    public void DrawCorridorWalls(int x, int y)
    {
        if (floorTiles[x - 1, y] == null && wallTiles[x - 1, y] == null)
        {
            ClearOverlap(x - 1, y, 1);
            InstantiateWall(x - 1, y, 0);
        }
        if (floorTiles[x + 1, y] == null && wallTiles[x + 1, y] == null)
        {
            ClearOverlap(x + 1, y, 1);
            InstantiateWall(x + 1, y, 1);
        }

        if (floorTiles[x, y + 1] == null && wallTiles[x, y + 1] == null)
        {
            ClearOverlap(x, y + 1, 1);
            InstantiateWall(x, y + 1, 2);
        }
        if (floorTiles[x, y - 1] == null && wallTiles[x, y - 1] == null)
        {
            ClearOverlap(x, y - 1, 1);
            InstantiateWall(x, y - 1, 3);
        }
    }

    private void InstantiateWall(int x, int y, int wallIndex)
    {
        GameObject instance = InstantiateDungeonObject(cardinalWalls[wallIndex], new Vector3(x, y, 0f), Quaternion.identity, "Dungeon");
        wallTiles[x, y] = instance;
    }
    private void InstantiateCorner(int x, int y, int wallIndex)
    {
        Destroy(wallTiles[x, y]);
        wallTiles[x, y] = null;
        GameObject instance = InstantiateDungeonObject(cornerWalls[wallIndex], new Vector3(x, y, 0f), Quaternion.identity, "Dungeon");
        wallTiles[x, y] = instance;
    }
    public void DrawCorners(int x, int y, Rect corridor, List<Rect> corridorRooms)
    {
        if (corridor.height > corridor.width)
            {
                if (IsOnYMin(y, corridorRooms) && !IsOnXMin(x, corridorRooms) && !IsOnXMax(x, corridorRooms))
                {
                    InstantiateCorner(x - 1, y, 2);
                    InstantiateCorner(x + 1, y, 3);
                    GameObject spawnTrigger = Instantiate(triggerPrefab, new Vector3(x, y, 0f), Quaternion.identity);
                    foreach (Rect c in completeRooms)
                    {
                        if (c.Contains(new Vector2(x, y)))
                        {
                            spawnTrigger.GetComponent<EnemySpawnTrigger>().triggerRoom = c;
                        }
                    }
                }
                if (IsOnYMax(y, corridorRooms))
                {
                    GameObject spawnTrigger = Instantiate(triggerPrefab, new Vector3(x, y, 0f), Quaternion.identity);
                    foreach (Rect c in completeRooms)
                    {
                        if (c.Contains(new Vector2(x, y)))
                        {
                            spawnTrigger.GetComponent<EnemySpawnTrigger>().triggerRoom = c;
                        }
                    }
                }
            }
            if (corridor.width > corridor.height)
            {

                if (IsOnXMin(x, corridorRooms) && !IsOnYMax(y, corridorRooms) && !IsOnYMin(y, corridorRooms))
                {
                    Destroy(wallTiles[x, y - 1]);
                    InstantiateCorner(x, y - 1, 2);
                }
                if (IsOnXMax(x, corridorRooms))
                {
                    Destroy(wallTiles[x, y - 1]);
                    InstantiateCorner(x, y - 1, 3);
                }
                else if (IsOnXMax(x, corridorRooms) && IsOnYMin(y, corridorRooms))
                {
                    Destroy(wallTiles[x, y - 1]);
                    InstantiateWall(x, y - 1, 3);
                }
                Destroy(wallTiles[x, y + 1]);
                InstantiateWall(x, y + 1, 2);
                GameObject spawnTrigger = InstantiateDungeonObject(triggerPrefab, new Vector3(x, y, 0f), Quaternion.identity);
                foreach (Rect c in completeRooms)
                {
                    if (c.Contains(new Vector2(x, y)))
                    {
                        spawnTrigger.GetComponent<EnemySpawnTrigger>().triggerRoom = c;
                    }
                }
            }
    }

    private bool IsOnYMax(int y, List<Rect> rooms)
    {
        foreach (Rect room in rooms)
        {
            if (room.yMax - 1 == y)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsOnYMin(int y, List<Rect> rooms)
    {
        foreach (Rect room in rooms)
        {
            if (room.yMin == y)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsOnXMax(int x, List<Rect> rooms)
    {
        foreach (Rect room in rooms)
        {
            if (room.xMax - 1 == x)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsOnXMin(int x, List<Rect> rooms)
    {
        foreach (Rect room in rooms)
        {
            if (room.xMin == x)
            {
                return true;
            }
        }
        return false;
    }

    public void DrawCorridor(Room currentRoom)
    {
        if (currentRoom == null)
        {
            return;
        }

        DrawCorridor(currentRoom.left);
        DrawCorridor(currentRoom.right);

        foreach (Rect corridor in currentRoom.corridors)
        {
            for (int x = (int)corridor.x; x < corridor.xMax; x++)
            {
                for (int y = (int)corridor.y; y < corridor.yMax; y++)
                {
                    if (floorTiles[x, y] == null)
                    {
                        Destroy(backgroundTiles[x, y]);
                        GameObject instance = InstantiateDungeonObject(floor, new Vector3(x, y, 0f), Quaternion.identity, "Dungeon");
                        List<Rect> roomConnections = GetCorridorRooms(x, y, corridor);
                        DrawCorridorWalls(x, y);
                        DrawCorners(x, y, corridor, roomConnections);
                        if (wallTiles[x, y] != null)
                        {
                            if (wallTiles[x, y].transform.position == instance.transform.position)
                            {
                                Destroy(wallTiles[x, y]);
                            }
                        }
                    }
                }
            }
        }
    }

    public List<Rect> GetCorridorRooms(int x, int y, Rect corridor)
    {
        List<Rect> rooms = new List<Rect>();
        foreach (Rect room in completeRooms)
        {
            if (corridor.Overlaps(room))
            {
                rooms.Add(room);
            }
        }
        return rooms;
    }

    public void DrawBackground()
    {
        for (int x = 0; x < rows; x++)
        {

            for (int y = 0; y < cols; y++)
            {
                if (x < 20)
                {
                    InstantiateBackground(x - 20, y);
                }
                if (y < 20)
                {
                    InstantiateBackground(x, y - 20);
                }
                if (x > rows - 21)
                {
                    InstantiateBackground(x + 20, y);
                }
                if (y > cols - 21)
                {
                    InstantiateBackground(x, y + 20);
                }

                if (y < 20 && x < 20)
                {
                    InstantiateBackground(x - 20, y - 20);
                }
                if (x > rows - 21 && y < 20)
                {
                    InstantiateBackground(x + 20, y - 20);
                }
                if (x > rows - 21 && y > cols - 21)
                {
                    InstantiateBackground(x + 20, y + 20);
                }
                if (x < 20 && y > cols - 21)
                {
                    InstantiateBackground(x - 20, y + 20);
                }
                GameObject instance = InstantiateDungeonObject(background, new Vector3(x, y, 0f), Quaternion.identity, "Background");
                if (x >= 0 && x <= rows && y >= 0 & y <= cols)
                {
                    backgroundTiles[x, y] = instance;
                }

            }
        }
    }

    private void InstantiateBackground(int x, int y)
    {
        GameObject instance = InstantiateDungeonObject(background, new Vector3(x, y, 0f), Quaternion.identity, "Background");
    }

    private GameObject InstantiateDungeonObject(GameObject prefab, Vector3 position, Quaternion rotation, string sortingLayer = null)
    {
        GameObject obj = Instantiate(prefab, position, rotation);
        obj.transform.SetParent(transform);
        if (sortingLayer != null)
            obj.GetComponent<Renderer>().sortingLayerName = sortingLayer;
        return obj;
    }

    public void Generate(Room room)
    {
        if (room.IsLeaf())
        {
            if (room.rect.width > rMaxSize || room.rect.height > rMaxSize || Random.Range(0.0f, 1.0f) > 0.25)
            {
                if (room.Split(rMinSize, rMaxSize))
                {
                    Generate(room.left);
                    Generate(room.right);
                }
            }
        }
    }

    void Awake()
    {
        Room root = new Room(new Rect(0, 0, rows, cols));
        Generate(root);
        root.CreateRoom();

        backgroundTiles = new GameObject[rows, cols];
        wallTiles = new GameObject[rows, cols];
        floorTiles = new GameObject[rows, cols];

        DrawBackground();
        DrawRoom(root);
        DrawCorridor(root);
    }
}