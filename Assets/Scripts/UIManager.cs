using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    //References
    
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

    [Header("Colours")] 
    public Color playerColour;
    public Color enemyColour;
    public Color stunColour;
 
    private int initiative = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //just for testing, delete later
        initiative = 3;
        SetActionPreview(2,6);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Sets the round timer
    public void SetRoundTimer(int roundNumber) { roundCounter.text = ("ROUND " + roundNumber); }
    
    //Sets the content of the dialogue box
    public void SetDialogueContent(string dialogue) { dialogueContent.text = dialogue; }
    
    //Sets the title of the dialogue box
    public void SetDialogueTitle(string title) { dialogueTitle.text = title; }
    
    //Enables / disables the action buttons
    public void ButtonsActive(bool active)
    {
        guardButton.gameObject.SetActive(active);
        jabButton.gameObject.SetActive(active);
        hookButton.gameObject.SetActive(active);
        haymakerButton.gameObject.SetActive(active);
    }

    //Sets the player's name
    public void SetPlayerName(string pName) { playerName.text = pName; }
    
    //Sets the player's HP bar
    public void SetPlayerHP(int hp) { playerHPBarSlider.value = hp; }
    
    //Enables / disables the player's status countdown panel
    public void PlayerStatusPanelActive(bool active) { playerstatusPanel.SetActive(active); }
    
    //Sets the text of the player's status countdown panel
    public void SetPlayerStatusText(string status) { playerStatusText.text = status; }
    
    //Sets the value of the player's status countdown
    public void SetPlayerStatusCounter(int counter) { playerStatusCounter.text = counter.ToString(); }
    
    
    //Sets the enemy's name
    public void SetEnemyName(string eName) { enemyName.text = eName; }
    
    //Sets the enemy's HP bar
    public void SetEnemyHP(int hp) { enemyHPBarSlider.value = hp; }
    
    //Enables / disables the enemy's status countdown panel
    public void EnemyStatusPanelActive(bool active) { enemystatusPanel.SetActive(active); }
    
    //Sets the text of the enemy's status countdown panel
    public void SetEnemyStatusText(string status) { enemyStatusText.text = status; }
    
    //Sets the value of the enemy's status countdown
    public void SetEnemyStatusCounter(int counter) { enemyStatusCounter.text = counter.ToString(); }

    //Sets the beat counter and beat slider
    public void SetBeat(int beat)
    {
        if (beat > 10 | beat < 0) { beat = 0;}
        beatCounter.text = beat.ToString();
        timelineSlider.value = beat;
    }

    //Enables / disables the player's starting position on the timeline
    public void PlayerBeatMarkerActive(bool active) { playerBeatSlider.gameObject.SetActive(active); }
    
    //Sets the player's starting position on the timeline
    public void SetPlayerBeatMarker(int beat) { playerBeatSlider.value = beat; }
    
    //Enables / disables the enemy's starting position on the timeline
    public void EnemyBeatMarkerActive(bool active) { enemyBeatSlider.gameObject.SetActive(active); }
    
    //Set's the enemy's starting position on the timeline
    public void SetEnemyBeatMarker(int beat) { enemyBeatSlider.value = beat; }
    
    //Enables / disables the action startup preview on the timeline
    public void ActionPreviewActive(bool active) { actionPreviewBackground.SetActive(active); }
    
    //Enables / disables the recovery preview on the timeline
    public void RecoveryPreviewActive(bool active) { recoveryPreviewBackground.SetActive(active); }

    //Sets the action preview with a given duration and starting beat, using the increment defined above
    //Due to this effecting the position of the following recovery preview, the functions to adjust it are called by this one
    //It also checks if the length of the preview exceeds the timeline length and sends it to the overtimeline if it does
    public void SetActionPreview(int duration, int startingBeat)
    {
        if (duration + startingBeat > 10)
        {
           int overTimeDuration = duration + startingBeat - 10;
           duration -= overTimeDuration;
           SetOverTimeLineActionPreview(overTimeDuration);
           RecoveryPreviewActive(false);
           
        }
        else
        {
            RecoveryPreviewActive(true);
            SetRecoveryPreview(initiative, duration, startingBeat);
        }
        
        float width = duration * timelinePreviewBackgroundIncrement;
        float offset = startingBeat * timelinePreviewBackgroundIncrement;


        
        actionPreviewBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(width, -60);
        actionPreviewBackground.GetComponent<RectTransform>().anchoredPosition = new Vector2(offset, 0);
    }

    public void SetRecoveryPreview(int duration, int actionPreviewOffset, int startingBeat)
    {
        if (startingBeat > 9)
        {
            RecoveryPreviewActive(false);
        }
        
        if (duration + startingBeat > 10)
        {
            
            int overTimeRecoveryDuration = duration + startingBeat - 10;
            duration -= overTimeRecoveryDuration;
            SetOverTimeLineRecoveryPreviewDuration(overTimeRecoveryDuration);
     
        }
        
        float width = duration * timelinePreviewBackgroundIncrement;
        float offset = actionPreviewOffset * timelinePreviewBackgroundIncrement;
        
        recoveryPreviewBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 0);
        recoveryPreviewBackground.GetComponent<RectTransform>().anchoredPosition = new Vector2(offset, 0);
    }

    public void OverTimeLineActive(bool active)
    {
        overTimeLineText.gameObject.SetActive(active);
        overTimeLineSlider.gameObject.SetActive(active);
    }

    public void SetOverTimeLineActionPreview(int duration)
    {
        overTimeLineSlider.value = duration;
        SetOverTimeLineRecoveryPreviewOffset(duration);
    }

    public void SetOverTimeLineRecoveryPreviewDuration(int duration)
    {
        float width = duration * overTimeLineRecoveryPreviewBackgroundWidthIncrement;

        
        overTimeLineRecoveryPreviewBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(width, -60);
    }
    
    public void SetOverTimeLineRecoveryPreviewOffset(int startingBeat)
    {
        float offset = startingBeat * overTimeLineRecoveryPreviewBackgroundXOffsetIncrement;
        
        overTimeLineRecoveryPreviewBackground.GetComponent<RectTransform>().anchoredPosition = new Vector2(offset, 0);
    }
    
    //To do for tomorrow: continue setting up UI functions
    
}
