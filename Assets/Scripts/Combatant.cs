using System;
using Unity.VisualScripting;
using UnityEngine;

public enum Status  { NONE, GUARDSTARTUP, JABSTARTUP, HOOKSTARTUP, HAYMAKERSTARTUP, RECOVERY, STUNNED, GUARDING }

public class Combatant : MonoBehaviour
{
    public Status combatantStatus = Status.NONE;
    public int combatantStatusRemainingDuration = 0;
    public string combatantName;
    
    public SpriteRenderer combatantSpriteRenderer;
    
    public Sprite[] combatantSprites;
    
    public int maxHP;
    public int currentHP;
    
    public int initiative;

    public int OnTakeDamage(int damage)
    {
        
        int tempDamage = damage;
        
        //Changes what happens when a combatant gets hit depending on what status they currently have
        switch (combatantStatus)
                {
                    case Status.NONE:
                        break;
                    
                    
                    case Status.GUARDSTARTUP:
                        ChangeStatus(Status.STUNNED);
                        SetSprite(3);
                        break;
                    
                    
                    case Status.GUARDING:
                        tempDamage /= 2;
                        break;
                    
                   
                    case Status.JABSTARTUP:
                        ChangeStatus(Status.STUNNED);
                        SetSprite(3);
                        break;
                    
                    
                    case Status.HOOKSTARTUP:
                        ChangeStatus(Status.STUNNED);
                        SetSprite(3);
                        break;
                    
                    
                    case Status.HAYMAKERSTARTUP:
                        ChangeStatus(Status.STUNNED);
                        SetSprite(3);
                        break;
                    
                    
                    case Status.STUNNED:
                        float tempDamageCalcFloat = 0f;
                        tempDamageCalcFloat = tempDamage * 1.25f;
                        tempDamage = Mathf.RoundToInt(tempDamageCalcFloat);
                    break;
                }
        
        
        
        currentHP -= tempDamage;
        
        return tempDamage;
    }

    public void SetStatusWithDuration(Status status, int statusDuration)
    {
        combatantStatus = status;
        combatantStatusRemainingDuration = statusDuration;
        //Debug.Log(combatantName + " SetStatusWithDuration: Set status to " + combatantStatus + " with duration " + combatantStatusRemainingDuration);
    }
    
    public void ChangeStatus(Status status)
    {
        combatantStatus = status;
        //Debug.Log(combatantName + " ChangeStatus: Changed status to " + status);
    }

    public void DecreaseStatus()
    {
        if (combatantStatusRemainingDuration > 0)
        {
            combatantStatusRemainingDuration -= 1;
        }
        //Debug.Log(combatantName + " DecreaseStatus: Status " + combatantStatus + " has " + combatantStatusRemainingDuration + " beats remaining");
    }

    public void SetSprite(int spriteID)
    {
        if (spriteID < 0 | spriteID >= combatantSprites.Length)
        {
            Debug.LogError("Sprite ID " + spriteID + " is out of range.");
        }
        
        combatantSpriteRenderer.sprite = combatantSprites[spriteID];
    }

    public void squashCombatant()
    {
        this.transform.localScale = new Vector3(1f, 0.5f, 1f);
    }
    
}
