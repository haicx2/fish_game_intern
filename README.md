# Fish Match Puzzle (Internship Project)

This is a match-3 puzzle game I worked on during my internship. The company provided a base match-3 template (originally featuring a fruit theme and basic animations), and my task was to completely overhaul the core gameplay mechanics, update the UI, refactor the game loop for stability, and reskin the game.

I changed the traditional adjacent-tile-swapping gameplay into a tray-based puzzle mechanic, where players must strategically clear the board without overflowing their inventory.

## How it Works

- **Matching:** When you click a fish on the board, it moves to a 5-slot tray at the bottom of the screen. If you get 3 of the same fish in the tray, they clear out.
- **Winning and Losing:** You win by clearing every fish off the board. You lose if you run out of moves (Normal mode) or time (Time Attack) before clearing the board.

## My Contributions

- **Gameplay & Theme Overhaul:** Reworked the core game loop to use the tray-based matching system and completely reskinned the original fruit assets to a new fish theme.
- **Custom Board Generation:** I wrote the board generation logic to make sure items always spawn in perfectly divisible groups of 3, ensuring every board is mathematically solvable. I also set it up so that every available type of fish is guaranteed to spawn at least once per game.
- **New Game Modes:**
  - **Normal Mode:** Players must clear the board within a limited number of moves.
  - **Time Attack:** A faster-paced mode where players only have 60 seconds.
- **Custom Time Attack Mechanics:**
  - I added a "take-back" mechanic exclusively for Time Attack. Players can click a fish in the tray to send it back to its original spot on the board if they make a mistake.
  - **Strict Rule Enforcement:** I implemented logic to strictly enforce this take-back rule, disabling it entirely in Normal mode so moves cannot be undone, preserving the strategic challenge.
  - I set up dynamic UI warnings. When the timer hits 10 seconds, the text turns red and the code dynamically instantiates a custom Bomb sprite to add tension.
- **Architecture & Bug Fixing (Refactoring the Game Loop):**
  - **Deterministic Execution:** I encountered a race condition where the timer hitting zero and a winning click occurring on the exact same frame could cause conflicting win/lose states. I fixed this by stripping out Unity's automatic `Update()` calls in the controller and timer scripts, replacing them with a custom `Tick()` method. This centralizes the execution order within the `GameManager`, guaranteeing the timer and move conditions are safely evaluated before processing player input.
  - **Optimized Execution:** Fixed an underlying bug where the board controller was accidentally being invoked twice per frame by consolidating its execution into the new centralized `Tick()` system.
- **Scalable UI Integration:**
  - I ripped out the base template's hard-coded Unity GUI debug boxes and fully connected the UI to a scalable `IMenu` interface system (`UIMainManager`).
  - The game state now cleanly triggers the actual production full-screen `PanelWin` and `PanelGameOver` screens without any overlapping UI elements.

## Tech Stack & Assets

- **Engine:** Unity 2020.3.38f (C#)
- **Base Template:** Provided by the internship program.
- **Animations:** DOTween (included in the original provided codebase to handle basic item scaling and movement).

## Opening the Project

1. Clone the repo.
2. Open the project folder with Unity Hub, using Unity **2020.3.38f**.
3. Open the main scene under `Assets/Scenes/`.
4. Press Play.
