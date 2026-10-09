# MicroApp 5.4.0 — Screen Color Picker and Productivity Shortcuts Hub

Screen Color Picker with magnifier loupe & color inspector, plus an offline Productivity Shortcuts Cheat Sheet Hub.

## Added

* **Screen Color Picker with Loupe & Inspector.** Press **Ctrl+Alt+C** (or choose *Pick Color* from the tray menu) to inspect and pick pixel-accurate colors from any screen:
  * **Real-time 11×11 pixel magnifier loupe** follows your cursor with a reticle and live HEX badge.
  * **Single-click instant copy**: Copies `#RRGGBB` directly to clipboard with a toast notification.
  * **Color Details inspector dialog**: View preview swatches, copyable HEX, RGB, and HSL formats with dedicated copy buttons.
  * **Recent Colors palette**: Persists up to 10 recently picked colors across sessions for fast re-inspection and reuse.

* **Productivity Shortcuts Cheat Sheet Hub.** A comprehensive dark-themed offline shortcut reference accessible from the tray menu (*Shortcuts Cheat Sheet*):
  * **145 essential shortcuts across 4 applications**:
    * **Windows** (55 shortcuts): Window management, desktop & taskbar, accessibility, system navigation.
    * **Microsoft Word** (32 shortcuts): File & basic actions, formatting, clipboard, alignment, navigation & edit.
    * **Microsoft Excel** (30 shortcuts): File & basics, selection & navigation, formulas & calculations, cell formatting, rows & columns, workbook & sheets.
    * **Microsoft PowerPoint** (28 shortcuts): Slide show presentation, slide management, formatting & editing, shapes & objects, view & navigation.
  * **App Switcher tabs** with active badges to quickly jump between applications.
  * **Categorized section divider cards** matching cheat sheet layouts.
  * **Instant real-time search** to filter any shortcut by action name or key combination.

* **ModernToggle switch control.** Smooth fluent toggle switch component with hover transitions and keyboard accessibility.

## Fixed

* **Shortcut Settings reset:** Fixed `InvalidCastException` when restoring default hotkey settings with string-typed defaults.

Everything new in 5.3.3 (Always on Top stays above Remote Desktop) and 5.3.2 (Always on Top, capture timer and Save As, ruler units and origin) is included.

## Installing

Three downloads: `MicroApp-5.4.0-setup.exe` (all users, Program Files), `MicroApp-5.4.0-peruser-setup.exe` (just you, no admin), or the portable `MicroApp-5.4.0-win-x64.zip`. Each is about 165 MB because the offline background-removal model is included. Details and silent switches are in [SETUP.md](https://github.com/Mahi-BD/MicroApp/blob/main/SETUP.md). Installing over an older version keeps your settings and notes.
