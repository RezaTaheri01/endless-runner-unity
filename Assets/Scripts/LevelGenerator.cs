using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private Transform[] levelPart;
    [SerializeField] private Vector3 nextPartPosition;

    [SerializeField] private float distanceToSpawn;
    [SerializeField] private float distanceToDelete;
    [SerializeField] private Transform playerTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CompareWidestPart();
    }


    private void CompareWidestPart()
    {
        if (levelPart == null || levelPart.Length == 0) return;

        float maxWidth = 0f;
        bool foundValidPlatform = false;

        for (int i = 0; i < levelPart.Length; i++)
        {
            Transform platform = levelPart[i]?.Find("Platform");
            if (platform != null)
            {
                Renderer renderer = platform.GetComponent<Renderer>();
                if (renderer != null)
                {
                    float width = renderer.bounds.size.x;
                    if (!foundValidPlatform || width > maxWidth)
                    {
                        maxWidth = width;
                        foundValidPlatform = true;
                    }
                }
            }
        }

        if (!foundValidPlatform) return;

        // Consider using a configurable multiplier instead of magic number 5
        float spawnDistance = maxWidth * 5f;

        if (distanceToSpawn < maxWidth)
        {
            distanceToSpawn = spawnDistance;
        }

        if (distanceToDelete < maxWidth)
        {
            distanceToDelete = spawnDistance;
        }
    }


    // Update is called once per frame
    void Update()
    {
        DeletePlatform();
        GeneratePlatform();
    }

    // private void GeneratePlatform()
    // {
    //     while (Vector2.Distance(playerTransform.position, nextPartPosition) < distanceToSpawn)
    //     {
    //         Transform part = levelPart[Random.Range(0, levelPart.Length)];

    //         Vector2 newPosition = new Vector2(nextPartPosition.x - part.Find("StartPoint").position.x, 0);

    //         Transform newPart = Instantiate(part, newPosition, transform.rotation, transform);

    //         nextPartPosition = newPart.Find("EndPoint").position;
    //     }
    // }

    private void GeneratePlatform()
    {
        while (Vector2.Distance(playerTransform.position, nextPartPosition) < distanceToSpawn)
        {
            Transform part = levelPart[Random.Range(0, levelPart.Length)];

            Transform startPoint = part.Find("StartPoint");
            Transform endPoint = part.Find("EndPoint");

            if (startPoint == null || endPoint == null)
            {
                Debug.LogError($"StartPoint or EndPoint missing in {part.name}");
                return;
            }

            // Calculate world position for new part
            // The StartPoint of the new part should be at nextPartPosition
            Vector3 startPointWorldOffset = startPoint.position - part.position;
            Vector3 newPosition = nextPartPosition - startPointWorldOffset;

            Transform newPart = Instantiate(part, newPosition, Quaternion.identity, transform);
            nextPartPosition = endPoint.position + (newPart.position - part.position);

            // Alternative: simpler way
            // nextPartPosition = newPart.Find("EndPoint").position;
        }
    }

    private void DeletePlatform()
    {
        if (transform.childCount > 0)
        {
            Transform partToDelete = transform.GetChild(0);

            // Check if playerTransform exists
            if (playerTransform == null) return;

            // Check if partToDelete still exists
            if (partToDelete == null) return;

            if (Vector2.Distance(playerTransform.position, partToDelete.position) > distanceToDelete)
            {
                Destroy(partToDelete.gameObject);
            }
        }
    }
}

