using UnityEngine;

public class Diamond : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            GameManager.instance.rechargeDash();
            Destroy(gameObject);
        }
    }
}
