using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int coinCount;
 
    public void IncrementCoin()
    {
        coinCount++;
    }
}
