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
    
    Stack<ActionInstance> playerActionInstanceStack = new Stack<ActionInstance>();

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

    public void ClearPlayerActionInstanceStack()
    {
        playerActionInstanceStack.Clear();
        DebugLogPlayerActionInstanceStack();
    }
    
    public ActionInstance PopPlayerActionInstanceStack()
    {
        if (playerActionInstanceStack.Count == 0) { return null; }
        
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

    private void Start()
    {
        
        
    }
}
