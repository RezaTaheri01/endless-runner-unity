using TMPro;
using UnityEngine;


// Note: 
// 1. There is a SwitchMenuTo in player.cs >> OnJump function
public class UI_Main : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject gameMenu;
    public GameObject pauseMenu;
    public GameObject endGameMenu;
    [HideInInspector] public static UI_Main instance;

    private bool gamePaused;


    [SerializeField] private TextMeshProUGUI lastScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI coinText;


    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Time.timeScale = 1;
        SwitchMenuTo(mainMenu);
    }

    public void SwitchMenuTo(GameObject targetMenu)
    {
        Debug.Log("Switching Menu");
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);

        }

        targetMenu.SetActive(true);

        coinText.text = PlayerPrefs.GetInt("TotalAmountOfCoins", 0).ToString("#,#");
        lastScoreText.text = "Last Score: " + PlayerPrefs.GetInt("LastScore", 0).ToString("#,#") + " m";
        highScoreText.text = "Best Score: " + PlayerPrefs.GetInt("HighScore", 0).ToString("#,#") + " m";
    }

    public void StartGameButton()
    {
        GameManager.instance.UnlockPlayer();
    }

    public void PauseGameButton()
    {
        gamePaused = !gamePaused;

        if (gamePaused)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    public void ResumeGameButton()
    {
        Time.timeScale = 1;
    }

    public void RestartGameButton() => GameManager.instance.RestartLevel();

    public void OpenEndGameUI()
    {
        SwitchMenuTo(endGameMenu);
    }
}
