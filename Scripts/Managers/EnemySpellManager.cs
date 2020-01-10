using System.Collections;
using UnityEngine;

namespace Polyreid
{
    public class EnemySpellManager : MonobehaviourReference
    {
        public static EnemySpellManager Instance { get; private set; }

        public bool IsEnemyAttacking { get; set; }
        public bool UsedUtilitySpellThisTurn { get; set; }

        [System.Serializable]
        public struct EnemySpells
        {
            public SpellObject spell;
            public bool isOnCooldown;
            public int currentCooldown;
        }

        #region Game Components

        [Header("---------- SPELL OBJECTS ----------", order = 0)]
        [Header("Enemy Spells", order = 1)]
        [SerializeField] private EnemySpells[] arrayOfEnemySpells = new EnemySpells[8];

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

        private void Start()
        {
            SpellManager.Instance.OnSpellStart += EnemyIsCurrentlyAttacking;
            SpellManager.Instance.OnSpellDone += EnemyFinishedAttacking;
            RoundEventManager.Instance.OnTurnChanged += DecrementCooldownOnSpells;
        }

        #endregion Initialization

        #region Custom Methods

        public IEnumerator InitiateEnemyAttack()
        {
            while (CharacterManager.Instance.EnemyObject.CurrentActionPoints > 0 && RoundEventManager.Instance.CurrentTurn == Turn.Enemy)
            {
                yield return new WaitForSeconds(1f);
                ChooseRandomSpell();

                while (IsEnemyAttacking)
                {
                    yield return new WaitForSeconds(0.25f);
                }
            }

            yield break;
        }

        private void ChooseRandomSpell()
        {
            int randomSpellIndex = DetermineEnemySpellThroughAttackPatern();
            StartCoroutine(SpellManager.Instance.RollForAttack(arrayOfEnemySpells[randomSpellIndex].spell));
        }

        private void EnemyIsCurrentlyAttacking()
        {
            if (RoundEventManager.Instance.CurrentTurn == Turn.Enemy)
            {
                IsEnemyAttacking = true;
                return;
            }
        }

        public void EnemyFinishedAttacking()
        {
            if (RoundEventManager.Instance.CurrentTurn == Turn.Enemy)
            {
                IsEnemyAttacking = false;
                return;
            }
        }

        private int DetermineEnemySpellThroughAttackPatern()
        {
            // These are the current checks that are being made:
            //      {1} - [DONE] Can't use spells that are on cooldown.
            //      {2} - [DONE] Use spells that meet the AP requirements.
            //      {3} - [DONE] Use Heal when less than 50% HP and not on cooldown; Don't use Heal when greater than 75% HP.
            //      {4} - [DONE] If use one of buff spells (Resilience or Foray Stance), can't use other buff skill.
            //      {5} - [DONE] If Foray Stance is off cooldown, use it first, then use either one of the 1 AP attack spells (Sword of Ret'ul or Ice Shards).
            //      {6} - [DONE] Use Recharge whenever it's off cooldown.
            //      {7} - [DONE] Use Foray Stance right after using Recharge, if Foray Stance is off cooldown.
            //      {8} - Prioritize spells that utilize highest attribute bonus.

            int spellIndex = 3;
            int numberOfSpellConditionChecks = 0;
            bool hasSpellMetConditions = false;

            // *** CAUTION!!!! ***
            // IF YOU'RE GOING TO FUCK WITH A WHILE LOOP, SAVE THE PROJECT BEFORE RUNNING IT OR ELSE THIS BITCH MIGHT GO INTO AN INFINITE LOOP FOR THE nth TIME!!!
            // *** CAUTION!!! ***
            while (!hasSpellMetConditions)
            {
                spellIndex = Random.Range(0, arrayOfEnemySpells.Length);

                //If too many checks have been made and hasn't met spell conditions, it's going to resort to Resilience, regardless whether it's on cooldown or not.
                //This is so the game doesn't stagger too long and waste a good chunk of time checking through the loops. I've also ran into a few infinite loops during debugging,
                //so this is a safeguard. Granted, there have been a few cases where Resilience was used twice in a row; not ideal, but fuck it. Good enough for me.
                if (numberOfSpellConditionChecks == 39)
                {
                    print("Made too many checks and no conditions have been met! Using Resilience...");
                    arrayOfEnemySpells[1].isOnCooldown = true;
                    arrayOfEnemySpells[1].currentCooldown = arrayOfEnemySpells[1].spell.totalCooldown;
                    return 1;
                }

                if (!arrayOfEnemySpells[8].isOnCooldown)
                {
                    print("Recharge is off cooldown! Using Recharge...");
                    arrayOfEnemySpells[8].isOnCooldown = true;
                    arrayOfEnemySpells[8].currentCooldown = arrayOfEnemySpells[8].spell.totalCooldown;
                    return 8;
                }

                if (CharacterManager.Instance.EnemyObject.CurrentActionPoints > 2 && !arrayOfEnemySpells[2].isOnCooldown)
                {
                    print("Used Recharge and Foray Stance is off cooldown! Using Foray Stance...");
                    arrayOfEnemySpells[2].isOnCooldown = true;
                    arrayOfEnemySpells[2].currentCooldown = arrayOfEnemySpells[2].spell.totalCooldown;
                    UsedUtilitySpellThisTurn = true;
                    return 2;
                }
                //This takes care of check #4 (can't use Foray Stance if used Resilience, and vice versa.)
                if (arrayOfEnemySpells[spellIndex].spell.typeOfSpell == SpellType.Utility && UsedUtilitySpellThisTurn)
                {
                    print("Foray Stance or Resilience has already been used this turn! Changing spells...");
                    hasSpellMetConditions = false;
                    numberOfSpellConditionChecks++;
                }

                //This takes care of the first part of check #3 (use Heal when less than 50% HP.)
                else if (CharacterManager.Instance.EnemyObject.CurrentHealthPoints <= CharacterManager.Instance.EnemyObject.MaxHealthPoints / 2 && !arrayOfEnemySpells[0].isOnCooldown)
                {
                    print("Less than 50% HP! Using Heal spell...");
                    spellIndex = 0;
                    hasSpellMetConditions = true;
                    arrayOfEnemySpells[0].isOnCooldown = true;
                    arrayOfEnemySpells[0].currentCooldown = arrayOfEnemySpells[1].spell.totalCooldown;
                    numberOfSpellConditionChecks++;
                }

                //This takes care of the second part of check #3 (don't use Heal when greater than 75% HP.)
                else if (spellIndex == 0 && CharacterManager.Instance.EnemyObject.CurrentHealthPoints >= CharacterManager.Instance.EnemyObject.MaxHealthPoints * 0.75f)
                {
                    print("Greater than 75% HP and trying to use Heal spell! Changing spells...");
                    hasSpellMetConditions = false;
                    numberOfSpellConditionChecks++;
                }
                else if (spellIndex == 2 && CharacterManager.Instance.EnemyObject.CurrentActionPoints == 1)
                {
                    print("Tried using Foray Stance as the last spell! Changing spells...");
                    hasSpellMetConditions = false;
                    numberOfSpellConditionChecks++;
                }
                else if (CharacterManager.Instance.PlayerObject.IsStunned && spellIndex == 1)
                {
                    print("Tried using Resilience while Player is stunned! Changing spells...");
                    hasSpellMetConditions = false;
                    numberOfSpellConditionChecks++;
                }
                else
                {
                    if (CharacterManager.Instance.EnemyObject.CurrentActionPoints >= arrayOfEnemySpells[spellIndex].spell.actionCost)
                    {
                        if (!arrayOfEnemySpells[spellIndex].isOnCooldown)
                        {
                            if (arrayOfEnemySpells[spellIndex].spell.typeOfSpell == SpellType.Utility && arrayOfEnemySpells[spellIndex].spell.ID != 0)
                            {
                                print("Used Foray Stance or Resilience! Enabling Utility roadblock...");
                                UsedUtilitySpellThisTurn = true;
                            }

                            arrayOfEnemySpells[spellIndex].isOnCooldown = true;
                            arrayOfEnemySpells[spellIndex].currentCooldown = arrayOfEnemySpells[spellIndex].spell.totalCooldown;
                            numberOfSpellConditionChecks++;
                            hasSpellMetConditions = true;
                        }
                        else
                        {
                            print(arrayOfEnemySpells[spellIndex].spell.name + " is on cooldown! Changing spells...");
                            hasSpellMetConditions = false;
                            numberOfSpellConditionChecks++;
                        }
                    }
                    else
                    {
                        print("Don't have enough Action Points to use " + arrayOfEnemySpells[spellIndex].spell.name + "! Changing spells...");
                        hasSpellMetConditions = false;
                        numberOfSpellConditionChecks++;
                    }
                }
            }

            return spellIndex;
        }

        private void DecrementCooldownOnSpells()
        {
            if (RoundEventManager.Instance.CurrentTurn == Turn.Enemy)
            {
                for (int i = 0; i < arrayOfEnemySpells.Length; i++)
                {
                    if (arrayOfEnemySpells[i].isOnCooldown)
                    {
                        arrayOfEnemySpells[i].currentCooldown--;

                        if (arrayOfEnemySpells[i].currentCooldown == 0)
                        {
                            arrayOfEnemySpells[i].isOnCooldown = false;
                        }
                    }
                }
            }
            else
            {
                return;
            }
        }

        #endregion Custom Methods
    }
}