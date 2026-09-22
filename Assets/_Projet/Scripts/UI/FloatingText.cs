using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Core;

namespace VillaDelChef.UI
{
    public class FloatingText : MonoBehaviour, IPoolable
    {
        public Text textComponent;
        public float moveSpeed = 1.2f;
        public float duration = 1.2f;

        private float timer = 0f;
        private Color initialColor = Color.white;
        private bool isFromPool = false;

        public void Setup(string text, Color color)
        {
            if (textComponent == null) textComponent = GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.text = text;
                textComponent.color = color;
                initialColor = color;
            }
            timer = 0f;
        }

        public void OnSpawnFromPool()
        {
            isFromPool = true;
            timer = 0f;
            if (textComponent != null)
            {
                textComponent.color = initialColor;
            }
        }

        public void OnReturnToPool()
        {
            timer = 0f;
            if (textComponent != null)
            {
                textComponent.text = "";
            }
        }

        private void Update()
        {
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
            timer += Time.deltaTime;

            if (textComponent != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, timer / duration);
                textComponent.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            }

            if (timer >= duration)
            {
                if (isFromPool && ObjectPoolManager.Instance != null)
                {
                    ObjectPoolManager.Instance.Despawn("FloatingText", gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}

