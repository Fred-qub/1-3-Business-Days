using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Actions are defined under battlemanager, these are instances of actions queued for resolution
//They store the ID of the action to perform and the beat their startup begins
//Everything else can be calculated back over in battlemanager
public class ActionInstance
{
    public int ID;
    public int StartBeat;

    public ActionInstance(int actionInstanceID, int actionInstanceStartBeat)
    {
        ID = actionInstanceID;
        StartBeat = actionInstanceStartBeat;
    }
}

public class ActionStacksManager : MonoBehaviour
{
    public BattleManager battleManager;
    
    public Stack<ActionInstance> playerActionInstanceStack = new Stack<ActionInstance>();
    public Stack<ActionInstance> enemyActionInstanceStack  = new Stack<ActionInstance>();

    //Adds an action instance to the player's stack
    public void AddActionInstanceToPlayerStack(int actionID, int actionInstanceStartBeat)
    {
        if (actionID < 0 || actionID >= battleManager.actionArray.Length)
        {
            Debug.Log("ActionStacksManager: AddActionInstanceToPlayerStack: actionID is out of range");
            return;
        }
        playerActionInstanceStack.Push(new ActionInstance(actionID, actionInstanceStartBeat));
        
        DebugLogPlayerActionInstanceStack();
    }
    
    //Adds an action instance to the enemy's stack
    public void AddActionInstanceToEnemyStack(int actionID, int actionInstanceStartBeat)
    {
        if (actionID < 0 || actionID >= battleManager.actionArray.Length)
        {
            Debug.Log("ActionStacksManager: AddActionInstanceToEnemyStack: actionID is out of range");
            return;
        }
        enemyActionInstanceStack.Push(new ActionInstance(actionID, actionInstanceStartBeat));

        DebugLogEnemyActionInstanceStack();
    }

    
    public void ClearPlayerActionInstanceStack()
    {
        playerActionInstanceStack.Clear();
        DebugLogPlayerActionInstanceStack();
    }
    
    public void ClearEnemyActionInstanceStack()
    {
        enemyActionInstanceStack.Clear();
        DebugLogEnemyActionInstanceStack();
    }
    
    //Used to remove the player's previously selected action from the stack
    public ActionInstance PopPlayerActionInstanceStack()
    {
        //Returns a blank action instance set to begin on the player's initial starting beat for the round if the stack is empty
        if (playerActionInstanceStack.Count == 0) { return new ActionInstance(4,battleManager.initialPlayerStartingBeatForAGivenRound); }
        
        ActionInstance actionInstance = playerActionInstanceStack.Pop();

        DebugLogPlayerActionInstanceStack();
        
        return actionInstance;
    }

    public void DebugLogPlayerActionInstanceStack()
    {
        Debug.Log("PAIS is now: ");
        foreach (ActionInstance actionInstance in playerActionInstanceStack)
        {
            Debug.Log("Action Instance ID: " + actionInstance.ID + " Action Instance Starting Beat: " + actionInstance.StartBeat);
        }
    }
    
    public void DebugLogEnemyActionInstanceStack()
    {
        Debug.Log("EAIS is now: ");
        foreach (ActionInstance actionInstance in enemyActionInstanceStack)
        {
            Debug.Log("Action Instance ID: " + actionInstance.ID + " Action Instance Starting Beat: " + actionInstance.StartBeat);
        }
    }

    public void StackResolution()
    {
        //Compares the player's action instance stack to the enemy's
        //Does this by going through the actions from beats 0 through 20
        //Creates a list of what events happen on what beat
        //If one combatant's action reaches the end of its startup, it causes either an attack or guard event
        //If it's an attack, and the other combatant is in startup, it causes a stun event targeting the other combatant
        //When stunned, a combatant's next attack won't resolve
        //If the two combatants attack each other at the same time, it causes a clash event
        
        for (int i = 0; i < (battleManager.roundLength * 2); i++)
        {
            foreach (ActionInstance playerActionInstance in playerActionInstanceStack)
            {
                if (playerActionInstance.StartBeat == i)
                {
                    
                }
            }
        }
    }
}
