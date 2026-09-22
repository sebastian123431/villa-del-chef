using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Core;
using VillaDelChef.Managers;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.UI
{
    public class QuestUI : MonoBehaviour
    {
        private static QuestUI _instance;
        public static QuestUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<QuestUI>(FindObjectsInactive.Include);
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("UI Root")]
        public GameObject panelRoot;
        public Transform questListContainer;
        public Button closeButton;
        public Text emptyMessageText;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);

            GameEvents.OnQuestProgressMade += (type, target, amount) =>
            {
                if (IsOpen())
                {
                    RefreshQuests();
                }
            };
            GameEvents.OnQuestCompleted += (quest) =>
            {
                if (IsOpen())
                {
                    RefreshQuests();
                }
            };
        }

        public bool IsOpen()
        {
            return (panelRoot != null && panelRoot.activeSelf) || gameObject.activeSelf;
        }

        public void Open()
        {
            gameObject.SetActive(true);
            if (panelRoot != null) panelRoot.SetActive(true);
            RefreshQuests();
        }

        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            else gameObject.SetActive(false);
        }

        public void Toggle()
        {
            if (IsOpen()) Close();
            else Open();
        }

        public void RefreshQuests()
        {
            if (questListContainer == null || QuestManager.Instance == null) return;

            foreach (Transform child in questListContainer)
            {
                Destroy(child.gameObject);
            }

            int populated = 0;
            foreach (var aq in QuestManager.Instance.activeQuests)
            {
                if (aq == null || aq.questData == null) continue;
                CreateQuestCard(aq);
                populated++;
            }

            if (emptyMessageText != null)
            {
                emptyMessageText.gameObject.SetActive(populated == 0);
            }
        }

        private void CreateQuestCard(ActiveQuest aq)
        {
            GameObject cardGO = new GameObject($"Quest_{aq.questData.title}");
            cardGO.transform.SetParent(questListContainer, false);

            RectTransform rt = cardGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(620, 110);

            Image bg = cardGO.AddComponent<Image>();
            bg.color = aq.isCompleted ? new Color(0.15f, 0.28f, 0.20f, 0.95f) : new Color(0.18f, 0.20f, 0.26f, 0.95f);

            // Icon
            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(cardGO.transform, false);
            RectTransform iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(60, 60);
            iconRT.anchoredPosition = new Vector2(-260f, 0f);
            Image iconImg = iconGO.AddComponent<Image>();
            if (aq.questData.icon != null) iconImg.sprite = aq.questData.icon;

            // Title
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(cardGO.transform, false);
            RectTransform titleRT = titleGO.AddComponent<RectTransform>();
            titleRT.sizeDelta = new Vector2(360, 30);
            titleRT.anchoredPosition = new Vector2(-40f, 25f);
            Text titleText = titleGO.AddComponent<Text>();
            titleText.text = aq.questData.title;
            titleText.fontSize = 20;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.color = aq.isCompleted ? new Color(0.4f, 1f, 0.4f) : Color.white;

            // Description & Reward
            GameObject descGO = new GameObject("Description");
            descGO.transform.SetParent(cardGO.transform, false);
            RectTransform descRT = descGO.AddComponent<RectTransform>();
            descRT.sizeDelta = new Vector2(360, 45);
            descRT.anchoredPosition = new Vector2(-40f, -12f);
            Text descText = descGO.AddComponent<Text>();
            descText.text = $"{aq.questData.description}\n<color=#FFD700>Recompensa: +${aq.questData.rewardCoins}  +{aq.questData.rewardXP} XP</color>";
            descText.fontSize = 13;
            descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            descText.alignment = TextAnchor.MiddleLeft;
            descText.color = new Color(0.85f, 0.85f, 0.9f);
            descText.supportRichText = true;

            // Progress / Status Badge
            GameObject statusGO = new GameObject("StatusBadge");
            statusGO.transform.SetParent(cardGO.transform, false);
            RectTransform statusRT = statusGO.AddComponent<RectTransform>();
            statusRT.sizeDelta = new Vector2(120, 35);
            statusRT.anchoredPosition = new Vector2(230f, 0f);

            Image statusBg = statusGO.AddComponent<Image>();
            statusBg.color = aq.isCompleted ? new Color(0.2f, 0.7f, 0.3f) : new Color(0.25f, 0.45f, 0.75f);

            GameObject statusTextGO = new GameObject("Text");
            statusTextGO.transform.SetParent(statusGO.transform, false);
            RectTransform stRT = statusTextGO.AddComponent<RectTransform>();
            stRT.anchorMin = Vector2.zero;
            stRT.anchorMax = Vector2.one;
            stRT.offsetMin = Vector2.zero;
            stRT.offsetMax = Vector2.zero;
            Text stText = statusTextGO.AddComponent<Text>();
            stText.text = aq.isCompleted ? "¡LISTA!" : $"{aq.currentProgress}/{aq.questData.requiredAmount}";
            stText.fontSize = 16;
            stText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            stText.alignment = TextAnchor.MiddleCenter;
            stText.color = Color.white;
        }
    }
}
