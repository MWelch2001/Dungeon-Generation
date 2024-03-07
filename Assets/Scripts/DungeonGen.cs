using UnityEngine;
using System.Collections.Generic;

public class DungeonGen : MonoBehaviour
{
    public int rows, cols;
    public int rMaxSize, rMinSize;
    public GameObject floor;
    public GameObject corridor;
    public GameObject background;
    public GameObject[] cardinalWalls = new GameObject[4];
    public GameObject[] cornerWalls = new GameObject[2];
    private GameObject[,] floorTiles;
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
                Rect lroom = left.GetRoom();
                if (lroom.x != -1)
                {
                    return lroom;
                }
            }
            if (right != null)
            {
                Rect rroom = right.GetRoom();
                if (rroom.x != -1)
                {
                    return rroom;
                }
            }

            return new Rect(-1, -1, 0, 0);
        }

        public void CreateCorridor(Room left, Room right)
        {
            Rect l = left.GetRoom();
            Rect r = right.GetRoom();
            Vector2 p1 = new Vector2((int)Random.Range(l.x + 1, l.xMax - 1), (int)Random.Range(l.y + 1, l.yMax - 1));
            Vector2 p2 = new Vector2((int)Random.Range(r.x + 1, r.xMax - 1), (int)Random.Range(r.y + 1, r.yMax - 1));

            if (p1.x > p2.x)
            {
                Vector2 temp = p1;
                p1 = p2;
                p2 = temp;
            }
            int w = (int)(p1.x - p2.x);
            int h = (int)(p1.y - p2.y);

            if (w != 0)
            {
                if (Random.Range(0, 1) > 2)
                {
                    corridors.Add(new Rect(p1.x, p1.y, Mathf.Abs(w) + 1, 1));                 
                    if (h < 0)
                    {
                        corridors.Add(new Rect(p2.x, p1.y, 1, Mathf.Abs(h)));
                    }
                    else
                    {
                        corridors.Add(new Rect(p2.x, p1.y, 1, -Mathf.Abs(h)));
                    }
                }
                else
                {
                    if (h < 0)
                    {
                        corridors.Add(new Rect(p1.x, p1.y, 1, Mathf.Abs(h)));
                    }
                    else
                    {
                        corridors.Add(new Rect(p1.x, p2.y, 1, Mathf.Abs(h)));
                    }
                    corridors.Add(new Rect(p1.x, p2.y, Mathf.Abs(w) + 1, 1));
                }
            }
            else
            {
                if (h < 0)
                {
                    corridors.Add(new Rect((int)p1.x, (int)p1.y, 1, Mathf.Abs(h)));
                }
                else
                {
                    corridors.Add(new Rect((int)p2.x, (int)p2.y, 1, Mathf.Abs(h)));
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
                    floorTiles[x, y] = instance;
                    
                    DrawWalls(currentRoom, x, y);
                }
            }
        } else {
            DrawRoom(currentRoom.left);
            DrawRoom(currentRoom.right);
        }
    }

    public void DrawWalls(Room currentRoom, int x, int y)
    {
        
        if (x == currentRoom.room.xMin)
        {
            Destroy(backgroundTiles[x - 1, y]);
            GameObject wall = Instantiate(cardinalWalls[0], new Vector3(x - 1, y, 0f), Quaternion.identity);
            wall.transform.SetParent(transform);
            wallTiles[x, y] = wall;
        }
        else if (x == currentRoom.room.xMax - 1)
        {
            Destroy(backgroundTiles[x + 1, y]);
            GameObject wall = Instantiate(cardinalWalls[1], new Vector3(x + 1, y, 0f), Quaternion.identity);
            wall.transform.SetParent(transform);
            wallTiles[x, y] = wall;
        }
        if (y == currentRoom.room.yMin)
        {
            Destroy(backgroundTiles[x, y - 1]);
            GameObject wall = Instantiate(cardinalWalls[3], new Vector3(x, y - 1, 0f), Quaternion.identity);
            wall.transform.SetParent(transform);
            wallTiles[x, y] = wall;
        }
        else if (y == currentRoom.room.yMax - 1)
        {
            Destroy(backgroundTiles[x, y + 1]);
            GameObject wall = Instantiate(cardinalWalls[2], new Vector3(x, y + 1, 0f), Quaternion.identity);
            wall.transform.SetParent(transform);
            wallTiles[x, y] = wall;
        }

        if (y == currentRoom.room.yMax - 1 && x == currentRoom.room.xMin)
        {
            Destroy(backgroundTiles[x - 1, y + 1]);
            GameObject topLeftWall = Instantiate(cardinalWalls[0], new Vector3(x - 1, y + 1, 0f), Quaternion.identity);
            topLeftWall.transform.SetParent(transform);
            wallTiles[x, y] = topLeftWall;
        }
        if (y == currentRoom.room.yMin && x == currentRoom.room.xMin)
        {
            Destroy(backgroundTiles[x - 1, y - 1]);
            GameObject bottomLeftWall = Instantiate(cornerWalls[0], new Vector3(x - 1, y - 1, 0f), Quaternion.identity);
            bottomLeftWall.transform.SetParent(transform);
            wallTiles[x, y] = bottomLeftWall;
        }

        if (y == currentRoom.room.yMax - 1 && x == currentRoom.room.xMax - 1)
        {
            Destroy(backgroundTiles[x + 1, y + 1]);
            GameObject topRightWall = Instantiate(cardinalWalls[1], new Vector3(x + 1, y + 1, 0f), Quaternion.identity);
            topRightWall.transform.SetParent(transform);
            wallTiles[x, y] = topRightWall;
        }
        if (y == currentRoom.room.yMin && x == currentRoom.room.xMax - 1)
        {
            Destroy(backgroundTiles[x + 1, y - 1]);
            GameObject bottomRightWall = Instantiate(cornerWalls[1], new Vector3(x + 1, y - 1, 0f), Quaternion.identity);
            bottomRightWall.transform.SetParent(transform);
            wallTiles[x, y] = bottomRightWall;
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
                        GameObject instance = Instantiate(this.corridor, new Vector3(x, y, 0f), Quaternion.identity);
                        instance.transform.SetParent(transform);
                        floorTiles[x, y] = instance;
                        if (wallTiles[x, y] == null)
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
        for (int x = 0; x < rows ; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                GameObject instance = Instantiate(background, new Vector3(x, y, 0f), Quaternion.identity);
                instance.transform.SetParent(transform);
                backgroundTiles[x, y] = instance;
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

    void Start()
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