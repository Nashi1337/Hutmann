using UnityEngine;
using UnityEngine.UI;

public class QuickSlotHighlightUI : MonoBehaviour
{
    [SerializeField] private PlayerEquipment equipment;

    [SerializeField] private Image[] slotBackgrounds;

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color32(0, 0, 0, 0);
    [SerializeField] private Color highlightedColor = new Color32(0, 0, 0, 180);

    void OnEnable()
    {
        if (equipment != null)
            equipment.OnEquippedChanged += OnEquippedChanged;
        
        OnEquippedChanged(equipment.CurrentItem, equipment.CurrentIndex);
    }

    void OnDisable()
    {
        if (equipment != null)
            equipment.OnEquippedChanged -= OnEquippedChanged;
    }

    private void OnEquippedChanged(ItemDefinition item, int index)
    {
        for (int i = 0; i < slotBackgrounds.Length; i++)
        {
            if (slotBackgrounds[i] == null) continue;
            slotBackgrounds[i].color = (i == index) ? highlightedColor : normalColor;
        }
    }
}