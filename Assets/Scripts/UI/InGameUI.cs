using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    private PlayerMovement player;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Image emptyHeart;
    [SerializeField] private Image fullHeart;

    private float distance;
    private int coins;

    void Start()
    {
        player = GameManager.instance.player;
        InvokeRepeating("UpdateInfo", 0, .2f);
    }

    void UpdateInfo()
    {
        distance = (int)GameManager.instance.distance;
        coins = GameManager.instance.coins;

        if (distance > 0)
            distanceText.text = "Score: " + (distance).ToString("#,#") + " M";
        
        if (coins > 0)
            coinsText.text = coins.ToString("#,#");

        emptyHeart.enabled = !player.extraLife;
        fullHeart.enabled = player.extraLife;
    }
}
