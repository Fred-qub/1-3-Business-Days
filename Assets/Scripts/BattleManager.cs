using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

//Enumerator for game states
public enum BattleState { INITIALIZATION, ROUNDSTART, ENEMYACT, RESOLUTION, ROUNDSETUP, WIN, LOSE, DRAW }
//Class for what an action actually is
public class Action
{
    public string ActionName;
    public bool DoesActionDealDamage;
    public int ActionDamage;
    public int ActionDamageVariance;
    public int ActionStartupDuration;

    public Action(string name, bool dealDamage, int damage, int damageVariance, int startupDuration)
    {
        ActionName = name;
        DoesActionDealDamage = dealDamage;
        ActionDamage = damage;
        ActionDamageVariance = damageVariance;
        ActionStartupDuration = startupDuration;
    }
}

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
    public ActionStacksManager actionStacksManager;
    public AudioManager audioManager;
    
    public BattleState battleState;
    
    public int currentRound = 1;
    public int currentBeat = 0;
    
    public Action[] actionArray = 
    {
        new Action( "Guard", false, 0, 0,1),
        new Action( "Jab", true, 10, 2,2),
        new Action( "Hook", true, 20, 5,3),
        new Action( "Haymaker", true, 40, 10,4),
        new Action( "Blank",false,0,0,0)
    };

    public char[] playerActionPreviewArray = new char[20] {'B','B','B','B','B','B','B','B','B','B','B','B','B','B','B','B','B','B','B','B',};

    
    public int playerStartingBeat;
    public int initialPlayerStartingBeatForAGivenRound;
    
    public int enemyStartingBeat;
    public int initialEnemyStartingBeatForAGivenRound;
    
    public int roundLength = 10;

    public uint seed;
    private Unity.Mathematics.Random rng;
    
    private WaitForSeconds BeatWaitTime = new WaitForSeconds(0.5f);
    private WaitForSeconds EventWaitTime = new WaitForSeconds(1.5f);
    private WaitForSeconds TinyWaitTime = new WaitForSeconds(0.5f);
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        seed = (uint)System.DateTime.Now.Ticks;
        rng = new Unity.Mathematics.Random(seed);
        
        
        ChangeBattleState(BattleState.INITIALIZATION);
        StartCoroutine(SpawnCombatants());
    }

    //Changes the battle state and updates the UI at the same time
    void ChangeBattleState(BattleState newState)
    {
        battleState = newState;
        UIManager.SetPhaseStatus(battleState);
    }

    //Spawns in the combatants and other setup stuff
    IEnumerator SpawnCombatants()
    {
       //Uses a prefab to spawn the player, sends info to UI, sets starting beat to initiative  
       GameObject playerGameObject = Instantiate(playerPrefab, playerBattleMarker);
       playerCombatant = playerGameObject.GetComponent<Combatant>();
       UIManager.SetPlayerName(playerCombatant.combatantName);
       playerStartingBeat = playerCombatant.initiative;
       initialPlayerStartingBeatForAGivenRound = playerStartingBeat;
       
       //Uses a random prefab to spawn the enemy, sends info to UI, sets starting beat to initiative  
       int randomEnemySelect =  rng.NextInt(0, enemyPrefabs.Length);
       GameObject enemyGameObject = Instantiate(enemyPrefabs[randomEnemySelect], enemyBattleMarker);
       enemyCombatant = enemyGameObject.GetComponent<Combatant>();
       UIManager.SetEnemyName(enemyCombatant.combatantName);
       enemyStartingBeat = enemyCombatant.initiative;
       initialEnemyStartingBeatForAGivenRound = enemyStartingBeat;
       
       //Updates the dialogue box
       UIManager.SetDialogueTitle("NEW CHALLENGER:");
       UIManager.SetDialogueContent(enemyCombatant.combatantName + " squares up!");
       
       yield return EventWaitTime;
       
       //Moves onto round start
       ChangeBattleState(BattleState.ENEMYACT);
       EnemyActStart();
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
        UIManager.SetEnemyBeatMarker(initialEnemyStartingBeatForAGivenRound);
        UIManager.ButtonsActive(true);
        UIManager.UpdateRoundPreview(playerActionPreviewArray);
    }

    public void ChangePlayerActionPreviewMidResolution()
    {
        Status playerStatus = playerCombatant.combatantStatus;
        int playerStatusRemainingDuration = playerCombatant.combatantStatusRemainingDuration;

        for (int i = currentBeat; i < currentBeat + playerStatusRemainingDuration; i++)
        {
            if (playerStatus == Status.GUARDING)
            {
                playerActionPreviewArray[i] = 'G';
            } else if (playerStatus == Status.STUNNED)
            {
                playerActionPreviewArray[i] = 'S';
            }
        }

        UIManager.UpdateRoundPreview(playerActionPreviewArray);
    }

    //Updates the player action preview, called by ActionPreview
    void UpdatePlayerActionPreview(int actionStartupDuration, int actionStartBeat)
    {
        if(battleState != BattleState.ROUNDSTART) return;
        
        //Calculates when the action will end and when subsequent recovery will begin and end
        int actionEndBeat = actionStartBeat + actionStartupDuration;
        int initiative = playerCombatant.initiative;
        int recoveryEndBeat = actionEndBeat + initiative;
        
        //Adds the startup beats to the preview
        for (int i = actionStartBeat; i < actionEndBeat; i++)
        {
            playerActionPreviewArray[i] = 'A';
        }
        
        //Adds the recovery beats to the preview
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
        UIManager.GoTextActive(recoveryEndBeat > roundLength - 1);
        UIManager.UpdateRoundPreview(playerActionPreviewArray);
    }

    //Function to clear the whole array
    public void ClearPlayerActionPreviewArray()
    {
        for (int i = 0; i < playerActionPreviewArray.Length; i++)
        {
            playerActionPreviewArray[i] = 'B';
        }
    }
    
    public void ActionSelect(int actionID)
    {
        //Adds the given action to the player's action instance stack using their current starting beat
        actionStacksManager.AddActionInstanceToPlayerStack(actionID, playerStartingBeat);
        
        //Checks if the player can select another action this round
        int turnDuration = actionArray[actionID].ActionStartupDuration + playerCombatant.initiative;
        int i = playerStartingBeat + turnDuration;
        if (i < roundLength)
        {
            //if the player has enough time to pick another action, updates the current starting beat
            playerStartingBeat = i;
       
            UIManager.SetPlayerBeatMarker(playerStartingBeat);
            UIManager.UndoButtonActive(true);
        }
        else
        {
            //If not, moves onto the next battle state
            ChangeBattleState(BattleState.RESOLUTION);
            StartCoroutine(StartResolution());
        }
    }

    //Updates the preview on the timeline when buttons are hovered over
    //This is also what is queuing them for later which doesn't seem like a great idea but hey what could go wrong
    public void ActionPreview(int actionID)
    {
        if(battleState != BattleState.ROUNDSTART) return;
        if (actionID < 0 || actionID >= actionArray.Length)
        {
            Debug.Log(actionID + " is not a valid action ID"); return;
        }
        
        UpdatePlayerActionPreview(actionArray[actionID].ActionStartupDuration, playerStartingBeat);
    }

    //Undoes actions selected this round
    public void ActionPreviewRollback()
    {
        ActionInstance actionInstance = actionStacksManager.PopPlayerActionInstanceStack();

        if (actionInstance.ID == 4)
        {
            Debug.Log("Blank action returned, aborting rollback");
            return;
        }
        
        Action action = actionArray[actionInstance.ID];
        
        for (int i = actionInstance.StartBeat; i < playerActionPreviewArray.Length; i++)
        {
            playerActionPreviewArray[i] = 'B'; 
        }

        playerStartingBeat = actionInstance.StartBeat;

        if (playerStartingBeat == initialPlayerStartingBeatForAGivenRound)
        {
            UIManager.UndoButtonActive(false);
        }
        
        
        UIManager.SetPlayerBeatMarker(playerStartingBeat);
        UIManager.UpdateRoundPreview(playerActionPreviewArray);
        
        UIManager.GoTextActive(false);
    }

    public void EnemyActStart()
    {
        UIManager.SetDialogueTitle("FEELING LUCKY?");
        UIManager.SetDialogueContent(enemyCombatant.combatantName + " is thinking of the best way to thrash you. If the game is working properly, you shouldn't have time to read this though.");
        UIManager.SetEnemyBeatMarker(initialEnemyStartingBeatForAGivenRound);
        UIManager.PlayerBeatMarkerActive(false);
        UIManager.EnemyBeatMarkerActive(false);

        UIManager.ButtonsActive(false);
        UIManager.UndoButtonActive(false);
        UIManager.GoTextActive(false);
        
        int randomActionSelect =  rng.NextInt(0, 3);
        bool enemyTurnFull = false;
        while (!enemyTurnFull)
        {
            enemyTurnFull = EnemyActionSelect(randomActionSelect, enemyStartingBeat);
        }
        
        ChangeBattleState(BattleState.ROUNDSTART);
        StartPlayerActionPreview();
    }

    public bool EnemyActionSelect(int actionID, int startingBeat)
    {
        actionStacksManager.AddActionInstanceToEnemyStack(actionID, startingBeat);
        
        //Checks if the enemy can select another action this round
        int turnDuration = actionArray[actionID].ActionStartupDuration + enemyCombatant.initiative;
        int i = enemyStartingBeat + turnDuration;
        if (i < roundLength)
        {
            //if the enemy has enough time to pick another action, updates the current starting beat
            enemyStartingBeat = i;
            //Debug.Log("BattleManager: EnemyActionSelect: enemy has time to select another action");
            return false;
        }
        else
        {
            //Debug.Log("BattleManager: EnemyActionSelect: enemy turn full");
            return true;
        }
    }

    public void UpdateCombatantStatusUI()
    {
        UIManager.PlayerStatusPanelActive(true);
        UIManager.EnemyStatusPanelActive(true);
        UIManager.SetPlayerStatusCounter(playerCombatant.combatantStatusRemainingDuration);
        switch (playerCombatant.combatantStatus)
        {
            case Status.GUARDSTARTUP:
                UIManager.SetPlayerStatusText("STARTUP - GUARD:");
                break;

            case Status.JABSTARTUP:
                UIManager.SetPlayerStatusText("STARTUP - JAB:");
                break;

            case Status.HOOKSTARTUP:
                UIManager.SetPlayerStatusText("STARTUP - HOOK:");
                break;

            case Status.HAYMAKERSTARTUP:
                UIManager.SetPlayerStatusText("STARTUP - HAYMAKER:");
                break;
            
            case Status.NONE:
                UIManager.PlayerStatusPanelActive(false);
                break;
            
            default:
                UIManager.SetPlayerStatusText(playerCombatant.combatantStatus.ToString());
                break;
        }
        UIManager.SetPlayerStatusTextColor(playerCombatant.combatantStatus);
        
        UIManager.SetEnemyStatusCounter(enemyCombatant.combatantStatusRemainingDuration);
        switch (enemyCombatant.combatantStatus)
        {
            case Status.GUARDSTARTUP:
                UIManager.SetEnemyStatusText("STARTUP - GUARD:");
                break;

            case Status.JABSTARTUP:
                UIManager.SetEnemyStatusText("STARTUP - JAB:");
                break;

            case Status.HOOKSTARTUP:
                UIManager.SetEnemyStatusText("STARTUP - HOOK:");
                break;

            case Status.HAYMAKERSTARTUP:
                UIManager.SetEnemyStatusText("STARTUP - HAYMAKER:");
                break;
            
            case Status.NONE:
                UIManager.EnemyStatusPanelActive(false);
                break;
            
            default:
                UIManager.SetEnemyStatusText(enemyCombatant.combatantStatus + ":");
                break;
        }
        UIManager.SetEnemyStatusTextColor(enemyCombatant.combatantStatus);
    }
    
    IEnumerator StartResolution()
    {
        UIManager.ButtonsActive(false);
        UIManager.UndoButtonActive(false);
        UIManager.PlayerBeatMarkerActive(false);
        UIManager.EnemyBeatMarkerActive(false);
        audioManager.PlaySound(audioManager.bellAudioClip, audioManager.transform, 100);
        
        //On every beat on this round and the next
        for (currentBeat = 1; currentBeat <= (roundLength); currentBeat++)
        {
            if (battleState != BattleState.RESOLUTION) { yield break; }
            UIManager.SetBeat(currentBeat);
            UIManager.SetDialogueTitle("WAITING...");
            UIManager.SetDialogueContent("You could cut the tension in the air with a knife.");

            //Decrease whatever statuses the combatants have by 1
            playerCombatant.DecreaseStatus();
            enemyCombatant.DecreaseStatus();
            
            UpdateCombatantStatusUI();
            
            //Debug.Log("BattleManager: Starting resolution for beat " + currentBeat);
            //Does most of the resolution
            yield return StartCoroutine(ResolveCombatantStatus(currentBeat));
            
            yield return BeatWaitTime;
            if (battleState != BattleState.RESOLUTION) { yield break; }
        }

        if (battleState != BattleState.WIN | battleState != BattleState.LOSE)
        {
            battleState = BattleState.ROUNDSETUP;
            SetupNewRound();
        }
    }

    IEnumerator ResolveCombatantStatus(int beat)
    {
        Status playerAttemptedStatusChange = Status.NONE;
        Status enemyAttemptedStatusChange = Status.NONE;

        if (playerCombatant.combatantStatusRemainingDuration <= 0)
        {
            switch (playerCombatant.combatantStatus)
            {
                case Status.GUARDSTARTUP:
                    playerAttemptedStatusChange = Status.GUARDING;
                    break;

                case Status.JABSTARTUP:
                    playerAttemptedStatusChange = Status.RECOVERY;
                    break;

                case Status.HOOKSTARTUP:
                    playerAttemptedStatusChange = Status.RECOVERY;
                    break;

                case Status.HAYMAKERSTARTUP:
                    playerAttemptedStatusChange = Status.RECOVERY;
                    break;

                case Status.STUNNED:
                    playerAttemptedStatusChange = Status.RECOVERY;
                    break;
                
                case Status.GUARDING:
                    playerAttemptedStatusChange = ResolveActionInstanceStack(beat,
                        actionStacksManager.playerActionInstanceStack, playerCombatant);
                    break;
                
                case Status.RECOVERY:
                    playerAttemptedStatusChange = ResolveActionInstanceStack(beat,
                        actionStacksManager.playerActionInstanceStack, playerCombatant);
                    break;
                
                case Status.NONE:
                    playerAttemptedStatusChange = ResolveActionInstanceStack(beat,
                        actionStacksManager.playerActionInstanceStack, playerCombatant);
                    break;
            }
        }
        Debug.Log("ResolveCombatantStatus: player attempted status change: " + playerAttemptedStatusChange);

        if (enemyCombatant.combatantStatusRemainingDuration <= 0)
        {
            switch (enemyCombatant.combatantStatus)
            {
                case Status.GUARDSTARTUP:
                    enemyAttemptedStatusChange = Status.GUARDING;
                    break;

                case Status.JABSTARTUP:
                    enemyAttemptedStatusChange = Status.RECOVERY;
                    break;

                case Status.HOOKSTARTUP:
                    enemyAttemptedStatusChange = Status.RECOVERY;
                    break;

                case Status.HAYMAKERSTARTUP:
                    enemyAttemptedStatusChange = Status.RECOVERY;
                    break;

                case Status.STUNNED:
                    enemyAttemptedStatusChange = Status.RECOVERY;
                    break;
                
                case Status.GUARDING:
                    enemyAttemptedStatusChange = ResolveActionInstanceStack(beat,
                        actionStacksManager.enemyActionInstanceStack, enemyCombatant);
                    break;

                default:
                    enemyAttemptedStatusChange = ResolveActionInstanceStack(beat,
                        actionStacksManager.enemyActionInstanceStack, enemyCombatant);
                    break;
            }
        }
        Debug.Log("ResolveCombatantStatus: enemy attempted status change: " + enemyAttemptedStatusChange);
        
        //The order of the following if statements dictates the priority of resolving different scenarios within a beat
        
        //If it's beat 10 and a combatant is either guarding or in recovery with 0 duration, set their status to NONE
        if (playerCombatant.combatantStatusRemainingDuration <= 0 
            && playerAttemptedStatusChange == Status.NONE 
            && beat == roundLength 
            && (playerCombatant.combatantStatus == Status.GUARDING | playerCombatant.combatantStatus == Status.RECOVERY))
        {
            playerCombatant.SetStatusWithDuration(Status.NONE, 0);
            UpdateCombatantStatusUI();
        }
        
        

        //If the player is attempting to start preparing to guard
        if (playerAttemptedStatusChange == Status.GUARDSTARTUP)
        {
            playerCombatant.SetStatusWithDuration(Status.GUARDSTARTUP, actionArray[0].ActionStartupDuration);
            UIManager.SetDialogueTitle("BATTLE EVENT:");
            UIManager.SetDialogueContent(playerCombatant.combatantName + " Prepares to guard.");
            audioManager.PlaySound(audioManager.startupAudioClip, this.transform, 100f);
            UpdateCombatantStatusUI();
            yield return EventWaitTime;
        } 
        
        //If the player is beginning to guard
        if (playerAttemptedStatusChange == Status.GUARDING)
        {
            playerCombatant.SetStatusWithDuration(Status.GUARDING, playerCombatant.initiative);
            playerCombatant.SetSprite(2);
            UIManager.SetDialogueTitle("BATTLE EVENT:");
            UIManager.SetDialogueContent(playerCombatant.combatantName + " Puts up their guard!");
            audioManager.PlaySound(audioManager.guardAudioClip, this.transform, 1);
            UpdateCombatantStatusUI();
            ChangePlayerActionPreviewMidResolution();
            yield return EventWaitTime;
        }
        
        //If the enemy is attempting to start preparing to guard
        if (enemyAttemptedStatusChange == Status.GUARDSTARTUP)
        {
            //Same as above player check, resolution order doesn't matter because no interrupt
            enemyCombatant.SetStatusWithDuration(Status.GUARDSTARTUP, actionArray[0].ActionStartupDuration);
            UIManager.SetDialogueTitle("BATTLE EVENT:");
            UIManager.SetDialogueContent(enemyCombatant.combatantName + " Prepares to guard.");
            audioManager.PlaySound(audioManager.startupAudioClip, this.transform, 1);
            UpdateCombatantStatusUI();
            yield return EventWaitTime;
        } 
        
        //If the enemy is beginning to guard
        if (enemyAttemptedStatusChange == Status.GUARDING)
        {
            //Same as above player check, resolution order doesn't matter because no interrupt
            enemyCombatant.SetStatusWithDuration(Status.GUARDING, enemyCombatant.initiative);
            enemyCombatant.SetSprite(2);
            UIManager.SetDialogueTitle("BATTLE EVENT:");
            UIManager.SetDialogueContent(enemyCombatant.combatantName + " Puts up their guard!");
            audioManager.PlaySound(audioManager.guardAudioClip, this.transform, 1);
            UpdateCombatantStatusUI();
            yield return EventWaitTime;
        }
        
        //if player is ending guard
        if (playerAttemptedStatusChange != Status.NONE && playerCombatant.combatantStatus == Status.GUARDING && playerCombatant.combatantStatusRemainingDuration <= 0)
        {
            playerCombatant.SetSprite(0);
            UIManager.SetDialogueTitle("BATTLE EVENT:");
            UIManager.SetDialogueContent(playerCombatant.combatantName + " puts down their guard.");
            UpdateCombatantStatusUI();
            yield return EventWaitTime;
        }
        
        //if enemy is ending guard
        if (enemyAttemptedStatusChange != Status.NONE && enemyCombatant.combatantStatus == Status.GUARDING && enemyCombatant.combatantStatusRemainingDuration <= 0)
        {
            enemyCombatant.SetSprite(0);
            UIManager.SetDialogueTitle("BATTLE EVENT:");
            UIManager.SetDialogueContent(enemyCombatant.combatantName + " puts down their guard.");
            UpdateCombatantStatusUI();
            yield return EventWaitTime;
        }
        
        //if player is attempting to start preparing an attack
        if (playerAttemptedStatusChange == Status.JABSTARTUP | playerAttemptedStatusChange == Status.HOOKSTARTUP |
            playerAttemptedStatusChange == Status.HAYMAKERSTARTUP)
        {
            switch (playerAttemptedStatusChange)
            {
                case Status.JABSTARTUP:
                    playerCombatant.SetStatusWithDuration(Status.JABSTARTUP, actionArray[1].ActionStartupDuration);
                    UIManager.SetDialogueTitle("BATTLE EVENT:");
                    UIManager.SetDialogueContent(playerCombatant.combatantName + " Starts winding up a jab.");
                    audioManager.PlaySound(audioManager.startupAudioClip, this.transform, 1);
                    UpdateCombatantStatusUI();
                    yield return EventWaitTime;
                    break;
                
                case Status.HOOKSTARTUP:
                    playerCombatant.SetStatusWithDuration(Status.HOOKSTARTUP, actionArray[2].ActionStartupDuration);
                    UIManager.SetDialogueTitle("BATTLE EVENT:");
                    UIManager.SetDialogueContent(playerCombatant.combatantName + " Starts winding up a hook.");
                    audioManager.PlaySound(audioManager.startupAudioClip, this.transform, 1);
                    UpdateCombatantStatusUI();
                    yield return EventWaitTime;
                    
                    break;
                case Status.HAYMAKERSTARTUP:
                    playerCombatant.SetStatusWithDuration(Status.HAYMAKERSTARTUP, actionArray[3].ActionStartupDuration);
                    UIManager.SetDialogueTitle("BATTLE EVENT:");
                    UIManager.SetDialogueContent(playerCombatant.combatantName + " Starts winding up a haymaker.");
                    audioManager.PlaySound(audioManager.startupAudioClip, this.transform, 1);
                    UpdateCombatantStatusUI();
                    yield return EventWaitTime;
                    
                    break;
            }
        }
        
        //if enemy is attempting to start preparing an attack
        if (enemyAttemptedStatusChange == Status.JABSTARTUP | enemyAttemptedStatusChange == Status.HOOKSTARTUP |
            enemyAttemptedStatusChange == Status.HAYMAKERSTARTUP)
        {
            switch (enemyAttemptedStatusChange)
            {
                case Status.JABSTARTUP:
                    enemyCombatant.SetStatusWithDuration(Status.JABSTARTUP, actionArray[1].ActionStartupDuration);
                    UIManager.SetDialogueTitle("BATTLE EVENT:");
                    UIManager.SetDialogueContent(enemyCombatant.combatantName + " starts winding up a jab.");
                    audioManager.PlaySound(audioManager.startupAudioClip, this.transform, 1);
                    UpdateCombatantStatusUI();
                    yield return EventWaitTime;
                    
                    break;
                case Status.HOOKSTARTUP:
                    enemyCombatant.SetStatusWithDuration(Status.HOOKSTARTUP, actionArray[2].ActionStartupDuration);
                    UIManager.SetDialogueTitle("BATTLE EVENT:");
                    UIManager.SetDialogueContent(enemyCombatant.combatantName + " starts winding up a hook.");
                    audioManager.PlaySound(audioManager.startupAudioClip, this.transform, 1);
                    UpdateCombatantStatusUI();
                    yield return EventWaitTime;
                    
                    break;
                case Status.HAYMAKERSTARTUP:
                    enemyCombatant.SetStatusWithDuration(Status.HAYMAKERSTARTUP, actionArray[3].ActionStartupDuration);
                    UIManager.SetDialogueTitle("BATTLE EVENT:");
                    UIManager.SetDialogueContent(enemyCombatant.combatantName + " starts winding up a haymaker.");
                    audioManager.PlaySound(audioManager.startupAudioClip, this.transform, 1);
                    UpdateCombatantStatusUI();
                    yield return EventWaitTime;
                    
                    break;
            }
        }

        
        
        //If a combatant is attempting to change status to recovery and is not currently stunned, that means they are resolving an attack on this beat
        //If both combatants are attacking each other
        if (playerAttemptedStatusChange == Status.RECOVERY && enemyAttemptedStatusChange ==  Status.RECOVERY
            && playerCombatant.combatantStatus != Status.STUNNED && enemyCombatant.combatantStatus != Status.STUNNED)
        {
            int playerActionSelect = 4;
            int enemyActionSelect = 4;
            
            switch (playerCombatant.combatantStatus)
            {
                case Status.JABSTARTUP:
                    playerActionSelect = 1;
                    break;
                case Status.HOOKSTARTUP:
                    playerActionSelect = 2;
                    break;
                case Status.HAYMAKERSTARTUP:
                    playerActionSelect = 3;
                    break;
            }
            
            switch (enemyCombatant.combatantStatus)
            {
                case Status.JABSTARTUP:
                    enemyActionSelect = 1;
                    break;
                case Status.HOOKSTARTUP:
                    enemyActionSelect = 2;
                    break;
                case Status.HAYMAKERSTARTUP:
                    enemyActionSelect = 3;
                    break;
            }
            
            yield return StartCoroutine(ClashEvent(actionArray[playerActionSelect],actionArray[enemyActionSelect]));
        }

        //if player is attacking enemy without interruption
        if (playerAttemptedStatusChange == Status.RECOVERY && enemyAttemptedStatusChange != Status.RECOVERY 
            && playerCombatant.combatantStatus != Status.STUNNED)
        {
            switch (playerCombatant.combatantStatus)
            {
                case Status.JABSTARTUP:
                    yield return StartCoroutine(AttackEvent(true, actionArray[1]));
                    playerCombatant.SetStatusWithDuration(Status.RECOVERY, playerCombatant.initiative);
                    break;
                case Status.HOOKSTARTUP:
                    yield return StartCoroutine(AttackEvent(true, actionArray[2]));
                    playerCombatant.SetStatusWithDuration(Status.RECOVERY, playerCombatant.initiative);
                    break;
                case Status.HAYMAKERSTARTUP:
                    yield return StartCoroutine(AttackEvent(true, actionArray[3]));
                    playerCombatant.SetStatusWithDuration(Status.RECOVERY, playerCombatant.initiative);
                    break;
            }
        }
        
        //if enemy is attacking player without interruption
        if (enemyAttemptedStatusChange == Status.RECOVERY && playerAttemptedStatusChange != Status.RECOVERY 
                                                           && enemyCombatant.combatantStatus != Status.STUNNED)
        {
            switch (enemyCombatant.combatantStatus)
            {
                case Status.JABSTARTUP:
                    yield return StartCoroutine(AttackEvent(false, actionArray[1]));
                    enemyCombatant.SetStatusWithDuration(Status.RECOVERY, enemyCombatant.initiative);
                    break;
                case Status.HOOKSTARTUP:
                    yield return StartCoroutine(AttackEvent(false, actionArray[2]));
                    enemyCombatant.SetStatusWithDuration(Status.RECOVERY, enemyCombatant.initiative);
                    break;
                case Status.HAYMAKERSTARTUP:
                    yield return StartCoroutine(AttackEvent(false, actionArray[3]));
                    enemyCombatant.SetStatusWithDuration(Status.RECOVERY, enemyCombatant.initiative);
                    break;
            }
        }

        if (battleState != BattleState.RESOLUTION)
        {
            yield break;
        }
        
        //If the player is recovering from being stunned
        if (playerAttemptedStatusChange == Status.RECOVERY && playerCombatant.combatantStatus == Status.STUNNED)
        {
            playerCombatant.SetStatusWithDuration(Status.RECOVERY, playerCombatant.initiative);
            playerCombatant.SetSprite(0);
            UIManager.SetDialogueTitle("BATTLE EVENT:");
            UIManager.SetDialogueContent(playerCombatant.combatantName + " shrugs off their stun.");
            UpdateCombatantStatusUI();
            yield return EventWaitTime;
        }
        
        //If the enemy is recovering from being stunned
        if (enemyAttemptedStatusChange == Status.RECOVERY && enemyCombatant.combatantStatus == Status.STUNNED)
        {
            enemyCombatant.SetStatusWithDuration(Status.RECOVERY, enemyCombatant.initiative);
            enemyCombatant.SetSprite(0);
            UIManager.SetDialogueTitle("BATTLE EVENT:");
            UIManager.SetDialogueContent(enemyCombatant.combatantName + " shrugs off their stun.");
            UpdateCombatantStatusUI();
            yield return EventWaitTime;
        }
    }
    
    
    //Goes through each combatant's action instance stack
    //If a combatant is starting an action this beat returns that status
    public Status ResolveActionInstanceStack(int beat, Stack<ActionInstance> actionInstanceStack, Combatant combatant)
    {
        Status statusType = Status.NONE;
        
        foreach (ActionInstance actionInstance in actionInstanceStack)
        {
            if (actionInstance.StartBeat == beat)
            {
                switch (actionInstance.ID)
                {
                    case 0:
                        statusType = Status.GUARDSTARTUP;
                        Debug.Log("found action instance, type: " + statusType + " on beat: " + beat);
                        break;
                        
                    case 1:
                        statusType = Status.JABSTARTUP;
                        Debug.Log("found action instance, type: " + statusType + " on beat: " + beat);
                        break;
                        
                    case 2:
                        statusType = Status.HOOKSTARTUP;
                        Debug.Log("found action instance, type: " + statusType + " on beat: " + beat);
                        break;
                        
                    case 3:
                        statusType = Status.HAYMAKERSTARTUP;
                        Debug.Log("found action instance, type: " + statusType + " on beat: " + beat);
                        break;
                        
                    default:
                        statusType = Status.NONE;
                        Debug.Log("BattleManager: ResolveCombatantStatus: ResolveActionInstanceStack: action instance ID was invalid somehow, status set to NONE ");
                        break;
                }
            }
            else
            {
                Debug.Log("no action instance found on beat " + beat);
            }
        }
        Debug.Log(statusType);
        return statusType;
    }
    
    
    IEnumerator AttackEvent(bool playerAttacking, Action action)
    {
        //If the action isn't an attack somehow, stop coroutine
        if (!action.DoesActionDealDamage){ yield break;}
        
        string actionName = action.ActionName;

        int damage = action.ActionDamage;
        
        damage += rng.NextInt(-action.ActionDamageVariance, action.ActionDamageVariance);
        
        
        Combatant targetCombatant;
        Combatant attackerCombatant;

        if (playerAttacking)
        {
            targetCombatant = enemyCombatant; 
            attackerCombatant = playerCombatant;
        }
        else
        {
            targetCombatant = playerCombatant; 
            attackerCombatant = enemyCombatant;
        }
        
        
        UIManager.SetDialogueTitle("BATTLE EVENT:");
        UIManager.SetDialogueContent(attackerCombatant.combatantName + " throws a " + actionName + " at " + targetCombatant.combatantName + "!");
        
        yield return EventWaitTime;
        
        
        audioManager.PlaySound(audioManager.attackAudioClip, this.transform, 1);
        attackerCombatant.SetSprite(1);
        int finalDamage = targetCombatant.OnTakeDamage(damage);
        yield return TinyWaitTime;
        attackerCombatant.SetSprite(0);
        UIManager.SetEnemyHP(enemyCombatant.currentHP);
        UIManager.SetPlayerHP(playerCombatant.currentHP);
        audioManager.PlayRandomSound(audioManager.hurtAudioClips, this.transform, 1);
        
        
        UIManager.SetDialogueContent(targetCombatant.combatantName + " takes " + finalDamage + " points of damage!");
        
        bool targetIsDead = targetCombatant.currentHP <=0;
        
        yield return EventWaitTime;

        if (targetCombatant.combatantStatus == Status.STUNNED)
        {
            UpdateCombatantStatusUI();
            UIManager.SetDialogueTitle("COUNTER!");
            UIManager.SetDialogueContent(targetCombatant.combatantName + " was interrupted and stunned!");
            audioManager.PlaySound(audioManager.stunAudioClip, this.transform, 1);
            ChangePlayerActionPreviewMidResolution();
            yield return EventWaitTime;
        }

        if (targetIsDead) 
        {
            if (playerAttacking)
            {
                UIManager.SetDialogueTitle("VICTORY!");
                UIManager.SetDialogueContent(enemyCombatant.combatantName + " has been knocked out!");
                yield return EventWaitTime;
                
                ChangeBattleState(BattleState.WIN);
                WinEvent();
            }
            else
            {
                UIManager.SetDialogueTitle("DEFEAT!");
                UIManager.SetDialogueContent(playerCombatant.combatantName + " has been knocked out!");
                yield return EventWaitTime;
                
                ChangeBattleState(BattleState.LOSE);
                LoseEvent();
            }
        }
    }

    IEnumerator ClashEvent(Action playerAction, Action enemyAction)
    {
        if (!playerAction.DoesActionDealDamage | !enemyAction.DoesActionDealDamage)
        {
            Debug.Log("BattleManger: ClashEvent: One of the two selected actions wasn't an attack somehow");
            yield break;
        }
        
        int playerDamage = playerAction.ActionDamage;
        int enemyDamage = playerAction.ActionDamage;
        
        playerDamage += rng.NextInt(-playerAction.ActionDamageVariance, playerAction.ActionDamageVariance);
        enemyDamage += rng.NextInt(-enemyAction.ActionDamageVariance, enemyAction.ActionDamageVariance);

        int recoilDamage = Mathf.RoundToInt((playerDamage + enemyDamage) / 2f);
        
        UIManager.SetDialogueTitle("CLASH!");
        UIManager.SetDialogueContent("Both combatants try to hit each other at the same time!");
        
        yield return EventWaitTime;
        
        int finalRecoildamage = playerCombatant.OnTakeDamage(recoilDamage);
        int finalEnemyRecoilDamage = enemyCombatant.OnTakeDamage(recoilDamage);
        
        bool playerIsDead = playerCombatant.currentHP <= 0;
        bool enemyIsDead = enemyCombatant.currentHP <= 0;
        
        
        
        audioManager.PlaySound(audioManager.attackAudioClip, this.transform, 1);
        playerCombatant.SetSprite(1);
        enemyCombatant.SetSprite(1);
        yield return TinyWaitTime;
        audioManager.PlaySound(audioManager.clashAudioClip, this.transform, 1);

        UIManager.SetEnemyHP(enemyCombatant.currentHP);
        audioManager.PlayRandomSound(audioManager.hurtAudioClips, this.transform, 1);
        UIManager.SetPlayerHP(playerCombatant.currentHP);
        audioManager.PlayRandomSound(audioManager.hurtAudioClips, this.transform, 1);
        
        UIManager.SetDialogueContent( playerCombatant.combatantName + " takes " + finalRecoildamage + " recoil damage! " +
                                      enemyCombatant.combatantName + " takes " + finalEnemyRecoilDamage  + " recoil damage!");
        
        yield return EventWaitTime;

        if (enemyIsDead && !playerIsDead) 
        {
            UIManager.SetDialogueTitle("VICTORY!");
            UIManager.SetDialogueContent(enemyCombatant.combatantName + " has been knocked out!");
            
            yield return EventWaitTime;
            ChangeBattleState(BattleState.WIN);
            WinEvent();
        }
        
        if (playerIsDead && !enemyIsDead)
        {
            UIManager.SetDialogueTitle("DEFEAT!");
            UIManager.SetDialogueContent(playerCombatant.combatantName + " has been knocked out!");
            
            yield return EventWaitTime;
            ChangeBattleState(BattleState.LOSE);
            LoseEvent();
        }

        if (playerIsDead && enemyIsDead)
        {
            
        }
    }
    
    public void WinEvent()
    {
        if(battleState != BattleState.WIN) return; 
        UIManager.triggerConclusionScreen(battleState);
        UIManager.SetPhaseStatus(battleState);
        enemyCombatant.SetSprite(3);
    }

    public void LoseEvent()
    {
        if(battleState != BattleState.LOSE) return; 
        UIManager.triggerConclusionScreen(battleState);
        UIManager.SetPhaseStatus(battleState);
        playerCombatant.SetSprite(3);
    }
    
    public void DrawEvent()
    {
        if(battleState != BattleState.DRAW) return; 
        UIManager.triggerConclusionScreen(battleState);
        UIManager.SetPhaseStatus(battleState);
        playerCombatant.SetSprite(3);
        enemyCombatant.SetSprite(3);
    }

    public void SetupNewRound()
    {
        //IMPLEMENT ROUND RESETTING
        //IMPLEMENT WIN / LOSE SCREEN
        //IMPLEMENT MAIN MENU
        //IMPLEMENT TUTORIAL PAGE
        //IMPLEMENT SIMPLE SFX FOR UI ELEMENTS
        //IMPLEMENT SPRITE SWAPPING FOR ATTACKS, GUARDING, STUNNED
        //ACTION PREVIEW TIMELINE LIVE COLOUR CHANGE
        
        if (battleState != BattleState.ROUNDSETUP) { return; }

        playerActionPreviewArray = ShiftPreviewArray();
        
        currentRound += 1;
        currentBeat = 0;

        switch (playerCombatant.combatantStatus)
        {
            case Status.GUARDING:
                initialPlayerStartingBeatForAGivenRound = playerCombatant.combatantStatusRemainingDuration;
                Debug.Log(playerCombatant.combatantStatus + " USED TO DETERMINE NEXT ROUND FOR PLAYER");
                break;
            
            case Status.RECOVERY:
                initialPlayerStartingBeatForAGivenRound = playerCombatant.combatantStatusRemainingDuration;
                Debug.Log(playerCombatant.combatantStatus + " USED TO DETERMINE NEXT ROUND FOR PLAYER");
                break;
            
            case Status.NONE:
                initialPlayerStartingBeatForAGivenRound = playerCombatant.combatantStatusRemainingDuration;
                Debug.Log(playerCombatant.combatantStatus + " USED TO DETERMINE NEXT ROUND FOR PLAYER");
                break;
            
            case Status.GUARDSTARTUP:
                initialPlayerStartingBeatForAGivenRound = playerCombatant.combatantStatusRemainingDuration + playerCombatant.initiative;
                 Debug.Log(playerCombatant.combatantStatus + " USED TO DETERMINE NEXT ROUND FOR PLAYER");
                break;
            
            case Status.JABSTARTUP:
                initialPlayerStartingBeatForAGivenRound = playerCombatant.combatantStatusRemainingDuration + playerCombatant.initiative;
                 Debug.Log(playerCombatant.combatantStatus + " USED TO DETERMINE NEXT ROUND FOR PLAYER");
                break;
            
            case Status.HOOKSTARTUP:
                initialPlayerStartingBeatForAGivenRound = playerCombatant.combatantStatusRemainingDuration + playerCombatant.initiative;
                  Debug.Log(playerCombatant.combatantStatus + " USED TO DETERMINE NEXT ROUND FOR PLAYER");
                break;
            
            case Status.HAYMAKERSTARTUP:
                initialPlayerStartingBeatForAGivenRound = playerCombatant.combatantStatusRemainingDuration + playerCombatant.initiative;
                 Debug.Log(playerCombatant.combatantStatus + " USED TO DETERMINE NEXT ROUND FOR PLAYER");
                break;
            
            case Status.STUNNED:
                initialPlayerStartingBeatForAGivenRound = playerCombatant.combatantStatusRemainingDuration + playerCombatant.initiative;
                Debug.Log(playerCombatant.combatantStatus + " USED TO DETERMINE NEXT ROUND FOR PLAYER");
                break;
        }
        
        switch (enemyCombatant.combatantStatus)
        {
            case Status.GUARDING:
                initialEnemyStartingBeatForAGivenRound = enemyCombatant.combatantStatusRemainingDuration;
                Debug.Log(enemyCombatant.combatantStatus + " USED TO DETERMINE NEXT ROUND FOR ENEMY");
                break;
            
            case Status.RECOVERY:
                initialEnemyStartingBeatForAGivenRound = enemyCombatant.combatantStatusRemainingDuration;
                Debug.Log(enemyCombatant.combatantStatus + " USED TO DETERMINE NEXT ROUND FOR ENEMY");
                break;
            
            case Status.NONE:
                initialEnemyStartingBeatForAGivenRound = enemyCombatant.combatantStatusRemainingDuration; 
                Debug.Log(enemyCombatant.combatantStatus + " USED TO DETERMINE NEXT ROUND FOR ENEMY");
                break;
            
            case Status.GUARDSTARTUP:
                initialEnemyStartingBeatForAGivenRound = enemyCombatant.combatantStatusRemainingDuration + enemyCombatant.initiative;
                Debug.Log(enemyCombatant.combatantStatus + " USED TO DETERMINE NEXT ROUND FOR ENEMY");
                break;
            
            case Status.JABSTARTUP:
                initialEnemyStartingBeatForAGivenRound = enemyCombatant.combatantStatusRemainingDuration + enemyCombatant.initiative;
                Debug.Log(enemyCombatant.combatantStatus + " USED TO DETERMINE NEXT ROUND FOR ENEMY");
                break;
            
            case Status.HOOKSTARTUP:
                initialEnemyStartingBeatForAGivenRound = enemyCombatant.combatantStatusRemainingDuration + enemyCombatant.initiative;
                 Debug.Log(enemyCombatant.combatantStatus + " USED TO DETERMINE NEXT ROUND FOR ENEMY");
                break;
            
            case Status.HAYMAKERSTARTUP:
                initialEnemyStartingBeatForAGivenRound = enemyCombatant.combatantStatusRemainingDuration + enemyCombatant.initiative;
                 Debug.Log(enemyCombatant.combatantStatus + " USED TO DETERMINE NEXT ROUND FOR ENEMY");
                break;
            
            case Status.STUNNED:
                initialEnemyStartingBeatForAGivenRound = enemyCombatant.combatantStatusRemainingDuration + enemyCombatant.initiative;
                 Debug.Log(enemyCombatant.combatantStatus + " USED TO DETERMINE NEXT ROUND FOR ENEMY");
                break;
        }

        if (playerCombatant.combatantStatusRemainingDuration == 0)
        {
            initialPlayerStartingBeatForAGivenRound = 1;
        }

        if (enemyCombatant.combatantStatusRemainingDuration == 0)
        {
            initialEnemyStartingBeatForAGivenRound = 1;
        }
        
        
        playerStartingBeat = initialPlayerStartingBeatForAGivenRound;
        enemyStartingBeat = initialEnemyStartingBeatForAGivenRound;
        
        actionStacksManager.ClearEnemyActionInstanceStack();
        actionStacksManager.ClearPlayerActionInstanceStack();
        
        UIManager.PlayerBeatMarkerActive(true);
        UIManager.EnemyBeatMarkerActive(true);
        UIManager.SetPlayerBeatMarker(playerStartingBeat);
        UIManager.SetEnemyBeatMarker(playerStartingBeat);
        UIManager.SetBeat(currentBeat);
        UIManager.SetRoundTimer(currentRound);
        UIManager.UpdateRoundPreview(playerActionPreviewArray);
        UIManager.SetPhaseStatus(battleState);
        
        battleState = BattleState.ROUNDSTART;

        //goes back to enemy act state
        ChangeBattleState(BattleState.ENEMYACT);
        EnemyActStart();
           
        
    }

    public char[] ShiftPreviewArray()
    {
        char[] tempArray = new char[playerActionPreviewArray.Length];
        Array.Copy(playerActionPreviewArray, 10, tempArray, 0, playerActionPreviewArray.Length / 2);

        for (int i = 10; i < tempArray.Length; i++)
        {
            tempArray[i] = 'B';
        }
        
        return tempArray;
        
    }
}
