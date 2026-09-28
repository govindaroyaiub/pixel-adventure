# Pixel Adventure — Game Design Document

Studio: High Devs · Engine: Unity 6 (URP 2D) · Status: living document
Last updated: 2026-09-29

This is the master design. Build specs for individual slices (e.g.
`docs/superpowers/specs/2026-09-28-pixel-adventure-slice1-design.md`) implement
parts of it. When the two disagree, this document wins and the slice spec is updated.

---

## 1. Vision

- 2D pixel-art action RPG, **Metal Slug-style side-scroller** (run, jump, platforms), landscape, mobile.
- Diablo-style loot and build depth on top of short, punchy stage fights.
- Players progress **at their own pace**: farming, crafting and gem systems reward time, not only money.
- Budget: $0 until the game is worth showing. Free tools and free assets (licenses in `CREDITS.md`).
- Platforms: **iOS first** (developer device), Android after. Single-player, offline first.

## 2. Controls

```
┌──────────────────────────────────────────────┐
│ [HP ▓▓▓▓░]                        [⏸]        │
│                                     (ULT)    │
│                            (PRIMARY)         │
│  ( ◉ )                              (ROLL)   │
│ joystick                   (ATTACK)          │
└──────────────────────────────────────────────┘
```

- Left: virtual joystick. Horizontal = move. Flick up = jump.
- Right: **Attack** (basic combo), **Roll** (dodge, i-frames), **Primary** skill, **Ult**.
- Only Primary and Ult are "skills" (upgradable by skill tree, Engraving, Inscription). Attack and Roll are base actions.

## 3. Account and start flow

```
Splash → Sign-in screen → Hero select → Map
```

- Sign-in screen shows **Sign in with Google**, **Sign in with Apple**, **Guest**.
- Until online features exist, Google and Apple are visible but disabled; Guest is the only working option.
- **Before App Store submission**, the disabled buttons must work or be hidden (App Review can reject non-functional buttons).
- Apple rule: if Google sign-in is offered on iOS, Sign in with Apple must be offered too.
- Guest save format must be linkable to a Google/Apple account later, so guest progress is never lost.

## 4. Heroes

| Hero | Role | Main hand | Off-hand | Access |
|---|---|---|---|---|
| Knight | Tank and/or DPS | Sword | Shield | Free |
| Priest | DPS + heal | Staff | Tome | Free |
| Gunslinger | Ranged DPS | Guns | Bullets | Free |
| Slayer | Melee DPS, life leech | Axe | Axe | Paid |
| Sorcerer | Ranged mage, high crit | Wand | Orb | Paid |
| Monk | Melee combo DPS (tentative) | Fists (tentative) | Prayer beads (tentative) | TBD (6th class, later) |

- New players pick one of the 3 free heroes.
- Each hero has its own stats, passive skills, Primary skill, Ult and skill tree.
- **Max level: 50.** 1 skill point per level.
- Skill tree: 3 branches per class. Knight: **Guardian** (tank) / **Vanguard** (DPS) / **Paladin** (hybrid).

### 4.1 Skills

Naming pattern: two words, **action + object** (Shield Bash) or **element + form** (Starfall). One clear theme per class.

| Hero | Theme | Primary | Ult | Passive examples |
|---|---|---|---|---|
| Knight | Shield + holy | Shield Bash | Holy Slam | Iron Skin, Last Stand |
| Priest | Light, heal on hit | Smite | Judgment Beam | Blessed Hands, Martyr |
| Gunslinger | Speed, crit shots | Fan the Hammer | Dead Eye | Quickdraw, Ricochet |
| Slayer | Rage, bleed, life leech | Cleave Rush | Red Frenzy | Bloodthirst, Carnage |
| Sorcerer | Elements, big crit | Arcane Bolt | Starfall | Overload, Glass Mind |
| Monk | Combos, chi | Palm Strike | Hundred Fists | Flow, Inner Peace |

- Knight's Shield Bash and Holy Slam are already in the build.
- **Nth-hit bonus**: every Nth basic attack triggers a free class-themed bonus hit (e.g. every 5th Knight hit releases a small holy shockwave). Makes the Attack button more interesting without adding buttons.

## 5. Items

### 5.1 Equipment slots (per hero)

Main hand, Off-hand, Head, Gloves, Chest, Boots, Ring 1, Ring 2, Bracer, Necklace (10 slots).
Accessories = Ring, Bracer, Necklace.

### 5.2 Rarity

| Rarity | Color | Enchantment slots | Engraving slots | Inscription slots |
|---|---|---|---|---|
| Normal | Ash (gray) | 0 | 0 | 0 |
| Rare | Blue | 2 | 0 | 0 |
| Epic | Purple | 2 | 1 | 0 |
| Legendary | Orange | 2 | 2 | 0 |
| Exotic | Red | 2 | 2 | 1 |

### 5.3 Item level

- Every item has a level. A hero can equip items **at or below** their level, never above.
- Weapon and off-hand items are class-specific. Armor and accessories are shared by all classes.

### 5.4 Item names

- Each item type has **10 names, one per 5 levels**. The name tells the player roughly how strong the base item is.
- **Rarity changes the color, not the name.** A Legendary Oathkeeper is orange; a Normal one is gray.
- Optional: Legendary and Exotic items get a generated suffix, e.g. *Oathkeeper of the Lion*.
- Themes by bracket: 1–10 humble (wood, rust, squire) · 11–20 soldier (iron, steel) · 21–30 veteran (oaths, lions, wardens) · 31–40 mythic (ember, frost, storm) · 41–50 divine (dawn, sun, stars).

**Weapons and off-hands**

| Lv | Sword | Shield | Staff | Tome | Gun | Bullets |
|---|---|---|---|---|---|---|
| 1–5 | Rusty Blade | Plank Guard | Walking Stick | Torn Psalter | Old Flintlock | Lead Shot |
| 6–10 | Squire's Edge | Oak Kite | Ashwood Rod | Novice Hymnal | Rusty Derringer | Paper Cartridges |
| 11–15 | Iron Brand | Iron Round | Pilgrim Staff | Book of Vows | Brass Pistol | Iron Rounds |
| 16–20 | Soldier Sword | Steel Bulwark | Chapel Crook | Choir Codex | Tin Revolver | Brass Shells |
| 21–25 | Oathkeeper | Lion Crest | Vigil Staff | Codex of Dawn | Silver Six | Silver Bullets |
| 26–30 | Lionheart | Warden's Wall | Lantern Staff | Saint's Ledger | Marshal's Iron | Hollow Points |
| 31–35 | Emberbrand | Emberguard | Ember Crozier | Ember Liturgy | Thunderbarrel | Ember Rounds |
| 36–40 | Frostfang | Frostward | Tidecaller | Tome of Tides | Frostbite Repeater | Frost Slugs |
| 41–45 | Dawnbringer | Dawnwall | Halo Staff | Gospel of Light | Starfall Six | Storm Shells |
| 46–50 | Sunforged Blade | Celestial Bastion | Seraph Staff | Seraph Scripture | Sunshot | Sunfire Rounds |

| Lv | Axe (main) | Axe (off) | Wand | Orb | Fists (Monk) | Beads (Monk) |
|---|---|---|---|---|---|---|
| 1–5 | Woodcutter | Kindling Axe | Twig Wand | Cloudy Orb | Cloth Wraps | Wooden Beads |
| 6–10 | Chopper | Throwing Axe | Birch Wand | Quartz Orb | Leather Wraps | Clay Beads |
| 11–15 | Raider Axe | Raider Hook | Glyph Wand | Glyph Orb | Iron Knuckles | River Stone Beads |
| 16–20 | Bone Splitter | Skull Biter | Scholar's Wand | Scholar's Globe | Tiger Claws | Jade Beads |
| 21–25 | Bloodfang | Bloodclaw | Arcanist Wand | Arcane Sphere | Jade Fists | Temple Beads |
| 26–30 | Red Maw | Red Tusk | Mooncut Wand | Moon Orb | Crane Talons | Lotus Beads |
| 31–35 | Stormcleaver | Stormhook | Ember Wand | Ember Heart | Ember Palms | Ember Beads |
| 36–40 | Frostbite Axe | Frostclaw | Frostspire | Frost Eye | Frost Knuckles | Frost Beads |
| 41–45 | Worldsplitter | Worldbreaker | Starcaller | Star Core | Thunder Fists | Storm Beads |
| 46–50 | Doomreaver | Doomfang | Astral Wand | Astral Sun | Nirvana Grips | Enlightened Beads |

**Armor**

| Lv | Head | Gloves | Chest | Boots |
|---|---|---|---|---|
| 1–5 | Cloth Cap | Work Gloves | Linen Shirt | Sandals |
| 6–10 | Leather Hood | Leather Grips | Padded Vest | Leather Boots |
| 11–15 | Iron Coif | Iron Gauntlets | Chain Shirt | Iron Greaves |
| 16–20 | Soldier Helm | Soldier Gauntlets | Soldier Cuirass | Soldier Boots |
| 21–25 | Veteran Visor | Veteran Grips | Veteran Hauberk | Veteran Treads |
| 26–30 | Warden Crown | Warden Gauntlets | Warden Plate | Warden Stompers |
| 31–35 | Ember Circlet | Ember Gloves | Ember Mail | Ember Striders |
| 36–40 | Frost Mask | Frost Grips | Frost Carapace | Frost Walkers |
| 41–45 | Dawn Halo | Dawn Gauntlets | Dawn Raiment | Dawn Steps |
| 46–50 | Sunforged Crown | Sunforged Hands | Sunforged Plate | Sunforged Sabatons |

**Accessories**

| Lv | Ring | Bracer | Necklace |
|---|---|---|---|
| 1–5 | Tin Band | Rope Bracer | String Charm |
| 6–10 | Iron Loop | Leather Cuff | Bone Charm |
| 11–15 | Steel Signet | Iron Bracer | Iron Chain |
| 16–20 | Soldier's Seal | Soldier Vambrace | Soldier's Tag |
| 21–25 | Garnet Ring | Etched Bracer | Oath Medallion |
| 26–30 | Oath Ring | Warden Cuff | Warden Locket |
| 31–35 | Ember Loop | Ember Cuff | Ember Talisman |
| 36–40 | Frost Band | Frost Vambrace | Frost Tear |
| 41–45 | Dawn Signet | Dawn Bracer | Dawn Locket |
| 46–50 | Starlight Ring | Starlight Cuff | Starlight Choker |

## 6. Gems and slot systems

| System | Socketed item | Boosts |
|---|---|---|
| **Enchantment** | Enchantment gems | Stats: ATK, crit, defense, physical / fire / cold / lightning damage, resistances, attack speed, move speed |
| **Engraving** | Engraving gems | Primary skill |
| **Inscription** | Sigils | Ult |

Some gems work only for Enchantment, others only for Engraving.

### 6.1 Gem grades (by level bracket)

| Grade | Level bracket | Stat tier range |
|---|---|---|
| Chipped | 1–10 | T1–T2 |
| Cut | 11–20 | T2–T3 |
| Polished | 21–30 | T3–T4 |
| Brilliant | 31–40 | T4–T5 |
| Flawless | 41–50 | T5–T6 |

Gem name = grade + stone, e.g. Chipped Ruby, Brilliant Sapphire, Flawless Topaz.

- Tier = strength of the rolled stat. Example: Physical dmg T1 +5%, T2 +10%; ATK T1 +5, T2 +10.
- Sigils follow the same grade/tier scheme.

### 6.2 Effect depends on the target item

The same gem gives different effects in a weapon, armor or accessory. Example, **Chipped Ruby**:

| Target | Effect |
|---|---|
| Weapon | Fire dmg +5 (T1) |
| Armor | Fire resist +5% (T1) |
| Accessory | Random one of: Attack speed +5 (T2), Move speed +5 (T2) |

Gem themes: Ruby = fire, Sapphire = cold, Topaz = lightning, Emerald = poison/nature, Diamond = physical/defense (later).

Engraving gems and Sigils work the same way: each has several possible effects for the skill it boosts, e.g. an Engraving gem on Shield Bash rolls "+10% damage" **or** "+0.5 s stun".

### 6.3 Flow

1. Player selects an item, then drags a gem onto a free slot.
2. UI detects the item type and previews every possible result.
3. Player confirms; the effect is rolled when more than one is possible.
4. **Remove**: player picks which slot to clear (Enchantment, Engraving or Inscription). Default: gem returns to inventory for a gold cost (open, §13).
5. **Re-roll**: re-rolls a multi-option effect. A few free re-rolls per day; after that, re-rolls cost Soulstones.

## 7. Professions: Crafting and Salvage

- Each profession has its own level, **1–50**, separate from hero level, in brackets 1–10, 11–20, 21–30, 31–40, 41–50.
- Profession XP comes from crafting/salvaging. Higher level unlocks the next bracket's materials and recipes and improves results (better items, more gold).
- Daily incentive: the first N crafts/salvages each day give bonus XP (no hard daily cap).

### 7.1 Materials

Materials drop from enemies the hero kills.

| Bracket | Materials |
|---|---|
| 1–10 | Wood, Hide, Chipped Topaz, Chipped Ruby, Chipped Emerald |
| 11–20 … 41–50 | TBD (one set per bracket) |

Chipped gems (and later grades) are both crafting materials and Enchantment gems.

### 7.2 Crafting recipes (bracket 1–10)

| Craft | Input | Output |
|---|---|---|
| Weapon | Wood | Random weapon of **any class** (sword, axe, staff, wand, gun, Monk weapon) |
| Off-hand | Wood + Hide | Random off-hand of **any class** |
| Head / Gloves / Chest / Boots | Hide | That slot |
| Accessory | Chipped Topaz + Chipped Ruby + Chipped Emerald | Random ring, necklace or bracer |

- Each craft also costs gold.
- Item level is random within the bracket. Chance of Rare or better is very low; Exotic is extremely rare.
- Accessories are the hardest items to drop, so their recipe costs 3 materials.
- Optional Soulstone hook: pay Soulstones to lock the class of a weapon/off-hand craft.

### 7.3 Salvage

- Destroys an item for **gold + materials** + Salvage XP.
- Wrong-class crafts and unwanted drops feed back into crafting:

```
Fight → drops + materials → Salvage → gold + materials → Craft → random item
  ↑                                                        ↓
  └──────── stronger hero → harder nodes ←── Enchant / Engrave / Inscribe
```

## 8. Shrine (Offering)

- Offering coins drop randomly from enemies, in 5 rarities (Normal → Exotic).
- Offer a coin at the **Shrine** → random item. Better coin = better loot table (e.g. Legendary coin = guaranteed Epic+, chance of Exotic).

## 9. Map and progression

- Difficulties: **Normal, Hard, Nightmare** (Hell and Torment later).
- Each difficulty: 3 stages × 10 nodes, named `Stage 1-1 … 1-10`, `2-1 … 2-10`, `3-1 … 3-10`.
- Nodes unlock in order. Nodes 1–9: waves of enemies + a **mini-boss**. Node 10: **stage boss**.
- Entering a stage boss needs a **key**, built from **key fragments** that drop in that stage's nodes 1–9. Farming is required, and players can see their progress.
- Hard and Nightmare reuse the stage maps with stronger enemies and better drops.
- Total: 90 nodes at launch scope (150 with Hell + Torment).

## 10. Economy

| Currency | Source | Used for |
|---|---|---|
| **Gold** | Drops, Salvage | Crafting, gem removal, everyday costs |
| **Soulstones** (premium) | Purchase (later: small free rewards) | Extra re-rolls, optional craft class lock, paid heroes |

- Apple/Google take 15% (small-business program) to 30% of purchases.
- Purchases and paid heroes need server-side saves (phase P7) to prevent cheating.

## 11. Presentation

Every feature goes through: define → greybox → playtest → animation → VFX/juice → theatrics (big moments only) → commit.

Big moments with scripted theatrics (Unity Timeline + Cinemachine):
- **Boss intro**: arena locks, music cuts, ground shake, camera pan to boss, roar + flash, name card, HP bar fills, boss music, camera returns. 3–5 s, skippable after first view.
- **Boss phase change**: freeze, roar, red tint, new music layer.
- **Boss death**: slow-mo, white flash, explosion, loot rain, "STAGE CLEAR".
- **Wave start/clear**: "WAVE N" banner, "GO →" arrow.
- **Legendary/Exotic drop**: colored light beam, special sound.

## 12. Originality

Pixel Adventure shares genre patterns with **TBH: Task Bar Hero** (class lineup, socket systems, gem-based gear). Mechanics are fine to share; names, item lists and data must be our own.

- **Do not use TBH names**, including: Decoration, Alchemy, Minor Ruby (and the Minor → Chaos gem line), Aegis Field, Sacred Blade, Piercing Thrust, Shield Charge, Retribution Strike, Axe Spin, Bloodlust, Arrow Rain, Flame Hydra, Wrath of Heaven, and their item names (Long Sword, Rapier, Rune Sword, Void Blade, Eternal Sword, Buckler, Heater Shield, Void/Abyss/Eclipse Amulet, etc.).
- TBH rarity names (Common, Uncommon, Immortal, Arcana, Beyond, Celestial, Divine, Cosmic) are not used as rarities.
- 4 of our class names (Knight, Priest, Sorcerer, Slayer) also appear in TBH. They are generic words, but consider more distinctive names before release (open, §13).
- Avoid real brand names in items (e.g. "Colt").

## 13. Open questions

1. Class names: keep Knight/Priest/Sorcerer/Slayer, or rename for distinctiveness?
2. Gem removal: returned to inventory (gold cost), or destroyed?
3. Crafting material names for brackets 11–50 (gem grades are set in §6.1).
4. Monk: confirm role and weapons (fists + prayer beads are tentative).
5. Priest, Gunslinger, Slayer, Sorcerer, Monk: stats, full passive lists, skill tree branches, numbers (skill names set in §4.1).
6. Stat list and numbers for every gem × target combination.
7. How enemy level and drop level scale per node and difficulty.
8. Daily free re-roll count; daily bonus-XP count.

## 14. Build phases

Each phase ends playable, committed and showable. Content is data-driven, so new classes, stages and items are new data, not rewrites.

| Phase | Scope |
|---|---|
| **P1 Core fight** | Knight, Normal Stage 1 (1-1 → 1-10), 3 enemy types, mini-boss, stage boss with intro, waves, drops |
| **P2 Loot loop** | 10 gear slots, 5 rarities, item levels, inventory, Salvage, gold, materials |
| **P3 Build depth** | Hero levels to 50, Knight skill tree, Crafting (bracket 1–10), Enchantment |
| **P4 Heroes** | Priest + Gunslinger, sign-in screen (Guest), hero select |
| **P5 Farming** | Stages 2–3, key fragments, Hard + Nightmare |
| **P6 Advanced** | Engraving, Inscription, Shrine + offering coins, crafting brackets 11–50 |
| **P7 Online** | Google/Apple login, cloud save, Soulstones store, Slayer + Sorcerer |
| **P8 Endgame** | Hell, Torment, Monk |

Current state: P1 partly done (Knight moves, attacks, Shield Bash, Holy Slam, training dummies, HUD, splash, main menu).
