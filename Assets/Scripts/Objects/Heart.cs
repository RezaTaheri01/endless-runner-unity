using UnityEngine;

public class Heart : MonoBehaviour
{
    [SerializeField, Range(0, 100)]
    private int chanceToSpawn = 100;

    private void Start()
    {
        // Decide whether this heart should appear when the level starts.
        // If the random roll fails, permanently remove the heart.
        if (Random.Range(0, 100) > chanceToSpawn)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore anything that isn't the player.
        if (!other.CompareTag("Player"))
            return;

        // Give the player an extra life.
        GameManager.instance.player.extraLifeCount++;

        // Remove the heart after it has been collected.
        Destroy(gameObject);
    }
}