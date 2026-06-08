using UnityEngine;

public class LimiterGizmos : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private Transform groundLevel;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawLine(startPoint.position, new Vector2(startPoint.position.x, startPoint.position.y + 1000));
        Gizmos.DrawLine(startPoint.position, new Vector2(startPoint.position.x, startPoint.position.y - 1000));
    
        Gizmos.color = Color.red;
        Gizmos.DrawLine(endPoint.position, new Vector2(endPoint.position.x, endPoint.position.y + 1000));
        Gizmos.DrawLine(endPoint.position, new Vector2(endPoint.position.x, endPoint.position.y - 1000));

        Gizmos.DrawLine(groundLevel.position, new Vector2(groundLevel.position.x + 1000, groundLevel.position.y));
        Gizmos.DrawLine(groundLevel.position, new Vector2(groundLevel.position.x - 1000, groundLevel.position.y));
    }
}
