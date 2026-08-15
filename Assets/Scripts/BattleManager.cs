using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

//Enumerator for game states
public enum BattleState { INITIALIZATION, ROUNDSTART, ENEMYACT, RESOLUTION, PLAYBACK, ROUNDSETUP, WIN, LOSE }
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
        StartResolution();
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
    

    public void StartResolution()
    {
        if (battleState != BattleState.RESOLUTION) { return; }
        //Goes through beats 0 through 20
        //On each beat, checks if a combatant starts an action on that beat
        //Creates a list of what events happen on what beat
        //If one combatant's action reaches the end of its startup, it causes either an attack or guard event
        //If it's an attack, and the other combatant is in startup, it causes a stun event targeting the other combatant
        //When stunned, a combatant's next attack won't resolve
        //If the two combatants attack each other at the same time, it causes a clash event
        
        
        
        //On every beat on this round and the next
        for (int currentBeat = 0; currentBeat < (roundLength); currentBeat++)
        {
            //Decrease whatever statuses the combatants have by 1
            playerCombatant.DecreaseStatus();
            enemyCombatant.DecreaseStatus();
            
            //Checks if either of the combatants status counter has hit zero
            //then starts events or sets other statuses accordingly
            //the order of these calls dictates the order of sub-beat resolution
            //in other words, the player will always be prioritized
            //CONSIDER CHANGING HOW ResolveCombatantStatus WORKS SO THAT CLASH EDGE CASE EVENT CAN BE DETECTED
            //maybe have it return something, then use that check if two events happened on the same beat
            //then have another function resolve that accordingly and have THAT call the events
            //that way you can define sub-beat action resolution on a case-by-case basis with respect to specific actions
            //i.e. guarding should always have priority to preserve intended functionality, attacks have the same priority but are handled by clash event edge case

            

            if (playerCombatant.combatantStatusRemainingDuration == 0 | enemyCombatant.combatantStatusRemainingDuration == 0)
            {
                ResolveCombatantStatus(currentBeat);
            }

         
            
            

            //Disables the status panel if the combatant doesn't have a status effect
            UIManager.PlayerStatusPanelActive(playerCombatant.combatantStatus == Status.NONE);
            UIManager.EnemyStatusPanelActive(enemyCombatant.combatantStatus == Status.NONE);
            
        }
    }

        public void ResolveCombatantStatus(int currentBeat)
        {
            Status playerAttemptedStatusChange = Status.NONE;
            Status enemyAttemptedStatusChange = Status.NONE;
            
            switch (playerCombatant.combatantStatus)
            {
                //If it was the startup of a guard, set the status to guarding
                case Status.GUARDSTARTUP:
                    playerAttemptedStatusChange = Status.GUARDING;
                    
                    //Guarding is the same as recovery, but it halves incoming damage
                    //playerCombatant.SetStatusWithDuration(Status.GUARDING, playerCombatant.initiative);
                    break;
                
                //If it was a jab, cause attack event with jab, then go into recovery
                case Status.JABSTARTUP:
                    playerAttemptedStatusChange = Status.RECOVERY;
                    
                    //AttackEvent(true, actionArray[1]);
                    //playerCombatant.SetStatusWithDuration(Status.RECOVERY, playerCombatant.initiative);
                    break;
                
                //If it was a hook, cause attack event with hook, then go into recovery
                case Status.HOOKSTARTUP:
                    playerAttemptedStatusChange = Status.RECOVERY;
                    
                    //AttackEvent(true, actionArray[2]);
                    //playerCombatant.SetStatusWithDuration(Status.RECOVERY, playerCombatant.initiative);
                    break;
                
                //If it was a haymaker, cause attack event with haymaker, then go into recovery
                case Status.HAYMAKERSTARTUP:
                    playerAttemptedStatusChange = Status.RECOVERY;
                    
                    //AttackEvent(true, actionArray[3]);
                    //playerCombatant.SetStatusWithDuration(Status.RECOVERY, playerCombatant.initiative);
                    break;
                
                //If it was the end of a stun, go into recovery
                case Status.STUNNED:
                    playerAttemptedStatusChange = Status.RECOVERY;
                    //playerCombatant.SetStatusWithDuration(Status.RECOVERY, playerCombatant.initiative);
                break;
                
                
                
                //If it was the end of guarding, recovery or none, check if a new action is starting this beat
                default:
                    playerAttemptedStatusChange = ResolveActionInstanceStack(currentBeat, actionStacksManager.playerActionInstanceStack, playerCombatant);
                    break;
            }
            
            switch (enemyCombatant.combatantStatus)
            {
                //If it was the startup of a guard, set the status to guarding
                case Status.GUARDSTARTUP:
                    enemyAttemptedStatusChange = Status.GUARDING;
                    
                    //Guarding is the same as recovery, but it halves incoming damage
                    //playerCombatant.SetStatusWithDuration(Status.GUARDING, playerCombatant.initiative);
                    break;
                
                //If it was a jab, cause attack event with jab, then go into recovery
                case Status.JABSTARTUP:
                   enemyAttemptedStatusChange = Status.RECOVERY;
                    
                    //AttackEvent(true, actionArray[1]);
                    //playerCombatant.SetStatusWithDuration(Status.RECOVERY, playerCombatant.initiative);
                    break;
                
                //If it was a hook, cause attack event with hook, then go into recovery
                case Status.HOOKSTARTUP:
                    enemyAttemptedStatusChange = Status.RECOVERY;
                    
                    //AttackEvent(true, actionArray[2]);
                    //playerCombatant.SetStatusWithDuration(Status.RECOVERY, playerCombatant.initiative);
                    break;
                
                //If it was a haymaker, cause attack event with haymaker, then go into recovery
                case Status.HAYMAKERSTARTUP:
                    enemyAttemptedStatusChange = Status.RECOVERY;
                    
                    //AttackEvent(true, actionArray[3]);
                    //playerCombatant.SetStatusWithDuration(Status.RECOVERY, playerCombatant.initiative);
                    break;
                
                //If it was the end of a stun, go into recovery
                case Status.STUNNED:
                    enemyAttemptedStatusChange = Status.RECOVERY;
                    //playerCombatant.SetStatusWithDuration(Status.RECOVERY, playerCombatant.initiative);
                break;
                
                
                
                //If it was the end of guarding, recovery or none, check if a new action is starting this beat
                default:
                    enemyAttemptedStatusChange = ResolveActionInstanceStack(currentBeat, actionStacksManager.enemyActionInstanceStack, enemyCombatant);
                    break;
            }
            
            
            //The order of the next checks determines the priority of resolution
            

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
            
            //It doesn't matter that the same checks for the opponent are done later here, because they can't counter and interrupt each other
            if (enemyAttemptedStatusChange == Status.GUARDSTARTUP)
            {
                enemyCombatant.SetStatusWithDuration(Status.GUARDSTARTUP, actionArray[0].ActionStartupDuration);
            } 
            
            if (enemyAttemptedStatusChange == Status.GUARDING)
            {
                enemyCombatant.SetStatusWithDuration(Status.GUARDING, playerCombatant.initiative);
            }

            
            
            //If a combatant is attempting to change status to recovery and is not currently stunned, that means they are resolving an attack on this beat
            
            //If both combatants are attacking each other
            if (playerAttemptedStatusChange == Status.RECOVERY && enemyAttemptedStatusChange ==  Status.RECOVERY
                && playerCombatant.combatantStatus != Status.STUNNED && enemyCombatant.combatantStatus != Status.STUNNED)
            {
                //trigger clash
            }

            if (playerAttemptedStatusChange == Status.RECOVERY && enemyAttemptedStatusChange != Status.RECOVERY)
            {
                //player is attacking enemy without interruption

                switch (playerCombatant.combatantStatus)
                {
                    case Status.JABSTARTUP:
                        break;
                    case Status.HOOKSTARTUP:
                        break;
                    case Status.HAYMAKERSTARTUP:
                        break;
                    case Status.STUNNED:
                        break;
                }
            }
            
            if (playerAttemptedStatusChange != Status.RECOVERY && enemyAttemptedStatusChange == Status.RECOVERY)
            {
                //enemy is attacking enemy without interruption
            }
            
            
            
        }
        
        
    //Goes through each combatant's action instance stack
    //If a combatant is starting an action this beat, sets their status accordingly
    public Status ResolveActionInstanceStack(int currentBeat, Stack<ActionInstance> actionInstanceStack, Combatant combatant)
    {
        Status statusType = Status.NONE;
        
        foreach (ActionInstance actionInstance in actionInstanceStack)
        {
            if (actionInstance.StartBeat == currentBeat)
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
        return statusType;
        //combatant.SetStatusWithDuration(statusType, actionArray[actionInstance.ID].ActionStartupDuration);
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
