using UnityEngine;
using VillaDelChef.Building;
using VillaDelChef.Core;
using VillaDelChef.Economy;
using VillaDelChef.Interaction;
using VillaDelChef.ScriptableObjects;
using VillaDelChef.UI;

namespace VillaDelChef.NPC
{
    /// <summary>
    /// Componente modular y reutilizable para representar los puestos y locales físicos
    /// de los comerciantes especializados de la villa en el mundo 2D.
    /// </summary>
    public class VendorBuilding : MonoBehaviour, IInteractable
    {
        [Header("Shop Identity")]
        public string shopID = "shop_default";
        public string shopName = "Puesto Comercial";
        public NPCSO npcData;
        public int unlockLevelRequirement = 1;

        [Header("Grid & Footprint")]
        public Vector2Int gridPosition;
        public int sizeX = 3;
        public int sizeY = 2;

        [Header("Components & Visuals")]
        public SpriteRenderer stallRenderer;
        public NPCController associatedNPC;
        public GameObject floatingSign;

        public string InteractionPrompt => $"🏪 {shopName}\n[Tocar para Comprar]";
        public bool CanInteract => true;

        public void Interact()
        {
            if (associatedNPC != null && associatedNPC.npcData != null)
            {
                associatedNPC.Interact();
            }
            else if (VendorUI.Instance != null && npcData != null)
            {
                if (associatedNPC == null)
                {
                    associatedNPC = gameObject.GetComponent<NPCController>() ?? gameObject.AddComponent<NPCController>();
                    associatedNPC.npcData = npcData;
                }
                VendorUI.Instance.OpenForNPC(associatedNPC);
            }
            else if (MarketUI.Instance != null)
            {
                MarketUI.Instance.Open();
            }
        }

        public void Setup(string id, string name, NPCSO npc, Vector2Int gridPos, Sprite stallSprite, int minLevel = 1)
        {
            shopID = id;
            shopName = name;
            npcData = npc;
            gridPosition = gridPos;
            unlockLevelRequirement = minLevel;

            if (stallRenderer == null) stallRenderer = GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
            if (stallSprite != null) stallRenderer.sprite = stallSprite;
            stallRenderer.sortingOrder = 5;

            // Box collider for 2D interaction
            BoxCollider2D col = GetComponent<BoxCollider2D>() ?? gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(sizeX, sizeY + 0.5f);
            col.offset = new Vector2(0f, 0.25f);

            // Register occupancy in GridManager
            if (GridManager.Instance != null)
            {
                GridManager.Instance.SetOccupancy(gridPosition.x, gridPosition.y, sizeX, sizeY, null, true);
            }

            // Spawn or configure NPC character attached to building
            if (npcData != null && associatedNPC == null)
            {
                GameObject npcGO = new GameObject($"NPC_{npcData.npcName}");
                npcGO.transform.SetParent(transform, false);
                npcGO.transform.localPosition = new Vector3(-0.6f, -0.5f, 0f);

                SpriteRenderer npcSR = npcGO.AddComponent<SpriteRenderer>();
                npcSR.sortingOrder = 7;
                npcSR.sprite = npcData.worldSprite ?? npcData.portrait;

                BoxCollider2D npcCol = npcGO.AddComponent<BoxCollider2D>();
                npcCol.size = new Vector2(1f, 1.4f);

                associatedNPC = npcGO.AddComponent<NPCController>();
                associatedNPC.npcData = npcData;
                associatedNPC.characterRenderer = npcSR;
            }

            // Setup floating 3D text / sign
            CreateFloatingSign();
        }

        private void CreateFloatingSign()
        {
            if (floatingSign != null) return;

            floatingSign = new GameObject("Sign_Indicator");
            floatingSign.transform.SetParent(transform, false);
            floatingSign.transform.localPosition = new Vector3(0f, 1.8f, 0f);

            TextMesh tm = floatingSign.AddComponent<TextMesh>();
            tm.text = $"🛒 {shopName}\n[Tocar]";
            tm.characterSize = 0.14f;
            tm.fontSize = 26;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = new Color(1f, 0.92f, 0.35f);

            MeshRenderer mr = floatingSign.GetComponent<MeshRenderer>();
            mr.sortingOrder = 25;
        }

        private void OnMouseDown()
        {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                Interact();
            }
        }
    }
}
