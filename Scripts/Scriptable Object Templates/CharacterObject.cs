using UnityEngine;

namespace Polyreid
{
    [CreateAssetMenu(fileName = "New Character", menuName = "Character")]
    public class CharacterObject : ScriptableObject
    {
        [Header("Name of Character")]
        public new string name = string.Empty;

        public int MaxHealthPoints { get; private set; }
        public int CurrentHealthPoints { get; private set; }
        public int MaxActionPoints { get; private set; }
        public int CurrentActionPoints { get; private set; }

        public int ArmorClassAttribute { get; private set; }
        public bool HasShield { get; private set; }
        public bool HasAddedDexMod { get; private set; }

        public int StrengthAttribute { get; private set; }
        public int StrengthModifier { get; private set; }

        public int DexterityAttribute { get; private set; }
        public int DexterityModifier { get; private set; }

        public int ConstitutionAttribute { get; private set; }
        public int ConstitutionModifier { get; private set; }

        public int IntelligenceAttribute { get; private set; }
        public int IntelligenceModifier { get; private set; }

        public int WisdomAttribute { get; private set; }
        public int WisdomModifier { get; private set; }

        public int CharismaAttribute { get; private set; }
        public int CharismaModifier { get; private set; }

        public bool HasIncreasedDefence { get; set; }
        public bool HasIncreasedAttack { get; set; }
        public bool HasDecreasedAttack { get; set; }
        public bool IsStunned { get; set; }

        #region Initialization

        public void InitializeAllProperties()
        {
            HasIncreasedAttack = false;
            HasDecreasedAttack = false;
            HasIncreasedDefence = false;
            IsStunned = false;

            InitializeAttributeValues();
            InitializeHealthAndActionPointValue();
        }

        private void InitializeAttributeValues()
        {
            StrengthAttribute = RollForAttributeValue();
            StrengthModifier = Mathf.FloorToInt((StrengthAttribute - 10) / 2);

            DexterityAttribute = RollForAttributeValue();
            DexterityModifier = Mathf.FloorToInt((DexterityAttribute - 10) / 2);

            ConstitutionAttribute = RollForAttributeValue();
            ConstitutionModifier = Mathf.FloorToInt((ConstitutionAttribute - 10) / 2);

            IntelligenceAttribute = RollForAttributeValue();
            IntelligenceModifier = Mathf.FloorToInt((IntelligenceAttribute - 10) / 2);

            WisdomAttribute = RollForAttributeValue();
            WisdomModifier = Mathf.FloorToInt((WisdomAttribute - 10) / 2);

            CharismaAttribute = RollForAttributeValue();
            CharismaModifier = Mathf.FloorToInt((CharismaAttribute - 10) / 2);

            InitializeArmorClassValue();
        }

        private void InitializeArmorClassValue()
        {
            int rollCheck = 0;
            ArmorClassAttribute = 0;
            HasAddedDexMod = false;

            rollCheck = DiceManager.Instance.MakeADiceRoll(DiceType.D10, 1);
            HasShield = rollCheck == 2 || rollCheck == 6 || rollCheck == 10 ? true : false;

            //Between 11 and 18.
            ArmorClassAttribute = Random.Range(11, 19);

            if (ArmorClassAttribute >= 11 && ArmorClassAttribute <= 15)
            {
                HasAddedDexMod = true;
                ArmorClassAttribute += DexterityModifier;
            }

            if (HasShield)
            {
                ArmorClassAttribute += 2;
            }
        }

        private void InitializeHealthAndActionPointValue()
        {
            MaxHealthPoints = /*1;*/DiceManager.Instance.MakeADiceRoll(DiceType.D20, 7);
            MaxHealthPoints += ConstitutionModifier * 2;
            CurrentHealthPoints = MaxHealthPoints;

            MaxActionPoints = 2;
            CurrentActionPoints = MaxActionPoints;
        }

        private int RollForAttributeValue()
        {
            int totalValue = 0;
            int lowestNumber = 6;
            int numberOfRolls = 4;

            for (int i = 0; i < numberOfRolls; i++)
            {
                int rolledValue = DiceManager.Instance.MakeADiceRoll(DiceType.D6, 1);

                //Lowest number starts at 6; if the rolledValue is less than the lowestNumber, then lowestNumber now equals the rolledValue. Otherwise, keep lowestNumber.
                lowestNumber = rolledValue < lowestNumber ? rolledValue : lowestNumber;

                totalValue += rolledValue;
            }

            totalValue -= lowestNumber;
            return totalValue;
        }

        #endregion Initialization

        public void LoseHitpoints(int value)
        {
            CurrentHealthPoints -= value;
            if (CurrentHealthPoints < 0)
            {
                CurrentHealthPoints = 0;
            }
        }

        public void GainHitpoints(int value)
        {
            CurrentHealthPoints += value;

            if (CurrentHealthPoints > MaxHealthPoints)
            {
                CurrentHealthPoints = MaxHealthPoints;
            }
        }

        public void LoseOrGainActionPoints(int value, bool isDeduct)
        {
            if (isDeduct)
            {
                CurrentActionPoints -= value;

                if (CurrentActionPoints < 0)
                {
                    CurrentActionPoints = 0;
                }
            }
            else
            {
                CurrentActionPoints += value;
            }
        }

        public void ResetActionPoints()
        {
            CurrentActionPoints = MaxActionPoints;
        }
    }
}