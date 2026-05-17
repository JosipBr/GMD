using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawner2D : MonoBehaviour
{
    [Header("Weapon Prefabs")]
    [SerializeField] private WeaponPickup2D[] weaponPrefabs;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Timing")]
    [SerializeField] private float minSpawnDelay = 2f;
    [SerializeField] private float maxSpawnDelay = 5f;

    [Header("Spawn Limits")]
    [SerializeField] private int maxActiveWeapons = 2;

    [Header("Drop Settings")]
    [SerializeField] private float spawnHeight = 3f;
    [SerializeField] private float dropSpeed = 2f;

    private readonly List<SpawnedWeaponInfo> spawnedWeapons = new();
    private Coroutine spawnRoutine;

    private class SpawnedWeaponInfo
    {
        public WeaponPickup2D Weapon { get; }
        public Transform SpawnPoint { get; }

        public SpawnedWeaponInfo(WeaponPickup2D weapon, Transform spawnPoint)
        {
            Weapon = weapon;
            SpawnPoint = spawnPoint;
        }
    }

    public void StartSpawningForRound()
    {
        StopSpawning();

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnRoutine == null)
        {
            return;
        }

        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    public void ClearSpawnedWeapons()
    {
        foreach (SpawnedWeaponInfo spawnedWeapon in spawnedWeapons)
        {
            if (spawnedWeapon != null && spawnedWeapon.Weapon != null)
            {
                Destroy(spawnedWeapon.Weapon.gameObject);
            }
        }

        spawnedWeapons.Clear();
    }

    public void SetSpawnPoints(Transform[] newSpawnPoints)
    {
        spawnPoints = newSpawnPoints;
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);

            RemoveMissingWeapons();

            if (spawnedWeapons.Count >= maxActiveWeapons)
            {
                continue;
            }

            SpawnRandomWeapon();
        }
    }

    private void SpawnRandomWeapon()
    {
        if (weaponPrefabs == null || weaponPrefabs.Length == 0)
        {
            Debug.LogWarning("WeaponSpawner2D has no weapon prefabs assigned.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("WeaponSpawner2D has no spawn points assigned.");
            return;
        }

        List<Transform> availableSpawnPoints = GetAvailableSpawnPoints();

        if (availableSpawnPoints.Count == 0)
        {
            Debug.Log("No free weapon spawn points available.");
            return;
        }

        WeaponPickup2D weaponPrefab = weaponPrefabs[Random.Range(0, weaponPrefabs.Length)];
        Transform spawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];

        Vector3 targetPosition = spawnPoint.position;
        Vector3 startPosition = targetPosition + Vector3.up * spawnHeight;

        WeaponPickup2D spawnedWeapon = Instantiate(
            weaponPrefab,
            startPosition,
            Quaternion.identity
        );

        spawnedWeapon.SetOriginalTransform(
            targetPosition,
            Quaternion.identity,
            spawnedWeapon.transform.localScale
        );

        spawnedWeapons.Add(new SpawnedWeaponInfo(spawnedWeapon, spawnPoint));

        StartCoroutine(DropWeaponToPoint(spawnedWeapon, targetPosition));
    }

    private List<Transform> GetAvailableSpawnPoints()
    {
        List<Transform> availableSpawnPoints = new();

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null)
            {
                continue;
            }

            if (!IsSpawnPointOccupied(spawnPoint))
            {
                availableSpawnPoints.Add(spawnPoint);
            }
        }

        return availableSpawnPoints;
    }

    private bool IsSpawnPointOccupied(Transform spawnPoint)
    {
        foreach (SpawnedWeaponInfo spawnedWeapon in spawnedWeapons)
        {
            if (spawnedWeapon == null || spawnedWeapon.Weapon == null)
            {
                continue;
            }

            if (spawnedWeapon.Weapon.IsEquipped)
            {
                continue;
            }

            if (spawnedWeapon.SpawnPoint == spawnPoint)
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator DropWeaponToPoint(WeaponPickup2D weapon, Vector3 targetPosition)
    {
        while (weapon != null && !weapon.IsEquipped)
        {
            weapon.transform.position = Vector3.MoveTowards(
                weapon.transform.position,
                targetPosition,
                dropSpeed * Time.deltaTime
            );

            if (Vector3.Distance(weapon.transform.position, targetPosition) <= 0.01f)
            {
                weapon.transform.position = targetPosition;
                yield break;
            }

            yield return null;
        }
    }

    private void RemoveMissingWeapons()
    {
        spawnedWeapons.RemoveAll(spawnedWeapon =>
            spawnedWeapon == null ||
            spawnedWeapon.Weapon == null
        );
    }
}