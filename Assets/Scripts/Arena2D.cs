using UnityEngine;

public class Arena2D : MonoBehaviour
{
    [Header("Player Spawn Points")]
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    [Header("Weapon Spawn Points")]
    [SerializeField] private Transform[] weaponSpawnPoints;

    public Transform Player1SpawnPoint => player1SpawnPoint;
    public Transform Player2SpawnPoint => player2SpawnPoint;
    public Transform[] WeaponSpawnPoints => weaponSpawnPoints;
}