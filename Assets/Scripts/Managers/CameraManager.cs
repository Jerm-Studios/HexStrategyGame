using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [Header("Movement Settings")]
    public float panSpeed = 20f;
    public float panBorderThickness = 10f;
    public bool useScreenEdgePanning = true;
    public Vector2 panLimit = new Vector2(20f, 20f);

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 15f;
    public float zoomSmoothness = 10f;

    [Header("Rotation Settings")]
    public bool allowRotation = true;
    public float rotationSpeed = 100f;
    public float rotationSmoothness = 10f;

    [Header("Visual Effects")]
    public bool useCameraShake = true;
    public float shakeIntensity = 0.2f;
    public float shakeDuration = 0.3f;

    // Private variables
    private Camera cam;
    private Vector2 mousePosition;
    private float scrollValue;
    private float targetZoom;
    private float targetRotationY;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 shakeOffset;
    private float currentShakeDuration;

    private void Start()
    {
        cam = GetComponent<Camera>();
        targetZoom = cam.orthographic ? cam.orthographicSize : transform.position.y;
        targetRotationY = transform.eulerAngles.y;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void Update()
    {
        // Get current mouse position
        mousePosition = Mouse.current.position.ReadValue();

        // Get scroll value
        scrollValue = Mouse.current.scroll.y.ReadValue();

        // Handle camera movement
        HandleCameraMovement();

        // Handle camera zoom
        HandleCameraZoom();

        // Handle camera rotation
        if (allowRotation)
        {
            HandleCameraRotation();
        }

        // Handle camera shake
        if (useCameraShake && currentShakeDuration > 0)
        {
            HandleCameraShake();
        }
    }

    private void HandleCameraMovement()
    {
        Vector3 pos = transform.position;

        // Keyboard controls
        if (Keyboard.current.wKey.isPressed || (useScreenEdgePanning && mousePosition.y >= Screen.height - panBorderThickness))
        {
            pos += transform.forward * panSpeed * Time.deltaTime;
        }
        if (Keyboard.current.sKey.isPressed || (useScreenEdgePanning && mousePosition.y <= panBorderThickness))
        {
            pos -= transform.forward * panSpeed * Time.deltaTime;
        }
        if (Keyboard.current.dKey.isPressed || (useScreenEdgePanning && mousePosition.x >= Screen.width - panBorderThickness))
        {
            pos += transform.right * panSpeed * Time.deltaTime;
        }
        if (Keyboard.current.aKey.isPressed || (useScreenEdgePanning && mousePosition.x <= panBorderThickness))
        {
            pos -= transform.right * panSpeed * Time.deltaTime;
        }

        // Middle mouse button panning
        if (Mouse.current.middleButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            pos -= transform.right * mouseDelta.x * 0.05f;
            pos -= transform.forward * mouseDelta.y * 0.05f;
        }

        if (Keyboard.current.spaceKey.isPressed)
        {
            pos = new Vector2(0, 0);
        }

        // Clamp position
        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);

        // Apply position
        transform.position = pos + shakeOffset;
    }

    private void HandleCameraZoom()
    {
        if (scrollValue != 0)
        {
            // Update target zoom
            if (cam.orthographic)
            {
                targetZoom = Mathf.Clamp(targetZoom - scrollValue * zoomSpeed * 0.01f, minZoom, maxZoom);
            }
            else
            {
                targetZoom = Mathf.Clamp(targetZoom - scrollValue * zoomSpeed * 0.1f, minZoom, maxZoom);
            }
        }

        // Smooth zoom
        if (cam.orthographic)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSmoothness);
        }
        else
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(pos.y, targetZoom, Time.deltaTime * zoomSmoothness);
            transform.position = pos + shakeOffset;
        }
    }

    private void HandleCameraRotation()
    {
        // Rotate with Q and E keys
        if (Keyboard.current.qKey.isPressed)
        {
            targetRotationY -= rotationSpeed * Time.deltaTime;
        }
        if (Keyboard.current.eKey.isPressed)
        {
            targetRotationY += rotationSpeed * Time.deltaTime;
        }

        // Smooth rotation
        float currentRotationY = transform.eulerAngles.y;
        float newRotationY = Mathf.LerpAngle(currentRotationY, targetRotationY, Time.deltaTime * rotationSmoothness);
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, newRotationY, 0);
    }

    private void HandleCameraShake()
    {
        // Decrease shake duration
        currentShakeDuration -= Time.deltaTime;

        // Calculate shake offset
        if (currentShakeDuration > 0)
        {
            float intensity = shakeIntensity * (currentShakeDuration / shakeDuration);
            shakeOffset = new Vector3(
                Random.Range(-intensity, intensity),
                0,
                Random.Range(-intensity, intensity)
            );
        }
        else
        {
            shakeOffset = Vector3.zero;
            currentShakeDuration = 0;
        }
    }

    public void ShakeCamera(float intensity = 1.0f)
    {
        if (useCameraShake)
        {
            currentShakeDuration = shakeDuration;
            shakeIntensity *= intensity;
        }
    }

    public void FocusOnPosition(Vector3 position)
    {
        Vector3 targetPos = new Vector3(position.x, transform.position.y, position.z);
        transform.position = targetPos;
    }

    public void ResetCamera()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        targetZoom = cam.orthographic ? cam.orthographicSize : transform.position.y;
        targetRotationY = transform.eulerAngles.y;
    }
}
