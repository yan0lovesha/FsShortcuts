#r "bin/Debug/FsWork.dll"
open FsWork.Screenshot
open FsWork.HotKeys.Command
open FsWork.HotKeys

Command.commands.AddRange(
    [
        // Mouse Speed
        { HotkeyCommand.Command = "Mouse Slow"; HotkeyCommand.Description = "Set the mouse speed to 2 for trackball"; Action = fun () -> MouseSpeed.setSpeed 2 }
        { HotkeyCommand.Command = "Mouse Fast"; HotkeyCommand.Description = "Set the mouse speed to 6 for mouse"; Action = fun () -> MouseSpeed.setSpeed 6 }

        { HotkeyCommand.Command = "Exit"; HotkeyCommand.Description = "Exit the hotkey app"; Action = Entrance.stop }
    ]
)
FsWork.HotKeys.Entrance.start()