namespace Polyreid
{
    public class PlayerStats : Stats
    {
        private void Start()
        {
            InitializeAllTextValues();
            DiceManager.Instance.OnRerolledStats += RerollAttributes;
            CharacterManager.Instance.OnBarInfluenced += UpdateActionBarText;
            CharacterManager.Instance.OnHasResilienceBuff += InitializeArmorClassTextValue;
            RoundEventManager.Instance.OnTurnChanged += UpdateActionBarText;
            CharacterManager.Instance.InitializePlayerObject(characterInfo);
        }

        private void RerollAttributes()
        {
            InitializeAllTextValues();
        }

        private void OnDestroy()
        {
            DiceManager.Instance.OnRerolledStats -= RerollAttributes;
            CharacterManager.Instance.OnBarInfluenced -= UpdateActionBarText;
            CharacterManager.Instance.OnHasResilienceBuff -= InitializeArmorClassTextValue;
            RoundEventManager.Instance.OnTurnChanged -= UpdateActionBarText;
        }
    }
}