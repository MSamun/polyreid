using System;

namespace Polyreid
{
    [Serializable]
    public class SaveData
    {
        public GraphicsSaveData graphicsSaveData;
        public AudioSaveData audioSaveData;
        public GameplaySaveData gameplaySaveData;

        public SaveData()
        {
        }
    }

    #region Settings Save Data

    //NOTE: JSON, for some reason, does not take in properties or private variables. It only takes public variables.
    [Serializable]
    public class AudioSaveData
    {
        public float backgroundMusicVolume;
        public float buttonSoundEffectVolume;

        public AudioSaveData(float backgroundMusicVolume, float buttonSoundEffectVolume)
        {
            this.backgroundMusicVolume = backgroundMusicVolume;
            this.buttonSoundEffectVolume = buttonSoundEffectVolume;
        }
    }

    [Serializable]
    public class GraphicsSaveData
    {
        public int currentQualityLevel;

        public GraphicsSaveData(int currentQualityLevel)
        {
            this.currentQualityLevel = currentQualityLevel;
        }
    }

    [Serializable]
    public class GameplaySaveData
    {
        public int numberOfWins;
        public int numberOfLosses;
        public int totalGameplayTime;

        public int bestGameplayTime;
        public int lifetimeDamageDealt;
        public int lifetimeDamageTaken;
        public int lifetimeDamageHealed;

        public GameplaySaveData(int numberOfWins, int numberOfLosses, int totalGameplayTime, int bestGameplayTime, int lifetimeDamageDealt, int lifetimeDamageTaken,
            int lifetimeDamageHealed)
        {
            this.numberOfWins = numberOfWins;
            this.numberOfLosses = numberOfLosses;
            this.totalGameplayTime = totalGameplayTime;

            this.bestGameplayTime = bestGameplayTime;
            this.lifetimeDamageDealt = lifetimeDamageDealt;
            this.lifetimeDamageTaken = lifetimeDamageTaken;
            this.lifetimeDamageHealed = lifetimeDamageHealed;
        }
    }

    #endregion Settings Save Data
}