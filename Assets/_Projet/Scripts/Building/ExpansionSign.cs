using UnityEngine;
using VillaDelChef.Interaction;
using VillaDelChef.Managers;
using VillaDelChef.UI;

namespace VillaDelChef.Building
{
    [RequireComponent(typeof(Collider2D))]
    public class ExpansionSign : MonoBehaviour, IInteractable
    {
        [Header("Expansion Link")]
        public ExpansionSO expansionData;

        [Header("Visual Feedback")]
        public SpriteRenderer signRenderer;
        public SpriteRenderer floatingIcon;
        public ParticleSystem purchaseParticles;

        private Vector3 initialIconPos;
        private bool isPurchased = false;

        private void Start()
        {
            if (floatingIcon != null)
            {
                initialIconPos = floatingIcon.transform.localPosition;
            }

            if (ExpansionManager.Instance != null)
            {
                ExpansionManager.Instance.RegisterSign(this);
            }
        }

        private void Update()
        {
            if (isPurchased) return;

            // Gentle bobbing animation for the floating notice/lock icon
            if (floatingIcon != null)
            {
                float offset = Mathf.Sin(Time.time * 2.5f) * 0.08f;
                floatingIcon.transform.localPosition = initialIconPos + new Vector3(0f, offset, 0f);
            }
        }

        #region IInteractable Implementation

        public bool CanInteract
        {
            get
            {
                if (isPurchased) return false;
                if (ExpansionManager.Instance != null && expansionData != null)
                {
                    return !ExpansionManager.Instance.IsExpansionUnlocked(expansionData.expansionID);
                }
                return true;
            }
        }

        public string InteractionPrompt => (expansionData != null) ? $"Expandir: {expansionData.displayName}" : "Desbloquear Terreno";

        public void Interact()
        {
            if (!CanInteract) return;

            ExpansionUI ui = Object.FindAnyObjectByType<ExpansionUI>(FindObjectsInactive.Include);
            if (ui != null)
            {
                ui.Open(expansionData);
            }
            else
            {
                Debug.LogWarning("[ExpansionSign] No se encontró ExpansionUI en la escena.");
            }
        }

        #endregion

        public void OnExpansionPurchased()
        {
            isPurchased = true;

            if (purchaseParticles != null)
            {
                purchaseParticles.Play();
            }

            // Animate scale shrink and disable
            StartCoroutine(DisappearRoutine());
        }

        private System.Collections.IEnumerator DisappearRoutine()
        {
            float elapsed = 0f;
            float duration = 0.5f;
            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
