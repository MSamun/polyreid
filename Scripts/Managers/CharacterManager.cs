using System;
using UnityEngine;

namespace Polyreid
{
    public class CharacterManager : MonobehaviourReference
    {
        public static CharacterManager Instance { get; private set; }
        public CharacterObject PlayerObject { get; private set; }
        public CharacterObject EnemyObject { get; private set; }

        public event Action OnBarInfluenced;

        public event Action OnHasResilienceBuff;

        #region Variables

        [Header("Player Buff/Debuff Game Objects")]
        [SerializeField] private GameObject[] playerStatusGameObjects = new GameObject[4];

        [Header("Enemy Buff/Debuff Game Objects")]
        [SerializeField] private GameObject[] enemyStatusGameObjects = new GameObject[4];

        #endregion Variables

        #region Initialization

        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);
            else
                Instance = this;
        }

        public void InitializePlayerObject(CharacterObject obj)
        {
            PlayerObject = obj;
        }

        public void InitializeEnemyObject(CharacterObject obj)
        {
            EnemyObject = obj;
        }

        public int DetermineModifierBonusAmount(ModifierBonus bonus)
        {
            int modBonus = 0;
            CharacterObject currentCharacterTurnObject = RoundEventManager.Instance.CurrentTurn == Turn.Player ? PlayerObject : EnemyObject;

            if (bonus == ModifierBonus.Strength)
            {
                modBonus = currentCharacterTurnObject.StrengthModifier;
                return modBonus;
            }

            if (bonus == ModifierBonus.Dexterity)
            {
                modBonus = currentCharacterTurnObject.DexterityModifier;
                return modBonus;
            }

            if (bonus == ModifierBonus.Constitution)
            {
                modBonus = currentCharacterTurnObject.ConstitutionModifier;
                return modBonus;
            }

            if (bonus == ModifierBonus.Intelligence)
            {
                modBonus = currentCharacterTurnObject.IntelligenceModifier;
                return modBonus;
            }

            if (bonus == ModifierBonus.Wisdom)
            {
                modBonus = currentCharacterTurnObject.WisdomModifier;
                return modBonus;
            }

            if (bonus == ModifierBonus.Charisma)
            {
                modBonus = currentCharacterTurnObject.CharismaModifier;
                return modBonus;
            }
            return modBonus;
        }

        #endregion Initialization

        #region Influence Health/Action Points

        public void InfluenceActionPoints(int value, bool isDeduct)
        {
            if (isDeduct)
            {
                if (RoundEventManager.Instance.CurrentTurn == Turn.Player)
                    PlayerObject.LoseOrGainActionPoints(value, true);
                else
                    EnemyObject.LoseOrGainActionPoints(value, true);
            }
            else
            {
                if (RoundEventManager.Instance.CurrentTurn == Turn.Player)
                    PlayerObject.LoseOrGainActionPoints(value, false);
                else
                    EnemyObject.LoseOrGainActionPoints(value, false);
            }

            OnBarInfluenced?.Invoke();
        }

        /// <summary>
        /// Directly affects Player's or Enemy's hitpoints.
        /// </summary>
        /// <param name="value">Amount of HP to be manipulated.</param>
        /// <param name="isDeduct">Is it meant to deal damage to opponent, or heal self? (deal damage = true; heal self = false;)</param>
        public void InfluenceHealthPoints(int value, bool isDeduct)
        {
            //Offensive Spell: meant to deal damage to opponent.
            if (isDeduct)
            {
                if (RoundEventManager.Instance.CurrentTurn == Turn.Player)
                {
                    EnemyObject.LoseHitpoints(value);
                    GameManager.Instance.CurrentDamageDealtInMatch += value;
                }
                else
                {
                    PlayerObject.LoseHitpoints(value);
                    GameManager.Instance.CurrentDamageTakenInMatch += value;
                }
            }

            //Heal Spell as of now (may include more Spells in the future.)
            else
            {
                if (RoundEventManager.Instance.CurrentTurn == Turn.Player)
                {
                    PlayerObject.GainHitpoints(value);
                    GameManager.Instance.CurrentDamageHealedInMatch += value;
                }
                else
                {
                    EnemyObject.GainHitpoints(value);
                }
            }

            OnBarInfluenced?.Invoke();

            if (PlayerObject.CurrentHealthPoints <= 0)
            {
                GameManager.Instance.PlayerLostTheGame();
                return;
            }

            if (EnemyObject.CurrentHealthPoints <= 0)
            {
                GameManager.Instance.PlayerWonTheGame();
                return;
            }
        }

        #endregion Influence Health/Action Points

        #region Enable/Disable Buffs/Debuffs

        public void CheckIfSwitchTurnsDueToNoActionPoints()
        {
            if (RoundEventManager.Instance.CurrentTurn == Turn.Player && PlayerObject.CurrentActionPoints <= 0)
            {
                RoundEventManager.Instance.StartEnemyTurn();
                return;
            }

            if (RoundEventManager.Instance.CurrentTurn == Turn.Enemy && EnemyObject.CurrentActionPoints <= 0)
            {
                EnemySpellManager.Instance.EnemyFinishedAttacking();
                RoundEventManager.Instance.StartPlayerTurn();
                return;
            }
        }

        public void EnableDebuffAndBuffGameObjects(int index, bool isEnabled)
        {
            if (RoundEventManager.Instance.CurrentTurn == Turn.Player)
            {
                //Player uses Resilience [index of 0] or Foray Stance [index of 1] on themself.
                if (index == 0 || index == 1)
                {
                    playerStatusGameObjects[index].SetActive(isEnabled);

                    switch (index)
                    {
                        case 0:
                        PlayerObject.HasIncreasedDefence = isEnabled;
                        OnHasResilienceBuff?.Invoke();
                        break;

                        case 1:
                        PlayerObject.HasIncreasedAttack = isEnabled;
                        break;
                    }
                }

                //Player uses Cripple [index of 2] or Chain Jail [index of 3] on Enemy.
                if (index == 2 || index == 3)
                {
                    enemyStatusGameObjects[index].SetActive(isEnabled);

                    switch (index)
                    {
                        case 2:
                        EnemyObject.HasDecreasedAttack = isEnabled;
                        break;

                        case 3:
                        EnemyObject.IsStunned = isEnabled;
                        break;
                    }
                }
            }
            else
            {
                //Enemy uses Resilience [index of 0] or Foray Stance [index of 1] on themself.
                if (index == 0 || index == 1)
                {
                    enemyStatusGameObjects[index].SetActive(isEnabled);

                    switch (index)
                    {
                        case 0:
                        EnemyObject.HasIncreasedDefence = isEnabled;
                        OnHasResilienceBuff?.Invoke();
                        break;

                        case 1:
                        EnemyObject.HasIncreasedAttack = isEnabled;
                        break;
                    }
                }

                //Enemy uses Cripple [index of 2] or Chain Jail [index of 3] on Player.
                if (index == 2 || index == 3)
                {
                    playerStatusGameObjects[index].SetActive(isEnabled);

                    switch (index)
                    {
                        case 2:
                        PlayerObject.HasDecreasedAttack = isEnabled;
                        break;

                        case 3:
                        PlayerObject.IsStunned = isEnabled;
                        break;
                    }
                }
            }
        }

        #endregion Enable/Disable Buffs/Debuffs
    }
}