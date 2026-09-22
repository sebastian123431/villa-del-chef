using UnityEngine;

namespace VillaDelChef.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea = new Rect(0, 0, 0, 0);
        private Vector2Int lastScreenSize = new Vector2Int(0, 0);
        private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            if (lastSafeArea != Screen.safeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height || lastOrientation != Screen.orientation)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            if (rectTransform == null) return;

            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            lastOrientation = Screen.orientation;

            // Convert safe area coordinates to normalized anchors
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
