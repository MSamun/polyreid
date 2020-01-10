using UnityEngine;
using UnityEngine.UI;

namespace Polyreid
{
    public class TooltipManager : MonobehaviourReference
    {
        public static TooltipManager Instance { get; private set; }
        public bool IsHoldingDownSpell { get; private set; }

        private bool canHideSpells;
        private bool hasAddedButtonListener;
        private float touchHoldDownDuration;
        private GameObject currentSpellThatExecutedTooltip = null;

        [Header("---------- SPELL FRAME COMPONENTS ----------", order = 0)]
        [Header("Utility Spell Frame Objects", order = 1)]
        [SerializeField] private GameObject healSpellFrame = null;

        [SerializeField] private GameObject resilienceSpellFrame = null;
        [SerializeField] private GameObject forayStanceSpellFrame = null;
        [SerializeField] private GameObject rechargeSpelFrame = null;

        [Header("Offensive Spell Frame Objects")]
        [SerializeField] private GameObject swordOfRetulSpellFrame = null;

        [SerializeField] private GameObject iceShardsSpellFrame = null;
        [SerializeField] private GameObject crippleSpellFrame = null;
        [SerializeField] private GameObject fireballSpellFrame = null;
        [SerializeField] private GameObject chainJailSpellFrame = null;

        [Header("---------- SPELL TOOLTIP COMPONENTS ----------", order = 0)]
        [Header("Spell Tooltip/Overlay Object", order = 1)]
        [SerializeField] private GameObject overlayObject = null;

        [SerializeField] private GameObject spellTooltipObject = null;

        [Header("Spell Tooltip Basic Info")]
        [SerializeField] private Image spellTooltipIcon = null;

        [SerializeField] private Text spellTooltipNameText = null;
        [SerializeField] private Text spellTooltipCostText = null;
        [SerializeField] private Text spellTooltipCooldownText = null;
        [SerializeField] private Image spellTooltipModifierIcon = null;

        [Header("Spell Tooltip Description")]
        [SerializeField] private Text spellTooltipTypeOfSpellText = null;

        [SerializeField] private Text spellTooltipDescriptionText = null;
        [SerializeField] private Button spellTooltipUseSpellButton = null;
        private Text useSpellButtonText = null;

        #region Initialization

        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);
            else
                Instance = this;

            canHideSpells = false;
            IsHoldingDownSpell = false;
            hasAddedButtonListener = false;

            useSpellButtonText = spellTooltipUseSpellButton.GetComponentInChildren<Text>();
        }

        private void Update()
        {
            //  List of things that this Raycasting is supposed to do:
            //  - Grab input from the touch screen.
            //  - Disregard input in "Dead Zones" (Spell area, Portraits, Pause button, Show Stats button, Exit buttons of all Panels.)
            //  - If input is detected in a "Safe Zone", hide the Spell buttons.
            //  - If Spell button is held for (as of now) 0.75 seconds or longer, show the Spell Tooltip panel.

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Ray raycast = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit raycastHit;

                if (Physics.Raycast(raycast, out raycastHit))
                {
                    if (raycastHit.collider.CompareTag("Spell") && touch.phase == TouchPhase.Stationary)
                    {
                        print("You've hit a Spell button!");
                        touchHoldDownDuration += 0.05f;
                        print(touchHoldDownDuration);

                        if (touchHoldDownDuration >= 0.75f)
                        {
                            IsHoldingDownSpell = true;
                            print(touchHoldDownDuration);
                            currentSpellThatExecutedTooltip = raycastHit.collider.gameObject;
                            ShowSpellTooltip();
                        }
                    }
                }
                else
                {
                    if (touch.phase == TouchPhase.Began && canHideSpells)
                    {
                        print("Hiding Spells!");
                        HideAllSpells();
                    }
                }
            }
            else
            {
                if (touchHoldDownDuration != 0)
                    touchHoldDownDuration = 0;
            }
        }

        #endregion Initialization

        #region Spell Tooltip

        private void ShowSpellTooltip()
        {
            IsHoldingDownSpell = false;
            hasAddedButtonListener = false;
            spellTooltipUseSpellButton.onClick.RemoveAllListeners();

            UpdateSpellTooltipComponents();

            overlayObject.gameObject.SetActive(true);
            spellTooltipObject.gameObject.SetActive(true);
        }

        private void UpdateSpellTooltipComponents()
        {
            Spell spell = currentSpellThatExecutedTooltip.GetComponentInParent<Spell>();

            spellTooltipIcon.sprite = spell.SpellInfo.sprite;
            spellTooltipNameText.text = string.Format("{0}", spell.SpellInfo.name.ToUpper());

            if (spell.SpellInfo.actionCost == 0)
                spellTooltipCostText.text = string.Format("NO COST");
            else
                spellTooltipCostText.text = string.Format("{0} ACTION POINT{1}", spell.SpellInfo.actionCost, spell.SpellInfo.actionCost > 1 ? "S" : string.Empty);

            spellTooltipCooldownText.text = string.Format("{0} TURN{1}", spell.SpellInfo.totalCooldown, spell.SpellInfo.totalCooldown > 1 ? "S" : string.Empty);
            spellTooltipModifierIcon.sprite = spell.SpellInfo.modifierSprite;

            spellTooltipTypeOfSpellText.text = string.Format("{0}", spell.SpellInfo.typeOfSpell.ToString().ToUpper());
            spellTooltipDescriptionText.text = string.Format("{0}", spell.SpellInfo.spellDescription);

            if (!hasAddedButtonListener && spellTooltipObject.gameObject.activeInHierarchy)
            {
                if (CharacterManager.Instance.PlayerObject.CurrentActionPoints >= spell.SpellInfo.actionCost && RoundEventManager.Instance.CurrentTurn == Turn.Player
                    && !spell.isOnCooldown)
                {
                    spellTooltipUseSpellButton.onClick.AddListener(() => UseSpellViaTooltip(spell));
                    spellTooltipUseSpellButton.onClick.AddListener(() => AudioManager.Instance.PlayButtonSFX());
                    hasAddedButtonListener = true;

                    spellTooltipUseSpellButton.interactable = true;
                    useSpellButtonText.color = Color.white;
                }
                else
                {
                    spellTooltipUseSpellButton.interactable = false;
                    spellTooltipUseSpellButton.onClick.RemoveAllListeners();
                    useSpellButtonText.color = new Color(0.6f, 0.6f, 0.6f, 0.6f);
                }
            }
        }

        private void UseSpellViaTooltip(Spell spell)
        {
            overlayObject.gameObject.SetActive(false);
            spellTooltipObject.gameObject.SetActive(false);

            spell.UseSpell();
        }

        #endregion Spell Tooltip

        #region Hide/Show Spells

        //Referenced by the Player portrait.
        public void ShowUtilitySpells()
        {
            healSpellFrame.gameObject.SetActive(true);
            resilienceSpellFrame.gameObject.SetActive(true);
            forayStanceSpellFrame.gameObject.SetActive(true);
            rechargeSpelFrame.gameObject.SetActive(true);

            canHideSpells = true;
        }

        //Referenced by the Enemy portrait.
        public void ShowOffensiveSpells()
        {
            swordOfRetulSpellFrame.gameObject.SetActive(true);
            iceShardsSpellFrame.gameObject.SetActive(true);
            fireballSpellFrame.gameObject.SetActive(true);
            crippleSpellFrame.gameObject.SetActive(true);
            chainJailSpellFrame.gameObject.SetActive(true);

            canHideSpells = true;
        }

        public void HideAllSpells()
        {
            //If one of the Utility Spell frames are active, then all of them are, so I only need to make the check for one.
            if (healSpellFrame.gameObject.activeInHierarchy)
            {
                healSpellFrame.gameObject.SetActive(false);
                resilienceSpellFrame.gameObject.SetActive(false);
                forayStanceSpellFrame.gameObject.SetActive(false);
                rechargeSpelFrame.gameObject.SetActive(false);
            }

            //If one of the Offensive Spell frames are active, then all of them are, so I only need to make the check for one.
            if (swordOfRetulSpellFrame.activeInHierarchy)
            {
                swordOfRetulSpellFrame.gameObject.SetActive(false);
                iceShardsSpellFrame.gameObject.SetActive(false);
                fireballSpellFrame.gameObject.SetActive(false);
                crippleSpellFrame.gameObject.SetActive(false);
                chainJailSpellFrame.gameObject.SetActive(false);
            }

            canHideSpells = false;
        }

        #endregion Hide/Show Spells
    }
}