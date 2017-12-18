#r "bin/Debug/FsShortcuts.dll"
open FsShortcuts.Screenshot
open FsShortcuts.HotKeys.Command
open FsShortcuts.HotKeys
open FsShortcuts.Common

Command.commands.AddRange(
    [
        // Mouse Speed
        { HotkeyCommand.Command = "Mouse Slow"; HotkeyCommand.Description = "Set the mouse speed to 1 for trackball"; Action = fun () -> MouseSpeed.setSpeed 1 }
        { HotkeyCommand.Command = "Mouse Fast"; HotkeyCommand.Description = "Set the mouse speed to 6 for mouse"; Action = fun () -> MouseSpeed.setSpeed 6 }

        // Hosts files
        { HotkeyCommand.Command = "Hosts Open"; HotkeyCommand.Description = "Open hosts file"; Action = fun () -> Hosts.openFile() |> ignore }

        { HotkeyCommand.Command = "Screenshot"; HotkeyCommand.Description = "Launch capture screen tool"; Action = fun () -> FsShortcuts.Screenshot.Main.PrScrn() |> ignore }

        { HotkeyCommand.Command = "Exit"; HotkeyCommand.Description = "Exit the hotkey app"; Action = Entrance.stop }
    ]
)
FsShortcuts.HotKeys.Entrance.start()
