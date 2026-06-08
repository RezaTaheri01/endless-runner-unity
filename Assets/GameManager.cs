using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int coins;

    public Color platformColor;

    public float ledgeBoxSize = 0.15f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake() {
        instance = this;
    }

    public void restartLevel() => SceneManager.LoadScene(0);
}
