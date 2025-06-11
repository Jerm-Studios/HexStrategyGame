using UnityEngine;
using System.Collections;

public class ProjectileLineEffect : MonoBehaviour
{
    [Header("Line Settings")]
    public float duration = 0.5f;
    public float startWidth = 0.1f;
    public float endWidth = 0.05f;
    public Color startColor = Color.yellow;
    public Color endColor = Color.red;

    private LineRenderer lineRenderer;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float startTime;

    public void Initialize(Vector3 start, Vector3 target)
    {
        // Store positions
        startPosition = start;
        targetPosition = target;

        // Get or add line renderer
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Configure line renderer
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;

        // Set material (optional - use default if you don't have a specific one)
        // lineRenderer.material = yourLineMaterial;

        // Set initial positions
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, startPosition); // Start with both points at the same position

        // Start animation
        startTime = Time.time;
        StartCoroutine(AnimateLine());
    }

    private IEnumerator AnimateLine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Calculate progress (0 to 1)
            float t = elapsedTime / duration;

            // Update the end position of the line
            Vector3 currentEndPosition = Vector3.Lerp(startPosition, targetPosition, t);
            lineRenderer.SetPosition(1, currentEndPosition);

            // Update elapsed time
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the line reaches the exact target position
        lineRenderer.SetPosition(1, targetPosition);

        // Optional: Add impact effect at the end
        CreateImpactEffect();

        // Destroy after a short delay
        Destroy(gameObject, 0.2f);
    }

    private void CreateImpactEffect()
    {
        // You can instantiate a particle effect here when the projectile hits
        // For example:
        // if (impactEffectPrefab != null)
        // {
        //     Instantiate(impactEffectPrefab, targetPosition, Quaternion.identity);
        // }

        // For now, just log that the impact occurred
        Debug.Log("Projectile impact at " + targetPosition);
    }
}
