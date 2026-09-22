using System;
using UnityEngine;
using VillaDelChef.Core;
using VillaDelChef.Economy;
using VillaDelChef.Progression;
using VillaDelChef.Save;

namespace VillaDelChef.Managers
{
    public enum TutorialStep
    {
        HarvestFirstCrop = 0,
        CookFirstDish = 1,
        ServeCustomer = 2,
        ExplainMarketAndFarming = 3,
        ExplainQuestsAndFinish = 4,
        Completed = 5
    }

    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        public TutorialStep currentStep = TutorialStep.HarvestFirstCrop;
        public bool isTutorialActive = true;

        public event Action<TutorialStep, string> OnTutorialStepChanged;
        public event Action OnTutorialFinished;

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
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                if (SaveManager.Instance.CurrentSave.tutorialCompleted)
                {
                    isTutorialActive = false;
                    currentStep = TutorialStep.Completed;
                    return;
                }
                currentStep = (TutorialStep)SaveManager.Instance.CurrentSave.tutorialStep;
            }

            RegisterEvents();
            TriggerCurrentStepMessage();
        }

        private void RegisterEvents()
        {
            GameEvents.OnCropHarvested += (plot, crop, amount) =>
            {
                if (isTutorialActive && currentStep == TutorialStep.HarvestFirstCrop)
                {
                    AdvanceStep(TutorialStep.CookFirstDish);
                }
            };

            GameEvents.OnCookingStarted += (station, recipe) =>
            {
                if (isTutorialActive && currentStep == TutorialStep.CookFirstDish)
                {
                    AdvanceStep(TutorialStep.ServeCustomer);
                }
            };

            GameEvents.OnDishDelivered += (counter, dish) =>
            {
                if (isTutorialActive && currentStep == TutorialStep.ServeCustomer)
                {
                    AdvanceStep(TutorialStep.ExplainMarketAndFarming);
                }
            };
        }

        public void AdvanceStep(TutorialStep nextStep)
        {
            currentStep = nextStep;
            SaveProgress();

            if (currentStep >= TutorialStep.Completed)
            {
                CompleteTutorial();
            }
            else
            {
                TriggerCurrentStepMessage();
            }
        }

        public void NextStepFromButton()
        {
            if (currentStep == TutorialStep.ExplainMarketAndFarming)
            {
                AdvanceStep(TutorialStep.ExplainQuestsAndFinish);
            }
            else if (currentStep == TutorialStep.ExplainQuestsAndFinish)
            {
                AdvanceStep(TutorialStep.Completed);
            }
        }

        public void TriggerCurrentStepMessage()
        {
            if (!isTutorialActive) return;

            string message = GetMessageForStep(currentStep);
            OnTutorialStepChanged?.Invoke(currentStep, message);
        }

        private string GetMessageForStep(TutorialStep step)
        {
            switch (step)
            {
                case TutorialStep.HarvestFirstCrop:
                    return "¡Bienvenido a Villa del Chef! Comencemos cosechando tu primer ingrediente fresco en la parcela de cultivo.";
                case TutorialStep.CookFirstDish:
                    return "¡Excelente cosecha! Ahora toca la Cocina a Gas para preparar tu primer platillo.";
                case TutorialStep.ServeCustomer:
                    return "¡En cocción! En cuanto termine, tu ayudante llevará el platillo a la mesa del cliente.";
                case TutorialStep.ExplainMarketAndFarming:
                    return "¡El cliente pagó con monedas y propina! Si te quedas sin ingredientes, cómpralos en el MERCADO o cultiva en tu huerto.";
                case TutorialStep.ExplainQuestsAndFinish:
                    return "¡Cumple MISIONES para ganar recompensas y subir de nivel! El restaurante es todo tuyo, ¡buena suerte chef!";
                default:
                    return "";
            }
        }

        private void CompleteTutorial()
        {
            isTutorialActive = false;
            currentStep = TutorialStep.Completed;
            SaveProgress();

            // Reward
            EconomyManager.Instance?.AddCoins(50);
            ProgressionManager.Instance?.AddExperience(25);
            AudioManager.Instance?.PlayLevelUp();

            OnTutorialFinished?.Invoke();
            Debug.Log("[TutorialManager] ¡Tutorial completado con éxito! Recompensa otorgada.");
        }

        private void SaveProgress()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                SaveManager.Instance.CurrentSave.tutorialStep = (int)currentStep;
                SaveManager.Instance.CurrentSave.tutorialCompleted = (currentStep == TutorialStep.Completed);
                SaveManager.Instance.SaveGame();
            }
        }
    }
}
