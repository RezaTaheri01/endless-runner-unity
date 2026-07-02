using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField, Range(0, 100)]
    protected int chanceToSpawn = 60;

    protected virtual void Start()
    {
        // Decide whether this trap should appear when the level starts.
        if (Random.Range(0, 100) > chanceToSpawn)
        {
            Destroy(gameObject);
        }
    }

    /// Sent when another object enters a trigger collider attached to this
    /// object (2D physics only).
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
            other.GetComponent<PlayerMovement>().Damage();
    }
}
