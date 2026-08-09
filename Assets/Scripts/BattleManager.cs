using System.Collections;
using UnityEngine;

public enum BattleState { INITIALIZATION, ROUNDSTART, ENEMYACT, RESOLUTION, PLAYBACK, ROUNDSETUP, WIN, LOSE }

public class BattleManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject[] enemyPrefabs;

    public Transform playerBattleMarker;
    public Transform enemyBattleMarker;
    
    private Combatant playerCombatant;
    private Combatant enemyCombatant;
    
    public UIManager UIManager;
    
    public BattleState battleState;
    
    public int currentRound = 1;
    public int currentBeat = 0;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        battleState = BattleState.INITIALIZATION;
        StartCoroutine(SpawnCombatants());
    }

    IEnumerator SpawnCombatants()
    {
       GameObject playerGameObject = Instantiate(playerPrefab, playerBattleMarker);
       playerCombatant = playerGameObject.GetComponent<Combatant>();
       UIManager.SetPlayerName(playerCombatant.combatantName);


       
       int randomEnemySelect =  Random.Range(0, enemyPrefabs.Length);
       
       GameObject enemyGameObject = Instantiate(enemyPrefabs[randomEnemySelect], enemyBattleMarker);
       enemyCombatant = enemyGameObject.GetComponent<Combatant>();
       UIManager.SetEnemyName(enemyCombatant.combatantName);
       
       UIManager.SetDialogueTitle("NEW CHALLENGER:");
       UIManager.SetDialogueContent(enemyCombatant.combatantName + " squares up!");
       Debug.Log("Spawned " + playerCombatant.combatantName);
       Debug.Log("Spawned " + enemyCombatant.combatantName);
       
       yield return new WaitForSeconds(3f);
       
       battleState = BattleState.ROUNDSTART;
       startActionQueue();
    }
    
    // 08/08/2026 - action queue system should go in here under ROUNDSTART phase
    // player can queue multiple actions within a round if their initiative is low enough and they pick a fast action
    // make UI buttons call functions in here to queue actions
    // pointer enter inserts it into the queue and updates preview, button select confirms it and moves onto the next action
    // action queue could be split off into another script but it'll probably be useful for resolution so idk
    // queue factors in action startup + recovery
    // once queue exceeds the round length stop accepting new entries
    // update preview on timelines through UIManager every time an action is inserted
    // uses code string from before
    void startActionQueue()
    {
        UIManager.SetDialogueTitle("SELECT AN ACTION:");
        UIManager.SetDialogueContent("Choose an action using the buttons to the right.");
        
        UIManager.EnemyBeatMarkerActive(true);
        UIManager.SetEnemyBeatMarker(enemyCombatant.initiative);
        
        UIManager.SetPlayerBeatMarker(playerCombatant.initiative);
        UIManager.PlayerBeatMarkerActive(true);
        
        UIManager.ButtonsActive(true);
        
        //string testString = "AEAEAEAEAERSRSRSRSRS";
        char[] blankPreviewCode = {'A','A','A','A','A','A','A','A','A','A','A','A','A','A','A','A','A','A','A','A',};
        
        UIManager.updateRoundPreview(blankPreviewCode);
    }

    void updateActionQueue(char actionType, int actionStartupDuration, int actionStartBeat)
    {
        if(battleState != BattleState.ROUNDSTART) return;
        
        
    }

    void guardButtonPreview()
    {
       if(battleState != BattleState.ROUNDSTART) return; 
       
       
    }

    void jabButtonPreview()
    {
        if(battleState != BattleState.ROUNDSTART) return; 
    }

    void hookButtonPreview()
    {
        if(battleState != BattleState.ROUNDSTART) return; 
    }

    void haymakerButtonPreview()
    {
        if(battleState != BattleState.ROUNDSTART) return; 
    }
}
