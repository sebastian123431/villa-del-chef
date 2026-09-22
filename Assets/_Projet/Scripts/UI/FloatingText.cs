using UnityEngine;
using UnityEngine.UI;

namespace VillaDelChef.UI
{
    public class FloatingText : MonoBehaviour
    {
        public Text textComponent;
        public float moveSpeed = 1.2f;
        public float duration = 1.5f;

        private float timer = 0f;
        private Color initialColor;

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
                Destroy(gameObject);
            }
        }
    }
}
