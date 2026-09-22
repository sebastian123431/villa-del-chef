using UnityEngine;
using VillaDelChef.Interaction;
using VillaDelChef.Managers;
using VillaDelChef.UI;

namespace VillaDelChef.Economy
{
    public class MerchantStall : MonoBehaviour, IInteractable
    {
        public string InteractionPrompt => "Abrir Mercado";
        public bool CanInteract => true;
        public void Interact() => OnInteract();

        [Header("Visual Elements")]
        public SpriteRenderer stallRenderer;
        public GameObject floatingIndicator;

        [Header("Associated NPC")]
        public NPC.NPCController associatedNPC;

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

            if (associatedNPC != null)
            {
                associatedNPC.Interact();
                return;
            }

            if (VendorUI.Instance != null)
            {
                var anyNPC = Object.FindAnyObjectByType<NPC.NPCController>();
                if (anyNPC != null)
                {
                    VendorUI.Instance.OpenForNPC(anyNPC);
                    return;
                }
            }

            if (MarketUI.Instance != null)
            {
                MarketUI.Instance.Open();
            }
            else
            {
                Debug.LogWarning("[MerchantStall] Ni VendorUI ni MarketUI fueron encontrados en la escena.");
            }
        }

        private void OnMouseDown()
        {
            if (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                OnInteract();
            }
        }
    }
}
