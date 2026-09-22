using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Managers;

namespace VillaDelChef.UI
{
    public class TutorialUI : MonoBehaviour
    {
        public static TutorialUI Instance { get; private set; }

        [Header("UI Root")]
        public GameObject panelRoot;
        public Image avatarImage;
        public Text speakerNameText;
        public Text instructionText;
        public Button nextButton;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnTutorialStepChanged += HandleStepChanged;
                TutorialManager.Instance.OnTutorialFinished += HandleTutorialFinished;

                if (!TutorialManager.Instance.isTutorialActive)
                {
                    if (panelRoot != null) panelRoot.SetActive(false);
                }
                else
                {
                    TutorialManager.Instance.TriggerCurrentStepMessage();
                }
            }

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(() =>
                {
                    TutorialManager.Instance?.NextStepFromButton();
                });
            }
        }

        private void HandleStepChanged(TutorialStep step, string message)
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            if (instructionText != null) instructionText.text = message;

            // Show "Entendido" button on explanatory steps
            if (nextButton != null)
            {
                bool isActionStep = (step == TutorialStep.ExplainMarketAndFarming || step == TutorialStep.ExplainQuestsAndFinish);
                nextButton.gameObject.SetActive(isActionStep);
            }
        }

        private void HandleTutorialFinished()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }
    }
}
