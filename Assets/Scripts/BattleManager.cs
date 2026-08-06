using UnityEngine;

public enum BattleState { INITIALIZATION, ROUNDSTART, ENEMYACT, RESOLUTION, PLAYBACK, ROUNDSETUP, WIN, LOSE }

public class BattleManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    public Transform playerBattleMarker;
    public Transform enemyBattleMarker;
    
    private Combatant playerCombatant;
    private Combatant enemyCombatant;
    
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
       
       GameObject enemyGameObject = Instantiate(enemyPrefab, enemyBattleMarker);
       enemyCombatant = enemyGameObject.GetComponent<Combatant>();

       Debug.Log("Spawned " + playerCombatant.combatantName);
       Debug.Log("Spawned " + enemyCombatant.combatantName);
    }

}
