# SPCore Singeplayer QBCore Beta

**SPCore** is a QBCore-inspired roleplay framework for **GTA V single-player**. It brings inventory, loot, shops, weapons, survival needs, banking, drugs, NPC interactions and other RP-style systems into a standalone ScriptHookVDotNet mod.

It does **not** require FiveM or QBCore.

> **Beta notice:** this is an early beta. SPCore has only been worked on for approximately two days and is not complete. Expect bugs, unfinished systems and balance changes.

## Quick Tutorial

SPCore adds a simple single-player RP progression system around Los Santos and Blaine County.

You can:

- Manage a full inventory.
- Loot dead NPCs and vehicles.
- Buy food, drinks and supplies.
- Obtain a legal Weapon License.
- Purchase legal firearms.
- Visit an underground gun dealer.
- Grow and package weed.
- Sell drugs to NPC customers at the Trap.
- Deposit money at the bank.
- Manage Hunger and Thirst.
- Find dropped loot around the world.
- Use weapons, food, medicine, drugs and other usable items.

---

## City Hall - Weapon License

Visit the marked **City Hall** location to purchase your **Weapon License**.

The Weapon License costs:

**$300**

The Weapon License allows you to purchase legal firearms from regular gun stores / Ammu-Nation.

### Legal Gun Progression

The normal legal firearm progression is:

**City Hall**

↓

**Purchase Weapon License for $300**

↓

**Visit Ammu-Nation / Gun Store**

↓

**Purchase Legal Firearms and Ammunition**

Your Weapon License is stored in your inventory as:

**Weapon License**

Without the required license, legal firearm purchases are restricted.

---

## Gun Stores / Ammu-Nation

Regular gun stores / Ammu-Nation locations are marked on the map.

These stores are the normal legal source for firearms and ammunition.

You can:

- Purchase legal firearms.
- Purchase ammunition.
- Sell firearms back to the gun store.
- Use your Weapon License for legal firearm purchases.

Ammunition costs:

**$50**

When selling a firearm back to the gun store, the firearm is sold for a percentage of its normal value and you receive **dirty cash**.

The legal firearm system is designed around getting your Weapon License from City Hall first and then purchasing weapons from a normal gun store.

---

## Regular Gas Stations / 24/7 Stores

Marked **24/7 / gas station stores** are general-purpose stores where you can purchase everyday supplies.

Available items include things such as:

- Water
- Food
- OXY
- Jerry Can
- Backpack
- Vehicle Repair Kit
- Lockpick
- Wrench
- Cigarettes
- Flashlight
- Lighter
- Bandages
- Switchblade / Knife
- Whiskey
- Wine
- Other general RP supplies

These stores are useful for keeping your character supplied while exploring the city.

### Important

SPCore does **not** currently use a vehicle fuel system.

The gas station is a general store and does not add:

- Fuel bars
- Fuel consumption
- Fuel pumps
- Vehicle refueling mechanics

---

## Greenhouse - Growing Weed

The marked **Greenhouse** is where you can grow your own weed.

The Greenhouse is part of the underground drug-production system.

### Step 1 - Buy Weed Seeds

Purchase **Weed Seeds** and take them to the Greenhouse.

Current price:

**$350 per seed**

### Step 2 - Plant the Seeds

At the Greenhouse, plant your Weed Seeds.

### Step 3 - Wait for the Plant to Grow

The current beta growing time is approximately:

**1 minute**

### Step 4 - Harvest

Once the plant has finished growing, harvest it.

Each completed plant produces:

**8 Weed Buds**

### Step 5 - Package the Weed

You need **Baggies** to package the weed.

Current price:

**$3 per Baggie**

The crafting process is:

**1 Weed Bud + 1 Baggie = 1 Bagged Bud**

Bagged Bud is the packaged product that can be sold through the Trap system.

### Weed Production Flow

The complete weed progression is:

**Buy Weed Seeds**

↓

**Plant at Greenhouse**

↓

**Wait for Growth**

↓

**Harvest 8 Weed Buds**

↓

**Use Baggies**

↓

**Create Bagged Bud**

↓

**Take Bagged Bud to the Trap**

↓

**Sell to NPC Customers**

---

## Weed Dealer

The **Weed Dealer** is an underground NPC located around the Grove Street area.

The dealer is part of the underground side of the drug system.

The Greenhouse is where you produce the weed, while the Trap is where you can sell supported drugs to NPC customers.

The basic weed system is:

**Seeds → Greenhouse → Weed Buds → Baggies → Bagged Bud → Trap**

---

## Shady Gun Dealer

The **Shady Gun Dealer** is an underground firearm dealer.

The dealer has been moved into a house rather than standing in the middle of the street.

The Shady Gun Dealer provides an alternative source for weapons outside of the normal legal gun-store system.

Weapons from the Shady Gun Dealer cost approximately:

**1.65x the normal Ammu-Nation price**

The Shady Gun Dealer is intended to represent an underground source for firearms.

### Legal vs Underground Guns

There are two different firearm routes:

**Legal Route**

City Hall

↓

Weapon License

↓

Ammu-Nation / Gun Store

↓

Legal Firearms

**Underground Route**

Shady Gun Dealer

↓

Higher Weapon Prices

↓

Underground Firearms

---

## Trap - Selling Drugs

The marked **Trap** area is used to sell supported drugs to NPC customers.

The Trap is the selling side of the underground drug system.

You must have a supported drug in your inventory before you can interact with the Trap.

When you have drugs available:

1. Travel to the marked Trap area.
2. Approach the Trap.
3. Press **E** when the interaction is available.
4. NPC customers can approach you.
5. A handoff interaction takes place.
6. The customer purchases a random amount.
7. The sold drugs are removed from your inventory.
8. You receive **dirty cash**.

Customers can purchase different quantities, so each sale can be different.

### Supported Drugs

The Trap can handle supported drugs such as:

- Morphine
- Ecstasy
- Meth
- Weed
- OXY
- Bagged Bud
- Other supported drug items

---

## Weed Selling Example

A basic weed run looks like this:

**1. Buy Weed Seeds**

↓

**2. Go to the Greenhouse**

↓

**3. Plant the Seeds**

↓

**4. Wait approximately 1 minute**

↓

**5. Harvest 8 Weed Buds**

↓

**6. Purchase Baggies**

↓

**7. Combine Weed Buds + Baggies**

↓

**8. Create Bagged Bud**

↓

**9. Travel to the Trap**

↓

**10. Sell to NPC Customers**

↓

**11. Receive Dirty Cash**

This gives the beta a simple production-to-sale RP loop.

---

## Inventory

Press **I** to open and close the inventory.

The inventory contains:

- A **5x5 player inventory**
- A **5x2 secondary panel**
- Nearby loot / vicinity
- Vehicle gloveboxes
- Shop inventories
- Other storage interactions

All items use one inventory slot.

You can:

- Drag items between inventories.
- Purchase items from shops.
- Drop items onto the ground.
- Pick up dropped items.
- Right-click items for available actions.
- Use food, drinks, medicine and other usable items.
- Search nearby loot.

### Amount Selection

The amount box is visible when purchasing items.

The default amount is:

**1**

You can type another amount and then drag the item directly into your inventory.

---

## Nearby Loot / Vicinity

Dropped items and nearby loot are grouped into a small vicinity area.

The current vicinity range is approximately:

**5 feet**

Nearby items within the same area are grouped together instead of creating unnecessary separate loot piles.

You can search and pick up nearby loot through the secondary inventory panel.

---

## Vehicle Gloveboxes

Vehicles have randomized glovebox loot.

Every vehicle can contain at least a basic food or water item.

Some vehicles can contain:

- Food
- Water
- Weapons
- Ammunition
- Other inventory items

Firearms are uncommon.

The chance of finding a firearm can also vary depending on the area of the city.

---

## Dead Pedestrian Loot

Dead pedestrians can contain randomized loot.

Possible loot includes:

- Cash
- Food
- Water
- Weapons
- Ammunition
- Drugs
- Cigarettes
- Baggies
- Other inventory items

If an armed NPC is killed, their carried firearm can also become available as loot.

Drug drops are uncommon and can contain varying quantities.

---

## Hunger and Thirst

SPCore includes basic survival needs.

**Hunger** and **Thirst** are displayed at the top-left of the screen.

Food restores Hunger.

Drinks restore Thirst.

Using food and drinks also triggers eating and drinking interactions.

Keeping your character supplied is part of the normal gameplay loop.

---

## Banking

The marked **Pacific Standard Bank** can be used to deposit money.

You can take dirty cash to the bank and deposit it into your bank balance.

The basic process is:

**Dirty Cash in Inventory**

↓

**Visit Pacific Standard Bank**

↓

**Deposit Cash**

↓

**Cash Leaves Inventory**

↓

**Money Added to Bank Balance**

Purchases use **dirty cash first**, then bank money if you do not have enough cash available.

---

## Weapons and Ammunition

SPCore synchronizes acquired weapons with the GTA V weapon system.

Weapons can come from:

- Ammu-Nation / legal gun stores
- Shady Gun Dealer
- NPC loot
- Vehicle loot

Different weapon types use different ammunition types.

The inventory also tracks ammunition used for reloading.

Ammunition is not removed from your inventory for every individual shot. Ammo is consumed when a reload uses ammunition.

---

## Suppressors

Suppressors can be purchased from a gun store.

Once you have a compatible suppressor:

1. Hold the compatible firearm.
2. Right-click / use the suppressor from your inventory.
3. The suppressor is applied to the compatible weapon.

---

## Drug Effects

Some drug-related items have effects when used.

Examples include:

- Joints
- Meth
- Other supported drug items

Using a joint triggers a short smoking interaction and visual effect.

Drug effects are part of the experimental beta gameplay systems and may change in future versions.

---

## Main Systems

- 5x5 player inventory.
- 5x2 secondary inventory panel.
- One-cell inventory items.
- GTA weapon-wheel synchronization.
- Randomized vehicle glovebox loot.
- Dead pedestrian loot.
- Ground loot and pickup system.
- Five-foot vicinity grouping.
- Hunger and Thirst.
- Food and drink interactions.
- Ammu-Nation / gun stores.
- City Hall Weapon License.
- Legal firearm progression.
- Shady Gun Dealer.
- 24/7 / gas station stores.
- Pacific Standard Bank.
- Bank deposits.
- Weed Seeds.
- Greenhouse weed growing.
- Weed harvesting.
- Baggies and weed packaging.
- Trap drug-selling system.
- NPC drug customers.
- Drug effects.
- Weapon ammunition.
- Suppressors.
- Inventory notifications.
- Compact inventory UI.
- Custom inventory icons.
- Configurable inventory and shop settings.

---

## Quick Start

If you just want to start playing:

1. Install SPCore.
2. Launch GTA V.
3. Press **I** to open your inventory.
4. Visit **City Hall** if you want to legally purchase firearms.
5. Purchase your **Weapon License** for $300.
6. Visit an **Ammu-Nation / Gun Store** for legal weapons and ammunition.
7. Visit a **24/7 / gas station** for food, water and general supplies.
8. Visit the **Greenhouse** if you want to start growing weed.
9. Buy **Weed Seeds** for $350 each.
10. Plant the seeds at the Greenhouse.
11. Harvest the plants after they finish growing.
12. Use **Baggies** to package your Weed Buds.
13. Take your **Bagged Bud** to the **Trap**.
14. Sell drugs to NPC customers and receive dirty cash.
15. Visit **Pacific Standard Bank** to deposit money.
16. Search dead NPCs, vehicles and nearby loot for additional items.

---

## Requirements

- Grand Theft Auto V
- ScriptHookV
- ScriptHookVDotNet v3/nightly
- .NET Framework 4.8
- RAGE Plugin Hook is supported for the user's launch setup.

---

## Installation

Copy these files into:

`Grand Theft Auto V\scripts\`

```text
SPCore.dll
inventory_config.json
inventory_icons\
