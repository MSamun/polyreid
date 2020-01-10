using System;
using UnityEngine;
using UnityEngine.UI;

namespace Polyreid
{
    public enum DiceType { D0, D4, D6, D8, D10, D12, D20 };

    public class DiceManager : MonobehaviourReference
    {
        public static DiceManager Instance { get; private set; }

        #region Variables

        private int numberOfRerolls = 1;

        public event Action OnRerolledStats;

        #endregion Variables

        #region Game Components

        [Header("---------- DICE COMPONENT ----------", order = 0)]
        [Header("Reroll Button", order = 1)]
        [SerializeField] private Button rerollButton = null;

        [SerializeField] private Text rerollButtonText = null;

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

        //Referenced via Reroll Button in Player Attributes panel.
        public void InitiateReroll()
        {
            OnRerolledStats?.Invoke();
            DisableRerollButton();
        }

        public void DisableRerollButton()
        {
            if (rerollButton.interactable != false || numberOfRerolls > 0)
            {
                numberOfRerolls = 0;
                rerollButton.interactable = false;
                rerollButtonText.text = string.Format("{0}/1", numberOfRerolls);
            }
        }

        public int MakeADiceRoll(DiceType dice, int numberOfRolls)
        {
            int total = 0;

            for (int i = 0; i < numberOfRolls; i++)
            {
                total += CalculateDiceValues(dice);
            }

            return total;
        }

        private int CalculateDiceValues(DiceType dice)
        {
            int newTotal = 0;

            if (dice == DiceType.D0)
            {
                return newTotal;
            }

            if (dice == DiceType.D4)
            {
                newTotal = UnityEngine.Random.Range(1, 5);
            }

            if (dice == DiceType.D6)
            {
                newTotal = UnityEngine.Random.Range(1, 7);
            }

            if (dice == DiceType.D8)
            {
                newTotal = UnityEngine.Random.Range(1, 9);
            }

            if (dice == DiceType.D10)
            {
                newTotal = UnityEngine.Random.Range(1, 11);
            }

            if (dice == DiceType.D12)
            {
                newTotal = UnityEngine.Random.Range(1, 13);
            }

            if (dice == DiceType.D20)
            {
                newTotal = UnityEngine.Random.Range(5, 21);
                //newTotal = 19;
            }

            return newTotal;
        }

        #endregion Custom Methods
    }
}