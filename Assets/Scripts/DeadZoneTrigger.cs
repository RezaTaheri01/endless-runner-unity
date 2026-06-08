using UnityEngine;

public class DeadZoneTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
     if (collision.tag == "Player")
        {
            GameManager.instance.restartLevel();
        }   
    }
}
