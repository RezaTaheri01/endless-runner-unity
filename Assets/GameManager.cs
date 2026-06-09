using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public PlayerMovement player;
    [HideInInspector] public static GameManager instance;

    [HideInInspector] public int coins;

    public Color platformColor;
    public Color platformMainColor;
    public Color PlayerColor = Color.white;

    public float ledgeBoxSize = 0.15f;
    [HideInInspector] public float distance;
    [HideInInspector] public float startPositionOffset;

    public float extraLifeRechargeTime = 5f;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        startPositionOffset = player.transform.position.x * -1;
    }

    void FixedUpdate()
    {

        if (player.transform.position.x + startPositionOffset > distance)
        {
            distance = player.transform.position.x + startPositionOffset;
        }
    }

    public void RestartLevel() => SceneManager.LoadScene(0);

    public void UnlockPlayer() => player.playerUnlocked = true;

    public void lockPlayer() => player.playerUnlocked = false;

}
