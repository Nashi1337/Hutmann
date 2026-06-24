using UnityEngine;

namespace Interaction
{
// Place on empty objects in target scenes (e.g. "Spawn_FromHouseDoor").
public class SceneSpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnPointId = "Default";
    public string SpawnPointId => spawnPointId;
}
}



