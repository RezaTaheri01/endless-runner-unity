using Unity.VisualScripting;
using UnityEngine;

public class LedgeCheck : MonoBehaviour
{
    [SerializeField] private float radius = 1f;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private LayerMask ledgeLayer;


    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    void FixedUpdate()
    {
        player.ledgeDetected = Physics2D.OverlapCircle(transform.position, radius, ledgeLayer);
    }

    /// Callback to draw gizmos that are pick-able and always drawn.
    void OnDrawGizmos()
    {   
        Gizmos.color = player.ledgeDetected ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

}
