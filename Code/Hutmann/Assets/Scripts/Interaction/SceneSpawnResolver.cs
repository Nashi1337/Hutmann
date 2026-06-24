using UnityEngine;

namespace Interaction
{
// Place one instance in each scene to position the player after scene load.
public class SceneSpawnResolver : MonoBehaviour
{
    [SerializeField] private Transform playerRoot;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private bool debugLogs;

    private void Start()
    {
        if (!PlayerPrefs.HasKey(SceneTeleportInteractable.PendingSpawnKey))
            return;

        EnsurePlayerExists();
        if (playerRoot == null)
        {
            Debug.LogWarning("[SceneSpawnResolver] No player found and no player prefab assigned.");
            return;
        }

        string wantedId = PlayerPrefs.GetString(SceneTeleportInteractable.PendingSpawnKey, "");
        PlayerPrefs.DeleteKey(SceneTeleportInteractable.PendingSpawnKey);
        SceneSpawnPoint[] points = FindObjectsByType<SceneSpawnPoint>(FindObjectsSortMode.None);
        foreach (SceneSpawnPoint point in points)
        {
            if (point.SpawnPointId != wantedId)
                continue;
            CharacterController controller = playerRoot.GetComponent<CharacterController>();
            if (controller != null)
                controller.enabled = false;
            playerRoot.SetPositionAndRotation(point.transform.position, point.transform.rotation);
            if (controller != null)
                controller.enabled = true;

            if (debugLogs)
                Debug.Log($"[SceneSpawnResolver] Spawned player at '{point.SpawnPointId}'.");

            return;
        }
        Debug.LogWarning($"[SceneSpawnResolver] Spawn point not found: {wantedId}");
    }

    private void EnsurePlayerExists()
    {
        if (playerRoot != null)
            return;

        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
        {
            playerRoot = existingPlayer.transform;
            return;
        }

        if (playerPrefab == null)
            return;

        GameObject spawned = Instantiate(playerPrefab);
        playerRoot = spawned.transform;
    }
}
}
