using UnityEngine;
using UnityEngine.EventSystems;
using VillaDelChef.Building;
using VillaDelChef.Cooking;
using VillaDelChef.Farming;

namespace VillaDelChef.PlayerInput
{
    public class TouchInputManager : MonoBehaviour
    {
        public static TouchInputManager Instance { get; private set; }

        [Header("Interaction Settings")]
        public float tapThreshold = 0.25f;
        public float dragThreshold = 10f; // pixels
        public float longPressDuration = 0.6f;

        private Vector2 touchStartPos;
        private float touchStartTime;
        private bool isDragging = false;
        private bool isLongPressTriggered = false;

        private Camera mainCamera;

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
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            try
            {
                // Handle touch on mobile
                if (Input.touchCount > 0)
                {
                    HandleTouches();
                }
                // Handle mouse on desktop / editor
                else
                {
                    HandleMouseInput();
                }
            }
            catch (System.InvalidOperationException)
            {
                // Fallback seguro cuando Unity está alternando el backend de Input
            }
        }

        private void HandleTouches()
        {
            // Pinch to zoom with 2 fingers
            if (Input.touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
                Vector2 prevPos1 = touch1.position - touch1.deltaPosition;

                float prevMagnitude = (prevPos0 - prevPos1).magnitude;
                float currentMagnitude = (touch0.position - touch1.position).magnitude;
                float deltaMagnitude = currentMagnitude - prevMagnitude;

                CameraController2D.Instance?.Zoom(deltaMagnitude * 0.1f);
                return;
            }

            // Single finger gesture
            Touch touch = Input.GetTouch(0);

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    touchStartTime = Time.time;
                    isDragging = false;
                    isLongPressTriggered = false;
                    break;

                case TouchPhase.Moved:
                    float distanceMoved = Vector2.Distance(touch.position, touchStartPos);
                    if (distanceMoved > dragThreshold)
                    {
                        isDragging = true;
                        Vector2 deltaWorld = mainCamera.ScreenToWorldPoint(touch.position) - mainCamera.ScreenToWorldPoint(touch.position - touch.deltaPosition);

                        if (BuildManager.Instance != null && BuildManager.Instance.isBuildMode && BuildManager.Instance.selectedFurniture != null)
                        {
                            Vector3 worldPos = mainCamera.ScreenToWorldPoint(touch.position);
                            worldPos.z = 0f;
                            BuildManager.Instance.UpdateHoverPosition(worldPos);
                        }
                        else
                        {
                            CameraController2D.Instance?.Pan(deltaWorld);
                        }
                    }
                    break;

                case TouchPhase.Stationary:
                    if (!isDragging && !isLongPressTriggered && (Time.time - touchStartTime >= longPressDuration))
                    {
                        isLongPressTriggered = true;
                        HandleLongPress(touch.position);
                    }
                    break;

                case TouchPhase.Ended:
                    if (!isDragging && !isLongPressTriggered && (Time.time - touchStartTime <= tapThreshold))
                    {
                        HandleTap(touch.position);
                    }
                    break;
            }
        }

        private void HandleMouseInput()
        {
            // Zoom with mouse scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                CameraController2D.Instance?.Zoom(scroll * 40f);
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // Update build hover
            if (BuildManager.Instance != null && BuildManager.Instance.isBuildMode && BuildManager.Instance.selectedFurniture != null)
            {
                Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = 0f;
                BuildManager.Instance.UpdateHoverPosition(mouseWorld);

                if (Input.GetMouseButtonDown(0))
                {
                    BuildManager.Instance.TryPlaceObject();
                    return;
                }
                if (Input.GetMouseButtonDown(1))
                {
                    BuildManager.Instance.RotateSelection();
                    return;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                touchStartPos = Input.mousePosition;
                touchStartTime = Time.time;
                isDragging = false;
                isLongPressTriggered = false;
            }
            else if (Input.GetMouseButton(0))
            {
                float dist = Vector2.Distance(Input.mousePosition, touchStartPos);
                if (dist > dragThreshold)
                {
                    isDragging = true;
                    Vector3 delta = mainCamera.ScreenToViewportPoint((Vector2)Input.mousePosition - touchStartPos);
                    Vector3 move = new Vector3(delta.x * 12f, delta.y * 12f, 0f);
                    CameraController2D.Instance?.Pan(move);
                    touchStartPos = Input.mousePosition;
                }
                else if (!isDragging && !isLongPressTriggered && (Time.time - touchStartTime >= longPressDuration))
                {
                    isLongPressTriggered = true;
                    HandleLongPress(Input.mousePosition);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (!isDragging && !isLongPressTriggered && (Time.time - touchStartTime <= tapThreshold))
                {
                    HandleTap(Input.mousePosition);
                }
            }
        }

        private void HandleTap(Vector2 screenPos)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

            RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero);
            if (hit.collider != null)
            {
                // Merchant Stall tapped
                VillaDelChef.Economy.MerchantStall stall = hit.collider.GetComponentInParent<VillaDelChef.Economy.MerchantStall>();
                if (stall != null)
                {
                    stall.OnInteract();
                    return;
                }

                // Station tapped
                CookingStation station = hit.collider.GetComponentInParent<CookingStation>();
                if (station != null)
                {
                    station.OnInteract();
                    return;
                }

                // Crop plot tapped
                CropPlot plot = hit.collider.GetComponentInParent<CropPlot>();
                if (plot != null)
                {
                    plot.OnInteract();
                    return;
                }

                // Furniture tapped in build mode
                if (BuildManager.Instance != null && BuildManager.Instance.isBuildMode)
                {
                    GridObject gridObj = hit.collider.GetComponentInParent<GridObject>();
                    if (gridObj != null)
                    {
                        BuildManager.Instance.StartMovingObject(gridObj);
                        return;
                    }
                }
            }
        }

        private void HandleLongPress(Vector2 screenPos)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

            RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero);
            if (hit.collider != null)
            {
                GridObject gridObj = hit.collider.GetComponentInParent<GridObject>();
                if (gridObj != null)
                {
                    if (BuildManager.Instance != null)
                    {
                        BuildManager.Instance.StartMovingObject(gridObj);
                    }
                }
            }
        }
    }
}
