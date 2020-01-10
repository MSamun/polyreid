using UnityEngine;
using UnityEngine.UI;

namespace Polyreid
{
    public abstract class Stats : MonobehaviourReference
    {
        private CharacterBarParticleSystem barParticleSystemScript;

        #region Game Components

        [Header("Character Object")]
        [SerializeField] protected CharacterObject characterInfo;

        [SerializeField] protected Text characterNameText = null;

        [Header("Bar Lerp Speed")]
        [Range(0.15f, 0.45f)]
        [SerializeField] private float healthLerpSpeed = 0.25f;

        [Range(0.025f, 0.05f)]
        [SerializeField] private float actionLerpSpeed = 0.035f;

        [Header("---------- HEALTH/ACTION BAR INFO COMPONENTS ----------", order = 0)]
        [Header("Health Bar and Text", order = 1)]
        [SerializeField] private Slider healthBar = null;

        [SerializeField] private Text healthBarText = null;
        [SerializeField] private ParticleSystem healthBarEdgeParticleSystem = null;

        [Header("Action Bar and Text", order = 1)]
        [SerializeField] private Slider actionBar = null;

        [SerializeField] private Text actionBarText = null;
        [SerializeField] private ParticleSystem actionBarEdgeParticleSystem = null;

        [Header("---------- ATTRIBUTES COMPONENTS ----------", order = 0)]
        [Header("Attributes", order = 1)]
        [SerializeField] private Text strengthAttributeText = null;

        [SerializeField] private Text dexterityAttributeText = null;
        [SerializeField] private Text constitutionAttributeText = null;
        [SerializeField] private Text intelligenceAttributeText = null;
        [SerializeField] private Text wisdomAttributeText = null;
        [SerializeField] private Text charismaAttributeText = null;
        [SerializeField] private Text armorClassAttributeText = null;

        #endregion Game Components

        #region Initialization

        protected void InitializeAllTextValues()
        {
            barParticleSystemScript = GetComponent<CharacterBarParticleSystem>();
            characterInfo.InitializeAllProperties();
            InitializeAttributeTextValues();
            InitializeHealthAndActionBars();

            characterNameText.text = string.Format("{0}", characterInfo.name);
        }

        private void InitializeAttributeTextValues()
        {
            strengthAttributeText.text = string.Format("{0} ({1}{2})", characterInfo.StrengthAttribute, characterInfo.StrengthModifier >= 0 ? "+" : string.Empty,
                characterInfo.StrengthModifier);

            dexterityAttributeText.text = string.Format("{0} ({1}{2})", characterInfo.DexterityAttribute, characterInfo.DexterityModifier >= 0 ? "+" : string.Empty,
                characterInfo.DexterityModifier);

            constitutionAttributeText.text = string.Format("{0} ({1}{2})", characterInfo.ConstitutionAttribute, characterInfo.ConstitutionModifier >= 0 ? "+" : string.Empty,
                characterInfo.ConstitutionModifier);

            intelligenceAttributeText.text = string.Format("{0} ({1}{2})", characterInfo.IntelligenceAttribute, characterInfo.IntelligenceModifier >= 0 ? "+" : string.Empty,
                characterInfo.IntelligenceModifier);

            wisdomAttributeText.text = string.Format("{0} ({1}{2})", characterInfo.WisdomAttribute, characterInfo.WisdomModifier >= 0 ? "+" : string.Empty,
                characterInfo.WisdomModifier);

            charismaAttributeText.text = string.Format("{0} ({1}{2})", characterInfo.CharismaAttribute, characterInfo.CharismaModifier >= 0 ? "+" : string.Empty,
                characterInfo.CharismaModifier);

            InitializeArmorClassTextValue();
        }

        protected void InitializeArmorClassTextValue()
        {
            if (characterInfo.HasIncreasedDefence)
            {
                int increasedDefence = (2 + characterInfo.StrengthModifier) < 2 ? 2 : 2 + characterInfo.StrengthModifier;
                armorClassAttributeText.text = string.Format("<color=#FFC800>{0}</color> ({1}<color=#FFC800>+{2}</color>)", characterInfo.ArmorClassAttribute + increasedDefence,
                    characterInfo.ArmorClassAttribute, increasedDefence);
            }
            else
                armorClassAttributeText.text = string.Format("{0}", characterInfo.ArmorClassAttribute);
        }

        private void InitializeHealthAndActionBars()
        {
            healthBar.minValue = 0;
            healthBar.maxValue = characterInfo.MaxHealthPoints;
            healthBar.value = characterInfo.CurrentHealthPoints;
            healthBarText.text = string.Format("{0}/{1}", characterInfo.CurrentHealthPoints, characterInfo.MaxHealthPoints);

            actionBar.minValue = 0;
            actionBar.maxValue = characterInfo.MaxActionPoints;
            actionBar.value = actionBar.maxValue;
            actionBarText.text = string.Format("{0}/{1}", characterInfo.CurrentActionPoints, characterInfo.MaxActionPoints);
        }

        #endregion Initialization

        #region Health Bars

        private void FixedUpdate()
        {
            if (healthBar.value != characterInfo.CurrentHealthPoints)
                UpdateHealthBar();
            else
                healthBarEdgeParticleSystem.Stop();

            if (actionBar.value != characterInfo.CurrentActionPoints)
                UpdateActionBar();
            else
                actionBarEdgeParticleSystem.Stop();
        }

        public void UpdateHealthBar()
        {
            if (!healthBarEdgeParticleSystem.isPlaying)
                healthBarEdgeParticleSystem.Play();

            //Bar shows 60HP, but current Character HP is at 45, so you subtract to match them.
            if (healthBar.value > characterInfo.CurrentHealthPoints)
            {
                healthBar.value -= healthLerpSpeed;
                healthBar.value = Mathf.Clamp(healthBar.value, characterInfo.CurrentHealthPoints, characterInfo.MaxHealthPoints);

                //Sometimes, the health bar lerps to a decimal when it's supposed to an int (goes to 45.256723525 or 44.978436543543 when it's supposed to be 45)
                //This normalizes the value.
                if (healthBar.value >= characterInfo.CurrentHealthPoints + healthLerpSpeed && healthBar.value <= characterInfo.CurrentHealthPoints)
                    healthBar.value = characterInfo.CurrentHealthPoints;
            }
            else
                //Bar shows 45HP, but current Character HP is at 60, so you add to match them.
                healthBar.value += healthLerpSpeed;

            healthBarText.text = string.Format("{0}/{1}", (int)healthBar.value, characterInfo.MaxHealthPoints);
            barParticleSystemScript.AdjustHealthBarInteriorParticlesBasedOnCurrentHitpoints(characterInfo.CurrentHealthPoints, characterInfo.MaxHealthPoints);
        }

        public void UpdateActionBar()
        {
            if (!actionBarEdgeParticleSystem.isPlaying && characterInfo.CurrentActionPoints <= 2)
            {
                actionBarEdgeParticleSystem.Play();
            }

            //Bar shows 2 AP, but current Character AP is at 1, so you subtract to match them.
            if (actionBar.value > characterInfo.CurrentActionPoints)
            {
                actionBar.value -= actionLerpSpeed;
                actionBar.value = Mathf.Clamp(actionBar.value, characterInfo.CurrentActionPoints, characterInfo.MaxActionPoints);
            }
            else
                actionBar.value += actionLerpSpeed;

            barParticleSystemScript.AdjustActionBarInteriorParticlesBasedOnCurrentActionPoints(characterInfo.CurrentActionPoints, characterInfo.MaxActionPoints);
        }

        //Text isn't updated after using Recharge then another spell due to the check being made in Update (L162), so subscribing to an event should do the trick.
        protected void UpdateActionBarText()
        {
            actionBarText.text = string.Format("{0}/{1}", characterInfo.CurrentActionPoints, characterInfo.MaxActionPoints);
        }

        #endregion Health Bars
    }
}