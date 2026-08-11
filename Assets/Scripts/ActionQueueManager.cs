using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class ActionQueueManager : MonoBehaviour
{
    public Action[] actionArray = 
    {
        new Action( "Guard", false, 0, 0,1),
        new Action( "Jab", true, 10, 2,3),
        new Action( "Hook", true, 25, 5,4),
        new Action( "Haymaker", true, 65, 5,6)
    };

    Queue<Action> actionQueue = new Queue<Action>();

    public void AddActionToQueue(int actionID)
    {
        actionQueue.Enqueue(actionArray[actionID]);
    }

    private void Start()
    {
        foreach (Action action in actionArray)
        {
            Debug.Log(action.ActionName);
        }
    }
}
