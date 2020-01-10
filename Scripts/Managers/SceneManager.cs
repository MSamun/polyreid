using UnityEngine;
using UnityEngine.SceneManagement;

namespace Polyreid
{
    public class SceneManager : MonobehaviourReference
    {
        public static SceneManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        #region On Button Click

        //Referenced by Start Game button in the Main Menu scene, Restart button (settings and results panel) in Game scene.
        public void OnLoadSceneButtonClick(int sceneToLoad)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }

        public void OnExitButtonClick()
        {
            DataManager.Instance.SaveToJson();
            Application.Quit();
        }

        #endregion On Button Click
    }
}