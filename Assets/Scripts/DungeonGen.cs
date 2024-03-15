using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using JetBrains.Annotations;
using Unity.Collections.LowLevel.Unsafe;
using static DungeonGen; 

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
    public GameObject[,] corridorTiles;
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
        } else {
            DrawRoom(currentRoom.left);
            DrawRoom(currentRoom.right);
        }
    }


    public void DrawWalls(Room currentRoom, int x, int y)
    {
        if (x == currentRoom.room.xMin && y != currentRoom.room.yMin)
        {
            ClearOverlap(x, y, 2);
            GameObject wall = Instantiate(cardinalWalls[0], new Vector3(x, y, 0f), Quaternion.identity);
            wall.transform.SetParent(transform);
            wall.GetComponent<Renderer>().sortingLayerName = "Dungeon";
            wallTiles[x, y] = wall;
        }
        else if (x == currentRoom.room.xMax - 1 && y != currentRoom.room.yMin)
        {
            ClearOverlap(x, y, 2);
            GameObject wall = Instantiate(cardinalWalls[1], new Vector3(x, y, 0f), Quaternion.identity);
            wall.transform.SetParent(transform);
            wall.GetComponent<Renderer>().sortingLayerName = "Dungeon";
            wallTiles[x, y] = wall;
            
        }

        if (y == currentRoom.room.yMin)
        {
            ClearOverlap(x, y, 2);
            GameObject wall = Instantiate(cardinalWalls[3], new Vector3(x, y, 0f), Quaternion.identity);
            wall.transform.SetParent(transform);
            wall.GetComponent<Renderer>().sortingLayerName = "Dungeon";
            wallTiles[x, y] = wall;
            
        }
        else if (y == currentRoom.room.yMax - 1)
        {
            ClearOverlap(x, y, 2);
            GameObject wall = Instantiate(cardinalWalls[2], new Vector3(x, y , 0f), Quaternion.identity);
            wall.transform.SetParent(transform);
            wall.GetComponent<Renderer>().sortingLayerName = "Dungeon";
            wallTiles[x, y] = wall;
        }

        if (y == currentRoom.room.yMax - 1 && x == currentRoom.room.xMin )
        {
            ClearOverlap(x, y, 3);
            GameObject topLeftWall = Instantiate(cardinalWalls[0], new Vector3(x , y, 0f), Quaternion.identity);
            topLeftWall.transform.SetParent(transform);
            topLeftWall.GetComponent<Renderer>().sortingLayerName = "Dungeon";
            wallTiles[x, y ] = topLeftWall;
        }
        if (y == currentRoom.room.yMin && x == currentRoom.room.xMin)
        {
            ClearOverlap(x, y, 3);
            GameObject bottomLeftWall = Instantiate(cornerWalls[0], new Vector3(x, y, 0f), Quaternion.identity);
            bottomLeftWall.transform.SetParent(transform);
            bottomLeftWall.GetComponent<Renderer>().sortingLayerName = "Dungeon";
            wallTiles[x , y] = bottomLeftWall;
        }

        if (y == currentRoom.room.yMax - 1 && x == currentRoom.room.xMax - 1)
        {
            ClearOverlap(x, y, 3);
            GameObject topRightWall = Instantiate(cardinalWalls[1], new Vector3(x , y , 0f), Quaternion.identity);
            topRightWall.transform.SetParent(transform);
            wallTiles[x, y] = topRightWall;
            topRightWall.GetComponent<Renderer>().sortingLayerName = "Dungeon";
        }
        if (y == currentRoom.room.yMin && x == currentRoom.room.xMax - 1)
        {
            ClearOverlap(x, y, 3);
            GameObject bottomRightWall = Instantiate(cornerWalls[1], new Vector3(x, y, 0f), Quaternion.identity);
            bottomRightWall.transform.SetParent(transform);
            bottomRightWall.GetComponent<Renderer>().sortingLayerName = "Dungeon";
            wallTiles[x, y] = bottomRightWall;
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
            GameObject lWall = Instantiate(cardinalWalls[0], new Vector3(x - 1, y, 0f), Quaternion.identity);
            lWall.transform.SetParent(transform);
            lWall.GetComponent<Renderer>().sortingLayerName = "Dungeon";
            wallTiles[x - 1, y] = lWall;
        }
        if (floorTiles[x + 1, y] == null && wallTiles[x + 1, y] == null)
        {
            ClearOverlap(x + 1, y, 1);
            GameObject lWall = Instantiate(cardinalWalls[1], new Vector3(x +  1, y, 0f), Quaternion.identity);
            lWall.transform.SetParent(transform);
            lWall.GetComponent<Renderer>().sortingLayerName = "Dungeon";
            wallTiles[x + 1, y] = lWall;
        }

        if (floorTiles[x , y + 1] == null && wallTiles[x, y + 1] == null)
        {
            ClearOverlap(x , y + 1, 1);
            GameObject lWall = Instantiate(cardinalWalls[2], new Vector3(x , y + 1, 0f), Quaternion.identity);
            lWall.transform.SetParent(transform);
            lWall.GetComponent<Renderer>().sortingLayerName = "Dungeon";
            wallTiles[x , y+ 1] = lWall;
        }
        if (floorTiles[x, y - 1] == null && wallTiles[x, y - 1] == null)
        {
            ClearOverlap(x, y - 1, 1);
            GameObject lWall = Instantiate(cardinalWalls[3], new Vector3(x, y - 1, 0f), Quaternion.identity);
            lWall.transform.SetParent(transform);
            lWall.GetComponent<Renderer>().sortingLayerName = "Dungeon";
            wallTiles[x, y - 1] = lWall;
        }
    }
    private void MakeCorner(int x, int y, int wallIndex)
    {
        Destroy(wallTiles[x, y]);
        wallTiles[x, y] = null;
        GameObject cornerWall = Instantiate(cornerWalls[wallIndex], new Vector3(x, y, 0f), Quaternion.identity);
        cornerWall.transform.SetParent(transform);
        cornerWall.GetComponent<Renderer>().sortingLayerName = "Dungeon";
        wallTiles[x, y] = cornerWall;
    }
    public void DrawCorners(int x, int y, Rect corridor)
    {
        if (corridor.height > corridor.width)
        {
            if (wallTiles[x + 2, y] != null && wallTiles[x - 2, y] != null)
            {
                if (IsUpperCloser(y, corridor))
                {
                    MakeCorner(x - 1, y, 2);
                    MakeCorner(x + 1, y, 3);
                    
                }
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
        if(corridor.width > corridor.height)
        {

            if (wallTiles[x, y + 2] != null && wallTiles[x, y - 2] != null)
            {
                if (IsLeftCloser(x, corridor))
                {
                    MakeCorner(x, y - 1, 2);
                }
                if(!IsLeftCloser(x,corridor))
                {
                    MakeCorner(x, y - 1, 3);
                }
                Destroy(wallTiles[x, y + 1]);
                wallTiles[x, y + 1] = null;
                GameObject cornerWall = Instantiate(cardinalWalls[2], new Vector3(x, y + 1, 0f), Quaternion.identity);
                GameObject spawnTrigger = Instantiate(triggerPrefab, new Vector3(x, y, 0f), Quaternion.identity);
                foreach (Rect c in completeRooms)
                {
                    if (c.Contains(new Vector2(x, y)))
                    {
                        spawnTrigger.GetComponent<EnemySpawnTrigger>().triggerRoom = c;
                    }
                }
                cornerWall.transform.SetParent(transform);
                cornerWall.GetComponent<Renderer>().sortingLayerName = "Dungeon";
                wallTiles[x, y + 1] = cornerWall;
            }
        }
    }

    public bool IsUpperCloser(int y, Rect corridor)
    {
        int dYMax = (int)corridor.yMax - y;
        int dYMin = y - (int)corridor.yMin;


        if (dYMin > dYMax)
        {
            return true;
        }
        else
        {
            return false; 
        }     
    }

    public bool IsLeftCloser(int x, Rect corridor)
    {
        int dXMax = (int)corridor.xMax - x;
        int dXMin = x - (int)corridor.xMin;

        if (dXMin > dXMax)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void DrawCorridor(Room currentRoom)
    {
        if (currentRoom == null)
        {
            return;
        }

        DrawCorridor(currentRoom.left);
        DrawCorridor(currentRoom.right);

        foreach(Rect corridor in currentRoom.corridors)
        {
            for (int x = (int)corridor.x; x < corridor.xMax; x++){
                for (int y = (int)corridor.y; y < corridor.yMax; y++)
                { 
                    if (floorTiles[x,y] == null)
                    {  
                        Destroy(backgroundTiles[x, y]);
                        GameObject instance = Instantiate(floor, new Vector3(x, y, 0f), Quaternion.identity);
                        instance.transform.SetParent(transform);
                        instance.GetComponent<Renderer>().sortingLayerName = "Dungeon";
                        corridorTiles[x, y] = instance; 
                        DrawCorridorWalls(x, y);
                        DrawCorners(x, y, corridor);
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

    public void DrawBackground()
    {
        for (int x = 0; x < rows; x++)
        {

            for (int y = 0; y < cols; y++)
            {
                if (x < 20)
                {
                    GameObject inst = Instantiate(background, new Vector3(x - 20, y, 0f), Quaternion.identity);
                    inst.transform.SetParent(transform);
                    inst.GetComponent<Renderer>().sortingLayerName = "Background";
                }
                if (y < 20)
                {
                    GameObject inst = Instantiate(background, new Vector3(x, y - 20, 0f), Quaternion.identity);
                    inst.transform.SetParent(transform);
                    inst.GetComponent<Renderer>().sortingLayerName = "Background";
                }
                if (x > rows - 21)
                {
                    GameObject inst = Instantiate(background, new Vector3(x + 20, y, 0f), Quaternion.identity);
                    inst.transform.SetParent(transform);
                    inst.GetComponent<Renderer>().sortingLayerName = "Background";
                }
                if (y > cols - 21)
                {
                    GameObject inst = Instantiate(background, new Vector3(x , y + 20, 0f), Quaternion.identity);
                    inst.transform.SetParent(transform);
                    inst.GetComponent<Renderer>().sortingLayerName = "Background";
                }

                if (y < 20 && x < 20)
                {
                    GameObject inst = Instantiate(background, new Vector3(x - 20, y - 20, 0f), Quaternion.identity);
                    inst.transform.SetParent(transform);
                    inst.GetComponent<Renderer>().sortingLayerName = "Background";
                }
                if (x > rows - 21 && y < 20)
                {
                    GameObject inst = Instantiate(background, new Vector3(x + 20, y - 20, 0f), Quaternion.identity);
                    inst.transform.SetParent(transform);
                    inst.GetComponent<Renderer>().sortingLayerName = "Background";
                }
                if (x > rows - 21 && y > cols - 21)
                {
                    GameObject inst = Instantiate(background, new Vector3(x + 20, y + 20, 0f), Quaternion.identity);
                    inst.transform.SetParent(transform);
                    inst.GetComponent<Renderer>().sortingLayerName = "Background";
                }
                if (x < 20 && y > cols - 21)
                {
                    GameObject inst = Instantiate(background, new Vector3(x - 20, y + 20, 0f), Quaternion.identity);
                    inst.transform.SetParent(transform);
                    inst.GetComponent<Renderer>().sortingLayerName = "Background";
                }

                GameObject instance = Instantiate(background, new Vector3(x, y, 0f), Quaternion.identity);
                instance.transform.SetParent(transform);
                instance.GetComponent<Renderer>().sortingLayerName = "Background";
                if (x >= 0 && x <= rows && y >= 0 & y <= cols)
                {
                    backgroundTiles[x, y] = instance;
                }
            }
        }
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
        corridorTiles = new GameObject[rows, cols];

        DrawBackground();
        DrawRoom(root);
        DrawCorridor(root);
    }
}