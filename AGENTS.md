# AGENTS.md — Unity-AI-Tilemap

Unity package `com.ivanmurzak.unity.mcp.tilemap` — wraps Unity's built-in **Tilemap** module
(`com.unity.modules.tilemap`) plus **2D Tilemap Extras** (`com.unity.2d.tilemap.extras`, for RuleTiles) available
to Unity-MCP's RQL tools (`rql-query` / `rql-mutate`; no extension MCP tools): the `tilemap` source
(`CreateTilemap`, `CreateTile`, `CreateRuleTile`), Tilemap painting methods (SetTile, BoxFill, FloodFill, …),
RuleTile `AddRule` / `ClearRules` and the read-only `tilemap.*` cell functions. An extension of
[AI Game Developer](https://github.com/IvanMurzak/Unity-MCP) (`com.ivanmurzak.unity.mcp`), generated from
`Unity-AI-Tools-Template`; the user-facing tool list is in `README.md`.

## Layout

- `Unity-Package/` — Unity project hosting the package; only `Packages/com.ivanmurzak.unity.mcp.tilemap/`
  ships.
  - `package.json` — the version source of truth and the dependency pins.
  - `Editor/Scripts/Rql/` — branch `experiment/rql`: the `TilemapModule` (IRqlModule: `tilemap` source, `tilemap.*`
    functions, one adapter per file, the agent guide); replaces the former `tilemap-*` tools.
  - `Runtime/` — runtime assembly (currently empty).
  - `Tests/Editor/Rql/` (EditMode, `TilemapTestBase` + one file per adapter / source) and `Tests/Runtime/` (PlayMode).
- `Unity-Tests/<unity-version>/` — one test project per supported Unity version, consuming the package via
  `file:`. A new version must also be added to `test_pull_request.yml` and `release.yml` (and
  `open-all-projects-*`).
- `Installer/` — Unity project that exports the `.unitypackage` installer
  (`Assets/com.IvanMurzak/<name> Installer/`).
- `commands/` — PowerShell helpers (below) and `check-versions.py` (synced in by the release cascade — do not
  hand-edit; CI runs it as the `version-consistency` gate).
- `.github/workflows/` — `test_pull_request.yml` (PR test matrix), `test_unity_plugin.yml` (reusable test
  job), `release.yml`, `bump_version.yml`.
- `docs/` — user docs: `Deploy-{OpenUPM,GitHub,npmjs}.md`, `Manual-Package-Rename.md`, `openupm-signing.md`,
  `img/`, `promo/`.

## Commands (PowerShell, from the repo root)

```powershell
./commands/bump-version.ps1 -NewVersion "x.y.z"   # package.json + Installer.cs in lock-step; -WhatIf to preview
./commands/get-version.ps1
./commands/update-ai-game-developer.ps1           # bump the com.ivanmurzak.unity.mcp pin; -WhatIf to preview
./commands/open-all-projects-windows.ps1          # open all Unity projects (or -unix.sh)
```

Tests run inside the Unity Editor (NUnit + `[UnityTest]`); CI runs them with `game-ci/unity-test-runner` in
EditMode / PlayMode / Standalone for every Unity version. `release.yml` releases on push to `main` only when
the `package.json` version has no tag yet (installer, signed UPM tarball for OpenUPM, test matrix, tag, GitHub
Release); dispatch `bump_version.yml` to bump in CI. Under `pull_request_target`, tests need the
maintainer-applied `ci-ok` label and abort if the PR touches `.github/workflows/`.

## Critical invariants

- **RQL surface.** Everything is registered by `TilemapModule` (`IRqlModule`, Order 100, found through
  TypeCache) and documented in its `AddGuide("Tilemap", …)` text, which is rendered into the `unity-rql` skill:
  keep the guide examples in sync with the adapters and functions.
- **Undo.** Every tile write records the whole Tilemap first (`TilemapOps.Record`); created assets are not undone.
- **Limits.** Writes are capped at `TilemapOps.MaxCells` cells per call, scans at `MaxScanCells`; function
  results at `TilemapFunctions.MaxList` items (RQL truncates longer lists). Function errors are returned as
  `RqlError`, not thrown.
- Every C# file starts with the copyright box header and `#nullable enable`.
