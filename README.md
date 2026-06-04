<h1 align="center"><a href="https://github.com/IvanMurzak/Unity-AI-Tilemap?tab=readme-ov-file#unity-ai-tilemap">Unity AI Tilemap</a></h1>

<div align="center" width="100%">

[![MCP](https://badge.mcpx.dev 'MCP Server')](https://modelcontextprotocol.io/introduction)
[![OpenUPM](https://img.shields.io/npm/v/com.ivanmurzak.unity.mcp.tilemap?label=OpenUPM&registry_uri=https://package.openupm.com&labelColor=333A41 'OpenUPM package')](https://openupm.com/packages/com.ivanmurzak.unity.mcp.tilemap/)
[![Unity Editor](https://img.shields.io/badge/Editor-X?style=flat&logo=unity&labelColor=333A41&color=2A2A2A 'Unity Editor supported')](https://unity.com/releases/editor/archive)
[![r](https://github.com/IvanMurzak/Unity-AI-Tilemap/workflows/release/badge.svg 'Tests Passed')](https://github.com/IvanMurzak/Unity-AI-Tilemap/actions/workflows/release.yml)</br>
[![Discord](https://img.shields.io/badge/Discord-Join-7289da?logo=discord&logoColor=white&labelColor=333A41 'Join')](https://discord.gg/cfbdMZX99G)
[![Stars](https://img.shields.io/github/stars/IvanMurzak/Unity-AI-Tilemap 'Stars')](https://github.com/IvanMurzak/Unity-AI-Tilemap/stargazers)
[![License](https://img.shields.io/github/license/IvanMurzak/Unity-AI-Tilemap?label=License&labelColor=333A41)](https://github.com/IvanMurzak/Unity-AI-Tilemap/blob/main/LICENSE)
[![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/badges/StandWithUkraine.svg)](https://stand-with-ukraine.pp.ua)

</div>

AI-powered tools for the Unity [Tilemap](https://docs.unity3d.com/Manual/class-Tilemap.html) workflow. Create `Grid` + `Tilemap` GameObjects, paint and clear tiles, box-fill regions, read individual tiles, create `Tile` and `RuleTile` assets, set per-cell collider type, color, and transform, configure tilemap orientation and anchor, list / get tilemap components, and modify any tilemap component field directly through natural language commands — no manual tile-palette painting. Wraps Unity's built-in **Tilemap** module (`com.unity.modules.tilemap`) plus the **2D Tilemap Extras** package (`com.unity.2d.tilemap.extras`) for RuleTiles. Ideal for rapidly building 2D levels, prototyping grid layouts, and procedural tilemap generation. Built on top of the [AI Game Developer](https://github.com/IvanMurzak/Unity-MCP) platform.

### How to use

- [Instructions](https://github.com/IvanMurzak/Unity-MCP?tab=readme-ov-file#step-2-install-mcp-client)
- [Video Tutorial for Visual Studio Code](https://www.youtube.com/watch?v=ZhP7Ju91mOE)
- [Video Tutorial for Visual Studio](https://www.youtube.com/watch?v=RGdak4T69mc)

[![DOWNLOAD INSTALLER](https://github.com/IvanMurzak/Unity-MCP/blob/main/docs/img/button/button_download.svg?raw=true)](https://github.com/IvanMurzak/Unity-AI-Tilemap/releases/latest/download/AI-Tilemap-Installer.unitypackage)

### Stability status

| Unity Version | Editmode                                                                                                                                                                                                          | Playmode                                                                                                                                                                                                          | Standalone                                                                                                                                                                                                          |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 2022.3.62f3   | [![r](https://github.com/IvanMurzak/Unity-AI-Tilemap/workflows/release/badge.svg?job=test-unity-2022-3-62f3-editmode)](https://github.com/IvanMurzak/Unity-AI-Tilemap/actions/workflows/release.yml)       | [![r](https://github.com/IvanMurzak/Unity-AI-Tilemap/workflows/release/badge.svg?job=test-unity-2022-3-62f3-playmode)](https://github.com/IvanMurzak/Unity-AI-Tilemap/actions/workflows/release.yml)       | [![r](https://github.com/IvanMurzak/Unity-AI-Tilemap/workflows/release/badge.svg?job=test-unity-2022-3-62f3-standalone)](https://github.com/IvanMurzak/Unity-AI-Tilemap/actions/workflows/release.yml)       |
| 2023.2.22f1   | [![r](https://github.com/IvanMurzak/Unity-AI-Tilemap/workflows/release/badge.svg?job=test-unity-2023-2-22f1-editmode)](https://github.com/IvanMurzak/Unity-AI-Tilemap/actions/workflows/release.yml)       | [![r](https://github.com/IvanMurzak/Unity-AI-Tilemap/workflows/release/badge.svg?job=test-unity-2023-2-22f1-playmode)](https://github.com/IvanMurzak/Unity-AI-Tilemap/actions/workflows/release.yml)       | [![r](https://github.com/IvanMurzak/Unity-AI-Tilemap/workflows/release/badge.svg?job=test-unity-2023-2-22f1-standalone)](https://github.com/IvanMurzak/Unity-AI-Tilemap/actions/workflows/release.yml)       |
| 6000.3.1f1    | [![r](https://github.com/IvanMurzak/Unity-AI-Tilemap/workflows/release/badge.svg?job=test-unity-6000-3-1f1-editmode)](https://github.com/IvanMurzak/Unity-AI-Tilemap/actions/workflows/release.yml)        | [![r](https://github.com/IvanMurzak/Unity-AI-Tilemap/workflows/release/badge.svg?job=test-unity-6000-3-1f1-playmode)](https://github.com/IvanMurzak/Unity-AI-Tilemap/actions/workflows/release.yml)        | [![r](https://github.com/IvanMurzak/Unity-AI-Tilemap/workflows/release/badge.svg?job=test-unity-6000-3-1f1-standalone)](https://github.com/IvanMurzak/Unity-AI-Tilemap/actions/workflows/release.yml)        |

## AI Tilemap Tools

13 tools, grouped by purpose:

### Tilemap lifecycle

- `tilemap-create` - Create a `Grid` + `Tilemap` GameObject in the active scene
- `tilemap-list` - List all `Tilemap`s in the active scene
- `tilemap-get` - Get a `Tilemap` / `TilemapRenderer` component's data via ReflectorNet
- `tilemap-set-orientation` - Set the tilemap's anchor and orientation

### Painting

- `tilemap-set-tile` - Set (paint) a tile at a cell position
- `tilemap-box-fill` - Box-fill a rectangular region of cells with a tile
- `tilemap-clear` - Clear all tiles from a tilemap
- `tilemap-get-tile` - Read the tile at a cell position
- `tilemap-set-tile-flags` - Set a tile's color and transform (flags) at a cell
- `tilemap-set-collider-type` - Set a tile's collider type (None / Sprite / Grid) at a cell

### Tile assets

- `tilemap-create-tile-asset` - Create a `Tile` asset (from a sprite)
- `tilemap-create-rule-tile` - Create a `RuleTile` asset (2D Tilemap Extras)

### Generic

- `tilemap-modify` - Generic write: apply a `SerializedMember` diff to any tilemap component via ReflectorNet (escape hatch for fields not covered by the dedicated tools)

## Installation

### Option 1 - Installer

- **[Download Installer](https://github.com/IvanMurzak/Unity-AI-Tilemap/releases/latest/download/AI-Tilemap-Installer.unitypackage)**
- **Import installer into Unity project**
  > - You can double-click on the file - Unity will open it automatically
  > - OR: Open Unity Editor first, then click on `Assets/Import Package/Custom Package`, and choose the file

### Option 2 - OpenUPM-CLI

- [Install OpenUPM-CLI](https://github.com/openupm/openupm-cli#installation)
- Open the command line in your Unity project folder

```bash
openupm add com.ivanmurzak.unity.mcp.tilemap
```
