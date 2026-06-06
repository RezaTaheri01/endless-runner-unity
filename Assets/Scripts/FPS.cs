using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private float timer;
    private int frames;
    private float fps;

    private void Update()
    {
        frames++;
        timer += Time.unscaledDeltaTime;

        if (timer >= 0.5f)
        {
            fps = frames / timer;

            frames = 0;
            timer = 0f;
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 150, 30), $"FPS: {fps:F1}");
    }
}