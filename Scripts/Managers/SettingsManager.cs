using UnityEngine;
using UnityEngine.UI;

namespace Polyreid
{
    public class SettingsManager : MonobehaviourReference
    {
        public static SettingsManager Instance { get; private set; }

        public int CurrentQualityLevel { get; private set; }
        public float BackgroundMusicVolume { get; private set; }
        public float ButtonEffectVolume { get; private set; }

        [Header("---------- GRAPHICS COMPONENTS ----------", order = 0)]
        [Header("Quality Buttons", order = 1)]
        [SerializeField] private Button[] qualityButtons = new Button[3];

        [Header("---------- AUDIO COMPONENTS ----------", order = 0)]
        [Header("Music & SFX Sliders", order = 1)]
        [SerializeField] private Slider backgroundMusicSlider = null;

        private Text backgroundMusicSliderText = null;

        [SerializeField] private Slider buttonSFXSlider = null;
        private Text buttonSFXSliderText = null;

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

            backgroundMusicSliderText = backgroundMusicSlider.GetComponentInChildren<Text>(true);
            buttonSFXSliderText = buttonSFXSlider.GetComponentInChildren<Text>(true);
        }

        private void Start()
        {
            string[] qualityNames = QualitySettings.names;

            for (int i = 0; i < qualityButtons.Length; i++)
            {
                qualityButtons[i].GetComponentInChildren<Text>(true).text = qualityNames[i];
            }
        }

        //Reads the data.json file and updates the audio values accordingly.
        public void SetGraphicAndAudioSettingsDataViaJson(GraphicsSaveData graphics_json, AudioSaveData audio_json)
        {
            //Graphics
            CurrentQualityLevel = graphics_json.currentQualityLevel;
            QualitySettings.SetQualityLevel(CurrentQualityLevel);
            AdjustGraphicQualityButtonColours();

            //Audio
            BackgroundMusicVolume = audio_json.backgroundMusicVolume;
            backgroundMusicSlider.value = BackgroundMusicVolume;
            backgroundMusicSliderText.text = string.Format("{0}%", Mathf.RoundToInt(backgroundMusicSlider.value * 100f));

            ButtonEffectVolume = audio_json.buttonSoundEffectVolume;
            buttonSFXSlider.value = ButtonEffectVolume;
            buttonSFXSliderText.text = string.Format("{0}%", Mathf.RoundToInt(buttonSFXSlider.value * 100f));

            AudioManager.Instance.GrabAudioJsonInformationFromSettingsManager(audio_json.backgroundMusicVolume, audio_json.buttonSoundEffectVolume);
        }

        #endregion Initialization

        #region Graphics Settings

        public void OnClickChangeQualitySetting(int qualityLevel)
        {
            QualitySettings.SetQualityLevel(qualityLevel);
            CurrentQualityLevel = qualityLevel;

            AdjustGraphicQualityButtonColours();
        }

        private void AdjustGraphicQualityButtonColours()
        {
            for (int i = 0; i < qualityButtons.Length; i++)
            {
                if (CurrentQualityLevel == i)
                {
                    qualityButtons[i].interactable = false;
                    qualityButtons[i].GetComponentInChildren<Text>(true).color = Color.white;
                }
                else
                {
                    qualityButtons[i].interactable = true;
                    qualityButtons[i].GetComponentInChildren<Text>(true).color = new Color(0.75f, 0.75f, 0.75f, 0.75f);
                }
            }
        }

        #endregion Graphics Settings

        #region Audio Settings

        public void OnSliderChangeUpdateBackgroundAudio()
        {
            BackgroundMusicVolume = backgroundMusicSlider.value;
            backgroundMusicSliderText.text = string.Format("{0}%", Mathf.RoundToInt(backgroundMusicSlider.value * 100f));
            AudioManager.Instance.UpdateBackgroundAudio(BackgroundMusicVolume);
        }

        public void OnSliderChangeUpdateButtonSFXAudio()
        {
            ButtonEffectVolume = buttonSFXSlider.value;
            buttonSFXSliderText.text = string.Format("{0}%", Mathf.RoundToInt(buttonSFXSlider.value * 100f));
            AudioManager.Instance.UpdateButtonEffectVolume(ButtonEffectVolume);
        }

        #endregion Audio Settings
    }
}