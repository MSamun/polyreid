using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyreid
{
    public class CharacterBarParticleSystem : MonobehaviourReference
    {
        #region Variables

        private float healthBarFillPercentage;
        private float actionBarFillPercentage;

        private ParticleSystem.ShapeModule healthBarInteriorRadius;
        private ParticleSystem.MainModule healthBarInteriorMain;
        private ParticleSystem.EmissionModule healthBarInteriorEmission;

        private ParticleSystem.ShapeModule actionBarInteriorRadius;
        private ParticleSystem.MainModule actionBarInteriorMain;
        private ParticleSystem.EmissionModule actionBarInteriorEmission;

        #endregion Variables

        #region Game Components

        [Header("---------- HEALTH BAR PARTICLE SYSTEM ----------", order = 0)]
        [Header("Interior Particle Effect", order = 1)]
        [SerializeField] private ParticleSystem healthBarInteriorParticleSystem = null;

        [Header("---------- ACTION BAR PARTICLE SYSTEM ----------", order = 0)]
        [Header("Interior Particle Effect", order = 1)]
        [SerializeField] private ParticleSystem actionBarInteriorParticleSystem = null;

        #endregion Game Components

        #region Initialization

        private void Start()
        {
            healthBarInteriorParticleSystem.Play();
            actionBarInteriorParticleSystem.Play();

            healthBarInteriorRadius = healthBarInteriorParticleSystem.shape;
            healthBarInteriorMain = healthBarInteriorParticleSystem.main;
            healthBarInteriorEmission = healthBarInteriorParticleSystem.emission;

            actionBarInteriorRadius = actionBarInteriorParticleSystem.shape;
            actionBarInteriorMain = actionBarInteriorParticleSystem.main;
            actionBarInteriorEmission = actionBarInteriorParticleSystem.emission;
        }

        #endregion Initialization

        #region Custom Methods

        public void AdjustHealthBarInteriorParticlesBasedOnCurrentHitpoints(int currentHealth, int maxHealth)
        {
            // Adjust the particle effect based off how much HP the Player/Enemy has.
            // Lower HP reduces the radius (the length of the particle effect) and how many particles are emitted, alongside the particle life.
            healthBarFillPercentage = (float)currentHealth / maxHealth;
            healthBarFillPercentage = Mathf.Round(healthBarFillPercentage * 100f) / 100f;

            healthBarInteriorRadius.radius = 0.8f * healthBarFillPercentage;
            healthBarInteriorRadius.radius = Mathf.Clamp(healthBarInteriorRadius.radius, 0, 0.8f);

            healthBarInteriorMain.maxParticles = (int)(20 * healthBarFillPercentage);
            healthBarInteriorMain.maxParticles = Mathf.Clamp(healthBarInteriorMain.maxParticles, 0, 13);

            float healthBarEmissionRate = (int)(40 * healthBarFillPercentage);
            healthBarInteriorEmission.rateOverTime = Mathf.Clamp(healthBarEmissionRate, 0, 26);
        }

        public void AdjustActionBarInteriorParticlesBasedOnCurrentActionPoints(int currentActionPoints, int maxActionPoints)
        {
            // Adjust the particle effect based off how much AP the Player/Enemy has.
            // Lower AP reduces the radius (the length of the particle effect) and how many particles are emitted, alongside the particle life.
            actionBarFillPercentage = (float)currentActionPoints / maxActionPoints;
            actionBarFillPercentage = Mathf.Round(actionBarFillPercentage * 100f) / 100f;

            actionBarInteriorRadius.radius = 0.8f * actionBarFillPercentage;
            actionBarInteriorRadius.radius = Mathf.Clamp(actionBarInteriorRadius.radius, 0, 0.8f);

            actionBarInteriorMain.maxParticles = (int)(20 * actionBarFillPercentage);
            actionBarInteriorMain.maxParticles = Mathf.Clamp(actionBarInteriorMain.maxParticles, 0, 13);

            float actionBarEmissionRate = (int)(40 * actionBarFillPercentage);
            actionBarInteriorEmission.rateOverTime = Mathf.Clamp(actionBarEmissionRate, 0, 26);
        }

        #endregion Custom Methods
    }
}