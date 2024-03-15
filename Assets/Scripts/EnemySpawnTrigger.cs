using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour
{
    public Rect triggerRoom = new Rect();
    private GameObject spawner;

    void Start()
    {
        spawner = GameObject.FindGameObjectWithTag("EnemySpawner");

    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            spawner.GetComponent<EnemySpawner>().ActivateRoom(triggerRoom);
        }
    }

}