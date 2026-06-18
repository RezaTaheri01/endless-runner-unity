using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private float moveDistance = 0.5f;
    [SerializeField] private float fadeDuration = 0.3f;

    private SpriteRenderer spriteRenderer;
    private Collider2D coinCollider;
    private bool collected;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        coinCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        collected = true;

        GameManager.instance.coins++;
        coinCollider.enabled = false;

        StartCoroutine(CollectAnimation());
    }

    private IEnumerator CollectAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * moveDistance;

        Color startColor = spriteRenderer.color;

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            transform.position = Vector3.Lerp(startPos, endPos, t);

            Color color = startColor;
            color.a = Mathf.Lerp(1f, 0f, t);
            spriteRenderer.color = color;

            yield return null;
        }

        Destroy(gameObject);
    }
}