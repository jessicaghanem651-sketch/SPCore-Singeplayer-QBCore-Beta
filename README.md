# SPCore Singeplayer QBCore Beta

**SPCore** is a QBCore-inspired roleplay framework for **GTA V single-player**. It adds inventory, loot, shops, weapons, banking, drugs, hunger/thirst and other RP-style systems.

It does **not** require FiveM or QBCore.

> **Beta:** SPCore is an early project that has only been worked on for approximately two days. Expect bugs, unfinished features and balance changes.

## Quick Tutorial

SPCore adds RP-style locations and activities around Los Santos and Blaine County.

### City Hall

Visit **City Hall** to purchase a **Weapon License** for **$300**.

The license allows you to purchase legal firearms from **Ammu-Nation / gun stores**.

### Gun Stores / Ammu-Nation

Gun stores are marked on the map and are used for legal firearms and ammunition.

- Weapon License required for legal firearm purchases.
- Ammunition costs **$50**.
- Firearms can be sold back for dirty cash.

### 24/7 / Gas Stations

24/7 and gas station stores sell general supplies such as:

- Food and Water
- OXY
- Backpacks
- Jerry Cans
- Repair Kits
- Lockpicks
- Wrenches
- Cigarettes
- Flashlights / Lighters
- Bandages
- Knives / Switchblades
- Whiskey and Wine

There is **no vehicle fuel system** in SPCore. Gas stations are regular stores.

### Greenhouse

The **Greenhouse** is used to grow weed.

- Weed Seeds: **$350**
- Baggies: **$3**
- Grow time: approximately **1 minute**
- Each plant produces **8 Weed Buds**
- **1 Weed Bud + 1 Baggie = 1 Bagged Bud**

Buy seeds, plant them at the Greenhouse, harvest the weed, package it with Baggies, then take the Bagged Bud to the **Trap** to sell it.

### Weed Dealer

The **Weed Dealer** is an underground NPC around Grove Street and is part of the drug system.

The Greenhouse handles growing and the Trap handles selling.

### Shady Gun Dealer

The **Shady Gun Dealer** is located at a house and provides an underground source for firearms.

Weapons cost approximately **1.65x Ammu-Nation prices**.

### Trap

The marked **Trap** is where you can sell supported drugs to NPC customers.

Have a supported drug in your inventory, visit the Trap and press **E** when available. Customers will approach, purchase a random amount and pay you in **dirty cash**.

Supported drugs include:

- Morphine
- Ecstasy
- Meth
- Weed
- OXY
- Bagged Bud

## Inventory

Press **I** to open or close the inventory.

- 5x5 player inventory
- 5x2 secondary inventory
- Vehicle gloveboxes
- Nearby loot
- Shop inventories
- Ground loot

Items can be dragged, dropped, picked up or used. Right-click items for available actions.

The amount box defaults to **1** when purchasing items.

## Loot

Dead NPCs and vehicles can contain randomized loot including:

- Cash
- Food and Water
- Weapons
- Ammunition
- Drugs
- Other items

Nearby ground loot is grouped within approximately **5 feet**.

## Hunger & Thirst

Hunger and Thirst are displayed at the top-left.

Food restores Hunger and drinks restore Thirst.

## Banking

Visit **Pacific Standard Bank** to deposit dirty cash into your bank balance.

Purchases use dirty cash first, then bank money if necessary.

## Weapons & Suppressors

Weapons can come from gun stores, the Shady Gun Dealer, NPCs and vehicles.

Ammunition is consumed when reloading rather than every time a shot is fired.

Suppressors can be purchased from gun stores and used on compatible weapons.

## Controls

| Key | Action |
|---|---|
| **I** | Open / close inventory |
| **E** | Interact / search |
| **Left Mouse** | Drag / select |
| **Right Mouse** | Context menu |
| **ESC** | Close inventory |

## Requirements

- Grand Theft Auto V
- ScriptHookV
- ScriptHookVDotNet v3/nightly
- .NET Framework 4.8
- RAGE Plugin Hook

## Installation

Copy these into:

`Grand Theft Auto V\scripts\`

```text
SPCore.dll
inventory_config.json
inventory_icons\
