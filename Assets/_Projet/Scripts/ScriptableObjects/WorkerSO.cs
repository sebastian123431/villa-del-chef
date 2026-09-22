using UnityEngine;

namespace VillaDelChef.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewWorker", menuName = "VillaDelChef/Worker")]
    public class WorkerSO : ScriptableObject
    {
        [Header("Worker Identity")]
        public string workerID;
        public string workerName;
        public Sprite characterSprite;

        [Header("Attributes")]
        public int workerLevel = 1;
        public float movementSpeed = 3.2f;
        public int carryCapacity = 1;
        public bool canCleanTables = false;
        public bool canCollectDishes = false;

        [Header("Unlock Requirements")]
        public int unlockLevel = 1;
        public int hireCost = 150;
    }
}
