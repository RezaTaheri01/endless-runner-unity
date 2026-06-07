using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int coins;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake() {
        instance = this;
    }
}
