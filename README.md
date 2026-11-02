# Burger Invaders

A burger-themed take on Space Invaders 3D, built on top of the class starter project for
Assignment 1 (Video Game Design and Development, Fall 2026).

You play a hungry customer throwing takeout boxes at a fleet of burgers. Catch every burger,
then beat the Giant Burger at the end of each wave.

Built with Unity 6 (6000.6.0f1). Open the project and play `Assets/Scenes/SampleScene.unity`.

---

## How to play

### Controls
| Key | Action |
|---|---|
| ← / → or A / D | Move left and right |
| Shift | Dash in the direction you last moved (recharges after 2 seconds; you can't be hit mid-dash) |
| Space | Start the game from the title screen / play again from the end screen |
| Esc (or the gear button, top right) | Pause menu, with Resume and How to Play |
| Mouse click, or 1 / 2 / 3, or arrows + Enter | Pick a difficulty or buff card |
| R (or the reroll button) | Reroll the highlighted buff card (once per card) |

You throw takeout boxes automatically, so focus on moving and dodging.

### Flow of a run
1. **Title screen.** Press Space.
2. **Pick a difficulty:** Easy, Medium or Hard.
3. **Pick a starting buff** from the cards. Each card can be rerolled once.
4. **Wave 1:** 20 burgers march across the screen in formation, switching between March, Wave
   and Zigzag patterns, and every so often one dives at you.
5. **Giant Burger boss:** a WARNING flashes, then the boss flies in. It has a health bar, fires
   volleys of bullets and does jump attacks. The spot it'll land on flashes first.
6. **Pick another buff**, then **Wave 2:** 24 burgers that each wander on their own while the
   group sinks toward you, followed by a tougher Giant Burger.
7. Beat the second boss to win. The end screen shows your wave and score.

### Health, pickups and buffs
- You have **3 hearts**, shown by the health bar. A burger bullet, or a burger ramming you, costs
  one heart, and you blink and can't be hit for a moment afterward. Lose all hearts and it's game over.
- Every 5th burger you catch may drop a pickup. Touch it to collect it:
  - **Lettuce:** restores a heart.
  - **Patty:** gives a shield bubble that absorbs the next hit.
  - **Star (rare):** lets you pick an extra buff.
- **Buffs:** Speedy Feet (move 20% faster), Double Order (+1 box per throw), Side Salad (+1 heart),
  Fast Hands (throw 20% faster).

### How you lose
- You run out of hearts, or
- a burger gets completely below your firing line, so it can no longer be caught.

---

## Assignment requirements

### Required
| Requirement | What I did | Where |
|---|---|---|
| Remaining aliens speed up as others are destroyed | The fleet's speed scales from `startSpeed` with a full fleet up to `lastAlienSpeed` with one burger left (each wave sets its own values). Dive frequency increases the same way. | `AlienFleet.FixedUpdate` |
| Detect when the game can no longer be won | The game ends as soon as any burger is entirely below the ship's firing line (boxes only travel upward, so it can't be caught), or when the ship is destroyed. | `AlienFleet.FixedUpdate`, `AlienFleet.LoseGame` |
| Prevent the ship from moving offscreen | The ship is clamped to the camera's visible edges, measured every frame (so it still works if the window is resized) and accounting for the width of the ship's own sprite. Dashing is clamped too. | `Spaceship.keepOnScreen` |
| Balance the game | Three difficulty levels; burgers dive at the player and fire faster while diving; two waves with different formations; a boss with volleys and jump attacks that gets more aggressive below half health. Easy is noticeably gentler, while Hard is faster and tougher. | `DifficultyManager`, `WaveManager`, `BossBurger` |

### Code issues fixed in the starter code
Compared with the original starter code (`upstream/main`):

1. **The alien counter rebuilt its text every frame.** `AlienCounter.Update()` reassigned
   `uiText.text` (allocating a new string) 60+ times a second even when nothing changed. The text is
   now updated only when the count changes, through a method the fleet calls.
2. **The fleet pushed its count to the UI every frame.** `AlienFleet.Update()` existed only to copy
   `aliens.Count` into `alienCounter.score` each frame. It's removed, and the count is sent when an
   alien is destroyed or a wave spawns.
3. **Redundant `GetComponent` and poor encapsulation in `Alien`.** `Alien.OnCollisionEnter` did
   `fleet.GetComponent<AlienFleet>().aliens.Remove(gameObject)`, even though `fleet` was already an
   `AlienFleet`, and it reached into the fleet's list directly. The list is now a typed `List<Alien>`,
   and aliens report through `fleet.AlienDestroyed(...)`, which also updates the counter and checks for
   the end of the wave.
4. **Empty Unity message methods.** Every script had empty `Start()` and `Update()` methods. Unity
   still calls these every frame, so they were removed.
5. **Dead and unused code.** Removed the unused `lowerBound` field and its commented-out block in
   `AlienFleet`, and the unused `using UnityEngine.UI` and `using UnityEngine.InputSystem` lines.
6. **Movement direction stored in the speed's sign.** The fleet flipped `speed = -speed` and then
   needed `Mathf.Abs(speed)` to move down. Direction is now its own `direction` variable, so speed is
   always positive and can change with the speed-up without sign bugs. Collider lookups the fleet
   needs every physics step are also cached once in `Alien.Awake`.
7. **Bullet collisions could double-count.** `OnCollisionEnter` can fire once per collider touched,
   so one box could "catch" two burgers in the same step. A hit flag and a `captured` check on the
   box now prevent that.

### Optional features (four required)
| Option | What I did |
|---|---|
| **Starting and ending screens** | Title screen; difficulty and buff selection; "You Win!" / "Game Over" end screen with the reason, wave and score; Space to play again. There's also a pause menu with a How to Play page. (`GameScreens`, `PauseMenu`) |
| **Additional levels** | Two waves, each ending in a Giant Burger boss. Wave 2 uses a different formation (6×4 instead of 10×2), burgers that move independently instead of as one block, and a tougher boss. A "WAVE 2" banner appears in between. (`WaveManager`, `BossFight`, `BossBurger`) |
| **Lives with a display** | 3 hearts shown on a health bar, brief invincibility (blinking) after each hit, and lettuce pickups to heal. (`Spaceship`, `HealthBar`) |
| **Better graphics** | The ship is replaced by the player character, who shouts each time they throw. The aliens are burgers and the darts are takeout boxes with hinged lids. |
| **Collision animations** | A box that catches a burger snaps its lid shut, floats up and shrinks away; the ship blinks when hit; the boss squashes and the camera shakes when it lands; the boss shrinks away when defeated. |
| **Shields** | Patty pickups put a shield bubble around the ship that absorbs one hit. |

### Other additions
Difficulty selection, a boss fight, burger dives and movement patterns, a dash with a recharge
indicator, loot drops, a buff/upgrade system with rerolls, scoring, and a pause menu.
