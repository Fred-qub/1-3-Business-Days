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

    public char[] playerActionPreviewArray = new char[20] {'B','B','B','B','B','B','B','B','B','B','B','B','B','B','B','B','B','B','B','B',};
    public int playerActionPreviewRollback = 0;
    
    public int playerStartingBeat;
    public int enemyStartingBeat;
    
    public static int guardStartupDuration = 1;
    public static int jabStartupDuration = 3;
    public static int hookStartupDuration = 4;
    public static int haymakerStartupDuration = 6;
    public static int roundLength = 10;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeBattleState(BattleState.INITIALIZATION);
        StartCoroutine(SpawnCombatants());
        
        
    }

    void ChangeBattleState(BattleState newState)
    {
        battleState = newState;
        UIManager.SetPhaseStatus(battleState);
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
       ChangeBattleState(BattleState.ROUNDSTART);
       StartPlayerActionPreview();
    }
    
    //Starts the action preview
    void StartPlayerActionPreview()
    {
        //Updates the UI
        UIManager.SetDialogueTitle("SELECT AN ACTION:");
        UIManager.SetDialogueContent("Choose an action using the buttons to the right.");
        UIManager.SetPlayerBeatMarker(playerStartingBeat);
        UIManager.PlayerBeatMarkerActive(true);
        UIManager.EnemyBeatMarkerActive(true);
        UIManager.SetEnemyBeatMarker(enemyStartingBeat);
        UIManager.ButtonsActive(true);
        
        UIManager.UpdateRoundPreview(playerActionPreviewArray);
        
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
            playerActionPreviewArray[i] = 'A';
        }
        
        //Adds the recovery beats to the queue
        for (int i = actionEndBeat; i < recoveryEndBeat; i++)
        {
            playerActionPreviewArray[i] = 'R';
        }
        
        //Clears all beats after recovery
        for (int i = recoveryEndBeat; i < playerActionPreviewArray.Length; i++)
        {
            playerActionPreviewArray[i] = 'B';
        }

        //Updates the UI
        UIManager.GoTextActive(recoveryEndBeat > 9);
        UIManager.UpdateRoundPreview(playerActionPreviewArray);
    }

    //Function to clear the whole array
    public void ClearPlayerActionQueueArray()
    {
        for (int i = 0; i < playerActionPreviewArray.Length; i++)
        {
            playerActionPreviewArray[i] = 'B';
        }
    }

    //Called when a button is clicked
    //Moves the player marker forwards and enables the reset button
    //If the end of the round is reached by the selected move, moves onto the next gamestate
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
        
        int turnDuration = moveStartup + playerCombatant.initiative;
        int i = playerStartingBeat + turnDuration;
        if (i < roundLength)
        {
            playerStartingBeat = i;
            playerActionPreviewRollback += turnDuration;
            UIManager.SetPlayerBeatMarker(playerStartingBeat);
            UIManager.ResetButtonActive(true);
        }
        else
        {
            ChangeBattleState(BattleState.ENEMYACT);
            EnemyActStart();
        }
    }

    //Updates the preview on the timeline when buttons are hovered over
    //This is also what is queuing them for later which doesn't seem like a great idea but hey what could go wrong
    public void ActionPreview(int actionNo)
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
        
        UpdatePlayerActionQueue(moveStartup, playerStartingBeat);
        
        
    }

    //Undoes all the actions selected this round
    //I wanted to make it so you could roll them back one at a time but that's actually complicated with how everything's set up
    public void ActionPreviewRollback()
    {
        
        int oldStartingBeat = playerStartingBeat - playerActionPreviewRollback;
        for (int i = oldStartingBeat; i < playerActionPreviewArray.Length; i++)
        {
            playerActionPreviewArray[i] = 'B';
        }

        playerStartingBeat = oldStartingBeat;
        playerActionPreviewRollback = 0;
        UIManager.SetPlayerBeatMarker(playerStartingBeat);
        UIManager.UpdateRoundPreview(playerActionPreviewArray);
        UIManager.ResetButtonActive(false);
        UIManager.GoTextActive(false);
    }

    public void EnemyActStart()
    {
        UIManager.SetDialogueTitle("FEELING LUCKY?");
        UIManager.SetDialogueContent(enemyCombatant.combatantName + " is thinking of the best way to thrash you. If the game is working properly, you shouldn't have time to read this though.");

        UIManager.PlayerBeatMarkerActive(false);
        UIManager.EnemyBeatMarkerActive(false);

        UIManager.ButtonsActive(false);
        UIManager.ResetButtonActive(false);
        UIManager.GoTextActive(false);

        StartCoroutine(AttackEvent(false, 100, "JARONA!"));
    }

    IEnumerator AttackEvent(bool playerAttacking, int damage, string actionName)
    {
        Combatant targetCombatant;
        Combatant attackerCombatant;

        if (playerAttacking)
        {
            targetCombatant = enemyCombatant; 
            attackerCombatant = playerCombatant;
            UIManager.SetDialogueContent("You throw a " + actionName + " at " + targetCombatant.combatantName + "!");
        }
        else
        {
            targetCombatant = playerCombatant; 
            attackerCombatant = enemyCombatant;
            UIManager.SetDialogueContent(attackerCombatant.combatantName + "throws a " + actionName + " at you!");
        }
        
        bool targetIsDead = targetCombatant.TakeDamageOrDie(damage);
        
        yield return new WaitForSeconds(2f);
        
        UIManager.SetDialogueContent(targetCombatant.combatantName + " takes " + damage + " points of damage!");
        UIManager.SetEnemyHP(enemyCombatant.currentHP);
        UIManager.SetPlayerHP(playerCombatant.currentHP);
        
        yield return new WaitForSeconds(2f);

        if (targetIsDead) 
        {
            if (playerAttacking)
            {
                UIManager.SetDialogueContent(targetCombatant.combatantName + " has been knocked out! You are the winner!");
                ChangeBattleState(BattleState.WIN);
            }
            else
            {
                UIManager.SetDialogueContent("You were clobbered by " + attackerCombatant.combatantName + "!");
                ChangeBattleState(BattleState.LOSE);
            }
        }
    }
}
