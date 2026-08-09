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
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        battleState = BattleState.INITIALIZATION;
        SpawnCombatants();
    }

    void SpawnCombatants()
    {
       GameObject playerGameObject = Instantiate(playerPrefab, playerBattleMarker);
       playerCombatant = playerGameObject.GetComponent<Combatant>();
       UIManager.SetPlayerName(playerCombatant.combatantName);
       UIManager.SetPlayerBeatMarker(playerCombatant.initiative);
     

       
       int randomEnemySelect =  Random.Range(0, enemyPrefabs.Length);
       
       GameObject enemyGameObject = Instantiate(enemyPrefabs[randomEnemySelect], enemyBattleMarker);
       enemyCombatant = enemyGameObject.GetComponent<Combatant>();
       UIManager.SetEnemyName(enemyCombatant.combatantName);
       UIManager.SetEnemyBeatMarker(enemyCombatant.initiative);
       
       
       
       UIManager.SetDialogueContent(enemyCombatant.combatantName + " squares up!");
       Debug.Log("Spawned " + playerCombatant.combatantName);
       Debug.Log("Spawned " + enemyCombatant.combatantName);
    }
    
    // 08/08/2026 - action queue system should go in here under ROUNDSTART phase
    // player can queue multiple actions within a round if their initiative is low enough and they pick a fast action
    // make UI buttons call functions in here to queue actions
    // pointer enter inserts it into the queue, button select confirms it
    // action queue could be split off into another script but it'll probably be useful for resolution so idk
    // queue factors in action startup + recovery
    // once queue exceeds the round length stop accepting new entries
    // update preview on timelines through UIManager every time an action is inserted
    // uses code string from before

}
