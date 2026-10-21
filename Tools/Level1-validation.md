# Four-level progression validation

## Scene order

1. `Assets/Scenes/Level1.unity`: Two Deliveries — two crates, two targets, two drains; 4-push solution.
2. `Assets/Scenes/Level2.unity`: Detour — two crates travel along the connecting floor route; two targets and paired drains; heater above the crates, cooler beneath the extended left platform, and a filled lower-right platform gap; 31-push solution.
3. `Assets/Scenes/Level2.5.unity`: the gag level — two targets movable by any form, two bridge blocks, two holes, heater, cooler, and paired drains.
4. `Assets/Scenes/Level3.unity`: the user's previous Level1, including its heaters, cooler, three objectives, two bridge blocks, holes and drains; 37-push solution.

The previous Level1 scene GUID is retained on Level3. The saved source scene had no SolidWall instances; Level3 preserves that layout, so gas can reach its goal area, but cannot complete it until all three targets are covered. Its existing layout is preserved, including the user's concurrent heater rename. The new levels reuse existing prefabs, original floor artwork, tile sizing and movement components. There are no new runtime scripts, instructional overlays, bridge blocks in the first two scenes, invisible wall objects, or destroy-after-use components added.

The merged build order is Main Menu → Tutorial → Level1 → Level2 → Level2.5 → Level3 → Ending. Each gameplay exit has an explicit next-scene destination. Main Menu starts Tutorial; Level3 now loads the existing Ending scene. The level-design Tutorial is the active version, while the main branch's version is preserved as TutorialMainBackup. PlayerMovement retains main's scene, and PlayerMovementLevelDesign preserves the alternative test scene. Backup/test scenes are not included in the build.

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

`Tools > Levels > Rebuild Starter and Gag Levels` replaces Level1, Level2 and Level2.5 from their approved layouts and restores the merged menu/tutorial/four-level/ending build order. It never rebuilds Level3 or overwrites the gameplay prefabs.

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

## Level transition integration — October 20, 2026

Imported the fade feature from `origin/level-transition`, commit `80fa62f` (`feat: fade to black between levels`): its Crossfade animator controller, start/end animation clips, LevelLoader prefab and script, with original asset GUIDs. Adapted Exit to retain named scene destinations and final-level completion. The loader ignores duplicate requests, validates destinations and uses unscaled animation/time for its one-second fade. Corrected the imported canvas scale to one and placed its overlay above other UI.

Added and referenced one loader in Level1, Level2, Level2.5, Level3 and PlayerMovement. The scene comparison verified that only the Exit reference and scene-root additions changed; all pre-existing layout records were preserved. The starter-level builder now includes the loader too. No branch scene layouts or unrelated .DS_Store changes were imported.

Unity 6000.4.1f1 Play Mode verification passed: four black-to-clear fades, three clear-to-black fades, delayed loads in the existing order, full-screen overlay dimensions, locked-exit rejection, duplicate-request suppression, and final-level completion without an extra load. This was a focused transition test; it did not replay the puzzle solutions or change the user's newer airflow layout.

## Merge into main — October 21, 2026

Combined level-design's four puzzle scenes, movable targets, bridge/hole rules and fade loader with main's menus, Ending scene, sound effects, thermal movement cooldown and animated-player support. Legacy scenes retain main's disabled E cycling and gas-through-crate behavior; LevelBoard scenes retain their existing form cycling and crate restrictions. Flat-sprite players safely skip animation calls. No new runtime scripts were introduced by the merge.

Active Tutorial is the level-design version (with its additional airflow). The other Tutorial and PlayerMovement scene versions are preserved as TutorialMainBackup and PlayerMovementLevelDesign, outside the build list. MainMenu script references were repaired after resolving conflicting metadata.

Validation on the merged project in an isolated Unity 6000.4.1f1 copy:

- Compilation/import succeeded; twelve gameplay/menu/test scenes had no missing script components. Asset metadata has no duplicate GUIDs.
- All four full puzzle solutions passed real movement and trigger callbacks, waiting for thermal cooldowns. Level2 and Level2.5 solved using heater/cooler entries without E.
- Locked goals, solid walls, crate restrictions, movable targets in all forms, permanent bridge locking, drains and Level1 → Level2 → Level2.5 → Level3 → Ending passed.
- Main Menu's PlayGame loaded Tutorial. Tutorial rejected its locked exit, then loaded Level1 after the test explicitly unlocked it. This focused check did not solve the Tutorial puzzle.
- Original gameplay transform positions in all four puzzle scenes were preserved. Unity's headless save produced zero-scale canvas overrides; those overrides were removed so the fade canvases inherit the prefab's unit scale.

Logs: /tmp/merge-import.log, /tmp/merge-playtest.log, /tmp/merge-menu-test.log. Unity's editor-only SearchDatabase indexing exception appeared during validation; it did not prevent compilation or successful gameplay tests.

## Follow-up merge with origin/main

Resolved the incoming menu/tutorial update while preserving the active level-design Tutorial, all four puzzle scenes and the build sequence byte-for-byte. TutorialMainBackup now contains the incoming revised Tutorial, with its exit explicitly linked to Level1. Accepted upstream folder metadata and deletion of the unused MainMenu scene (the active scene is Main Menu). Retained the updated credits, close-button artwork and Ending button label; routed menu buttons to the existing valid MainMenu component instead of keeping incoming duplicate components with a stale script GUID.

## Shared player prefab — October 21, 2026

Tutorial, Level1, Level2, Level2.5, Level3 and PlayerMovementLevelDesign now use linked instances of Assets/Prefabs/Player.prefab. PlayerMovement and TutorialMainBackup already used that prefab. Spawn positions, floor/exit references and all other scene objects were preserved; the development-only BunnySpriteTesting scene was not converted. The level builder now creates the same prefab instead of a standalone square player.

The shared prefab uses the bunny's solid sprite as its editor preview, retains the six-state animation controller and movement sounds, adds the drain sound, and uses the existing designed levels' 0.65-unit collision box. PlayerController keeps movement cardinal on the grid, refreshes form animations immediately, leaves animated artwork untinted, and preserves directional sprite flipping. Placeholder players without an animator retain their color feedback.

Unity validation in /tmp/bunny-prefab-validation confirmed prefab connections and non-null floor/exit references in all eight gameplay/alternate scenes. All four puzzle solutions passed with the prefab player, including animation selection, untinted artwork, temperature-only solutions for Level2 and Level2.5, bridge locking, movable targets, drains, and transition to Ending. Logs: /tmp/player-prefab-conversion.log and /tmp/player-prefab-playtest.log.

A separate Tutorial Play Mode test sent actual Input System keyboard events: D moved exactly one tile, simultaneous D/W remained cardinal and grid-aligned, and W moved one tile upward with the matching front animation and upward sprite flip. A rendered preview confirmed the bunny artwork displays correctly. Background input routing was enabled only in the isolated test harness. Log: /tmp/player-prefab-input.log; preview: /tmp/bunny-prefab-preview.png.
