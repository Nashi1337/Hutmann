# Shovel Interaction Test Setup

This folder contains test scripts for shovel-only interactions:

- `ShovelTapDigTarget`: needs 4 separate interact presses.
- `ShovelHoldDigTarget`: needs 4 seconds of total interact hold.
- `ShovelDigTargetBase`: updates visual fill (scale + color).

## Quick Scene Setup

1. Add `PlayerShovelInteractor` to the player object (same object as `PlayerEquipment`).
2. Set `Ray Origin` to your camera transform (or leave empty to auto-use `Camera.main`).
3. In the shovel `ItemDefinition` asset, set `Item Type` to `Shovel`.
4. Create a `Quad` for tap testing and add:
   - `BoxCollider` (if missing)
   - `ShovelTapDigTarget`
5. Create a second `Quad` for hold testing and add:
   - `BoxCollider` (if missing)
   - `ShovelHoldDigTarget`
6. Optional: assign each target's `Fill Visual` to a child object so only that child scales while digging.

## Notes

- Digging only works while the shovel is equipped.
- If `Required Shovel Item` is assigned on `PlayerShovelInteractor`, only that exact item works.
- If not assigned, any equipped item with `Item Type = Shovel` works.

