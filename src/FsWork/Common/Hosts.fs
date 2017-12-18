// Provides functions to operate Hosts file
module FsWork.Common.Hosts

open System
open System.IO

let private hostsFilePath = System.Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\drivers\etc\hosts")

let private printHostsEntry (entry: string) =
    let entryParts = entry.Split([| ' ' |], StringSplitOptions.RemoveEmptyEntries)
    printfn "%s\t%s" entryParts.[0] entryParts.[1]

type HostEntry = string * string

let list () =
    let entries = System.IO.File.ReadLines hostsFilePath |>
                    Seq.filter (fun s -> not(s.Trim().StartsWith("#") || String.IsNullOrWhiteSpace(s)))
    entries |> Seq.iter printHostsEntry

let openFile () =
    let fileName = "notepad.exe"
    FsWork.Common.Utility.runProc fileName hostsFilePath None

let addEntry (entries: HostEntry list) =
    let entryStrings = entries |> List.map (fun (ip, name) -> String.Format("{0}\t{1}", ip, name))
    // an empty string as the first emelemtn could make sure the first real element will not be added at the end of existing line.
    File.AppendAllLines(hostsFilePath, "" :: entryStrings)

let deleteEntry ip =
    let existingLines = File.ReadAllLines hostsFilePath
    let isNeeded (line: string) = line.Trim().StartsWith(ip) |> not
    let neededLines = existingLines |> Array.filter isNeeded
    File.WriteAllLines(hostsFilePath, neededLines)