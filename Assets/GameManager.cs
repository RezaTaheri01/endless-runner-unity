using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public PlayerMovement player;
    public static GameManager instance;

    [HideInInspector] public int coins;

    public Color platformColor;
    public Color platformMainColor;
    public Color playerColor = Color.white;

    public float ledgeBoxSize = 0.15f;
    [HideInInspector] public float distance;
    [HideInInspector] public float startPositionOffset;

    public float extraLifeRechargeTime = 5f;

    void Awake()
    {
        instance = this;
        Time.timeScale = 1;
        Debug.Log("GameManager initialized successfully");
    }

    void Start()
    {
        LoadColor();
        startPositionOffset = player.transform.position.x * -1;
    }

    void FixedUpdate()
    {

        if (player.transform.position.x + startPositionOffset > distance)
        {
            distance = player.transform.position.x + startPositionOffset;
        }
    }

    public void RestartLevel()
    {
        SaveInfo();
        SceneManager.LoadScene(0);
    }

    public void UnlockPlayer() => player.playerUnlocked = true;

    public void lockPlayer() => player.playerUnlocked = false;

    #region Save and Load

    public void SaveInfo()
    {
        int savedCoins = PlayerPrefs.GetInt("TotalAmountOfCoins", 0);

        // coins get reset at each death
        PlayerPrefs.SetInt("TotalAmountOfCoins", coins + savedCoins);

        PlayerPrefs.SetInt("LastScore", (int)distance);

        if (PlayerPrefs.GetInt("HighScore", 0) < (int)distance)
        {
            PlayerPrefs.SetInt("HighScore", (int)distance);
        }
    }


    public void SaveColor(float r, float g, float b)
    {
        PlayerPrefs.SetFloat("ColorR", r);
        PlayerPrefs.SetFloat("ColorG", g);
        PlayerPrefs.SetFloat("ColorB", b);
    }

    private void LoadColor()
    {
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();

        Color newColor = new Color(PlayerPrefs.GetFloat("ColorR", 255),
                                   PlayerPrefs.GetFloat("ColorG", 255),
                                   PlayerPrefs.GetFloat("ColorB", 255),
                                   PlayerPrefs.GetFloat("ColorA", 1));

        sr.color = newColor;
        playerColor = newColor;
    }


    public void GameEnded()
    {
        player.playerUnlocked = false;
        SaveInfo();
        UI_Main.instance.OpenEndGameUI();
    }
    # endregion
}

