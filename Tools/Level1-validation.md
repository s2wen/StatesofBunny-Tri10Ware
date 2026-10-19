# Four-level progression validation

## Scene order

1. `Assets/Scenes/Level1.unity`: Two Deliveries — two crates, two targets, two drains; 4-push solution.
2. `Assets/Scenes/Level2.unity`: Detour — two crates travel along the connecting floor route; two targets and paired drains; heater above the crates, cooler beneath the extended left platform, and a filled lower-right platform gap; 31-push solution.
3. `Assets/Scenes/Level2.5.unity`: the gag level — two targets movable by any form, two bridge blocks, two holes, heater, cooler, and paired drains.
4. `Assets/Scenes/Level3.unity`: the user's previous Level1, including its heaters, cooler, three objectives, two bridge blocks, holes and drains; 37-push solution.

The previous Level1 scene GUID is retained on Level3. The saved source scene had no SolidWall instances; Level3 preserves that layout, so gas can reach its goal area, but cannot complete it until all three targets are covered. Its existing layout is preserved, including the user's concurrent heater rename. The new levels reuse existing prefabs, original floor artwork, tile sizing and movement components. There are no new runtime scripts, instructional overlays, bridge blocks in the first two scenes, invisible wall objects, or destroy-after-use components added.

The build scene order is Level1, Level2, Level2.5, Level3. Level1 loads Level2; Level2 loads Level2.5; Level2.5 loads Level3. Exit completes the final enabled build scene without loading another scene or looping. The final goal remains visible; no additional win screen is added. Original PlayerMovement, SampleScene and Main Menu files remain available but are not part of this four-level build sequence.

## Verification

Unity 6000.4.1f1 Play Mode checks, October 19, 2026:

- Complete crate solutions replayed through the real PlayerController movement coroutines and Push code.
- Level1 and Level3 form changes use the same ChangeState handler as the E key. Level2 and Level2.5 use only heater/cooler trigger entries during its full solution, without E.
- Every target must be covered before the goal opens.
- Gas touching an unsolved goal cannot progress.
- All three forms are blocked by every goal-room solid wall in the new Level1 and Level2 scenes.
- Liquid and gas cannot push ordinary crates; ordinary crates cannot be pushed into missing floor to bypass the detour.
- Actual liquid trigger entry teleports through the runtime drain network into each goal area.
- Actual goal entry loads Level2, then Level2.5, then Level3; final goal entry completes Level3.
- Both Level3 bridge blocks fill holes and refuse subsequent pushes.
- Drain counts stay correct across scene transitions.
- Rendered all four scenes and checked visibility, continuous borderless tiles and complete square-view camera framing.

This proves a working solution for each level, not that every possible crate placement is recoverable. R restarts a level if a crate is pushed into a dead end. WASD/arrows move; E cycles Solid, Liquid and Gas.

## Reproduce

Run `python3 Tools/solve_level1.py` to regenerate the four routes in `Tools/level1-solution.json`. The historical filenames are retained for existing tooling.

Run Unity in an isolated project copy with `-batchmode -nographics -executeMethod VerifyLevel1.Run` (omit `-quit`; the verifier exits with success/failure). Success logs `PROGRESSION_PLAYTEST_PASSED`.

`BuildLevel1.Capture` renders all four scenes to `/tmp/LevelN-preview.png`; run it without `-nographics`.

`Tools > Levels > Rebuild Starter and Gag Levels` replaces Level1, Level2 and Level2.5 from their approved layouts and restores the four-scene build order. It never rebuilds Level3 or overwrites the gameplay prefabs.

The isolated validation project's existing ShaderGraph package-cache compatibility fixes (UnityEngine.GUID to UnityEditor.GUID in three cache files) were retained. Source-project packages were not modified. An editor SearchDatabase indexing exception occurred during the isolated import; it did not prevent compilation or the gameplay checks.

## Level2 sketch update

Applied the October 19 screenshot on the existing one-unit grid:

- Heater at (3, -2), directly above the upper crate.
- Added floor x=3..5, y=-7..-9 and a cooler floor cell at (3, -10).
- Filled the lower-right notch at (8, -8).
- Cooler at (3, -10), using the original blue Cooler prefab.
- Preserved the crates, objectives, player spawn, drains, enclosed goal, camera and Level3 destination.

The revised solver uses 141 moves including 31 pushes, five heater entries, four cooler entries, a liquid drain transfer and goal entry. Every move is checked against the expected form in Play Mode; the route does not call ChangeState. Manual E controls remain available because disabling them was not requested. The two new objects are existing Heater and Cooler prefab instances; no runtime scripts were added or changed for this update.

## Level2.5 gag level

Uses the screenshot's upper rooms, heater, movable green targets and enclosed goal room. The new lower-left room occupies x=1..5, y=-10..-12, with a cooler at (1,-12), yellow bridge blocks at (3,-11) and (4,-11), and two consecutive white hole cells at (4,-8) and (4,-9). The numbered connection is exactly three cells: floor (4,-7), hole (4,-8), hole (4,-9); the lower room starts at y=-10. The right platform is widened with x=10, y=-6..-8. Floor remains continuous and borderless.

Only Level2.5 enables BlockTarget's serialized `movable` option. Solid, liquid and gas can push an uncovered target. Targets stay on floor or completed bridges, cannot overlap one another, and may slide underneath an ordinary objective crate to count as covered. A crate already on a target continues to block player movement normally. All other scenes retain fixed targets. The goal reevaluates when either a crate or a movable target finishes moving.

The full solution uses six bridge pushes and 31 target pushes in 143 moves, including temperature triggers, the liquid drain transfer and goal entry. The normal crates stay in place while the targets travel to them. The Play Mode verifier also separately pushes a target with all three forms, checks target movement cannot enter void, checks both bridge blocks lock permanently, and checks target mobility is disabled in the other scenes. No new runtime script files or new prefab assets were needed; the existing BlockTarget, PlayerController and PuzzleManager components were extended.
