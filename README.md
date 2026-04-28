# ClipVault

ClipVault is a minimalist background utility for Windows that provides ten independent clipboard slots. It is designed for power users and developers who require quick access to multiple text snippets without overwriting their primary system clipboard.

---

## Core Functionality

The application listens for global hotkeys to capture or inject text into any active window. 

### Hotkeys

| Action | Shortcut |
| :--- | :--- |
| **Save to Slot** | Alt + [0-9] |
| **Paste from Slot** | Ctrl + Alt + [0-9] |

---

## Technical Features

- **Persistence**: Saved snippets are stored locally in the user AppData directory, ensuring data is retained after system restarts.
- **Global Availability**: High-performance hotkey registration allows for instant use across all Windows applications.
- **Background Operation**: The application runs in the system tray to minimize workspace clutter.
- **Architecture**: Built using .NET and WPF with low-level P/Invoke keyboard event handling for zero-latency response.

---

## Noetic Kit

This project is part of Noetic Kit, a suite of focused productivity utilities.