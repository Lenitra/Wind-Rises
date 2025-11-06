using UnityEngine;
using TMPro;

public class DebugScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;

    private float frameCount = 0f;
    private float deltaTime = 0f;
    private float fps = 0f;
    private const float updateInterval = 0.5f;
    private float timeSinceLastUpdate = 0f;

    private void Update()
    {
        frameCount++;
        deltaTime += Time.deltaTime;
        timeSinceLastUpdate += Time.deltaTime;

        if (timeSinceLastUpdate >= updateInterval)
        {
            fps = frameCount / deltaTime;
            frameCount = 0f;
            deltaTime = 0f;
            timeSinceLastUpdate = 0f;

            if (fpsText != null)
            {
                fpsText.text = $"FPS: {fps:F1}";
            }
        }
    }
}
