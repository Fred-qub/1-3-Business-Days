using System;
using UnityEngine;

public enum Status  { NONE, GUARDSTARTUP, JABSTARTUP, HOOKSTARTUP, HAYMAKERSTARTUP, RECOVERY, STUNNED, GUARDING }

public class Combatant : MonoBehaviour
{
    public Status combatantStatus = Status.NONE;
    public int combatantStatusRemainingDuration = 0;
    public string combatantName;
    
    public int maxHP;
    public int currentHP;
    
    public int initiative;

    public bool TakeDamageOrDie(int damage)
    {
        if (combatantStatus == Status.GUARDING) { damage /= 2; }
        
        
        
        
        currentHP -= damage;
        
        return (currentHP <= 0);

    }

    public void SetStatusWithDuration(Status status, int statusDuration)
    {
        combatantStatus = status;
        combatantStatusRemainingDuration = statusDuration;
    }
    
    public void ChangeStatus(Status status)
    {
        combatantStatus = status;
    }

    public void DecreaseStatus()
    {
        if (combatantStatusRemainingDuration > 0)
        {
            combatantStatusRemainingDuration -= 1;
        }
    }
    
    
}
