using UnityEngine;
using VillaDelChef.Managers;
using VillaDelChef.UI;

namespace VillaDelChef.Economy
{
    public class MerchantStall : MonoBehaviour
    {
        [Header("Visual Elements")]
        public SpriteRenderer stallRenderer;
        public GameObject floatingIndicator;

        [Header("Floating Animation")]
        public float bobSpeed = 2.5f;
        public float bobHeight = 0.15f;
        private Vector3 initialIndicatorLocalPos;

        private void Start()
        {
            if (floatingIndicator != null)
            {
                initialIndicatorLocalPos = floatingIndicator.transform.localPosition;
            }
        }

        private void Update()
        {
            if (floatingIndicator != null)
            {
                float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                floatingIndicator.transform.localPosition = initialIndicatorLocalPos + new Vector3(0f, offset, 0f);
            }
        }

        public void OnInteract()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (MarketUI.Instance != null)
            {
                MarketUI.Instance.Open();
            }
            else
            {
                Debug.LogWarning("[MerchantStall] MarketUI.Instance no fue encontrado en la escena.");
            }
        }

        private void OnMouseDown()
        {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                OnInteract();
            }
        }
    }
}
