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
    public Button jabButton;
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
    public void SetDialogueTitle(string title) { dialogueTitle.text = title; }
    public void ButtonsActive(bool active)
    {
        guardButton.gameObject.SetActive(active);
        jabButton.gameObject.SetActive(active);
        hookButton.gameObject.SetActive(active);
        haymakerButton.gameObject.SetActive(active);
    }

    public void SetPlayerName(string pName) { playerName.text = pName; }
    public void SetPlayerHP(int hp) { playerHPBarSlider.value = hp; }
    public void PlayerStatusPanelActive(bool active) { playerstatusPanel.SetActive(active); }
    public void SetPlayerStatusText(string status) { playerStatusText.text = status; }
    public void SetPlayerStatusCounter(int counter) { playerStatusCounter.text = counter.ToString(); }
    
    public void SetEnemyName(string eName) { enemyName.text = eName; }
    public void SetEnemyHP(int hp) { enemyHPBarSlider.value = hp; }
    public void EnemyStatusPanelActive(bool active) { enemystatusPanel.SetActive(active); }
    public void SetEnemyStatusText(string status) { enemyStatusText.text = status; }
    public void SetEnemyStatusCounter(int counter) { enemyStatusCounter.text = counter.ToString(); }

    public void SetBeat(int beat)
    {
        beatCounter.text = beat.ToString();
        timelineSlider.value = beat;
    }

    public void SetPlayerBeatMarker(int beat) { playerBeatSlider.value = beat; }
    public void SetEnemyBeatMarker(int beat) { enemyBeatSlider.value = beat; }
    
    public void ActionPreviewActive(bool active) { actionPreviewBackground.SetActive(active); }

    public void SetActionPreview(int duration)
    {
        float width = duration * timelinePreviewBackgroundIncrement;
        actionPreviewBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(width, -60);
    }
    
    //To do for tomorrow: finish setting up UI functions
    
}
