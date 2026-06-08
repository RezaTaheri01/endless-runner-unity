using UnityEngine;

public class MovingTrap : Trap
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private Transform[] movePoints;

    private int index;
    private int iterateDir = 1;
    private int movePointsLength;
    private bool isDestroyed = false;

    protected override void Start()
    {
        base.Start();

        movePointsLength = movePoints.Length;
        if (movePointsLength == 0)
        {
            Destroy(gameObject);
            isDestroyed = true;
            return; // Don't continue execution
        }

        transform.position = movePoints[0].position;
        index = 0;
    }

    private void FixedUpdate()
    {
        if (isDestroyed) return; // Stop if destroyed

        if (movePointsLength > 1)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                                                        movePoints[index].position,
                                                        speed * Time.deltaTime);

            // Use distance check instead of ==
            if (Vector3.Distance(transform.position, movePoints[index].position) < 0.01f)
            {
                // Check boundaries
                if (index >= movePointsLength - 1)
                    iterateDir = -1;
                else if (index <= 0)
                    iterateDir = 1;

                index += iterateDir;

                // Clamp index to valid range
                index = Mathf.Clamp(index, 0, movePointsLength - 1);
            }
        }

        // Only apply rotation if trap still exists and has move points
        if (!isDestroyed && movePointsLength > 0 && index >= 0 && index < movePointsLength)
        {
            if (transform.position.x > movePoints[index].position.x)
                transform.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime));
            else
                transform.Rotate(new Vector3(0, 0, -rotationSpeed * Time.deltaTime));
        }
    }
}