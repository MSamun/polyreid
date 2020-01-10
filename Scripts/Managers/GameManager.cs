using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Polyreid
{
    public class GameManager : MonobehaviourReference
    {
        public static GameManager Instance { get; private set; }

        public bool ShouldKeepTrackOfTimeElapsedInMatch { get; private set; }
        public int CurrentTimeElaspedInMatch { get; private set; }
        public int CurrentDamageDealtInMatch { get; set; }
        public int CurrentDamageTakenInMatch { get; set; }
        public int CurrentDamageHealedInMatch { get; set; }

        [Header("---------- RESULTS COMPONENTS ----------", order = 0)]
        [Header("Result Screen Objects", order = 1)]
        [SerializeField] private GameObject overlayObject = null;

        [SerializeField] private GameObject resultScreenObject = null;
        [SerializeField] private Text gameStatusText = null;

        [Header("---------- CURRENT MATCH COMPONENTS ----------", order = 0)]
        // current[..] variables represent values that have to do with the current match being played.
        [Header("Current Damage Dealt, Taken, Healed Text", order = 1)]
        [SerializeField] private Text currentDamageDealtText = null;

        [SerializeField] private Text currentDamageTakenText = null;
        [SerializeField] private Text currentDamageHealedText = null;

        [Header("Current/Best Time Text")]
        [SerializeField] private Text currentTimeElapsedInMatchText = null;

        [SerializeField] private Text bestTimeElapsedInMatchText = null;

        #region Initialization

        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);
            else
                Instance = this;
        }

        #endregion Initialization

        #region Current Match Tracking

        public void ResetCurrentMatchValues(bool shouldTrack)
        {
            ShouldKeepTrackOfTimeElapsedInMatch = shouldTrack;

            if (shouldTrack)
            {
                CurrentTimeElaspedInMatch = 0;
                CurrentDamageTakenInMatch = 0;
                CurrentDamageDealtInMatch = 0;
                CurrentDamageHealedInMatch = 0;

                StartCoroutine(TrackCurrentMatchGameplayTime());
            }
            else
            {
                StopCoroutine(TrackCurrentMatchGameplayTime());
            }
        }

        public IEnumerator TrackCurrentMatchGameplayTime()
        {
            while (ShouldKeepTrackOfTimeElapsedInMatch)
            {
                yield return new WaitForSeconds(1f);
                CurrentTimeElaspedInMatch += 1;
            }
        }

        public void UpdateResultScreenStatValues()
        {
            // The -1 is for a new SaveData. This assumes that the player hasn't played their first match yet, which would mean they haven't set a record yet.
            if (StatsManager.Instance.BestTimeElapsedInMatch > CurrentTimeElaspedInMatch || StatsManager.Instance.BestTimeElapsedInMatch == -1)
            {
                StatsManager.Instance.BestTimeElapsedInMatch = CurrentTimeElaspedInMatch;
            }

            float seconds = Mathf.Floor(StatsManager.Instance.BestTimeElapsedInMatch % 60);
            float minutes = Mathf.Floor(StatsManager.Instance.BestTimeElapsedInMatch / 60) % 60;
            bestTimeElapsedInMatchText.text = string.Format("{0:00m}:{1:00s}", minutes, seconds);

            seconds = Mathf.Floor(CurrentTimeElaspedInMatch % 60);
            minutes = Mathf.Floor(CurrentTimeElaspedInMatch / 60) % 60;

            currentTimeElapsedInMatchText.text = string.Format("{0:00m}:{1:00s}", minutes, seconds);
            currentDamageDealtText.text = string.Format("{0}", CurrentDamageDealtInMatch.ToString("n0"));
            currentDamageTakenText.text = string.Format("{0}", CurrentDamageTakenInMatch.ToString("n0"));
            currentDamageHealedText.text = string.Format("{0}", CurrentDamageHealedInMatch.ToString("n0"));

            StatsManager.Instance.LifetimeDamageDealt += CurrentDamageDealtInMatch;
            StatsManager.Instance.LifetimeDamageTaken += CurrentDamageTakenInMatch;
            StatsManager.Instance.LifetimeDamageHealed += CurrentDamageHealedInMatch;
            StatsManager.Instance.UpdateStatPanelValues();

            DataManager.Instance.SaveToJson();
        }

        #endregion Current Match Tracking

        #region Result of Win/Loss

        private void EnableResultsScreenObjectComponents()
        {
            overlayObject.gameObject.SetActive(true);
            resultScreenObject.gameObject.SetActive(true);

            EnemySpellManager.Instance.IsEnemyAttacking = false;
            CharacterManager.Instance.EnemyObject.LoseOrGainActionPoints(CharacterManager.Instance.EnemyObject.CurrentActionPoints, true);
            ResetCurrentMatchValues(false);
        }

        public void PlayerWonTheGame()
        {
            EnableResultsScreenObjectComponents();
            gameStatusText.text = "VICTORY";
            StatsManager.Instance.NumberOfWins++;
            UpdateResultScreenStatValues();
            StartCoroutine(AudioManager.Instance.PlayAndLoopThroughResultMusic(true));
        }

        public void PlayerLostTheGame()
        {
            EnableResultsScreenObjectComponents();
            gameStatusText.text = "DEFEAT";
            StatsManager.Instance.NumberOfLosses++;
            UpdateResultScreenStatValues();
            StartCoroutine(AudioManager.Instance.PlayAndLoopThroughResultMusic(false));
        }

        #endregion Result of Win/Loss
    }
}