using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinGenerator : MonoBehaviour
{
    private int amountOfCoin;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int minCoins;
    [SerializeField] private int maxCoins;
    [SerializeField] private SpriteRenderer[] coinsIMG;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Only visible on editor
        for (int i = 0; i < coinsIMG.Length; i++)
        {
            coinsIMG[i].sprite = null;
        }

        amountOfCoin = Random.Range(minCoins, maxCoins);
        int additionalOffset = amountOfCoin / 2;
        for (int i = 0; i < amountOfCoin; i++)
        {
            Vector3 offset = new Vector3(i - additionalOffset, 0, 0);
            Instantiate(coinPrefab, transform.position + offset, Quaternion.identity, transform);
        }
    }
}
