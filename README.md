# The Garbage Man

> Plunge into relentless dungeons where every bullet counts. Fight through waves of elemental enemies, mastering the art of dodge, block, and parry. Collect gold, upgrade your arsenal, and push deeper: pistol, shotgun, or SMG in hand. How long will it take for you to get rid of the name of "The Garbage Man"?

A fast-paced 3D top-down dungeon crawler built in Unity, inspired by *Hades*. Fight through procedurally generated dungeons using a combat system built around shooting, dodging, blocking, and parrying. Gold earned during runs is deposited at a hub ATM to unlock new weapons and permanent upgrades, creating a loop of risk, reward, and progression.

**Group 7 | Gold Master Release**

## Play the Game

**WebGL build:** https://play.unity.com/en/games/53bc4aa4-6c25-4b19-9d7b-b7c360d517ad/thegarbageman

## Story

After 30 years of war with AI, the Human Alliance finally destroyed the core of the Mother AI. Without the strategic system built by Mother AI, humanity took back 80% of its territory. The remaining 20%, the "Garbage Area", is still controlled by lost AI bots. To the government, it's a chance to recover technology lost during the war. To you, a "garbage man", it's your chance to move out of the Garbage Area.

The game is designed to feel relaxed and low-pressure. Death punishment is mild, switching between the dungeon and your base is easy, and the 1-2 hours of content lean toward rewarding the player rather than punishing them.

## Goal

Collect gold in the dungeon and deposit it into the ATM in the hub to fill the milestone bar. **Reaching 2000 gold deposited wins the game.** Milestones along the way unlock new weapons and permanent upgrades, and you can keep playing after winning to enjoy your fully upgraded build.

## Controls

| Input | Action |
|---|---|
| Move keys | Move the player |
| Left Mouse (hold) | Shoot in the direction you're facing |
| Right Mouse (hold) | Block: cancels bullets and you take 50% damage by default |
| Space | Parry: a successful parry cancels the bullet with no damage |
| Shift | Dodge: a short burst in the input direction with invincibility frames |
| Q | Use a healing potion (upgradeable through milestones) |
| E | Interact with the ATM and weapon shelves |
| Esc | Pause menu; Skip Intro ; press Q or E to switch between the pause menu and the character status page |

## Gameplay

### Hub
- You return to the hub between runs and enter the dungeon through a door.
- Deposit gold at the **ATM** to progress the milestone bar and unlock permanent upgrades and weapons.
- Three **weapon shelves** let you switch between the **pistol**, **shotgun**, and **SMG**. The shotgun and SMG are unlocked through ATM milestones.

### Dungeon
- Each run generates a map of multiple rooms connected by paths built from different floor prefabs.
- Entering a combat room locks the doors until every enemy inside is defeated.
- After clearing a room, open a chest to collect buff pickups.
- After completing a level, spend trash collected in the dungeon to choose from 6 buffs.
- An exit portal takes you to the next level once the required rooms are cleared.
- If you die, 50% of your resources are removed and you are teleported back to the hub.

### Enemies
Corrupted AI robots left over from the war. Each has unique movement logic (approach-retreat and wandering) and is driven by a simple FSM with idle, chase, and attack states. Enemies drop gold and trash when destroyed.

| Type | Effect |
|---|---|
| Normal | Standard bullets |
| Fire | Inflicts burn damage over time |
| Ice | Slows the player's movement speed |
