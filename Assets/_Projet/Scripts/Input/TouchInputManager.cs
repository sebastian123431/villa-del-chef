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
        [Header("Pinch Zoom Settings")]
        public float pinchZoomSensitivity = 0.05f;

        private Vector2 touchStartPos;
        private float touchStartTime;
        private bool isDragging = false;
        private bool isLongPressTriggered = false;
        private bool wasPinching = false;
        private float prevNewInputPinchDistance = -1f;

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

            if (touch != null && touch.touches.Count > 0)
            {
                // Pinch to zoom with 2 fingers
                if (touch.touches.Count >= 2 && touch.touches[0].press.isPressed && touch.touches[1].press.isPressed)
                {
                    wasPinching = true;
                    Vector2 p0 = touch.touches[0].position.ReadValue();
                    Vector2 p1 = touch.touches[1].position.ReadValue();
                    float currentDist = Vector2.Distance(p0, p1);

                    if (prevNewInputPinchDistance > 0f)
                    {
                        float deltaDist = currentDist - prevNewInputPinchDistance;
                        CameraController2D.Instance?.Zoom(deltaDist * pinchZoomSensitivity);
                    }
                    prevNewInputPinchDistance = currentDist;
                    isDragging = true;
                    return;
                }
                else
                {
                    prevNewInputPinchDistance = -1f;
                }

                var t0 = touch.touches[0];
                Vector2 pos = t0.position.ReadValue();

                if (t0.press.wasPressedThisFrame)
                {
                    if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                    {
                        touchStartPos = pos;
                        touchStartTime = Time.time;
                        isDragging = false;
                        isLongPressTriggered = false;
                    }
                }
                else if (t0.press.isPressed)
                {
                    if (!isDragging)
                    {
                        float dist = Vector2.Distance(pos, touchStartPos);
                        if (dist > dragThreshold)
                        {
                            isDragging = true;
                        }
                        else if (!isLongPressTriggered && (Time.time - touchStartTime >= longPressDuration))
                        {
                            isLongPressTriggered = true;
                            HandleLongPress(pos);
                        }
                    }

                    if (isDragging)
                    {
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
                    if (BuildManager.Instance != null && BuildManager.Instance.isBuildMode && BuildManager.Instance.selectedFurniture != null)
                    {
                        Vector3 worldPos = mainCamera.ScreenToWorldPoint(pos);
                        worldPos.z = 0f;
                        BuildManager.Instance.UpdateHoverPosition(worldPos);
                        BuildManager.Instance.TryPlaceObject();
                    }
                    else if (!isDragging && !isLongPressTriggered && !wasPinching && (Time.time - touchStartTime <= tapThreshold))
                    {
                        if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                        {
                            HandleTap(pos);
                        }
                    }
                    isDragging = false;
                    isLongPressTriggered = false;
                    wasPinching = false;
                    prevNewInputPinchDistance = -1f;
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
            if (Input.touchCount >= 2)
            {
                wasPinching = true;
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

            if (Input.touchCount == 0)
            {
                wasPinching = false;
                return;
            }

            // Single finger gesture
            Touch touch = Input.GetTouch(0);

            if (wasPinching)
            {
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    wasPinching = false;
                }
                return;
            }

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
                    if (BuildManager.Instance != null && BuildManager.Instance.isBuildMode && BuildManager.Instance.selectedFurniture != null)
                    {
                        Vector3 worldPos = mainCamera.ScreenToWorldPoint(touch.position);
                        worldPos.z = 0f;
                        BuildManager.Instance.UpdateHoverPosition(worldPos);
                        BuildManager.Instance.TryPlaceObject();
                    }
                    else if (!isDragging && !isLongPressTriggered && !wasPinching && (Time.time - touchStartTime <= tapThreshold))
                    {
                        HandleTap(touch.position);
                    }
                    isDragging = false;
                    isLongPressTriggered = false;
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

            // 1. If in build mode
            if (BuildManager.Instance != null && BuildManager.Instance.isBuildMode)
            {
                // If a furniture item is currently selected, tap confirms placement
                if (BuildManager.Instance.selectedFurniture != null)
                {
                    BuildManager.Instance.UpdateHoverPosition(worldPos);
                    BuildManager.Instance.TryPlaceObject();
                    return;
                }

                // If nothing is selected, tap on movable furniture selects it to move
                RaycastHit2D buildHit = Physics2D.Raycast(worldPos2D, Vector2.zero);
                if (buildHit.collider != null)
                {
                    GridObject gridObj = buildHit.collider.GetComponentInParent<GridObject>();
                    if (gridObj != null && gridObj.playerMovable && gridObj.furnitureData != null)
                    {
                        BuildManager.Instance.StartMovingObject(gridObj);
                        return;
                    }
                }
                return;
            }

            RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero);
            if (hit.collider != null)
            {
                // 2. Polymorphic IInteractable detection (Station, CropPlot, MerchantStall, DeliveryCounter, etc.)
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    if (interactable.CanInteract)
                    {
                        interactable.Interact();
                    }
                    else
                    {
                        var building = hit.collider.GetComponentInParent<VillaDelChef.NPC.VendorBuilding>();
                        if (building != null)
                        {
                            building.ShowLockedFeedback();
                        }
                    }
                    return;
                }
            }
        }

        public void RotateBuildSelection()
        {
            BuildManager.Instance?.RotateSelection();
        }

        private void HandleLongPress(Vector2 screenPos)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

            RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero);
            if (hit.collider != null)
            {
                GridObject gridObj = hit.collider.GetComponentInParent<GridObject>();
                if (gridObj != null && gridObj.playerMovable && gridObj.furnitureData != null)
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
