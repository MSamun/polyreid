using UnityEngine;

namespace Polyreid
{
    public class EnemyStats : Stats
    {
        #region Variables

        private int highestAttribute = 0;

        #endregion Variables

        private void Start()
        {
            InitializeAllTextValues();
            CalculateHighestAttribute();
            CharacterManager.Instance.OnBarInfluenced += UpdateActionBarText;
            CharacterManager.Instance.OnHasResilienceBuff += InitializeArmorClassTextValue;
            RoundEventManager.Instance.OnTurnChanged += UpdateActionBarText;
            CharacterManager.Instance.InitializeEnemyObject(characterInfo);
        }

        private void CalculateHighestAttribute()
        {
            highestAttribute = Mathf.Max(characterInfo.StrengthAttribute, characterInfo.DexterityAttribute, characterInfo.ConstitutionAttribute, characterInfo.IntelligenceAttribute,
                characterInfo.WisdomAttribute, characterInfo.CharismaAttribute);

            if (highestAttribute == characterInfo.StrengthAttribute)
            {
                print("Strength is the highest!");
            }
            if (highestAttribute == characterInfo.DexterityAttribute)
            {
                print("Dexterity is the highest!");
            }
            if (highestAttribute == characterInfo.ConstitutionAttribute)
            {
                print("Constitution is the highest!");
            }
            if (highestAttribute == characterInfo.IntelligenceAttribute)
            {
                print("Intelligence is the highest!");
            }
            if (highestAttribute == characterInfo.WisdomAttribute)
            {
                print("Wisdom is the highest!");
            }
            if (highestAttribute == characterInfo.CharismaAttribute)
            {
                print("Charisma is the highest!");
            }
        }

        private void OnDestroy()
        {
            CharacterManager.Instance.OnBarInfluenced -= UpdateActionBarText;
            CharacterManager.Instance.OnHasResilienceBuff -= InitializeArmorClassTextValue;
            RoundEventManager.Instance.OnTurnChanged -= UpdateActionBarText;
        }
    }
}