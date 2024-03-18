using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour
{
    public Rect triggerRoom = new Rect();
    public GameObject spawner;


    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            spawner.GetComponent<EnemySpawner>().ActivateRoom(triggerRoom);
        }
    }

    public void SetSpawner()
    {
        spawner = GameObject.FindGameObjectWithTag("EnemySpawner");
    }

}