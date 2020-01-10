using UnityEngine;
using UnityEngine.UI;

namespace Polyreid
{
    public class Spell : MonobehaviourReference
    {
        public SpellObject SpellInfo { get { return spellInfo; } }

        #region Game Components

        [Header("Spell Object")]
        [SerializeField] private SpellObject spellInfo = null;

        [Header("Cooldown Info")]
        public bool isOnCooldown;

        [SerializeField] private int currentCooldown;

        [Header("---------- SPELL COMPONENTS ----------", order = 0)]
        [Header("Spell Button", order = 1)]
        [SerializeField] private Button spellButton = null;

        [Header("Spell Action Cost Text")]
        [SerializeField] private Text spellActionCostText = null;

        [Header("Spell Cooldown Panel", order = 1)]
        [SerializeField] private Image spellCooldownPanel = null;

        private Text spellCooldownText = null;

        #endregion Game Components

        #region Initialization

        private void Awake()
        {
            spellButton.GetComponent<Image>().sprite = spellInfo.sprite;
            spellCooldownText = spellCooldownPanel.GetComponentInChildren<Text>();
        }

        private void Start()
        {
            if (spellButton.gameObject.activeInHierarchy)
                EnableSpellsThatCanBeUsedAfterUseOfFirstSpell();

            RoundEventManager.Instance.OnTurnChanged += CheckCooldownOnSpell;
            RoundEventManager.Instance.OnTurnChanged += DisableSpellsOnEnemyTurn;
            SpellManager.Instance.OnSpellDone += EnableSpellsThatCanBeUsedAfterUseOfFirstSpell;

            spellButton.onClick.AddListener(() => UseSpell());
            isOnCooldown = false;
            spellActionCostText.text = string.Format("{0}", spellInfo.actionCost);
        }

        #endregion Initialization

        #region Custom Methods

        public void UseSpell()
        {
            if (!TooltipManager.Instance.IsHoldingDownSpell)
            {
                isOnCooldown = true;
                currentCooldown = spellInfo.totalCooldown;

                spellCooldownPanel.gameObject.SetActive(true);
                spellCooldownPanel.fillAmount = 1;
                spellCooldownText.text = string.Format("{0}", spellInfo.totalCooldown);

                StartCoroutine(SpellManager.Instance.RollForAttack(spellInfo));
            }
        }

        private void DisableSpellsOnEnemyTurn()
        {
            if (RoundEventManager.Instance.CurrentTurn != Turn.Enemy && !CharacterManager.Instance.PlayerObject.IsStunned)
                EnableSpellsThatCanBeUsedAfterUseOfFirstSpell();
            else
                spellButton.interactable = false;
        }

        private void EnableSpellsThatCanBeUsedAfterUseOfFirstSpell()
        {
            if (CharacterManager.Instance.PlayerObject.CurrentActionPoints >= spellInfo.actionCost && RoundEventManager.Instance.CurrentTurn == Turn.Player && !isOnCooldown)
                spellButton.interactable = true;
        }

        private void CheckCooldownOnSpell()
        {
            if (RoundEventManager.Instance.CurrentTurn == Turn.Player)
            {
                if (isOnCooldown)
                {
                    currentCooldown--;
                    spellCooldownText.text = string.Format("{0}", currentCooldown);
                    spellCooldownPanel.fillAmount = currentCooldown / (float)spellInfo.totalCooldown;

                    if (currentCooldown == 0)
                    {
                        isOnCooldown = false;
                        spellCooldownPanel.gameObject.SetActive(false);
                        spellButton.interactable = true;
                    }
                }
            }
            else
                return;
        }

        #endregion Custom Methods
    }
}