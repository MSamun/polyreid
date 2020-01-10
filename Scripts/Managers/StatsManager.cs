using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Polyreid
{
    public class StatsManager : MonobehaviourReference
    {
        public static StatsManager Instance { get; private set; }
        public int NumberOfWins { get; set; }
        public int NumberOfLosses { get; set; }

        public int BestTimeElapsedInMatch { get; set; }

        public int TotalTimeElapsed { get; private set; }
        public int LifetimeDamageDealt { get; set; }
        public int LifetimeDamageTaken { get; set; }
        public int LifetimeDamageHealed { get; set; }

        #region Game Components

        [Header("Wins/Losses/Ratio Text")]
        [SerializeField] private Text winsText = null;

        [SerializeField] private Text lossesText = null;
        [SerializeField] private Text ratioText = null;

        [Header("Time Elapsed Text")]
        [SerializeField] private Text totalTimeElapsedText = null;

        [Header("Best Time Text")]
        [SerializeField] private Text bestTimeElapsedInMatchText = null;

        [Header("Lifetime Damage Dealt, Taken, Healed Text")]
        [SerializeField] private Text lifetimeDamageDealtText = null;

        [SerializeField] private Text lifetimeDamageTakenText = null;
        [SerializeField] private Text lifetimeDamageHealedText = null;

        #endregion Game Components

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

        public void SetGameplayStatsValuesViaJson(GameplaySaveData json)
        {
            TotalTimeElapsed = json.totalGameplayTime;
            NumberOfWins = json.numberOfWins;
            NumberOfLosses = json.numberOfLosses;

            BestTimeElapsedInMatch = json.bestGameplayTime;
            LifetimeDamageDealt = json.lifetimeDamageDealt;
            LifetimeDamageTaken = json.lifetimeDamageTaken;
            LifetimeDamageHealed = json.lifetimeDamageHealed;

            UpdateTotalTimeElapsed();
            UpdateStatPanelValues();
            StartCoroutine(TrackTotalGameplayTime());
        }

        private IEnumerator TrackTotalGameplayTime()
        {
            bool isKeepingTrackOfTime = true;
            while (isKeepingTrackOfTime)
            {
                yield return new WaitForSeconds(1);
                TotalTimeElapsed += 1;
                UpdateTotalTimeElapsed();
            }
        }

        #endregion Initialization

        private void UpdateTotalTimeElapsed()
        {
            float seconds = Mathf.Floor(TotalTimeElapsed % 60);
            float minutes = Mathf.Floor(TotalTimeElapsed / 60) % 60;
            float hours = Mathf.Floor(TotalTimeElapsed / 3600) % 24;
            float days = Mathf.Floor(TotalTimeElapsed / 86400);
            totalTimeElapsedText.text = string.Format("{0:0d}:{1:00h}:{2:00m}:{3:00s}", days, hours, minutes, seconds);
        }

        public void UpdateStatPanelValues()
        {
            //Win/Loss/Ratio
            float winLossRatio = 0f;
            winsText.text = string.Format("{0} WINS", NumberOfWins);
            lossesText.text = string.Format("{0} LOSS", NumberOfLosses);

            //Can't divide by 0; will give error.
            if (NumberOfLosses != 0)
            {
                winLossRatio = NumberOfWins * 1.0f / NumberOfLosses * 1.0f;
                winLossRatio = Mathf.Round(winLossRatio * 100f) / 100f;
            }
            else
            {
                winLossRatio = NumberOfWins * 1.0f;
                winLossRatio = Mathf.Round(winLossRatio * 100f) / 100f;
            }

            if (winLossRatio < 1)
            {
                ratioText.text = string.Format("= {0:0.00}", winLossRatio);
            }
            else
            {
                ratioText.text = string.Format("= {0:#.00}", winLossRatio);
            }

            //Best Time.
            if (BestTimeElapsedInMatch != -1)
            {
                float seconds = Mathf.Floor(BestTimeElapsedInMatch % 60);
                float minutes = Mathf.Floor(BestTimeElapsedInMatch / 60) % 60;
                bestTimeElapsedInMatchText.text = string.Format("{0:00m}:{1:00s}", minutes, seconds);
            }
            else
            {
                bestTimeElapsedInMatchText.text = string.Format("00m:00s");
            }

            //Lifetime.
            lifetimeDamageDealtText.text = string.Format("{0}", LifetimeDamageDealt.ToString("n0"));
            lifetimeDamageTakenText.text = string.Format("{0}", LifetimeDamageTaken.ToString("n0"));
            lifetimeDamageHealedText.text = string.Format("{0}", LifetimeDamageHealed.ToString("n0"));
        }
    }
}