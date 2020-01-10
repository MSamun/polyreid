using System;
using System.IO;
using UnityEngine;

namespace Polyreid
{
    public class DataManager : MonobehaviourReference
    {
        public static DataManager Instance { get; private set; }

        public SaveData SaveData { get; private set; }

        private string saveDataPath = string.Empty;
        private string saveDataFileName = ("data.json");

        #region Initialization

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

        private void Start()
        {
            // Grabs the folder/file that has all the saved data.
            saveDataPath = Application.persistentDataPath + "/" + saveDataFileName;
            LoadFromJson();
        }

        #endregion Initialization

        #region Save To Json

        public void SaveToJson()
        {
            // Creates a new SaveData class, which resets everything to default values of 0.
            SaveData = new SaveData();

            // Grabs all the necessary information and shoves it into the SaveData class.
            SaveSettingsData();

            // Shoves all the SaveData information into one long ass string.
            string saveContent = JsonUtility.ToJson(SaveData, true);

            // Writes it all to the dataFileName (data.json) and stores that file into the dataPath...
            // ...(Application.persistentDataPath, or the folder that the phone or computers allows us to store.)
            File.WriteAllText(saveDataPath, saveContent);
        }

        private void SaveSettingsData()
        {
            SaveData.graphicsSaveData = new GraphicsSaveData(SettingsManager.Instance.CurrentQualityLevel);
            SaveData.audioSaveData = new AudioSaveData(SettingsManager.Instance.BackgroundMusicVolume, SettingsManager.Instance.ButtonEffectVolume);
            SaveData.gameplaySaveData = new GameplaySaveData(StatsManager.Instance.NumberOfWins, StatsManager.Instance.NumberOfLosses, StatsManager.Instance.TotalTimeElapsed,
                StatsManager.Instance.BestTimeElapsedInMatch, StatsManager.Instance.LifetimeDamageDealt, StatsManager.Instance.LifetimeDamageTaken,
                StatsManager.Instance.LifetimeDamageHealed);
        }

        #endregion Save To Json

        #region Load From Json

        public void LoadFromJson()
        {
            if (File.Exists(saveDataPath))
            {
                // Writes all the data.json values into one long ass string.
                string saveContent = File.ReadAllText(saveDataPath);

                // Shoves all the content from the string "contents" into the SaveData information.
                SaveData = JsonUtility.FromJson<SaveData>(saveContent);

                // Loads up and replaces all the necessary information.
                LoadSettingsData();
            }
            else
            {
                EraseDataAndResetJson();
                Start();
            }
        }

        private void LoadSettingsData()
        {
            SettingsManager.Instance.SetGraphicAndAudioSettingsDataViaJson(SaveData.graphicsSaveData, SaveData.audioSaveData);
            StatsManager.Instance.SetGameplayStatsValuesViaJson(SaveData.gameplaySaveData);
        }

        #endregion Load From Json

        #region Erase Data and Reset Json

        public void EraseDataAndResetJson()
        {
            //Resets all the SaveData.
            SaveData = new SaveData
            {
                graphicsSaveData = new GraphicsSaveData(1),
                audioSaveData = new AudioSaveData(1, 1),
                gameplaySaveData = new GameplaySaveData(0, 0, 0, -1, 0, 0, 0)
            };

            //Shoves all the SaveData information into one long ass string.
            string saveContent = JsonUtility.ToJson(SaveData, true);

            // Writes it all to the dataFileName (data.json) and stores that file into the dataPath...
            // ...(Application.persistentDataPath, or the folder that the phone or computers allows us to store.)
            File.WriteAllText(saveDataPath, saveContent);

            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        #endregion Erase Data and Reset Json
    }
}