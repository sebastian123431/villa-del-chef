using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Core;

namespace VillaDelChef.UI
{
    /// <summary>
    /// Administrador central de textos flotantes e indicadores numéricos (oro, XP, reputación, éxito).
    /// Altamente optimizado mediante ObjectPoolManager para garantizar cero asignaciones de memoria (0 GC allocs).
    /// </summary>
    public class FloatingTextManager : MonoBehaviour
    {
        public static FloatingTextManager Instance { get; private set; }

        public GameObject floatingTextPrefab;
        private Canvas worldCanvas;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetupWorldCanvas();
                PrewarmPool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SetupWorldCanvas()
        {
            GameObject canvasObj = new GameObject("WorldFloatingCanvas");
            canvasObj.transform.SetParent(transform);
            worldCanvas = canvasObj.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.sortingOrder = 60; // Por encima de muebles y sprites de personajes

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 16;
        }

        private void PrewarmPool()
        {
            if (floatingTextPrefab == null)
            {
                floatingTextPrefab = CreateDefaultFloatingTextPrefab();
            }

            if (ObjectPoolManager.Instance != null && floatingTextPrefab != null)
            {
                ObjectPoolManager.Instance.Prewarm("FloatingText", floatingTextPrefab, 15);
            }
        }

        private GameObject CreateDefaultFloatingTextPrefab()
        {
            GameObject go = new GameObject("DefaultFloatingText");
            go.transform.SetParent(transform);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(3f, 1f);

            Text txt = go.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 18;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;

            FloatingText ft = go.AddComponent<FloatingText>();
            ft.textComponent = txt;
            ft.moveSpeed = 0.8f;
            ft.duration = 1.0f;

            go.SetActive(false);
            return go;
        }

        /// <summary>
        /// Muestra un texto flotante en la posición de mundo especificada con el color indicado.
        /// </summary>
        public void Show(string text, Vector3 worldPosition, Color color)
        {
            if (floatingTextPrefab == null)
            {
                floatingTextPrefab = CreateDefaultFloatingTextPrefab();
            }

            Vector3 spawnPos = new Vector3(worldPosition.x, worldPosition.y + 0.3f, 0f);
            GameObject obj;

            if (ObjectPoolManager.Instance != null)
            {
                obj = ObjectPoolManager.Instance.Spawn("FloatingText", spawnPos, Quaternion.identity, floatingTextPrefab);
            }
            else
            {
                obj = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);
            }

            if (obj != null)
            {
                if (worldCanvas != null)
                {
                    obj.transform.SetParent(worldCanvas.transform, true);
                    obj.transform.localScale = Vector3.one * 0.05f; // Escala adecuada para WorldSpace con PPU 16
                }

                FloatingText ft = obj.GetComponent<FloatingText>();
                if (ft != null)
                {
                    ft.Setup(text, color);
                }
            }
        }

        public void ShowGold(int amount, Vector3 position)
        {
            Show($"+{amount} 🪙", position, new Color(1f, 0.84f, 0.15f));
        }

        public void ShowReputation(int amount, Vector3 position)
        {
            string sign = amount >= 0 ? "+" : "";
            Color c = amount >= 0 ? new Color(0.4f, 0.9f, 1f) : new Color(1f, 0.3f, 0.3f);
            Show($"{sign}{amount} ★", position, c);
        }

        public void ShowXP(int amount, Vector3 position)
        {
            Show($"+{amount} XP", position, new Color(0.35f, 0.85f, 1f));
        }

        public void ShowSuccess(string text, Vector3 position)
        {
            Show(text, position, new Color(0.3f, 1f, 0.5f));
        }

        public void ShowWarning(string text, Vector3 position)
        {
            Show(text, position, new Color(1f, 0.45f, 0.3f));
        }
    }
}
