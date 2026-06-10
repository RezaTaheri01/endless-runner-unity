using System;
using TMPro;
using UnityEngine;

public class UI_EndGame : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 0;
        scoreText.text = "Score: " + GameManager.instance.distance.ToString("#,#") + " m";
        coinText.text = "Coins: " + GameManager.instance.coins.ToString("#,#");
    }
}
