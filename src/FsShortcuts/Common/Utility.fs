module FsShortcuts.Common.Utility

open System.IO
open System
open System.Diagnostics
open FsShortcuts.Common.Log

let downloadFile url fileName=
    let tempFolder = Path.Combine(__SOURCE_DIRECTORY__, "Download")
    if not (System.IO.Directory.Exists(tempFolder)) then
        Directory.CreateDirectory tempFolder |> ignore
        printfn "Created directory %s" tempFolder

    let tempPath = Path.Combine(tempFolder, fileName)
    let wc = new System.Net.WebClient()

    let locker = Object()
    wc.DownloadProgressChanged.Add(
        fun args -> lock locker (fun () ->
            printf "\rDownloaded %d%%" args.ProgressPercentage)
    )
    wc.DownloadFileCompleted.Add(
        fun _ -> printfn "\rDownload completed for %s" tempPath
    )

    wc.AsyncDownloadFile (System.Uri(url), tempPath) |> Async.RunSynchronously
    tempPath

let copyToClipboard text =
    System.Windows.Forms.Clipboard.SetText text

let runProc filename args startDir = 
    let timer = Stopwatch.StartNew()
    let procStartInfo = 
        ProcessStartInfo(
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            FileName = filename,
            Arguments = args
        )
    match startDir with | Some d -> procStartInfo.WorkingDirectory <- d | _ -> ()

    let outputs = System.Collections.Generic.List<string>()
    let errors = System.Collections.Generic.List<string>()
    let outputHandler f (_sender:obj) (args:DataReceivedEventArgs) = f args.Data
    let outputHandlerError f (sender:obj) (args:DataReceivedEventArgs) = 
        logTrace LogType.Error args.Data
        outputHandler f sender args
    let outputHandlerOutput f (sender:obj) (args:DataReceivedEventArgs) = 
        logTrace LogType.Info args.Data
        outputHandler f sender args

    let p = new Process(StartInfo = procStartInfo)
    p.OutputDataReceived.AddHandler(DataReceivedEventHandler (outputHandlerOutput outputs.Add))
    p.ErrorDataReceived.AddHandler(DataReceivedEventHandler (outputHandlerError errors.Add))
    let started = 
        try
            let dir = match startDir with
                        | Some (d) -> d
                        | None -> ""
            logTrace LogType.Info (String.Format("Executing [{0} {1}] under {2}", filename, args, dir))
            p.Start()
        with | ex ->
            ex.Data.Add("filename", filename)
            reraise()
    if not started then
        failwithf "Failed to start process %s" filename
    printfn "Started %s with pid %i" p.ProcessName p.Id
    p.BeginOutputReadLine()
    p.BeginErrorReadLine()
    p.WaitForExit()
    timer.Stop()
    printfn "Finished %s after %A milliseconds" filename timer.ElapsedMilliseconds
    let cleanOut l = l |> Seq.filter (String.IsNullOrEmpty >> not)
    cleanOut outputs,cleanOut errors