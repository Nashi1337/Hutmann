using UnityEngine;
using UnityEngine.SceneManagement;

namespace Interaction
{
public class SceneTeleportInteractable : MonoBehaviour
{
    [Header("Target Scene")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private int targetSceneBuildIndex = -1;
    [SerializeField] private LoadSceneMode loadMode = LoadSceneMode.Single;

    [Header("Optional Spawn ID in Target Scene")]
    [SerializeField] private string targetSpawnPointId = "";

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    public const string PendingSpawnKey = "PendingSpawnPointId";

    // Hook this method into Interactable -> On Interact().
    public void Interact()
    {
        if (string.IsNullOrWhiteSpace(targetSceneName) && targetSceneBuildIndex < 0)
        {
            Debug.LogWarning("[SceneTeleportInteractable] No target scene configured.");
            return;
        }

        if (string.IsNullOrWhiteSpace(targetSpawnPointId))
            PlayerPrefs.DeleteKey(PendingSpawnKey);
        else
            PlayerPrefs.SetString(PendingSpawnKey, targetSpawnPointId);

        if (debugLogs)
        {
            string label = string.IsNullOrWhiteSpace(targetSceneName)
                ? $"BuildIndex {targetSceneBuildIndex}"
                : targetSceneName;
            Debug.Log($"[SceneTeleportInteractable] Loading {label}");
        }

        if (!string.IsNullOrWhiteSpace(targetSceneName))
            SceneManager.LoadScene(targetSceneName, loadMode);
        else
            SceneManager.LoadScene(targetSceneBuildIndex, loadMode);
    }
}
}


