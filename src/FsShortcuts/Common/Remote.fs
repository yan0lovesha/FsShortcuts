module FsShortcuts.Common.Remote

open System
open System.Diagnostics
open FsShortcuts.Common.Log

let (private defaultUsername, private defaultPassword) = (@"~\Administrator", @"T!T@n1130")

let mutable getDefaultCred: Option<(unit -> (string * string))> = None

let To (server: string) (cred: Option<(string * string)>) =
    // let (username, password) =  getCred(server)
    let (username, password) = match cred with
                                    | Some credValue -> credValue
                                    | None -> match getDefaultCred with
                                                    | Some f -> f()
                                                    | None -> failwith "Credential required. Please either specify credential in paramter, or define getDefaultCred function."
    logTrace LogType.Info (String.Format("username: {0}, password: {1}", username, password))

    let rdcProcess = new Process()
    rdcProcess.StartInfo.FileName <- Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\cmdkey.exe")
    rdcProcess.StartInfo.Arguments <- String.Format("/generic:TERMSRV/{0} /user:{1} /pass:{2}", server, username, password)
    rdcProcess.Start() |> ignore

    rdcProcess.StartInfo.FileName <- Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\mstsc.exe")
    rdcProcess.StartInfo.Arguments <- String.Format("/v {0}", server)
    rdcProcess.Start() |> ignore