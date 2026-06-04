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

    // Todo: Compare with screen size
    private void CompareWidestPart()
    {
        Transform widest = levelPart[0];
        float maxWidth = widest.GetComponent<Renderer>().bounds.size.x;

        for (int i = 1; i < levelPart.Length; i++)
        {
            float width = levelPart[i].GetComponent<Renderer>().bounds.size.x;
            if (width > maxWidth)
            {
                maxWidth = width;
            }
        }
        if (distanceToSpawn < maxWidth)
        {
            distanceToSpawn = maxWidth * 5;
        }
        if (distanceToDelete < maxWidth)
        {
            distanceToDelete = maxWidth * 5;
        }
    }


    // Update is called once per frame
    void Update()
    {
        DeletePlatform();
        GeneratePlatform();
    }

    private void GeneratePlatform()
    {
        while (Vector2.Distance(playerTransform.position, nextPartPosition) < distanceToSpawn)
        {
            Transform part = levelPart[Random.Range(0, levelPart.Length)];

            Vector2 newPosition = new Vector2(nextPartPosition.x - part.Find("StartPoint").position.x, 0);

            Transform newPart = Instantiate(part, newPosition, transform.rotation, transform);

            nextPartPosition = newPart.Find("EndPoint").position;
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

