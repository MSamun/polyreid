using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Polyreid
{
    public enum Turn { Player, Enemy };

    public class RoundEventManager : MonobehaviourReference
    {
        public static RoundEventManager Instance { get; private set; }
        public string RoundEventDescription { get; private set; }
        public Turn CurrentTurn { get; private set; }

        #region Variables

        private int numberOfRounds = 0;

        public readonly static string playerNameColour = "#00C800";         //Green (0, 200, 0).
        public readonly static string enemyNameColour = "#E10000";           //Red-ish (225, 0, 0).
        public readonly static string skillNameColour = "#FFC800";          //Orange (255, 200, 0).
        public readonly static string diceRollColour = "#00A0FF";           //Blue-ish (0, 160, 255).
        public readonly static string damageAmountColour = "#FF0000";       //Red (255, 0, 0).

        public event Action OnTurnChanged;

        private GameObject battleLogObject = null;
        private Text[] battleLogTextComponents = new Text[3];
        private string battleLogDescription;

        #endregion Variables

        #region Game Components

        [Header("---------- ROUND EVENT COMPONENTS ----------", order = 0)]
        [Header("Round Event Text", order = 1)]
        [SerializeField] private Text roundEventDescriptionText = null;

        [Header("Round Event Title Text")]
        [SerializeField] private Text roundEventTitleText = null;

        [Header("Round Event Scroll Text")]
        [SerializeField] private ScrollRect roundEventScrollRect = null;

        [Header("End Turn Button", order = 1)]
        [SerializeField] private Button endTurnButton = null;

        private Text endTurnButtonText = null;

        [Header("---------- BATTLE LOG COMPONENTS ----------", order = 0)]
        [Header("Battle Log Prefab", order = 1)]
        [SerializeField] private GameObject battleLogPrefab = null;

        [Header("Battle Log Object Parent")]
        [SerializeField] private GameObject battleLogParentObject = null;

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

            endTurnButtonText = endTurnButton.GetComponentInChildren<Text>();
            RoundEventDescription = string.Empty;
        }

        private void Start()
        {
            StartPlayerTurn();
            SpellManager.Instance.OnSpellStart += ClearRoundEventDescriptionText;
        }

        public void StartPlayerTurn()
        {
            CurrentTurn = Turn.Player;
            numberOfRounds++;
            battleLogDescription = string.Empty;
            EnemySpellManager.Instance.IsEnemyAttacking = false;

            CharacterManager.Instance.PlayerObject.ResetActionPoints();
            CharacterManager.Instance.EnemyObject.ResetActionPoints();

            DisableCharacterStatusEffects();
            ToggleEndButtonInteract(true);
            InstantiateBattleLogPrefabAndInitializeObjectComponents();
            ClearRoundEventDescriptionText();

            roundEventTitleText.text = string.Format($"[Round {numberOfRounds} - Your Turn]");
            SpellManager.Instance.StopAllCoroutines();

            if (!GameManager.Instance.ShouldKeepTrackOfTimeElapsedInMatch)
            {
                GameManager.Instance.ResetCurrentMatchValues(true);
            }

            OnTurnChanged?.Invoke();

            if (CharacterManager.Instance.PlayerObject.IsStunned)
            {
                StartCoroutine(DelayStunnedTextInEventDisplay());
                ToggleEndButtonInteract(false);
            }
        }

        //Used by End Turn button.
        public void StartEnemyTurn()
        {
            CurrentTurn = Turn.Enemy;
            battleLogDescription = string.Empty;
            EnemySpellManager.Instance.IsEnemyAttacking = false;
            EnemySpellManager.Instance.UsedUtilitySpellThisTurn = false;

            DisableCharacterStatusEffects();
            ToggleEndButtonInteract(false);
            ClearRoundEventDescriptionText();

            roundEventTitleText.text = string.Format("[Round {0} - " + CharacterManager.Instance.EnemyObject.name + "'s Turn]", numberOfRounds);
            SpellManager.Instance.StopAllCoroutines();

            OnTurnChanged?.Invoke();

            if (CharacterManager.Instance.EnemyObject.IsStunned)
            {
                StartCoroutine(DelayStunnedTextInEventDisplay());
            }
            else
            {
                StartCoroutine(EnemySpellManager.Instance.InitiateEnemyAttack());
            }
        }

        /// <summary>
        /// <para>Creates a new Battle Log Object prefab at the start of every round. Can't put all in one text box cause max vertices is 65K, so each turn gets its own text box.</para>
        /// <para>Since the amount of rounds is unknown, creating a prefab for it seems like the best solution. There may be a better way of doing it, but this is what I'm going with.</para>
        /// </summary>
        private void InstantiateBattleLogPrefabAndInitializeObjectComponents()
        {
            battleLogObject = Instantiate(battleLogPrefab, transform.position, transform.rotation);
            battleLogObject.transform.SetParent(battleLogParentObject.transform, false);
            battleLogObject.name = "Round " + numberOfRounds;

            //Three values in this array: 0 = Round Title, 1 = Player Battle Logs, 2 = Enemy Battle Logs.
            battleLogTextComponents = battleLogObject.GetComponentsInChildren<Text>(true);
            battleLogTextComponents[0].text = "Round " + numberOfRounds;
        }

        #endregion Initialization

        #region Custom Methods

        private void ClearRoundEventDescriptionText()
        {
            RoundEventDescription = string.Empty;
            UpdateRoundEventDescriptionText(RoundEventDescription);
        }

        public void UpdateRoundEventDescriptionText(string desc)
        {
            RoundEventDescription += desc;
            roundEventDescriptionText.text = string.Format(RoundEventDescription);
            UpdateBattleLogText(desc);

            AudioManager.Instance.PlayRoundEventSFX();
            roundEventScrollRect.verticalNormalizedPosition = Mathf.Lerp(roundEventScrollRect.verticalNormalizedPosition, 0, 2);
        }

        public void ToggleEndButtonInteract(bool isEnabled)
        {
            if (isEnabled)
            {
                endTurnButton.interactable = true;
                endTurnButtonText.color = Color.white;
                return;
            }
            else
            {
                endTurnButton.interactable = false;
                endTurnButtonText.color = new Color(0.6f, 0.6f, 0.6f, 1);
                return;
            }
        }

        private void DisableCharacterStatusEffects()
        {
            for (int i = 0; i < 4; i++)
            {
                CharacterManager.Instance.EnableDebuffAndBuffGameObjects(i, false);
            }
        }

        private IEnumerator DelayStunnedTextInEventDisplay()
        {
            yield return new WaitForSeconds(0.75f);

            string temp = string.Format("\n<color={0}>{1}</color> is stunned! Skipping turn...",
                CurrentTurn == Turn.Player ? playerNameColour : enemyNameColour,
                CurrentTurn == Turn.Player ? CharacterManager.Instance.PlayerObject.name : CharacterManager.Instance.EnemyObject.name);
            UpdateRoundEventDescriptionText(temp);

            yield return new WaitForSeconds(2f);

            if (CurrentTurn == Turn.Player)
            {
                StartEnemyTurn();
            }
            else
            {
                StartPlayerTurn();
            }

            yield break;
        }

        private void UpdateBattleLogText(string desc)
        {
            battleLogDescription += desc;

            if (CurrentTurn == Turn.Player)
            {
                battleLogTextComponents[1].text = string.Format(battleLogDescription);
            }
            else
            {
                battleLogTextComponents[2].text = string.Format(battleLogDescription);
            }
        }

        #endregion Custom Methods
    }
}