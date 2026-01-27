using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEquipment : MonoBehaviour
{
    [Header("3 quick slots (index 0..2)")]
    [SerializeField] private ItemDefinition[] quickSlots = new ItemDefinition[4];

    [Header("Input")]
    [SerializeField] private bool wrapAround = true;

    public event Action<ItemDefinition, int> OnEquippedChanged;

    public int CurrentIndex { get; private set; } = 4;
    public ItemDefinition CurrentItem => IsValidIndex(CurrentIndex) ? quickSlots[CurrentIndex] : null;

    void Start()
    {
        Notify();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null)
        {
            if(kb.digit1Key.wasPressedThisFrame) EquipIndex(0);
            if(kb.digit2Key.wasPressedThisFrame) EquipIndex(1);
            if(kb.digit3Key.wasPressedThisFrame) EquipIndex(2);
            if(kb.digit4Key.wasPressedThisFrame) EquipIndex(3);
        }

        var mouse = Mouse.current;
        if (mouse != null)
        {
            float scrollY = mouse.scroll.ReadValue().y;
            if (scrollY > 0f) Step(+1);
            else if(scrollY < 0f) Step(-1);
        }
    }

    public void EquipIndex(int index)
    {
        if (!IsValidIndex(index)) return;
        if (index == CurrentIndex) return;

        CurrentIndex = index;
        Notify();
    }

    private void Step(int dir)
    {
        int next = CurrentIndex + dir;

        if (wrapAround)
        {
            next = Mod(next, quickSlots.Length);
        }
        else
        {
            next = Mathf.Clamp(next, 0, quickSlots.Length - 1);
        }

        EquipIndex(next);
    }

    private void Notify()
    {
        OnEquippedChanged?.Invoke(CurrentItem, CurrentIndex);
    }

    private bool IsValidIndex(int i) => i >= 0 && i < quickSlots.Length;

    private int Mod(int x, int m) => (x % m + m) % m;
}