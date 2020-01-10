using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Polyreid
{
    public enum SpellType { Offensive, Utility, Debuff, Stun };

    public enum ModifierBonus { None, Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma };

    public class SpellManager : MonobehaviourReference
    {
        public static SpellManager Instance { get; private set; }

        #region Variables

        private int subtotalDiceRoll = 0;
        private int modBonus = 0;
        private int totalDiceRoll = 0;
        private bool isPlayerTurn = true;
        private bool rolledNat20 = false;

        public event Action OnSpellStart;    //Used to update Health and Action bars, alongside to know when Enemy is attacking, so it can wait to show text.

        public event Action OnSpellDone;    //Used to disable Spells that Player can't use due to lack of Action Points.

        private bool hasIncreasedAttack;
        private readonly string increasedAttackText = "<color=#FFC800>+3</color>";

        private bool hasDecreasedAttack;
        private readonly string decreasedAttackText = "<color=#FFC800>-3</color>";

        private int increasedDefenceBonusAmount = 0;

        #endregion Variables

        #region Game Components

        //Used to disable all buttons when Player is using a spell, or is Enemy's turn.
        [Header("Array of Spell Buttons")]
        [SerializeField] private Button[] arrayOfSpellButtons = new Button[8];

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

        #endregion Initialization

        #region Custom Methods

        /// <summary>
        /// Used whenever the Player/Enemy wants to use a spell to attack the opponent.
        /// </summary>
        /// <param name="spell">The spell being used.</param>
        public IEnumerator RollForAttack(SpellObject spell)
        {
            rolledNat20 = false;

            //Not Recharge Spell.
            if (spell.ID != 8)
            {
                CharacterManager.Instance.InfluenceActionPoints(spell.actionCost, true);
            }

            DiceManager.Instance.DisableRerollButton();

            modBonus = CharacterManager.Instance.DetermineModifierBonusAmount(spell.modifierBonus);
            isPlayerTurn = RoundEventManager.Instance.CurrentTurn == Turn.Player ? true : false;

            if (isPlayerTurn)
            {
                DisablePlayerButtonSpells();
                RoundEventManager.Instance.ToggleEndButtonInteract(false);
            }

            OnSpellStart?.Invoke();
            hasIncreasedAttack = CheckIfHasForayStanceBuff();
            hasDecreasedAttack = CheckIfHasCrippleDebuff();
            CheckIfHasResilienceBuff();

            //Not an attacking spell.
            if (spell.typeOfSpell == SpellType.Utility)
            {
                StartCoroutine(RollForUtility(spell));
                yield break;
            }

            string flexDesc = string.Empty;

            //Rolls first to see if whoever is using spell lands a hit on opponent.
            subtotalDiceRoll = DiceManager.Instance.MakeADiceRoll(DiceType.D20, 1);
            totalDiceRoll = subtotalDiceRoll + modBonus;
            totalDiceRoll = totalDiceRoll < 0 ? 0 : totalDiceRoll;
            totalDiceRoll += hasIncreasedAttack ? 3 : 0;
            totalDiceRoll -= hasDecreasedAttack ? 3 : 0;

            //[NAME] rolls a D20(+MOD).
            flexDesc += string.Format("\n<color={0}>{1}</color> rolls a <color=#00A0FF>D20</color>({2}{3}{4}{5}) ",
                isPlayerTurn == true ? RoundEventManager.playerNameColour : RoundEventManager.enemyNameColour,
                isPlayerTurn == true ? CharacterManager.Instance.PlayerObject.name : CharacterManager.Instance.EnemyObject.name,
                modBonus >= 0 ? "+" : string.Empty, modBonus,
                hasIncreasedAttack ? increasedAttackText : string.Empty,
                hasDecreasedAttack ? decreasedAttackText : string.Empty);

            //to attack [NAME] with [SKILL NAME].
            flexDesc += string.Format("to attack <color={0}>{1}</color> with <color={2}>{3}</color>.\n",
                isPlayerTurn == true ? RoundEventManager.enemyNameColour : RoundEventManager.playerNameColour,
                isPlayerTurn == true ? CharacterManager.Instance.EnemyObject.name : CharacterManager.Instance.PlayerObject.name,
                RoundEventManager.skillNameColour, spell.name);

            RoundEventManager.Instance.UpdateRoundEventDescriptionText(flexDesc);

            yield return new WaitForSeconds(1f);

            flexDesc = string.Empty;

            if (subtotalDiceRoll == 20)
            {
                rolledNat20 = true;

                //[NAME] rolled a NATURAL 20!
                flexDesc += string.Format("<color={0}>{1}</color> rolled a <color=#00A0FF>NATURAL 20!</color>\n",
                                isPlayerTurn == true ? RoundEventManager.playerNameColour : RoundEventManager.enemyNameColour,
                                isPlayerTurn == true ? CharacterManager.Instance.PlayerObject.name : CharacterManager.Instance.EnemyObject.name);
                RoundEventManager.Instance.UpdateRoundEventDescriptionText(flexDesc);

                StartCoroutine(RollForDamage(spell));
                yield break;
            }

            //[NAME] rolls a [#] (DICE+MOD).
            flexDesc += string.Format("<color={0}>{1}</color> rolls a {2} (<color=#00A0FF>{3}</color>{4}{5}{6}{7}).\n",
                 isPlayerTurn == true ? RoundEventManager.playerNameColour : RoundEventManager.enemyNameColour,
                 isPlayerTurn == true ? CharacterManager.Instance.PlayerObject.name : CharacterManager.Instance.EnemyObject.name,
                 totalDiceRoll, subtotalDiceRoll, modBonus >= 0 ? "+" : string.Empty, modBonus,
                 hasIncreasedAttack ? increasedAttackText : string.Empty,
                 hasDecreasedAttack ? decreasedAttackText : string.Empty);
            RoundEventManager.Instance.UpdateRoundEventDescriptionText(flexDesc);

            yield return new WaitForSeconds(1f);
            flexDesc = string.Empty;

            //Checks to see if Attack Roll is higher than Armor Class.
            int armorClassToCompare = isPlayerTurn == true ? CharacterManager.Instance.EnemyObject.ArmorClassAttribute : CharacterManager.Instance.PlayerObject.ArmorClassAttribute;
            bool hasPassedCheck = totalDiceRoll >= armorClassToCompare + increasedDefenceBonusAmount ? true : false;

            flexDesc += string.Format("<color={0}>{1}</color>", hasPassedCheck ? RoundEventManager.playerNameColour : RoundEventManager.enemyNameColour,
            hasPassedCheck ? "SUCCESS!\n" : "MISS!");

            RoundEventManager.Instance.UpdateRoundEventDescriptionText(flexDesc);

            if (hasPassedCheck)
            {
                StartCoroutine(RollForDamage(spell));
                yield break;
            }
            else
            {
                yield return new WaitForSeconds(1f);
                OnSpellDone?.Invoke();
                CharacterManager.Instance.CheckIfSwitchTurnsDueToNoActionPoints();

                if (isPlayerTurn && CharacterManager.Instance.PlayerObject.CurrentActionPoints > 0)
                {
                    RoundEventManager.Instance.ToggleEndButtonInteract(true);
                }

                yield break;
            }
        }

        /// <summary>
        /// Used whenever the Player/Enemy uses an Utility spell (Heal, Resilience, Foray Stance.)
        /// </summary>
        /// <param name="spell">The Utility spell being used.</param>
        public IEnumerator RollForUtility(SpellObject spell)
        {
            string flexDesc = string.Empty;

            flexDesc += string.Format("\n<color={0}>{1}</color> uses <color=#FFC800>{2}</color>.\n",
                   isPlayerTurn == true ? RoundEventManager.playerNameColour : RoundEventManager.enemyNameColour,
                   isPlayerTurn == true ? CharacterManager.Instance.PlayerObject.name : CharacterManager.Instance.EnemyObject.name,
                   spell.name);
            RoundEventManager.Instance.UpdateRoundEventDescriptionText(flexDesc);

            //Heal Spell.
            if (spell.ID == 0)
            {
                StartCoroutine(RollForDamage(spell));
                yield break;
            }

            //Resilience (Increase Armor Class).
            if (spell.ID == 1)
            {
                CharacterManager.Instance.EnableDebuffAndBuffGameObjects(0, true);
            }

            //Foray Stance (Increase Attack and Damage Roll).
            if (spell.ID == 2)
            {
                CharacterManager.Instance.EnableDebuffAndBuffGameObjects(1, true);
            }

            //Recharge (Increases AP by one).
            if (spell.ID == 8)
            {
                CharacterManager.Instance.InfluenceActionPoints(1, false);
            }

            PlaySpellSFX(spell.spellSFXClip);

            yield return new WaitForSeconds(1f);
            OnSpellDone?.Invoke();
            CharacterManager.Instance.CheckIfSwitchTurnsDueToNoActionPoints();

            if (isPlayerTurn && CharacterManager.Instance.PlayerObject.CurrentActionPoints > 0)
            {
                RoundEventManager.Instance.ToggleEndButtonInteract(true);
            }

            yield break;
        }

        /// <summary>
        /// Used to calculate damage (or how much health restored) from Spells.
        /// </summary>
        /// <param name="spell">The spell being used.</param>
        public IEnumerator RollForDamage(SpellObject spell)
        {
            //Calculates the total damage (or health restored) from Spell.
            //NOTE: modBonus is already calculated in RollForAttack(); method (Line 69).
            subtotalDiceRoll = DiceManager.Instance.MakeADiceRoll(spell.diceToRoll, spell.numberOfRolls);
            if (rolledNat20)
            { subtotalDiceRoll *= 2; }
            totalDiceRoll = subtotalDiceRoll + modBonus;
            totalDiceRoll = totalDiceRoll < 0 ? 0 : totalDiceRoll;
            totalDiceRoll += hasIncreasedAttack ? 3 : 0;

            string flexDesc = string.Empty;

            //Waits 1 second before executing the code below this line.
            yield return new WaitForSeconds(1f);

            //Heal spell.
            if (spell.ID == 0)
            {
                //[NAME] rolls [DICE](+MOD) to heal themself.
                flexDesc += string.Format("<color={0}>{1}</color> rolls <color=#00A0FF>{2}{3}</color>({4}{5}{6}) to heal themself.\n",
                 isPlayerTurn == true ? RoundEventManager.playerNameColour : RoundEventManager.enemyNameColour,
                 isPlayerTurn == true ? CharacterManager.Instance.PlayerObject.name : CharacterManager.Instance.EnemyObject.name,
                 spell.numberOfRolls, spell.diceToRoll.ToString(), modBonus >= 0 ? "+" : string.Empty, modBonus,
                 hasIncreasedAttack ? increasedAttackText : string.Empty);
                RoundEventManager.Instance.UpdateRoundEventDescriptionText(flexDesc);

                yield return new WaitForSeconds(1f);
                flexDesc = string.Empty;

                //[NAME] heals for [#HP](DICE+MOD).
                flexDesc += string.Format("<color={0}>{1}</color> heals for <color=#00C800>{2}HP</color> (<color=#00A0FF>{3}</color>{4}{5}{6}).",
                 isPlayerTurn == true ? RoundEventManager.playerNameColour : RoundEventManager.enemyNameColour,
                 isPlayerTurn == true ? CharacterManager.Instance.PlayerObject.name : CharacterManager.Instance.EnemyObject.name,
                 totalDiceRoll, subtotalDiceRoll, modBonus >= 0 ? "+" : string.Empty, modBonus,
                 hasIncreasedAttack ? increasedAttackText : string.Empty);
                RoundEventManager.Instance.UpdateRoundEventDescriptionText(flexDesc);

                CharacterManager.Instance.InfluenceHealthPoints(totalDiceRoll, false);
                PlaySpellSFX(spell.spellSFXClip);
            }
            else
            {
                //[NAME] rolls [DICE](+MOD) for damage.
                flexDesc += string.Format("<color={0}>{1}</color> rolls <color=#00A0FF>{2}{3}</color>({4}{5}{6}) for damage.\n",
                     isPlayerTurn == true ? RoundEventManager.playerNameColour : RoundEventManager.enemyNameColour,
                     isPlayerTurn == true ? CharacterManager.Instance.PlayerObject.name : CharacterManager.Instance.EnemyObject.name,
                     spell.numberOfRolls, spell.diceToRoll.ToString(), modBonus >= 0 ? "+" : string.Empty, modBonus,
                     hasIncreasedAttack ? increasedAttackText : string.Empty);
                RoundEventManager.Instance.UpdateRoundEventDescriptionText(flexDesc);

                yield return new WaitForSeconds(1f);
                flexDesc = string.Empty;

                if (rolledNat20)
                {
                    //[NAME] deals [#DMG] (DICEx2+MOD) to [NAME].
                    flexDesc += string.Format("<color={0}>{1}</color> deals <color=#FF0000>{2}DMG</color> (<color=#00A0FF>{3}x2</color>{4}{5}{6}) to <color={7}>{8}</color>.",
                         isPlayerTurn == true ? RoundEventManager.playerNameColour : RoundEventManager.enemyNameColour,
                         isPlayerTurn == true ? CharacterManager.Instance.PlayerObject.name : CharacterManager.Instance.EnemyObject.name,
                         totalDiceRoll, subtotalDiceRoll / 2, modBonus >= 0 ? "+" : string.Empty, modBonus,
                         hasIncreasedAttack ? increasedAttackText : string.Empty,
                         isPlayerTurn == true ? RoundEventManager.enemyNameColour : RoundEventManager.playerNameColour,
                          isPlayerTurn == true ? CharacterManager.Instance.EnemyObject.name : CharacterManager.Instance.PlayerObject.name);
                    RoundEventManager.Instance.UpdateRoundEventDescriptionText(flexDesc);
                    PlaySpellSFX(spell.spellSFXClip);
                }
                else
                {
                    //[NAME] deals [#DMG] (DICE+MOD) to [NAME].
                    flexDesc += string.Format("<color={0}>{1}</color> deals <color=#FF0000>{2}DMG</color> (<color=#00A0FF>{3}</color>{4}{5}{6}) to <color={7}>{8}</color>.",
                         isPlayerTurn == true ? RoundEventManager.playerNameColour : RoundEventManager.enemyNameColour,
                         isPlayerTurn == true ? CharacterManager.Instance.PlayerObject.name : CharacterManager.Instance.EnemyObject.name,
                         totalDiceRoll, subtotalDiceRoll, modBonus >= 0 ? "+" : string.Empty, modBonus,
                         hasIncreasedAttack ? increasedAttackText : string.Empty,
                         isPlayerTurn == true ? RoundEventManager.enemyNameColour : RoundEventManager.playerNameColour,
                          isPlayerTurn == true ? CharacterManager.Instance.EnemyObject.name : CharacterManager.Instance.PlayerObject.name);
                    RoundEventManager.Instance.UpdateRoundEventDescriptionText(flexDesc);
                    PlaySpellSFX(spell.spellSFXClip);
                }

                CharacterManager.Instance.InfluenceHealthPoints(totalDiceRoll, true);
            }

            //Cripple (Debuff).
            if (spell.ID == 4)
            {
                CharacterManager.Instance.EnableDebuffAndBuffGameObjects(2, true);
            }

            //Chain Jail (Stun).
            if (spell.ID == 3)
            {
                CharacterManager.Instance.EnableDebuffAndBuffGameObjects(3, true);
            }

            yield return new WaitForSeconds(1f);
            OnSpellDone?.Invoke();
            CharacterManager.Instance.CheckIfSwitchTurnsDueToNoActionPoints();

            if (isPlayerTurn && CharacterManager.Instance.PlayerObject.CurrentActionPoints > 0)
            {
                RoundEventManager.Instance.ToggleEndButtonInteract(true);
            }

            yield break;
        }

        private void DisablePlayerButtonSpells()
        {
            //Disables all other spells, so Player can't use multiple spells at once.
            for (int i = 0; i < arrayOfSpellButtons.Length; i++)
            {
                arrayOfSpellButtons[i].interactable = false;
            }
        }

        //Increases Attack and Damage rolls.
        private bool CheckIfHasForayStanceBuff()
        {
            //Use PlayerObject if it's Player's Turn; else, use EnemyObject.
            CharacterObject charObject = RoundEventManager.Instance.CurrentTurn == Turn.Player ? CharacterManager.Instance.PlayerObject : CharacterManager.Instance.EnemyObject;
            return charObject.HasIncreasedAttack;
        }

        private void CheckIfHasResilienceBuff()
        {
            //Use PlayerObject if it's Player's Turn; else, use EnemyObject.
            CharacterObject charObject = RoundEventManager.Instance.CurrentTurn == Turn.Player ? CharacterManager.Instance.EnemyObject : CharacterManager.Instance.PlayerObject;
            increasedDefenceBonusAmount = 0;

            if (charObject.HasIncreasedDefence)
            {
                increasedDefenceBonusAmount = 2 + charObject.StrengthModifier;

                if (increasedDefenceBonusAmount < 2)
                {
                    increasedDefenceBonusAmount = 2;
                }
            }
        }

        private bool CheckIfHasCrippleDebuff()
        {
            //Use PlayerObject if it's Player's Turn; else, use EnemyObject.
            CharacterObject charObject = RoundEventManager.Instance.CurrentTurn == Turn.Player ? CharacterManager.Instance.PlayerObject : CharacterManager.Instance.EnemyObject;
            return charObject.HasDecreasedAttack;
        }

        private void PlaySpellSFX(AudioClip clip)
        {
            //Play appropriate Spell SFX.
            if (clip != null)
            {
                AudioManager.Instance.PlaySpellSFX(clip);
            }
        }

        #endregion Custom Methods
    }
}