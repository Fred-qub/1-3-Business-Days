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

public class ActionQueueManager : MonoBehaviour
{
    public BattleManager battleManager;
    
    Queue<ActionInstance> playerActionInstanceQueue = new Queue<ActionInstance>();

    public void AddActionInstanceToPlayerQueue(int actionID, int actionInstanceStartBeat)
    {
        if (actionID < 0 || actionID >= battleManager.actionArray.Length)
        {
            Debug.Log("ActionQueueManager: AddActionInstanceToPlayerQueue: actionID is out of range");
            return;
        }
        playerActionInstanceQueue.Enqueue(new ActionInstance(actionID, actionInstanceStartBeat));

       DebugLogPlayerActionInstanceQueue();
    }

    public void ClearPlayerActionInstanceQueue()
    {
        playerActionInstanceQueue.Clear();
        DebugLogPlayerActionInstanceQueue();
    }
    
    public ActionInstance DequeuePlayerActionInstanceQueue()
    {
        ActionInstance actionInstance = playerActionInstanceQueue.Dequeue();

        DebugLogPlayerActionInstanceQueue();
        
        return actionInstance;
    }

    public void DebugLogPlayerActionInstanceQueue()
    {
        Debug.Log("Queue is now: ");
        foreach (ActionInstance actionInstance in playerActionInstanceQueue)
        {
            Debug.Log("Action Instance ID: " + actionInstance.ID + " Action Instance Starting Beat: " + actionInstance.StartBeat);
        }
    }

    private void Start()
    {
        
        
    }
}
