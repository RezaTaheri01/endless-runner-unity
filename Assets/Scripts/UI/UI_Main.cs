using UnityEngine;


// Note: 
// 1. There is a SwitchMenuTo in player.cs >> OnJump function
public class UI_Main : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    public GameObject gameMenu;
    [HideInInspector] public static UI_Main instance;

    private bool gamePaused;


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

    public void RestartGameButton() => GameManager.instance.RestartLevel();
}
