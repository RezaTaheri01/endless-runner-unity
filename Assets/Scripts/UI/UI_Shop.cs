using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct ColorToSell
{
    public Color color;
    public int price;
}

public enum ColorType
{
    PlayerColor,
    Platform
}

public class UI_Shop : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI notifyText;

    [Header("Platform Info")]
    [SerializeField] private ColorToSell[] platformColor;
    [SerializeField] private GameObject platformColorButtonPrefab;
    [SerializeField] private Transform platformColorParent;
    [SerializeField] private Image platformDisplay;
    [SerializeField] private Image platformMainDisplay;

    [Header("Player Info")]
    [SerializeField] private ColorToSell[] playerColor;
    [SerializeField] private GameObject playerColorButtonPrefab;
    [SerializeField] private Transform playerColorParent;

    [SerializeField] private Image playerDisplay;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        coinText.text = PlayerPrefs.GetInt("TotalAmountOfCoins", 0).ToString();
        CreateShopButtons();
    }

    public void PurchaseColor(Color color, int price, ColorType colorType)
    {
        if (EnoughMoney(price))
        {
            if (colorType == ColorType.Platform)
            {
                platformDisplay.color = color;
                GameManager.instance.platformColor = color;
                GameManager.instance.SaveColorPlatform(color.r, color.g, color.b);
            }
            else if (colorType == ColorType.PlayerColor)
            {
                playerDisplay.color = color;
                GameManager.instance.playerColor = color;
                GameManager.instance.player.GetComponent<SpriteRenderer>().color = color;
                GameManager.instance.SaveColor(color.r, color.g, color.b);
            }
            if (price == 0)
                StartCoroutine(Notify("Already Purchased!", 1f));
            else
            {
                StartCoroutine(Notify("Purchase Successful!", 1f));
                RefreshShop();
            }
        }
        else
            StartCoroutine(Notify("Not Enough Money!", 1f));
    }

    private bool EnoughMoney(int price)
    {
        int coinBalance = PlayerPrefs.GetInt("TotalAmountOfCoins", 0);

        if (coinBalance >= price)
        {
            int newCoinBalance = coinBalance - price;
            PlayerPrefs.SetInt("TotalAmountOfCoins", newCoinBalance);
            coinText.text = PlayerPrefs.GetInt("TotalAmountOfCoins", 0).ToString();
            return true;
        }
        return false;
    }

    private IEnumerator Notify(string text, float seconds)
    {
        notifyText.text = text;
        yield return new WaitForSeconds(seconds);
        notifyText.text = "Click To Buy";
    }

    private void RefreshShop()
    {
        foreach (Transform child in platformColorParent)
            Destroy(child.gameObject);

        foreach (Transform child in playerColorParent)
            Destroy(child.gameObject);

        CreateShopButtons();
    }

    private void CreateShopButtons()
    {
        coinText.text = PlayerPrefs.GetInt("TotalAmountOfCoins", 0).ToString();

        for (int i = 0; i < platformColor.Length; i++)
        {
            Color color = platformColor[i].color;
            int price = platformColor[i].price;

            if (color == GameManager.instance.platformColor)
            {
                platformDisplay.color = color;
                price = 0;
            }

            GameObject newButton = Instantiate(platformColorButtonPrefab, platformColorParent);

            newButton.transform.GetChild(0).GetComponent<Image>().color = color;
            newButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = price.ToString();

            newButton.GetComponent<Button>().onClick.AddListener(() => PurchaseColor(color, price, ColorType.Platform));
        }

        for (int i = 0; i < playerColor.Length; i++)
        {
            Color color = playerColor[i].color;
            int price = playerColor[i].price;

            if (color == GameManager.instance.playerColor)
            {
                playerDisplay.color = color;
                price = 0;
            }

            GameObject newButton = Instantiate(playerColorButtonPrefab, playerColorParent);

            newButton.transform.GetChild(0).GetComponent<Image>().color = color;
            newButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = price.ToString();

            newButton.GetComponent<Button>().onClick.AddListener(() => PurchaseColor(color, price, ColorType.PlayerColor));

        }
    }
}
