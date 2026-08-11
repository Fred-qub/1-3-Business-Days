using System;
using UnityEngine;

public class Combatant : MonoBehaviour
{
    //enum status { NONE, ACTIONSTARTUP, RECOVERY, STUNNED, GUARDING }
    public string combatantName;
    
    public int maxHP;
    public int currentHP;
    
    public int initiative;

    public bool TakeDamageOrDie(int damage)
    {
        currentHP -= damage;
        
        return (currentHP <= 0);

    }
    
    
}
