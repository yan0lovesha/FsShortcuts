#r "bin/Debug/FsShortcuts.dll"
open FsShortcuts.Screenshot
open FsShortcuts.HotKeys.Command
open FsShortcuts.HotKeys

Command.commands.AddRange(
    [
        // Mouse Speed
        { HotkeyCommand.Command = "Mouse Slow"; HotkeyCommand.Description = "Set the mouse speed to 2 for trackball"; Action = fun () -> MouseSpeed.setSpeed 2 }
        { HotkeyCommand.Command = "Mouse Fast"; HotkeyCommand.Description = "Set the mouse speed to 6 for mouse"; Action = fun () -> MouseSpeed.setSpeed 6 }

        { HotkeyCommand.Command = "Exit"; HotkeyCommand.Description = "Exit the hotkey app"; Action = Entrance.stop }
    ]
)
FsShortcuts.HotKeys.Entrance.start()
