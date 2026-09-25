# SPCore Singeplayer QBCore Beta

**SPCore** is a QBCore-inspired roleplay framework for **GTA V single-player**. It brings inventory, loot, shops, weapons, survival needs, banking, drugs, NPC interactions and other RP-style systems into a standalone ScriptHookVDotNet mod. It does **not** require FiveM or QBCore.

> **Beta notice:** this is an early beta. SPCore has only been worked on for approximately two days and is not complete. Expect bugs, unfinished systems and balance changes.

## Quick tutorial

1. Put `SPCore.dll`, `inventory_config.json`, and the `inventory_icons` folder in your GTA V `scripts` folder.
2. Launch GTA V through your normal ScriptHookVDotNet/RAGE Plugin Hook setup.
3. Press **I** to open/close the inventory.
4. Drag items between your inventory and nearby loot, gloveboxes, shops or storage.
5. Right-click an item for **USE** or **DROP**.
6. At shops, the amount box defaults to **1**. Type another amount if needed, then drag the purchase.
7. Loot nearby dead peds or dropped items within the small vicinity radius.
8. Eat and drink to maintain **Hunger** and **Thirst**.
9. Visit stores for supplies, Ammu-Nation for weapons/ammo, the bank for deposits, and the marked Trap area to work when carrying sellable drugs.
10. Grow weed at the marked Greenhouse, bag it with Baggies, and use the wider RP systems to build your single-player character.

## Main systems

- 5x5 player inventory with 5x2 secondary storage/vicinity panels.
- One-cell inventory items.
- GTA weapon-wheel synchronization for acquired weapons.
- Randomized glovebox loot: every vehicle contains at least a basic consumable; weapons are uncommon and area-sensitive.
- Dead pedestrian loot with weapons, ammo, cash, food, drinks and uncommon drug drops.
- Five-foot vicinity grouping so nearby floor loot is presented together.
- Hunger and thirst with eating/drinking animations.
- Ammu-Nation and 24/7 gas-station stores.
- Weapon License, banking and bank deposits.
- Shady Gun Dealer.
- Weed growing, bagging and selling.
- Trap work with NPC customers and dirty-money payouts.
- Drug effects and item-use interactions.
- Transparent inventory icons and compact RP notifications.

## Requirements

- Grand Theft Auto V
- ScriptHookV
- ScriptHookVDotNet v3/nightly
- .NET Framework 4.8
- RAGE Plugin Hook is supported for the user's launch setup.

## Installation

Copy these into:

`Grand Theft Auto V\scripts\`

```text
SPCore.dll
inventory_config.json
inventory_icons\
```

The included `build.bat` can be used on a Windows development machine with Visual Studio/MSBuild and a valid `GTA5_DIR` pointing to the GTA V installation.

## Controls

| Key | Action |
|---|---|
| **I** | Open / close inventory |
| **E** | Search nearby / interact with marked locations |
| **Right Mouse** | Context menu |
| **Left Mouse** | Drag / select |
| **R** | Rotate held item (legacy control; items are 1x1) |
| **ESC** | Close inventory |

## Configuration

`inventory_config.json` controls inventory sizes, loot settings, shop prices and other framework values. The beta is intentionally configurable so future versions can rebalance systems without redesigning the whole framework.

## Credits

Created by **Lethal**.

SPCore is an independent single-player project inspired by ideas commonly found in QBCore-style RP frameworks. It is not affiliated with or a port of QBCore.
