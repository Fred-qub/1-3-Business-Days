using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

//Enumerator for game states
public enum BattleState { INITIALIZATION, ROUNDSTART, ENEMYACT, RESOLUTION, ROUNDSETUP, WIN, LOSE }
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
    
    public BattleState battleState;
    
    public int currentRound = 1;
    public int currentBeat = 0;
    
    public Action[] actionArray = 
    {
        new Action( "Guard", false, 0, 0,1),
        new Action( "Jab", true, 10, 2,3),
        new Action( "Hook", true, 25, 5,4),
        new Action( "Haymaker", true, 65, 5,6),
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
       
       yield return new WaitForSeconds(2f);
       
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
            ChangeBattleState(BattleState.ENEMYACT);
            EnemyActStart();
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
        
        ChangeBattleState(BattleState.RESOLUTION);
        StartCoroutine(StartResolution());
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
            UIManager.SetEnemyBeatMarker(enemyStartingBeat);
            Debug.Log("BattleManager: EnemyActionSelect: enemy has time to select another action");
            return false;
        }
        else
        {
            Debug.Log("BattleManager: EnemyActionSelect: enemy turn full");
            return true;
        }
    }
    
    IEnumerator StartResolution()
    {
        if (battleState != BattleState.RESOLUTION) { yield break; }
        
        //On every beat on this round and the next
        for (int beat = 0; beat <= (roundLength); beat++)
        {
            UIManager.SetBeat(beat);

            //Decrease whatever statuses the combatants have by 1
            playerCombatant.DecreaseStatus();
            enemyCombatant.DecreaseStatus();
            
            //UIManager.PlayerStatusPanelActive(playerCombatant.combatantStatus != Status.NONE);
            UIManager.SetPlayerStatusText(playerCombatant.combatantStatus.ToString());
            UIManager.SetPlayerStatusCounter(playerCombatant.combatantStatusRemainingDuration);
            
            //UIManager.EnemyStatusPanelActive(enemyCombatant.combatantStatus != Status.NONE);
            UIManager.SetEnemyStatusText(enemyCombatant.combatantStatus.ToString());
            UIManager.SetEnemyStatusCounter(enemyCombatant.combatantStatusRemainingDuration);
            
            //Does most of the resolution
           
            ResolveCombatantStatus(beat);
            

            
            yield return new WaitForSeconds(1f);
        }
    }

    public void ResolveCombatantStatus(int beat)
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

                default:
                    playerAttemptedStatusChange = ResolveActionInstanceStack(beat,
                        actionStacksManager.playerActionInstanceStack, playerCombatant);
                    break;
            }
        }


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

                default:
                    enemyAttemptedStatusChange = ResolveActionInstanceStack(beat,
                        actionStacksManager.enemyActionInstanceStack, enemyCombatant);
                    break;
            }
        }

        if (playerAttemptedStatusChange == Status.NONE) {return;}
        if (enemyAttemptedStatusChange == Status.NONE) {return;}

        //The order of the following if statements dictates the priority of resolving different scenarios within a beat

        if (playerAttemptedStatusChange == Status.GUARDSTARTUP)
        {
            //The player is beginning to prepare to guard, which should always be resolved before actions that can cause counters
            playerCombatant.SetStatusWithDuration(Status.GUARDSTARTUP, actionArray[0].ActionStartupDuration);
        } 
        
        if (playerAttemptedStatusChange == Status.GUARDING)
        {
            //The player is beginning to guard, which should always be resolved before actions that can cause counters
            playerCombatant.SetStatusWithDuration(Status.GUARDING, playerCombatant.initiative);
        }
        
        if (enemyAttemptedStatusChange == Status.GUARDSTARTUP)
        {
            //Same as above player check, resolution order doesn't matter because no interrupt
            enemyCombatant.SetStatusWithDuration(Status.GUARDSTARTUP, actionArray[0].ActionStartupDuration);
        } 
        
        if (enemyAttemptedStatusChange == Status.GUARDING)
        {
            //Same as above player check, resolution order doesn't matter because no interrupt
            enemyCombatant.SetStatusWithDuration(Status.GUARDING, playerCombatant.initiative);
        }

        
        
        //If a combatant is attempting to change status to recovery and is not currently stunned, that means they are resolving an attack on this beat
        
        //If both combatants are attacking each other
        if (playerAttemptedStatusChange == Status.RECOVERY && enemyAttemptedStatusChange ==  Status.RECOVERY
            && playerCombatant.combatantStatus != Status.STUNNED && enemyCombatant.combatantStatus != Status.STUNNED)
        {
            //StartCoroutine(ClashEvent())
        }

        if (playerAttemptedStatusChange == Status.RECOVERY && enemyAttemptedStatusChange != Status.RECOVERY 
            && playerCombatant.combatantStatus != Status.STUNNED)
        {
            //player is attacking enemy without interruption

            switch (playerCombatant.combatantStatus)
            {
                case Status.JABSTARTUP:
                    StartCoroutine(AttackEvent(true, actionArray[1]));
                    playerCombatant.SetStatusWithDuration(Status.RECOVERY, playerCombatant.initiative);
                    break;
                case Status.HOOKSTARTUP:
                    StartCoroutine(AttackEvent(true, actionArray[2]));
                    playerCombatant.SetStatusWithDuration(Status.RECOVERY, playerCombatant.initiative);
                    break;
                case Status.HAYMAKERSTARTUP:
                    StartCoroutine(AttackEvent(true, actionArray[3]));
                    playerCombatant.SetStatusWithDuration(Status.RECOVERY, playerCombatant.initiative);
                    break;
            }
        }
        
        if (playerAttemptedStatusChange != Status.RECOVERY && enemyAttemptedStatusChange == Status.RECOVERY 
                                                           && enemyCombatant.combatantStatus != Status.STUNNED)
        {
            //enemy is attacking player without interruption
            switch (enemyCombatant.combatantStatus)
            {
                case Status.JABSTARTUP:
                    StartCoroutine(AttackEvent(false, actionArray[1]));
                    enemyCombatant.SetStatusWithDuration(Status.RECOVERY, enemyCombatant.initiative);
                    break;
                case Status.HOOKSTARTUP:
                    StartCoroutine(AttackEvent(false, actionArray[2]));
                    enemyCombatant.SetStatusWithDuration(Status.RECOVERY, enemyCombatant.initiative);
                    break;
                case Status.HAYMAKERSTARTUP:
                    StartCoroutine(AttackEvent(false, actionArray[3]));
                    enemyCombatant.SetStatusWithDuration(Status.RECOVERY, enemyCombatant.initiative);
                    break;
            }
        }
        
        //16/08/2026: FINISH SUB-BEAT CASE-BY-CASE DICTATED PRIORITY ACTION RESOLUTION HERE
        
        
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
                        break;
                        
                    case 1:
                        statusType = Status.JABSTARTUP;
                        break;
                        
                    case 2:
                        statusType = Status.HOOKSTARTUP;
                        break;
                        
                    case 3:
                        statusType = Status.HAYMAKERSTARTUP;
                        break;
                        
                    default:
                        statusType = Status.NONE;
                        Debug.Log("BattleManager: ResolveCombatantStatus: ResolveActionInstanceStack: action instance ID was invalid somehow, status set to NONE ");
                        break;
                }
            }
        }
        Debug.Log("ResolveActionInstanceStack: " + combatant + " is attempting to change status to " + statusType);
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
        
        UIManager.SetDialogueContent(attackerCombatant.combatantName + " throws a " + actionName + " at " + targetCombatant.combatantName + "!");
        bool targetIsDead = targetCombatant.OnTakeDamagePlusCheckIfDie(damage);
        
        yield return new WaitForSeconds(2f);
        
        UIManager.SetDialogueContent(targetCombatant.combatantName + " takes " + damage + " points of damage!");
        UIManager.SetEnemyHP(enemyCombatant.currentHP);
        UIManager.SetPlayerHP(playerCombatant.currentHP);
        
        yield return new WaitForSeconds(2f);

        if (targetIsDead) 
        {
            if (playerAttacking)
            {
                ChangeBattleState(BattleState.WIN);
                WinEvent();
            }
            else
            {
                ChangeBattleState(BattleState.LOSE);
                LoseEvent();
            }
        }
    }

    IEnumerator ClashEvent(Action playerAction, Action enemyAction)
    {
        int playerDamage = playerAction.ActionDamage;
        int enemyDamage = enemyAction.ActionDamage;
        
        playerDamage += rng.NextInt(-playerAction.ActionDamageVariance, playerAction.ActionDamageVariance);
        enemyDamage += rng.NextInt(-enemyAction.ActionDamageVariance, enemyAction.ActionDamageVariance);

        int finalRecoilDamage = Mathf.RoundToInt((playerDamage + enemyDamage) / 2f);
        
        Combatant targetCombatant;
        Combatant attackerCombatant;
        
        UIManager.SetDialogueContent("Both combatants try to hit each other at the same time!");
        
        yield return new WaitForSeconds(2f);
        
        UIManager.SetDialogueTitle("CLASH!");
        UIManager.SetDialogueContent("Both combatants take " + finalRecoilDamage + " recoil damage!");
        
        bool playerIsDead = playerCombatant.OnTakeDamagePlusCheckIfDie(finalRecoilDamage);
        bool enemyIsDead = enemyCombatant.OnTakeDamagePlusCheckIfDie(finalRecoilDamage);

        UIManager.SetEnemyHP(enemyCombatant.currentHP);
        UIManager.SetPlayerHP(playerCombatant.currentHP);
        
        yield return new WaitForSeconds(2f);

        if (playerIsDead) 
        {
                ChangeBattleState(BattleState.WIN);
                WinEvent();
        }
        
        if (enemyIsDead)
        {
                ChangeBattleState(BattleState.LOSE);
                LoseEvent();
        }
    }
    
    public void WinEvent()
    {
        if(battleState != BattleState.WIN) return; 
        UIManager.SetDialogueContent(enemyCombatant.combatantName + " has been knocked out! You are the winner!");
    }

    public void LoseEvent()
    {
        if(battleState != BattleState.LOSE) return; 
        UIManager.SetDialogueContent("You were clobbered by " + enemyCombatant.combatantName + "!");
    }
}
