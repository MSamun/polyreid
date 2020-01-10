using UnityEngine;
using UnityEngine.UI;

namespace Polyreid
{
    public class SpellTooltip : MonobehaviourReference
    {
        [Header("Spell Object")]
        [SerializeField] private SpellObject spell = null;

        [Header("Spell Tooltip Basic Info")]
        [SerializeField] private Image spellTooltipIcon = null;

        [SerializeField] private Text spellTooltipNameText = null;
        [SerializeField] private Text spellTooltipCostText = null;
        [SerializeField] private Text spellTooltipCooldownText = null;
        [SerializeField] private Image spellTooltipModifierIcon = null;
        [SerializeField] private Text spellTooltipDescriptionText = null;

        private void Start()
        {
            InitializeSettingsSpellInformation();
        }

        private void InitializeSettingsSpellInformation()
        {
            spellTooltipIcon.sprite = spell.sprite;
            spellTooltipNameText.text = string.Format("{0}", spell.name.ToUpper());

            if (spell.actionCost == 0)
                spellTooltipCostText.text = string.Format("NO COST");
            else
                spellTooltipCostText.text = string.Format("{0} ACTION POINT{1}", spell.actionCost, spell.actionCost > 1 ? "S" : string.Empty);

            spellTooltipCooldownText.text = string.Format("{0} TURN{1}", spell.totalCooldown, spell.totalCooldown > 1 ? "S" : string.Empty);
            spellTooltipModifierIcon.sprite = spell.modifierSprite;
            spellTooltipDescriptionText.text = string.Format("{0}", spell.spellDescription);
        }
    }
}