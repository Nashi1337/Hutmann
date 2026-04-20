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

        [Header("First Person Equip Pose")]
        public Vector3 equipLocalPosition = Vector3.zero;
        public Vector3 equipLocalEuler = Vector3.zero;
        public Vector3 equipLocalScale = Vector3.one;
    }
}