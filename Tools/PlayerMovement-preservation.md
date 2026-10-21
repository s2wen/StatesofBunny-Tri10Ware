# PlayerMovement preservation audit

Compared with Git HEAD 8080c42 on October 18, 2026.

## Scene and assets

All 53 original PlayerMovement scene records remain intact. Differences to existing records are default fields added by Unity for the extended Push and Exit components, plus the scene root list for newly placed objects. The scene additionally contains the bridge, hole and drains placed during this session; these additions were retained.

Original input bindings and generated input class, ground tile asset, target prefab, heater prefab, cooler prefab and drain prefab match Git byte for byte.

## Corrected compatibility issue

Cardinal grid snapping introduced for Level1 was also affecting PlayerMovement. It now applies only to scenes whose floor has a LevelBoard component. The original scene retains its original diagonal and analog movement.

## Unity regression verification

Loaded the Git version of PlayerMovement in a temporary Unity project with the current runtime scripts. Play Mode checks passed for:

- Gas crossing empty space, with solid and liquid blocked.
- Airflow blocking gas.
- Only solid pushing regular blocks.
- Original target coverage and initially locked exit.
- Cooler gas-to-liquid-to-solid and heater solid-to-liquid-to-gas transitions.
- Manual form cycling.
- Actual diagonal and analog movement through the coroutine.

The earlier Level1 Play Mode verifier also checks liquid drain teleportation and prevents immediate return teleportation.

## Intentional extensions and fixes

Bridge blocks fill holes and become walkable; regular blocks retain their original behavior. Bridge blocks do not count as objective crates. Heating and cooling now refresh player color. Initial target coverage is checked on startup. Drains register on enable so script reloads do not empty their registry. Exit supports a named destination; when unset, the original next-build-index behavior remains. Unused speed assignments and deprecated lookups were removed without changing effective movement speed.
