using UnityEngine;
using UnityEngine.EventSystems;
using VillaDelChef.Building;
using VillaDelChef.Cooking;
using VillaDelChef.Farming;
using VillaDelChef.Interaction;

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
        private bool isLegacyInputAvailable = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            mainCamera = Camera.main;

            try
            {
                int _ = Input.touchCount;
                isLegacyInputAvailable = true;
            }
            catch (System.InvalidOperationException)
            {
                isLegacyInputAvailable = false;
            }
        }

        private void Update()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

#if ENABLE_INPUT_SYSTEM
            if (!isLegacyInputAvailable)
            {
                HandleNewInputSystem();
                return;
            }
#endif

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

#if ENABLE_INPUT_SYSTEM
        private void HandleNewInputSystem()
        {
            var touch = UnityEngine.InputSystem.Touchscreen.current;
            var mouse = UnityEngine.InputSystem.Mouse.current;

            if (touch != null && touch.touches.Count > 0 && touch.touches[0].press.isPressed)
            {
                var t0 = touch.touches[0];
                Vector2 pos = t0.position.ReadValue();

                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                if (t0.press.wasPressedThisFrame)
                {
                    touchStartPos = pos;
                    touchStartTime = Time.time;
                    isDragging = false;
                    isLongPressTriggered = false;
                }
                else if (t0.press.isPressed)
                {
                    float dist = Vector2.Distance(pos, touchStartPos);
                    if (dist > dragThreshold)
                    {
                        isDragging = true;
                        if (BuildManager.Instance != null && BuildManager.Instance.isBuildMode && BuildManager.Instance.selectedFurniture != null)
                        {
                            Vector3 worldPos = mainCamera.ScreenToWorldPoint(pos);
                            worldPos.z = 0f;
                            BuildManager.Instance.UpdateHoverPosition(worldPos);
                        }
                        else
                        {
                            Vector3 delta = mainCamera.ScreenToViewportPoint(pos - touchStartPos);
                            Vector3 move = new Vector3(delta.x * 12f, delta.y * 12f, 0f);
                            CameraController2D.Instance?.Pan(move);
                            touchStartPos = pos;
                        }
                    }
                }
                else if (t0.press.wasReleasedThisFrame)
                {
                    if (!isDragging && (Time.time - touchStartTime <= tapThreshold))
                    {
                        HandleTap(pos);
                    }
                }
            }
            else if (mouse != null)
            {
                Vector2 scroll = mouse.scroll.ReadValue();
                if (Mathf.Abs(scroll.y) > 0.01f)
                {
                    CameraController2D.Instance?.Zoom(scroll.y * 0.05f);
                }

                Vector2 mousePos = mouse.position.ReadValue();

                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                if (BuildManager.Instance != null && BuildManager.Instance.isBuildMode && BuildManager.Instance.selectedFurniture != null)
                {
                    Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mousePos);
                    mouseWorld.z = 0f;
                    BuildManager.Instance.UpdateHoverPosition(mouseWorld);

                    if (mouse.leftButton.wasPressedThisFrame)
                    {
                        BuildManager.Instance.TryPlaceObject();
                        return;
                    }
                    if (mouse.rightButton.wasPressedThisFrame)
                    {
                        BuildManager.Instance.RotateSelection();
                        return;
                    }
                }

                if (mouse.leftButton.wasPressedThisFrame)
                {
                    touchStartPos = mousePos;
                    touchStartTime = Time.time;
                    isDragging = false;
                    isLongPressTriggered = false;
                }
                else if (mouse.leftButton.isPressed)
                {
                    float dist = Vector2.Distance(mousePos, touchStartPos);
                    if (dist > dragThreshold)
                    {
                        isDragging = true;
                        Vector3 delta = mainCamera.ScreenToViewportPoint(mousePos - touchStartPos);
                        Vector3 move = new Vector3(delta.x * 12f, delta.y * 12f, 0f);
                        CameraController2D.Instance?.Pan(move);
                        touchStartPos = mousePos;
                    }
                }
                else if (mouse.leftButton.wasReleasedThisFrame)
                {
                    if (!isDragging && (Time.time - touchStartTime <= tapThreshold))
                    {
                        HandleTap(mousePos);
                    }
                }
            }
        }
#endif

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
                // 1. If in build mode, tap selects furniture to move
                if (BuildManager.Instance != null && BuildManager.Instance.isBuildMode)
                {
                    GridObject gridObj = hit.collider.GetComponentInParent<GridObject>();
                    if (gridObj != null)
                    {
                        BuildManager.Instance.StartMovingObject(gridObj);
                        return;
                    }
                }

                // 2. Polymorphic IInteractable detection (Station, CropPlot, MerchantStall, DeliveryCounter, etc.)
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null && interactable.CanInteract)
                {
                    interactable.Interact();
                    return;
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
