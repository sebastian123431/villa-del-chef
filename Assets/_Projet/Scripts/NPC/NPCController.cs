using UnityEngine;
using VillaDelChef.Interaction;
using VillaDelChef.ScriptableObjects;
using VillaDelChef.UI;

namespace VillaDelChef.NPC
{
    public class NPCController : MonoBehaviour, IInteractable
    {
        [Header("NPC Data")]
        public NPCSO npcData;
        public VendorController vendorController;

        [Header("Visual Components")]
        public SpriteRenderer characterRenderer;
        public SpriteRenderer shadowRenderer;
        public GameObject talkIndicator;

        private VendorBuilding _parentBuilding;
        public VendorBuilding ParentBuilding
        {
            get
            {
                if (_parentBuilding == null)
                    _parentBuilding = GetComponentInParent<VendorBuilding>();
                return _parentBuilding;
            }
        }

        public string InteractionPrompt
        {
            get
            {
                if (ParentBuilding != null && !ParentBuilding.CanInteract)
                {
                    return $"🔒 Bloqueado (Nivel {ParentBuilding.unlockLevelRequirement})";
                }
                return (npcData != null) ? $"Hablar con {npcData.npcName}" : "Hablar con Comerciante";
            }
        }

        public bool CanInteract
        {
            get
            {
                if (ParentBuilding != null)
                {
                    return ParentBuilding.CanInteract;
                }
                return npcData != null;
            }
        }

        private void Awake()
        {
            if (vendorController == null)
            {
                vendorController = GetComponent<VendorController>();
                if (vendorController == null && npcData != null && npcData.vendorData != null)
                {
                    vendorController = gameObject.AddComponent<VendorController>();
                    vendorController.vendorData = npcData.vendorData;
                }
            }
        }

        private void Start()
        {
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            if (npcData == null) return;

            if (npcData.animatorController != null)
            {
                Animator anim = GetComponent<Animator>() ?? gameObject.AddComponent<Animator>();
                anim.runtimeAnimatorController = npcData.animatorController;
                anim.enabled = true;
            }
            else
            {
                Animator anim = GetComponent<Animator>();
                if (anim != null) anim.enabled = false;
                if (characterRenderer != null)
                {
                    characterRenderer.sprite = npcData.worldSprite ?? npcData.portrait;
                }
            }

            if (vendorController != null && npcData.vendorData != null)
            {
                vendorController.vendorData = npcData.vendorData;
            }
        }

        public void Interact()
        {
            if (ParentBuilding != null && !ParentBuilding.CanInteract)
            {
                ParentBuilding.ShowLockedFeedback();
                return;
            }

            OpenVendorInterface();
        }

        public void OpenVendorInterface()
        {
            if (npcData == null) return;

            if (VendorUI.Instance != null)
            {
                VendorUI.Instance.OpenForNPC(this);
            }
            else
            {
                Debug.LogWarning($"[NPCController] VendorUI no encontrado en la escena. Abriendo MarketUI genérico.");
                MarketUI.Instance?.Open();
            }
        }

        private void OnMouseDown()
        {
            if (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                Interact();
            }
        }
    }
}
