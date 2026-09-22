using System.Collections.Generic;
using UnityEngine;

namespace VillaDelChef.ScriptableObjects
{
    public enum CustomerArchetype
    {
        Normal,
        Impaciente,
        Generoso,
        Gourmet,
        VIP,
        CriticoGastronomico,
        Turista,
        Familia,
        Celebridad,
        MisionEspecial
    }

    [CreateAssetMenu(fileName = "NewCustomer", menuName = "VillaDelChef/Customer")]
    public class CustomerSO : ScriptableObject
    {
        [Header("Identity")]
        public string customerID;
        public string customerTitle;
        public CustomerArchetype archetype = CustomerArchetype.Normal;
        public Sprite characterSprite;

        [Header("Behavior & Attributes")]
        public float movementSpeed = 2.5f;
        public float basePatienceSeconds = 60f;
        [Range(0f, 1f)]
        public float tipProbability = 0.5f;
        public float tipMultiplier = 1.0f;

        [Header("Food Preferences")]
        public List<RecipeSO> preferredFoods = new List<RecipeSO>();
        public int unlockLevel = 1;

        [Header("Reputation & Dialogues")]
        public int reputationReward = 2;
        public int reputationPenalty = 3;
        public int bonusXP = 15;
        public string arrivalDialogue;
        public string satisfiedDialogue;
        public string angryDialogue;
    }
}
