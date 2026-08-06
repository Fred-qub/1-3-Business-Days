using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    [Header("Round Counter")]
    public TextMeshProUGUI roundCounter;
    
    [Header("Player Battle UI")]
    public TextMeshProUGUI playerName;
    public Slider playerHPBarSlider;
    public GameObject playerstatusPanel;
    public TextMeshProUGUI playerStatusText;
    public TextMeshProUGUI playerStatusCounter;
    
    [Header("Enemy Battle UI")]
    public TextMeshProUGUI enemyName;
    public Slider enemyHPBarSlider;
    public GameObject enemystatusPanel;
    public TextMeshProUGUI enemyStatusText;
    public TextMeshProUGUI enemyStatusCounter;

    [Header("Dialogue Box")]
    public TextMeshProUGUI dialogueTitle;
    public TextMeshProUGUI dialogueContent;
    
    [Header("Action Buttons")]
    public Button guardButton;
    public Button jawButton;
    public Button hookButton;
    public Button haymakerButton;

    [Header("Timeline")]
    public TextMeshProUGUI beatCounter;
    public Slider timelineSlider;
    public Slider playerBeatSlider;
    public Slider enemyBeatSlider;
    public GameObject actionPreviewBackground;
    public GameObject recoveryPreviewBackground;
    public float timelinePreviewBackgroundIncrement = 76.2f;
    
    [Header("OverTimeLine")]
    public TextMeshProUGUI overTimeLineText;
    public Slider overTimeLineSlider;
    public GameObject overTimeLineRecoveryPreviewBackground;
    public float overTimeLineRecoveryPreviewBackgroundWidthIncrement = 32f;
    public float overTimeLineRecoveryPreviewBackgroundXOffsetIncrement = 72f;
 

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetRoundTimer(int roundNumber) { roundCounter.text = ("ROUND " + roundNumber); }
    public void SetDialogueContent(string dialogue) { dialogueContent.text = dialogue; }
    public void SetPlayerName(string name) { playerName.text = name; }
    public void SetPlayerHP(int hp) { playerHPBarSlider.value = hp; }
    public void PlayerStatusPanelActive(bool active) { playerstatusPanel.SetActive(active); }
    
    public void SetEnemyName(string name)
    {
        enemyName.text = name;
    }
}
