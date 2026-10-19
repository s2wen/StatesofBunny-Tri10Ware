# Level1 validation

Unity 6000.4.1f1 Play Mode verification passed on October 18, 2026.

- Replayed a complete solution: 37 pushes, 101 cardinal moves, with legal form changes.
- Both bridge blocks filled holes, became static and refused further pushes.
- Regular objective blocks could not enter unfilled holes.
- Solid could not enter an unfilled hole or a dark wall cell; gas could cross both.
- Gas uses the original PlayerMovement rule: it can cross missing floor, including space beyond the board.
- Heaters, coolers and liquid drain teleportation were checked.
- The exit stayed locked before solving the targets.
- All three targets unlocked the exit; entering it actually loaded Level2.
- Every runtime script and the delivered Level1 scene matched the tested copy.
- Rendered the scene in Unity and visually checked the grid, object visibility and complete camera framing.

The first clean import exposed existing ShaderGraph GUID compatibility errors. Three files in the temporary validation copy's package cache were adjusted from UnityEngine.GUID to UnityEditor.GUID so Unity could compile that copy. Project package files were not changed.

## Repeat the test

Run `python3 Tools/solve_level1.py` to regenerate `Tools/level1-solution.json`.
Run Unity in batch mode with `-executeMethod VerifyLevel1.Run` (omit `-quit`; the verifier exits with success or failure).
`Tools > Levels > Build Level1 From Plan` rebuilds the scene and its generated assets, replacing edits to them.

Open Assets/Scenes/Level1.unity to play. WASD/arrows move; E cycles forms; R restarts the scene.
Level2 is the existing destination scene, which currently contains only a camera.

## Enclosed goal room update

Added 22 SolidWall prefab instances around the lower room (outer ring x=4..10, y=-1..4), preserving every existing scene object. Verified in Unity that all three forms are blocked at every perimeter cell, liquid can enter through the existing drain and walk to the goal, and the goal remains locked until the objectives are covered. After setting the objectives to their completed state, reaching the goal loaded Level2. Rendered and visually checked the updated perimeter.

The earlier 101-move solver route predates this enclosure: its final walking approach to the goal is now intentionally blocked. Entry into the goal room now requires the drain. The full-route solver and VerifyLevel1 replay describe the earlier layout; the perimeter update was verified with targeted checks as described above.
