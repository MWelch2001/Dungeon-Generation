using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{

    [SerializeField]
    private GameObject eSpawnerPrefab;
    private GameObject eSpawner;

    public GameObject playerPrefab;
    private GameObject player;
    private Vector3 offset;
    void Start()
    {
        bool isSpawned = false;

        GameObject[,] floorTiles = GameObject.Find("DungeonGenHandler").GetComponent<DungeonGen>().floorTiles;
        GameObject[,] corridorTiles = GameObject.Find("DungeonGenHandler").GetComponent<DungeonGen>().corridorTiles;
        int rows = GameObject.Find("DungeonGenHandler").GetComponent<DungeonGen>().rows;
        int cols = GameObject.Find("DungeonGenHandler").GetComponent<DungeonGen>().cols;
        while (!isSpawned)
        {
            int x = Random.Range(0, rows);
            int y = Random.Range(0, cols);
            if (floorTiles[x, y] != null && corridorTiles[x, y] == null)
            {
                player = Instantiate(playerPrefab, new Vector3(x, y, 0), Quaternion.identity);
                eSpawner = Instantiate(eSpawnerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                eSpawner.tag = "EnemySpawner";
                isSpawned = true;
            }
        }
        offset = new Vector3(0, 0, -10);
    }

    void LateUpdate()
    {

        transform.position = player.transform.position + offset;
    }
}   
