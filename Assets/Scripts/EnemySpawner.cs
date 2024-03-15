using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject goblinPrefab;
    private Vector3[] spawnedLoc = new Vector3[3];
    GameObject player;
    private List<Rect> rooms;
    private List<GameObject> enemies = new List<GameObject>();
    void Start()
    {
        rooms = GameObject.Find("DungeonGenHandler").GetComponent<DungeonGen>().completeRooms;
        player = GameObject.FindGameObjectWithTag("Player");

        foreach (Rect room in rooms)
        {
            for (int i = 0; i < spawnedLoc.Length; i++)
            {

                int x = (int)Random.Range(room.xMin + 1, room.xMax - 1);
                int y = (int)Random.Range(room.yMin + 1, room.yMax - 1);
                if (IsValidSpawn(x, y))
                {
                    GameObject enemy = Instantiate(goblinPrefab, new Vector3(x, y, 0), Quaternion.identity);
                    spawnedLoc[i] = new Vector3(x, y, 0.0f);
                    enemy.SetActive(false);
                    enemy.GetComponent<EnemyBehaviour>().spawnRoom = room;
                    enemies.Add(enemy);
                }
            }
        }  
    }

    private bool IsValidSpawn(int x, int y)
    {
        foreach (Vector3 loc in spawnedLoc)
        {
            if (loc.x == x && loc.y == y || loc == player.transform.position)
            {
                return false;
            }
        }
        return true;
    }

    public void ActivateRoom(Rect room)
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                if (enemy.GetComponent<EnemyBehaviour>().spawnRoom.x == room.x && enemy.GetComponent<EnemyBehaviour>().spawnRoom.y == room.y)
                {
                    enemy.SetActive(true);
                }
            }
            
        }
    }
}