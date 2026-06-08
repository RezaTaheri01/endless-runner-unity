using UnityEngine;

public class PlatformController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private SpriteRenderer headerSr;
    [SerializeField] private GameObject ledgePrefab; // Change to GameObject instead of Transform
    
    private Transform ledge;

    void Start()
    {
        headerSr.color = sr.color;
        headerSr.transform.parent = transform.parent;
        headerSr.transform.localScale = new Vector2(sr.bounds.size.x, .2f);
        headerSr.transform.position = new Vector2(transform.position.x, sr.bounds.max.y - .1f);
        
        // Instantiate the ledge prefab
        InstantiateLedgeAndPositionAtLeftTop();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            headerSr.color = GameManager.instance.platformColor;
        }
    }
    
    private void InstantiateLedgeAndPositionAtLeftTop()
    {
        if (ledgePrefab == null || sr == null) return;
        
        // Instantiate the ledge as a child of this platform
        GameObject ledgeObject = Instantiate(ledgePrefab, transform, transform);
        ledge = ledgeObject.transform;
        
        // Get the BoxCollider2D component from the instantiated ledge
        BoxCollider2D ledgeCollider = ledge.GetComponent<BoxCollider2D>();
        if (ledgeCollider == null)
        {
            Debug.LogWarning("No BoxCollider2D found on ledge prefab!");
            return;
        }
        
        // Get sprite bounds
        Bounds spriteBounds = sr.bounds;
        
        // Define collider size (adjust as needed)
        float colliderSize = 0.2f;
        
        // Calculate position at left top corner
        // Position is the center of the collider, so offset by half size
        float centerX = spriteBounds.min.x + (colliderSize / 2);
        float centerY = spriteBounds.max.y - (colliderSize / 2);
        
        // Set position and collider size
        ledge.position = new Vector2(centerX, centerY);
        ledgeCollider.size = new Vector2(colliderSize, colliderSize);
    }
}