# Pixel Adventure — Slice 1 Design

Date: 2026-09-28
Status: Draft, awaiting review
Project folder: `D:\Codes\KnightSlug` · Game title: **Pixel Adventure**

## 1. Intent

- Hobby project, built by vibe coding: Claude writes all code and editor tooling; the developer (new to Unity/C#) plays, gives feedback, and approves.
- Budget: $0. Free tools (Unity Personal) and free assets only (Kenney.nl CC0, free itch.io packs — check each license).
- Long-term goal: a mobile game where players can buy extra hero classes. Not in this slice, but the data model must support it.
- Success for Slice 1: one Metal Slug-style stage, playable start to finish with the Knight, on an Android phone, using touch controls.

## 2. Platform and constraints

- Unity 6 LTS (6000.0.x), 2D, Universal Render Pipeline 2D.
- Landscape only. Android is the primary test target; the project stays iOS-compatible (iOS builds need a Mac or a free-tier cloud build later).
- Single-player, offline.
- Placeholder art (colored shapes) until gameplay is proven; sprites and animations are referenced from data, so art can be swapped without code changes.
- Target: 60 FPS on mid-range Android. Pool frequently spawned objects (projectiles, enemies).

## 3. Milestones

| Milestone | Scope | Spec sections |
|---|---|---|
| **M1 (build now)** | Project skeleton, input, Knight movement/roll/combo/skills, test room with training dummies, HUD cooldown buttons | 4, 5 |
| M2 | Enemies, waves, boss, lives/checkpoints | 6 |
| M3 | Menus, scene flow, Android build | 7 |

Each milestone ends with a playable build and a git commit.

## 4. Architecture

Plain Unity components, data in ScriptableObjects, systems talking through C# events.

```
Assets/_Game/
  Scripts/
    Core/        GameManager, StageManager, CheckpointSystem, LivesSystem
    Input/       InputReader (touch + keyboard → events)
    Player/      PlayerController (move/jump/roll), PlayerCombat, SkillRunner
    Combat/      Health, Hitbox, Hurtbox, DamageInfo, Projectile, Cooldown
    Skills/      SkillData (base), MeleeCombo, ShieldBash, HolySlam
    Enemies/     EnemyBase, GruntAI, ArcherAI, FlyerAI            (M2)
    Boss/        OgreBoss                                          (M2)
    Waves/       WaveZone, WaveSpawner                             (M2)
    UI/          HUD, SkillButton, GameOver, StageClear
  Editor/        Menu tools that generate scenes, prefabs, data assets
  Data/          HeroClasses/, Skills/, Enemies/, Waves/, Stages/
  Prefabs/  Scenes/  Art/Placeholder/
Assets/Tests/    EditMode/ (pure logic), PlayMode/ (behavior)
```

Rules:

- **Data vs logic.** Every tunable number (HP, speed, damage, cooldown, ranges) lives in a ScriptableObject. A new hero class = new `HeroClassData` + its skill assets/scripts; core code unchanged.
- **Unlock hook.** `HeroClassData` has `bool unlockedByDefault` and a stable `id` string, so a shop/IAP can unlock classes later.
- **Combat contract.** Damage dealers own a `Hitbox`; damage takers own a `Hurtbox` + `Health`. Player, enemies and boss share this path. `DamageInfo` carries amount, source, knockback, stun duration.
- **Loose coupling.** Events such as `Health.OnDamaged`, `Health.OnDied`, `WaveSpawner.OnWaveCleared`. Managers never hold direct references to each other beyond what the scene wires.
- **Testable core.** Cooldown timers, damage math, combo state, wave and life rules are plain C# classes covered by EditMode tests, independent of scenes.
- **Editor generation.** Scenes, prefabs and data assets for the slice are created by `Pixel Adventure/…` menu commands, so the developer does not hand-assemble them. Regenerating must be safe (overwrites generated assets only).

## 5. Player and Knight (M1)

### 5.1 Controls

```
┌──────────────────────────────────────────────┐
│ [HP ▓▓▓▓░] ♥♥♥                    [⏸]        │
│                                    (ULTRA)   │
│                            (PRIMARY)         │
│  ( ◉ )                              (ROLL)   │
│ joystick                   (ATTACK)          │
└──────────────────────────────────────────────┘
```

- **Left: virtual joystick.** Horizontal axis moves. A flick up past 60% of stick range triggers jump (edge-triggered; must return below threshold before the next jump). 0.1 s jump input buffer.
- **Right: 4 buttons** — Attack, Roll, Primary, Ultra. Each skill button shows a radial cooldown overlay.
- **Keyboard (editor testing):** A/D move, W or Space jump, J attack, K roll, L primary, I ultra.
- Implemented with the Unity Input System package; touch and keyboard both feed `InputReader`, which exposes events/values to gameplay code.

### 5.2 Movement feel

- Kinematic `Rigidbody2D` with a custom controller (ground checks via casts).
- Coyote time 0.1 s; higher gravity when falling than rising; variable jump height is not used (flick = full jump).
- Facing direction follows last horizontal input; attacks and skills use facing direction.

### 5.3 Shared hero actions

- **Roll:** 0.4 s dash in facing direction, invulnerable for its duration, 1 s cooldown. Usable on ground only.

### 5.4 Knight kit

| Action | Behavior | Numbers (defaults, all in data) |
|---|---|---|
| Attack | 3-hit melee combo in front; pressing during a hit queues the next; combo resets after 0.6 s idle | 10 / 10 / 15 dmg, short reach |
| Primary: Shield Bash | Dash forward, knockback + stun on hit | ~2 units dash, 1 s stun, 5 s cooldown |
| Ultra: Holy Slam | Short leap, slam, damages all enemies in radius | ~4 units radius, 30 s cooldown |

- Stats: 100 HP, move speed 5, high defense (flat damage reduction value in data, default 0 until enemies exist).
- Hit-pause (~50 ms time freeze) when the Knight's hits land.
- Aiming rule for future classes: basic attacks always use facing direction; individual skills may auto-target.

### 5.5 M1 test room

- Flat ground, two platforms, walls at both ends.
- 3 training dummies (Health + Hurtbox, regenerate after death) to verify combo, bash knockback/stun and slam radius.
- HUD: HP bar and skill buttons with cooldowns.

## 6. Enemies, waves, boss (M2)

Enemy base: `Health`, `Hurtbox`, attack `Hitbox`, state machine Idle → Chase → Attack → Hurt → Dead; stats from `EnemyData`. Every attack has a visible wind-up tell.

| Enemy | HP | Behavior | Counterplay |
|---|---|---|---|
| Grunt | 30 | Walks to player, 0.5 s red flash, swings | Hit first or roll away |
| Archer | 20 | Keeps ~6 units away, arrow every 2 s | Jump or roll through arrow |
| Flyer | 15 | Hovers, then straight-line dive | Dodge, punish during recovery |

Stage 1: side-scrolling level ~3 screens wide with platforms. Three `WaveZone`s: camera locks, invisible edge walls appear, `WaveData` spawns enemies from given sides with delays. Clear → unlock camera, save checkpoint, show "GO →".

- Wave 1: 5 grunts. Wave 2: grunts + archers. Wave 3: grunts + archers + flyers.

Ogre boss, 300 HP, top-of-screen HP bar:

- Phase 1 (100–50%): one attack at a time with pauses — Club Swing (roll behind), Ground Pound shockwave (jump over), Charge (roll through).
- Phase 2 (<50%): roar, tinted red, faster, shorter pauses, double ground pound.

Lives and checkpoints: 3 lives. Death with lives left → respawn at last checkpoint with full HP, lose 1 life, current wave restarts. Boss fight is its own checkpoint. 0 lives → Game Over → Retry restarts the stage.

## 7. Flow, builds (M3)

```
MainMenu ──Play──► Stage1 ──boss dead──► StageClear ──Menu──► MainMenu
                     └── 0 lives ──► GameOver ──Retry──► Stage1
```

- MainMenu: title, Play, hero select (Knight only; locked classes shown with a lock, driven by the unlock flag).
- Pause: Resume, Restart, Quit to menu.
- Android: menu command builds an APK for USB install. iOS: deferred.

## 8. Testing

- EditMode tests for pure logic: `Cooldown`, damage calculation, combo state machine, jump-flick detection, life/checkpoint rules, wave progression.
- Tests run headless via Unity command line after changes.
- Game feel verified by the developer in Play mode (keyboard) and on an Android phone (touch).
- Commit after each working step.

## 9. Out of scope (Slice 1)

Other hero classes, shop/IAP, save/progression, audio (beyond placeholder), additional stages, settings menu, localization, iOS build.
