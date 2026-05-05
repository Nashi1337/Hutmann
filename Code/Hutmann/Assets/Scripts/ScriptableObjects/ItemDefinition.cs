using UnityEngine;

namespace ScriptableObjects
{
    public enum ItemType
    {
        None,
        Shovel,
        Flashlight,
        Map
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

        [Header("Flashlight")]
        public Vector3 flashlightLocalPosition = new Vector3(0f, 0f, 0.1f);
        public Vector3 flashlightLocalEuler = Vector3.zero;
        public Color flashlightColor = Color.white;
        public float flashlightIntensity = 2.5f;
        public float flashlightRange = 14f;
        [Range(1f, 179f)] public float flashlightOuterSpotAngle = 55f;
        [Range(1f, 179f)] public float flashlightInnerSpotAngle = 35f;
        public bool flashlightCastsShadows;
    }
}