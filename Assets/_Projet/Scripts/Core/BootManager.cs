using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using VillaDelChef.Save;

namespace VillaDelChef.Core
{
    public class BootManager : MonoBehaviour
    {
        [Header("Scene Loading")]
        public string nextSceneName = "01_MainMenu";
        public float splashDelay = 1.0f;

        private void Start()
        {
            StartCoroutine(BootRoutine());
        }

        private IEnumerator BootRoutine()
        {
            // Ensure SaveManager is created and loaded
            if (SaveManager.Instance == null)
            {
                var smGO = new GameObject("SaveManager");
                smGO.AddComponent<SaveManager>();
            }

            // Target frame rate for smooth mobile gameplay
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = true;

            yield return new WaitForSeconds(splashDelay);

            // Transition to Main Menu
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
