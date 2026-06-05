using UnityEngine;

public class Parallex : MonoBehaviour
{
    private GameObject cam;

    [SerializeField] private float parallaxEffect;

    [SerializeField] private float xOffset=0;

    private float length;
    private float xPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GameObject.Find("Main Camera");
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        xPosition = transform.position.x;
    }

    // Update is called once per frame
    private void Update()
    {
        float distanceMoved = cam.transform.position.x *  (1- parallaxEffect);
        float distanceToMove = cam.transform.position.x * parallaxEffect;

        transform.position = new Vector3(xPosition + distanceToMove, transform.position.y);

        if (distanceMoved> xPosition + length)
        {
            xPosition = xPosition + length + xOffset;
        }
    }
}
