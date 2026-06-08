using UnityEngine;

public class Trap : MonoBehaviour
{
    protected float chanceToSpawn = 60;
    protected virtual void Start()
    {
        bool canSpawn = chanceToSpawn <= Random.Range(0, 100);

        if (!canSpawn)
            Destroy(gameObject);
    }

    /// Sent when another object enters a trigger collider attached to this
    /// object (2D physics only).
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
            other.GetComponent<PlayerMovement>().Damage();
    }
}
