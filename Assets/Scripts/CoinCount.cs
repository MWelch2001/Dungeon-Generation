using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinCount : MonoBehaviour
{
    TextMeshProUGUI coinCount;
    private GameObject player;
    private void Start()
    {
        coinCount = GetComponent<TextMeshProUGUI>();
    }
    void Update()
    {
        player = GameObject.FindWithTag("Player");
        coinCount.text = player.GetComponent<PlayerInventory>().coinCount.ToString();
    }
}
