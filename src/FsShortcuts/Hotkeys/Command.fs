module FsShortcuts.HotKeys.Command

open System.Collections.Generic

type HotkeyCommand = 
    {
        Command: string;
        Description: string
        Action: unit -> unit
    }
    member this.DisplayText with get() = this.Command + "    " + this.Description

let commands = 
    new List<HotkeyCommand>( 
        [
        ]
    )