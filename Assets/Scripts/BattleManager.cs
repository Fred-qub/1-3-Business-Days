using System.Collections;
using UnityEngine;

//Enumerator for game states
public enum BattleState { INITIALIZATION, ROUNDSTART, ENEMYACT, RESOLUTION, PLAYBACK, ROUNDSETUP, WIN, LOSE }

public class BattleManager : MonoBehaviour
{
    //References
    public GameObject playerPrefab;
    public GameObject[] enemyPrefabs;

    public Transform playerBattleMarker;
    public Transform enemyBattleMarker;
    
    private Combatant playerCombatant;
    private Combatant enemyCombatant;
    
    public UIManager UIManager;
    
    public BattleState battleState;
    
    public int currentRound = 1;
    public int currentBeat = 0;

    public char[] playerActionQueueArray = new char[20] {'B','B','B','B','B','B','B','B','B','B','B','B','B','B','B','B','B','B','B','B',};

    public int playerStartingBeat;
    public int enemyStartingBeat;
    
    public static int guardStartupDuration = 1;
    public static int jabStartupDuration = 3;
    public static int hookStartupDuration = 4;
    public static int haymakerStartupDuration = 6;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        battleState = BattleState.INITIALIZATION;
        StartCoroutine(SpawnCombatants());
    }

    IEnumerator SpawnCombatants()
    {
       //Uses a prefab to spawn the player, sends info to UI, sets starting beat to initiative  
       GameObject playerGameObject = Instantiate(playerPrefab, playerBattleMarker);
       playerCombatant = playerGameObject.GetComponent<Combatant>();
       UIManager.SetPlayerName(playerCombatant.combatantName);
       playerStartingBeat = playerCombatant.initiative;
       
       //Uses a random prefab to spawn the enemy, sends info to UI, sets starting beat to initiative  
       int randomEnemySelect =  Random.Range(0, enemyPrefabs.Length);
       GameObject enemyGameObject = Instantiate(enemyPrefabs[randomEnemySelect], enemyBattleMarker);
       enemyCombatant = enemyGameObject.GetComponent<Combatant>();
       UIManager.SetEnemyName(enemyCombatant.combatantName);
       enemyStartingBeat = enemyCombatant.initiative;
       
       //Updates the dialogue box
       UIManager.SetDialogueTitle("NEW CHALLENGER:");
       UIManager.SetDialogueContent(enemyCombatant.combatantName + " squares up!");
       
       yield return new WaitForSeconds(3f);
       
       //Moves onto round start
       battleState = BattleState.ROUNDSTART;
       StartPlayerActionQueue();
    }
    
    // 08/08/2026 - action queue system should go in here under ROUNDSTART phase
    // player can queue multiple actions within a round if their initiative is low enough and they pick a fast action
    // make UI buttons call functions in here to queue actions
    // pointer enter inserts it into the queue and updates preview, button select confirms it and moves onto the next action
    // action queue could be split off into another script but it'll probably be useful for resolution so idk
    // queue factors in action startup + recovery
    // once queue exceeds the round length stop accepting new entries
    // update preview on timelines through UIManager every time an action is inserted
    // uses code string from before
    void StartPlayerActionQueue()
    {
        //Updates the UI
        UIManager.SetDialogueTitle("SELECT AN ACTION:");
        UIManager.SetDialogueContent("Choose an action using the buttons to the right.");
        UIManager.SetPlayerBeatMarker(playerStartingBeat);
        UIManager.PlayerBeatMarkerActive(true);
        UIManager.EnemyBeatMarkerActive(true);
        UIManager.SetEnemyBeatMarker(enemyStartingBeat);
        UIManager.ButtonsActive(true);
        
        UIManager.UpdateRoundPreview(playerActionQueueArray);
        
    }

    void UpdatePlayerActionQueue(int actionStartupDuration, int actionStartBeat)
    {
        if(battleState != BattleState.ROUNDSTART) return;
        
        //Calculates when the action will end and when subsequent recovery will begin and end
        int actionEndBeat = actionStartBeat + actionStartupDuration;
        int initiative = playerCombatant.initiative;
        int recoveryEndBeat = actionEndBeat + initiative;
        
        //Adds the startup beats to the queue
        for (int i = actionStartBeat; i < actionEndBeat; i++)
        {
            playerActionQueueArray[i] = 'A';
        }
        
        //Adds the recovery beats to the queue
        for (int i = actionEndBeat; i < recoveryEndBeat; i++)
        {
            playerActionQueueArray[i] = 'R';
        }
        
        //Clears all beats after recovery
        for (int i = recoveryEndBeat; i < playerActionQueueArray.Length; i++)
        {
            playerActionQueueArray[i] = 'B';
        }
        
        UIManager.UpdateRoundPreview(playerActionQueueArray);
    }

    public void ClearPlayerActionQueueArray()
    {
        for (int i = 0; i < playerActionQueueArray.Length; i++)
        {
            playerActionQueueArray[i] = 'B';
        }
    }

    public void ActionSelect(int actionNo)
    {
        if(battleState != BattleState.ROUNDSTART) return; 
        
        int moveStartup = 0;
        
        switch (actionNo)
        {
            case 0:
                moveStartup = guardStartupDuration;
                break;
                
            case 1:
                moveStartup = jabStartupDuration;
                break;
                
            case 2:
                moveStartup = hookStartupDuration;
                break;
                
            case 3:
                moveStartup = haymakerStartupDuration;
                break;
                
            default:
                Debug.Log(actionNo + " is not a valid action number");
                break;
        }
        
        int i = playerStartingBeat + moveStartup + playerCombatant.initiative;
        if (i < 9) { playerStartingBeat = i; }
        UIManager.SetPlayerBeatMarker(playerStartingBeat);
    }

    public void GuardButtonPreview()
    {
       if(battleState != BattleState.ROUNDSTART) return; 
       
       UpdatePlayerActionQueue(guardStartupDuration, playerStartingBeat);
    }
    
    public void JabButtonPreview()
    {
        if(battleState != BattleState.ROUNDSTART) return; 
        UpdatePlayerActionQueue(jabStartupDuration, playerStartingBeat);
    }
    
    public void HookButtonPreview()
    {
        if(battleState != BattleState.ROUNDSTART) return; 
        
        UpdatePlayerActionQueue(hookStartupDuration, playerStartingBeat);
    }
    
    public void HaymakerButtonPreview()
    {
        if(battleState != BattleState.ROUNDSTART) return;
        UpdatePlayerActionQueue(haymakerStartupDuration, playerStartingBeat);
    }
}
