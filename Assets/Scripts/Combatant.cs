using System;
using UnityEngine;

public enum Status  { NONE, ACTIONSTARTUP, RECOVERY, STUNNED, GUARDING }

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
        currentHP -= damage;
        
        return (currentHP <= 0);

    }

    public void SetStatus(Status status)
    {
        combatantStatus = status;
    }
    
    
}
