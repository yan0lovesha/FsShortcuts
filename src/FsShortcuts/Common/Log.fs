module FsShortcuts.Common.Log

type LogType =
| Info
| Success
| Warning
| Error

let logTrace (logType: LogType) (message: string) = 
    let originColor = System.Console.ForegroundColor
    match logType with
    | LogType.Info -> ()
    | LogType.Success -> System.Console.ForegroundColor <- System.ConsoleColor.Green
    | LogType.Warning -> System.Console.ForegroundColor <- System.ConsoleColor.Yellow
    | LogType.Error -> System.Console.ForegroundColor <- System.ConsoleColor.Red

    System.Console.WriteLine(message)

    System.Console.ForegroundColor <- originColor