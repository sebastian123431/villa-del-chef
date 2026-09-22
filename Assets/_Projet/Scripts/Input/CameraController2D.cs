using UnityEngine;

namespace VillaDelChef.PlayerInput
{
    public class CameraController2D : MonoBehaviour
    {
        public static CameraController2D Instance { get; private set; }

        [Header("Target Camera")]
        public Camera targetCamera;

        [Header("Zoom Bounds")]
        public float minZoom = 4f;
        public float maxZoom = 12f;
        public float defaultZoom = 7f;
        public float zoomSpeed = 0.05f;

        [Header("Pan Settings")]
        public float panSpeed = 1f;
        public float panDamping = 10f;

        [Header("World Limits")]
        public Vector2 minBounds = new Vector2(-22f, -18f);
        public Vector2 maxBounds = new Vector2(22f, 18f);

        private Vector3 targetPosition;
        private float targetZoom;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
                if (targetCamera == null) targetCamera = Camera.main;
            }

            targetPosition = transform.position;
            targetZoom = targetCamera != null ? targetCamera.orthographicSize : defaultZoom;
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;

            // Smooth pan
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * panDamping);

            // Smooth zoom
            targetCamera.orthographicSize = Mathf.Lerp(targetCamera.orthographicSize, targetZoom, Time.deltaTime * panDamping);
        }

        public void Pan(Vector2 deltaWorld)
        {
            targetPosition -= new Vector3(deltaWorld.x * panSpeed, deltaWorld.y * panSpeed, 0f);
            ClampPosition();
        }

        public void Zoom(float deltaZoom)
        {
            targetZoom = Mathf.Clamp(targetZoom - deltaZoom * zoomSpeed, minZoom, maxZoom);
        }

        public void SetZoom(float zoom)
        {
            targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        }

        public void FocusOn(Vector3 worldPos)
        {
            targetPosition = new Vector3(worldPos.x, worldPos.y, transform.position.z);
            ClampPosition();
        }

        private void ClampPosition()
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }
    }
}
