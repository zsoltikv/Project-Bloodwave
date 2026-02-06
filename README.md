<div align="center">

# 💥 Project: Bloodwave  
### _Top-down Roguelite / Survival Arena (Unity • Android)_

[![Tech Stack](https://skillicons.dev/icons?i=unity,visualstudio,github,cs)](https://skillicons.dev)

![Platform](https://img.shields.io/badge/Platform-Android-red?style=flat-square)
![Repo Size](https://img.shields.io/github/repo-size/zsoltikv/Project-Bloodwave?style=flat-square)
![Last Commit](https://img.shields.io/github/last-commit/zsoltikv/Project-Bloodwave?style=flat-square)
![Commits](https://img.shields.io/github/commit-activity/m/zsoltikv/Project-Bloodwave?style=flat-square)

**Core gameplay code** for a fast, top-down **roguelite / survival arena** game built in **Unity**.  
Focus: **modular weapons**, **enemy AI + difficulty scaling**, **achievements**, and **meta progression**.

</div>

---

## ✨ Highlights
- **Modular weapon framework** (SO-based definitions + runtime instances)
- **Targeting strategies & spawn patterns** (single, shotgun, forward aim, nearest enemy)
- **Status effect modifiers** (bleed, slow, lifesteal)
- **Weighted enemy spawner** with unlock levels + **elite variants**
- **Dynamic difficulty scaling** based on player level and weapon strength
- **Achievement system** with persistence + UI browser
- **In-run shop & inventory** with non-repeatable per-run purchases
- **Persistent leaderboard** saved to `leaderboard.json`
- **Polish layer** (fade transitions, screen shake, damage popups, sorting bands)

---

## 🧭 Table of Contents
- [Features](#-features)
- [Gameplay & Systems Overview](#-gameplay--systems-overview)
  - [Core Game Flow](#core-game-flow)
  - [Player Systems](#player-systems)
  - [Weapon System](#weapon-system)
  - [Enemy Systems](#enemy-systems)
  - [Achievements](#achievements)
  - [Meta Systems](#meta-systems)
  - [Visual Polish](#visual-polish)
  - [Audio & Video](#audio--video)
  - [UI Flow & Pause Logic](#ui-flow--pause-logic)
- [Project Structure Overview](#-project-structure-overview)
- [Getting Started](#-getting-started)
  - [Requirements](#requirements)
  - [Setup](#setup)
- [Notes & Troubleshooting](#-notes--troubleshooting)

---

## 🚀 Features

### 🧩 Modular weapon system
- **ScriptableObject** weapon definitions (`WeaponDefinition`)
- Multiple **targeting strategies** and **spawn patterns**
- Reusable projectile **modifiers** (bleed, slow, lifesteal, etc.)

### 👾 Enemy and difficulty system
- Weighted spawns with **unlock levels** + **elite variants**
- Difficulty scales with **player level** + **weapon strength**
- AI movement modes:
  - straight, circle, zigzag, strafe, ambush, retreat

### 📈 Player progression
- XP + leveling, level-up upgrades
- Per-run stats:
  - timers, kills, AFK time, no-hit detection

### 🏆 Achievements
- Central `AchievementManager` with persistence (`PlayerPrefs` JSON)
- UI list with progress tracking and wipe button
- Unlocks triggered by many gameplay events

### 🛒 Meta systems
- In-run shop with **non-repeatable purchases per run**
- Inventory + stat-modifying items
- Persistent leaderboard saved to `leaderboard.json`

### 🎬 Polish
- Global fade manager for clean scene transitions
- Damage popups, UI lerps, screen shake, VFX
- Intro/cutscenes with skip support
- Sorting-band based 2D rendering order
- Fixed framerate initialization

---

## 🧠 Gameplay & Systems Overview

### Core Game Flow

#### `GameManagerScript`
- Global game state (pause, run flags, etc.)
- Controls `Time.timeScale` for pause/resume

#### `FadeManager`
- Runtime black overlay canvas (singleton)
- Scene loads via `LoadSceneWithFade` (fade in/out)

#### Scene Loader scripts
`GameLoader`, `RestartGame`, `BackToMainMenu`, `HowToPlayLoader`  
- Minimal wrappers calling `FadeManager` for specific scenes

#### `SetFPS`
- Sets v-sync and target FPS once per app lifetime

---

### Player Systems

#### `PlayerStats`
- HP/XP/level + base multipliers + run statistics
- Drives HP/XP UI (lerps, indicators, shake)
- Level-up logic + achievement triggers
- Death flow: animation → blood → disable shop/pause → camera zoom → fade to Game Over

#### `RunTimer`
- Per-run timer (TMP display)
- Survival achievements at **5 / 10 / 15 / 30 minutes**

#### `LevelUpScript` & `UpgradeOptionUI`
- Spawns **2 random upgrade options** on level-up
- Applies upgrade on click, then closes UI

#### Movement & Aiming
##### `PlayerMovement`
- Drag-based virtual joystick (touch + mouse)
- Dead zone, max drag distance, max speed
- Updates `Rigidbody2D.linearVelocity`, animator params, and shadow stretch
- Aim uses `AimDirection2D`

##### `AimDirection2D`
- Stores last non-zero move direction
- Used by forward-aiming weapons for shot direction

---

### Weapon System

#### Architecture
- Weapon = **data** (`WeaponDefinition`) + **runtime state** (`WeaponInstance`)
- `WeaponController` owns equipped weapons and runs the firing loop

#### `WeaponDefinition` (ScriptableObject)
- damage, cooldown, projectile count, range, speed, icon, name
- composed from:
  - `TargetingStrategy`
  - `SpawnPattern`
  - `ProjectileFactory` / `OrbitingWeaponFactory`
  - `WeaponModifier[]`

#### `WeaponInstance`
- runtime values:
  - level, cooldown timer, bonus stats, multipliers
- final stats = base weapon stats × player multipliers + instance bonuses

#### `WeaponController`
- Up to **3 active weapons** + extra inventory
- Handles:
  - cooldown loop → targeting → pattern spawn → projectile creation
  - orbiting weapons
  - weapon switching UI
- Unlocks weapon achievements (e.g. arsenal, orbit_master)

#### `WeaponUpgrade`
Types: Damage / ProjectileCount / Cooldown / Range / OrbitalSpeed  
- Applies to a specific `WeaponInstance`
- Refreshes orbitals if needed
- Unlocks upgrade achievements (first upgrade, cooldown_50, range_150, etc.)

#### `WeaponDisplayManager`
- UI slots: icon + cooldown fill
- slot buttons open the weapon switch panel

#### Targeting / Spawn Patterns / Modifiers
**Targeting (`TargetingStrategy`):**
- `NearestEnemyTargeting` → `Physics2D.OverlapCircleAll` then closest
- `AimForwardTargeting` → `firePoint.right` or `AimDirection2D.Direction`

**Spawn (`SpawnPattern`):**
- `SinglePattern`
- `ShotgunPattern` (spread scales with projectile count)

**Modifiers (`WeaponModifier`):**
- `BleedOnHit` → `EnemyHealth.ApplyBleed`
- `SlowOnHit` → `EnemyHealth.ApplySlow`
- `LifeStealOnHit` → heal % of dealt damage
- `ProjectileEffects` bridges hits/kills → modifiers

Shared structs: `WeaponContext`, `TargetInfo`, `Shot`

---

### Enemy Systems

#### Spawning & Difficulty
##### `EnemySpawner`
- Weighted enemy list (`EnemySpawnData`)
- Scaling:
  - enemy HP/damage scales relative to player stats
  - difficulty ramps over time (interval, stats, spawn count)
- Spawns outside camera view on valid tiles
- Optional elite spawns (multipliers + visuals)

##### `EnemySpawnData`
- prefab, unlock level, spawn weight, always spawn flag
- scaling ratios relative to player stats

#### Behavior & Health
##### `EnemyAI`
- 2D `NavMeshAgent`
- Modes: Straight / Circle / Zigzag / Strafe / Ambush / Retreat
- On player trigger: deal damage then self-destruct

##### `EnemyHealth`
- health, speed, damage, XP/coin rewards
- damage text optional via `DamageTextSpawner`
- death: rewards player + blood VFX
- status effects: `ApplySlow`, `ApplyBleed`

---

### Achievements

#### `Achievement` / `AchievementSaveData`
- Serializable achievement definitions + save data

#### `AchievementManager`
- Singleton registry
- Unlock/check/list helpers
- Saves unlocked IDs to `PlayerPrefs` (JSON)
- Meta-achievements for unlocking N/all achievements

#### `AchievementUIManager`
- Scroll list with styling + locked/unlocked states
- Shows global progress counter
- Button to wipe saved achievements

---

### Meta Systems

#### Inventory
##### `PlayerInventory`
- Singleton attached to player
- `InventorySlot` list: (`ShopItem`, quantity)
- Add/remove applies stat effects via `ShopItem`

> `ShopItem` is expected to expose `ApplyToPlayer` and `RemoveFromPlayer`, and may reference `WeaponDefinition` for weapon items.

#### Shop
##### `ShopManager`
- In-run shop UI controller
- Ensures items are not offered/purchased more than once per run
- Filters weapon offers to avoid duplicates
- Purchase: coins check → inventory add → refresh
- Tracks purchases + lifetime spend, unlocks shop achievements

#### Leaderboard & Saving
##### `SaveData`
- player name, level, total time, minutes, seconds

##### `SaveScript`
- Reads TMP input + level + `RunTimer`
- Saves top runs into `leaderboard.json`
- Shows “saved” text animation

##### `LeaderboardManager`
- Loads `leaderboard.json`, sorts by level desc
- Populates UI rows + pop-in animation

---

### Visual Polish

#### Damage Text
- `DamageTextSpawner`: world → UI anchored, spawns `DamageTextUI`
- `DamageTextUI`: curved float + fade + destroy

#### Sorting Bands
- `SortingBandManager`: sorting order from world Y
- `CharacterSortingByBand`: uses “feet” transform
- `PropSortingRegister`: static props sorted by bottom point

#### Tilemap Decoration
- `TilemapSetup`: overlays + prop spawning with spacing rules

---

### Audio & Video

#### `AudioManager`
- Menu: loop track
- Gameplay: shuffle without repeating last track
- Unlocks `music_lover` on first gameplay music start

#### Intro / Cutscene
- `VideoPlayer` clips with skip support
- Intro preloads menu scene and activates via `FadeManager`
- Unlocks `movie_buff` if watched fully

---

### UI Flow & Pause Logic

#### `PauseGame`
- Pause menu fade/scale animation
- Toggles `Time.timeScale`
- Tracks `PausedThisRun` for no-pause achievements

#### `ShopManager.ToggleShopUI`
- Opens/closes shop and pauses/resumes via `GameManagerScript`
- Avoids conflicts with pause state

---

## 🗂 Project Structure Overview
- **Core Game Flow**: `GameManagerScript`, `FadeManager`, loaders, `SetFPS`
- **Player**: `PlayerStats`, `RunTimer`, `LevelUpScript`, `UpgradeOptionUI`, `PlayerMovement`, `AimDirection2D`
- **Weapons**: `WeaponDefinition`, `WeaponInstance`, `WeaponController`, `WeaponUpgrade`, `WeaponDisplayManager`
- **Enemies**: `EnemySpawner`, `EnemySpawnData`, `EnemyAI`, `EnemyHealth`
- **Achievements**: `Achievement`, `AchievementSaveData`, `AchievementManager`, `AchievementUIManager`
- **Meta**: `ShopManager`, `PlayerInventory`, `SaveData`, `SaveScript`, `LeaderboardManager`
- **Polish**: damage text, sorting, tilemap setup, audio/video, pause flow

---

## 🧰 Getting Started

### Requirements
- **Unity 2021 LTS+** (recommended)
- **TextMeshPro** package installed
- **2D Renderer** setup
- **NavMesh** configured for 2D (required for `EnemyAI`)

### Setup
1. Clone the repository and open it in Unity.
2. Ensure scene names match code references:
   - `MenuScene`, `MainScene`, `HowToPlayScene`, `LeaderboardScene`, `AchievementScene`, `GameOverScene`, `CutsceneScene`
3. Create Weapon Definitions:
   - `Create > Weapons > Weapon Definitions`
4. Configure enemies:
   - assign prefabs + spawn data in `EnemySpawner`
   - verify NavMesh for your map
5. Hook UI references in the Inspector:
   - `PlayerStats`, `RunTimer`, `ShopManager`, `AchievementUIManager`, `LeaderboardManager`, etc.
6. Run and verify:
   - weapons fire
   - enemies spawn + scale correctly
   - achievements unlock
   - shop/pause pauses time correctly

---

<div align="center">
Made with ❤️ for PENdroid using Unity
</div>