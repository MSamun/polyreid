using System.Collections;
using UnityEngine;

namespace Polyreid
{
    public class AudioManager : MonobehaviourReference
    {
        public static AudioManager Instance { get; private set; }

        private bool hasBackgroundSource;
        private bool hasButtonSource;
        private bool hasRoundEventSource;
        private bool hasSpellSFXSource;

        #region Game Components

        [Header("Background Game Music")]
        [SerializeField] private AudioClip[] backgroundMusicAudioClips = new AudioClip[3];

        [Header("Game SFX Sources")]
        [SerializeField] private AudioSource backgroundMusicSource = null;

        [SerializeField] private AudioSource buttonSFXSource = null;

        [SerializeField] private AudioSource roundEventTextSFXSource = null;
        [SerializeField] private AudioSource spellSFXSource = null;

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

            hasBackgroundSource = CheckForAudioSource(backgroundMusicSource);
            hasButtonSource = CheckForAudioSource(buttonSFXSource);
            hasRoundEventSource = CheckForAudioSource(roundEventTextSFXSource);
            hasSpellSFXSource = CheckForAudioSource(spellSFXSource);
        }

        private void Start()
        {
            if (hasBackgroundSource)
                backgroundMusicSource.playOnAwake = true;

            if (hasButtonSource)
                buttonSFXSource.playOnAwake = false;

            if (hasRoundEventSource)
                roundEventTextSFXSource.playOnAwake = false;

            if (hasSpellSFXSource)
                spellSFXSource.playOnAwake = false;
        }

        public void GrabAudioJsonInformationFromSettingsManager(float json_backgroundVolume, float json_buttonEffectVolume)
        {
            if (hasBackgroundSource)
                backgroundMusicSource.volume = json_backgroundVolume;

            if (hasButtonSource)
                buttonSFXSource.volume = json_buttonEffectVolume;

            if (hasRoundEventSource)
                roundEventTextSFXSource.volume = json_buttonEffectVolume;

            if (hasSpellSFXSource)
                spellSFXSource.volume = json_buttonEffectVolume;

            StartCoroutine(LoopThroughGameBackgroundMusic());
        }

        private IEnumerator LoopThroughGameBackgroundMusic()
        {
            bool isLooping = true;

            int numberOfSongsInArray = 0;

            if (backgroundMusicAudioClips.Length > 2)
                numberOfSongsInArray = 3;
            else
                numberOfSongsInArray = 2;

            while (isLooping)
            {
                for (int i = 0; i < numberOfSongsInArray; i++)
                {
                    if (hasBackgroundSource)
                    {
                        backgroundMusicSource.clip = backgroundMusicAudioClips[i];
                        backgroundMusicSource.Play();
                        yield return new WaitForSeconds(backgroundMusicSource.clip.length);
                    }
                }
            }
        }

        #endregion Initialization

        #region Play Audio

        public void PlayButtonSFX()
        {
            if (hasButtonSource)
                buttonSFXSource.Play();
        }

        public void PlayRoundEventSFX()
        {
            if (hasRoundEventSource)
                roundEventTextSFXSource.Play();
        }

        public void PlaySpellSFX(AudioClip clip)
        {
            if (hasSpellSFXSource)
            {
                spellSFXSource.clip = clip;
                spellSFXSource.Play();
            }
        }

        public IEnumerator PlayAndLoopThroughResultMusic(bool isWon)
        {
            StopCoroutine(LoopThroughGameBackgroundMusic());

            if (hasBackgroundSource)
            {
                backgroundMusicSource.Stop();
                backgroundMusicSource.loop = false;

                // NOTE: Clips in index 3 and 4 are for victory music, while clips in index of 5 and 6 are for defeat music. It'll play the appropriate one.

                backgroundMusicSource.clip = isWon == true ? backgroundMusicAudioClips[3] : backgroundMusicAudioClips[5];
                backgroundMusicSource.Play();

                yield return new WaitForSeconds(backgroundMusicSource.clip.length - 2);

                bool isLooping = true;
                backgroundMusicSource.clip = isWon == true ? backgroundMusicAudioClips[4] : backgroundMusicAudioClips[6];
                backgroundMusicSource.loop = true;
                backgroundMusicSource.Play();

                while (isLooping)
                {
                    yield return new WaitForSeconds(backgroundMusicSource.clip.length);
                }
            }
        }

        #endregion Play Audio

        #region Update Audio Volume

        public void UpdateBackgroundAudio(float volume)
        {
            if (hasBackgroundSource)
                backgroundMusicSource.volume = volume;
        }

        public void UpdateButtonEffectVolume(float volume)
        {
            if (hasButtonSource)
                buttonSFXSource.volume = volume;

            if (hasSpellSFXSource)
                spellSFXSource.volume = volume;

            if (hasRoundEventSource)
                roundEventTextSFXSource.volume = volume;
        }

        #endregion Update Audio Volume

        #region Check For Audio Sources

        private bool CheckForAudioSource(AudioSource audioSource)
        {
            return audioSource != null ? true : false;
        }

        #endregion Check For Audio Sources
    }
}