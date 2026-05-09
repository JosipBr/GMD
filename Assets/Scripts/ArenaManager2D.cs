using UnityEngine;

public class ArenaManager2D : MonoBehaviour
{
    [Header("Arena Prefabs")]
    [SerializeField] private Arena2D[] arenaPrefabs;

    [Header("Container")]
    [SerializeField] private Transform arenaContainer;

    private int currentArenaIndex = -1;
    private Arena2D currentArenaInstance;

    public Arena2D CurrentArena => currentArenaInstance;

    public Arena2D LoadFirstArena()
    {
        return LoadArena(0);
    }

    public Arena2D LoadNextArena()
    {
        int nextIndex = currentArenaIndex + 1;

        if (nextIndex >= arenaPrefabs.Length)
        {
            nextIndex = 0;
        }

        return LoadArena(nextIndex);
    }

    private Arena2D LoadArena(int arenaIndex)
    {
        if (arenaPrefabs == null || arenaPrefabs.Length == 0)
        {
            Debug.LogWarning("ArenaManager2D has no arena prefabs assigned.");
            return null;
        }

        if (arenaContainer == null)
        {
            Debug.LogWarning("ArenaManager2D has no arena container assigned.");
            return null;
        }

        if (currentArenaInstance != null)
        {
            Destroy(currentArenaInstance.gameObject);
        }

        currentArenaIndex = arenaIndex;

        currentArenaInstance = Instantiate(
            arenaPrefabs[currentArenaIndex],
            arenaContainer.position,
            Quaternion.identity,
            arenaContainer
        );

        Debug.Log($"Loaded arena: {currentArenaInstance.gameObject.name}");

        return currentArenaInstance;
    }
}