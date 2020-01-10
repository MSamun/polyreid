using UnityEngine;

namespace Polyreid
{
    [CreateAssetMenu(fileName = "New Spell", menuName = "Spell")]
    public class SpellObject : ScriptableObject
    {
        [Header("Spell Basic Information")]
        public int ID;

        public Sprite sprite;
        public new string name;
        public SpellType typeOfSpell;
        public AudioClip spellSFXClip;

        [Header("Dice Information")]
        public DiceType diceToRoll;

        public int numberOfRolls;
        public ModifierBonus modifierBonus;
        public Sprite modifierSprite;

        [Header("Skill Cooldown")]
        public int totalCooldown;

        [Header("Skill Action Cost")]
        public int actionCost;

        [Header("Spell Description")]
        [TextArea(5, 10)]
        public string spellDescription;
    }
}