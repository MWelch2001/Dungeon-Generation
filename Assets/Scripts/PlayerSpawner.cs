using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private List<Rect> rooms;
    private Rect spawnRoom;
    private List<GameObject> spawnTriggers;
    void Start()
    {
        rooms = GameObject.Find("DungeonGenHandler").GetComponent<DungeonGen>().completeRooms;
        spawnRoom = rooms[Random.Range(0, rooms.Count)];
        int x = (int)Random.Range(spawnRoom.xMin + 1, spawnRoom.xMax - 1);
        int y = (int)Random.Range(spawnRoom.yMin + 1, spawnRoom.yMax - 1);
        player = Instantiate(playerPrefab, new Vector3(x, y, 0), Quaternion.identity);
        player.GetComponent<PlayerBehaviour>().spawnRoom = this.spawnRoom;
        StartCoroutine(GetEnemySpawner(3f));
        offset = new Vector3(0, 0, -10);
    }

    void LateUpdate()
    {
        transform.position = player.transform.position + offset;
    }

    private IEnumerator GetEnemySpawner(float time)
    {
        yield return new WaitForSeconds(time);
        eSpawner = Instantiate(eSpawnerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        eSpawner.tag = "EnemySpawner";
        foreach (GameObject trigger in spawnTriggers = GameObject.FindGameObjectsWithTag("Trigger").ToList<GameObject>())
        {
            trigger.GetComponent<EnemySpawnTrigger>().SetSpawner();
        }
    }
}   
