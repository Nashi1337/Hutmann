using UnityEngine;

namespace ScriptableObjects
{
    public enum ItemType
    {
        None,
        Shovel
    }

    [CreateAssetMenu(menuName = "Game/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        public string displayName;

        public ItemType itemType = ItemType.None;

        public GameObject equippedPrefab;
    }
}