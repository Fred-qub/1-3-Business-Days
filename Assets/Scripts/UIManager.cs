using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//using Debug = System.Diagnostics.Debug;

public class UIManager : MonoBehaviour
{
    //References
    
    [Header("Round Counter")]
    public TextMeshProUGUI roundCounter;
    
    [Header("Phase Status Panel")]
    public TextMeshProUGUI phaseStatusPanel;
    
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
    public Button undoButton;

    [Header("Timeline")]
    public TextMeshProUGUI beatCounter;
    public Slider timelineSlider;
    public Slider playerBeatSlider;
    public Slider enemyBeatSlider;
    public GameObject[] APCells;
    public TextMeshProUGUI goText;
    //public float timelinePreviewBackgroundIncrement = 76.2f;
    
    [Header("OverTimeLine")]
    public TextMeshProUGUI overTimeLineText;
    public GameObject overTimeLine;
    public GameObject[] OAPCells;
    //public float overTimeLineRecoveryPreviewBackgroundWidthIncrement = 36f;
    //public float overTimeLineRecoveryPreviewBackgroundXOffsetIncrement = 72f;

    [Header("Colours")] 
    public Color playerColour;
    public Color enemyColour;
    public Color stunColour;
    public Color recoveryColour;
    public Color guardColour;
    public Color blankColour;
    public Color errorColour;
 
    //private int initiative = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Sets the round timer
    public void SetRoundTimer(int roundNumber) { roundCounter.text = ("ROUND " + roundNumber); }
    
    //Sets the phase status
    public void SetPhaseStatus(BattleState battleState)
    {
        phaseStatusPanel.text = ("BATTLE STATE: " + battleState);
    }
    
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

    public void UndoButtonActive(bool active)
    {
        undoButton.gameObject.SetActive(active);
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
    
    //Sets the color of the player's status countdown panel text
    public void SetPlayerStatusTextColor(Status status)
    {
        switch (status)
        {
            case Status.NONE:
                playerStatusText.color = blankColour;
                break;
            case Status.RECOVERY:
                playerStatusText.color = recoveryColour;
                break;
            case Status.STUNNED:
                playerStatusText.color = stunColour;
                break;
            case Status.GUARDING:
                playerStatusText.color = guardColour;
                break;
            default:
                playerStatusText.color = playerColour;
                break;
        };
    }
    
    
    
    
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
    
    //Sets the color of the enemy's status countdown panel text
    public void SetEnemyStatusTextColor(Status status)
    {
        switch (status)
        {
            case Status.NONE:
                enemyStatusText.color = blankColour;
                break;
            case Status.RECOVERY:
                enemyStatusText.color = recoveryColour;
                break;
            case Status.STUNNED:
                enemyStatusText.color = stunColour;
                break;
            case Status.GUARDING:
                enemyStatusText.color = guardColour;
                break;
            default:
                enemyStatusText.color = enemyColour;
                break;
        };
    }

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

    //Sets the colour of a single action preview cell in either the main timeline or overtimeline
    public void SetAPCell(int cell, Color colour)
    {
        if (cell < 10)
        {
            APCells[cell].GetComponent<Image>().color = colour;  
        }
        else
        {
            OAPCells[cell - 10].GetComponent<Image>().color = colour;
        }
    }
    
    //Enables / disables the go text
    public void GoTextActive(bool active) { goText.gameObject.SetActive(active); }
    
    //updates the timeline preview using a char array
    //different characters in the string correspond to different events
    //see switch case block for explanations via colour names
    public void UpdateRoundPreview(char[] previewCodeString)
    {
        if (previewCodeString.Length != 20)
        {
            Debug.Log("Preview code array length is " + previewCodeString.Length + "the array has to be length 20");
            return;
        }
        
        for (int i = 0; i < previewCodeString.Length; i++)
        {
          
            
            switch (previewCodeString[i])
            {
                case 'A':
                    SetAPCell(i, playerColour);
                    break;
                
                case 'S':
                    SetAPCell(i, stunColour);
                    break;
                
                case 'R':
                    SetAPCell(i,recoveryColour);
                    break;
                
                case 'B':
                    SetAPCell(i,blankColour);
                    break;
                
                case 'G':
                    SetAPCell(i, guardColour);
                    break;
                
                default:
                    Debug.Log(previewCodeString[i] + " is not a valid character");
                    SetAPCell(i,errorColour);
                    break;
            }
        }
    }
    
    /*
    
    So as it turns out none of this stuff below here is an effective solution because the timeline needs to be able
    to display information wayyyyyy more flexibly than what offsetting and increasing the width of a couple of boxes
    will allow.
    
    I'm going to replace it with an array of 20 grid cells which can independently change colour to display startup
    and recovery periods for this round and the next, including information left over from the previous round
    
    I'm leaving all this commented out code here so I can remember how my previous solution worked
    
    
    
    
    
    
     
    
    //Enables / disables the action startup preview on the timeline
    public void ActionPreviewActive(bool active) { actionPreviewBackground.SetActive(active); }
    
    //Enables / disables the recovery preview on the timeline
    public void RecoveryPreviewActive(bool active) { recoveryPreviewBackground.SetActive(active); }

    //Sets the action preview with a given duration and starting beat, using the increment defined above, on the timeline
    public void SetActionPreview(int duration, int startingBeat)
    {
        ActionPreviewActive(true);
        
        //if the startup duration exceeds the remainder of the round, shortens the preview and sends the remainder to overtimeline
        //also calls other functions to enable / disable the recovery preview and set the recovery preview
        if (duration + startingBeat > 10)
        {
           int overTimeDuration = duration + startingBeat - 10;
           duration -= overTimeDuration;
           SetOverTimeLineActionPreview(initiative, overTimeDuration);
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

    //sets the recovery preview on the timeline
    public void SetRecoveryPreview(int recoveryDuration, int actionStartupDuration, int actionStartingBeat)
    {
        //if the action is starting later than 9 it means there wouldn't be any room for the preview
        if (actionStartingBeat > 9)
        {
            RecoveryPreviewActive(false);
        }
        
        //if the recovery preview exceeds the remainder of the round, shortens it and sends the remainder to overtimeline 
        if (recoveryDuration + actionStartingBeat + actionStartupDuration > 10)
        {
            int overTimeRecoveryDuration = recoveryDuration + actionStartingBeat + actionStartupDuration - 10;
            recoveryDuration -= overTimeRecoveryDuration;
            SetOverTimeLineRecoveryPreview(0,overTimeRecoveryDuration);
        }
        //sets the width and offset of the preview
        float width = recoveryDuration * timelinePreviewBackgroundIncrement;
        float offset = actionStartupDuration * timelinePreviewBackgroundIncrement;
        recoveryPreviewBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 0);
        recoveryPreviewBackground.GetComponent<RectTransform>().anchoredPosition = new Vector2(offset, 0);
    }
    //enables / disables the overtimeline
    public void OverTimeLineActive(bool active)
    {
        overTimeLineText.gameObject.SetActive(active);
        overTimeLineSlider.gameObject.SetActive(active);
    }
    //sets the action preview on the overtimeline
    public void SetOverTimeLineActionPreview(int recoveryDuration, int duration)
    {
        OverTimeLineActive(true);
        overTimeLineSlider.value = duration;
        SetOverTimeLineRecoveryPreview(duration, recoveryDuration);
    }
    //sets the recovery preview on the overtimeline
    public void SetOverTimeLineRecoveryPreview(int startingBeat, int recoveryDuration)
    {
        OverTimeLineActive(true);
        
        if (startingBeat + recoveryDuration > 10)
        {
            int recoverDurationExcess = startingBeat + recoveryDuration - 10;
            recoveryDuration -= recoverDurationExcess;
        }
        
        float width = recoveryDuration * overTimeLineRecoveryPreviewBackgroundWidthIncrement;
        overTimeLineRecoveryPreviewBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(width, -120);
    }
    
    */
}
