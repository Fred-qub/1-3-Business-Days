using UnityEngine;

public enum BattleState { INITIALIZATION, ROUNDSTART, ENEMYACT, RESOLUTION, PLAYBACK, ROUNDSETUP, WIN, LOSE }

public class BattleManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    public Transform playerBattleMarker;
    public Transform enemyBattleMarker;
    
    public BattleState battleState;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        battleState = BattleState.INITIALIZATION;
        SpawnCombatants();
    }

    void SpawnCombatants()
    {
        Instantiate(playerPrefab, playerBattleMarker);
        Instantiate(enemyPrefab, enemyBattleMarker);
    }

}
