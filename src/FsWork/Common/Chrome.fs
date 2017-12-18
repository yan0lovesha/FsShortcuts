module FsWork.Common.Chrome

open System.Diagnostics

let browse url =
    Process.Start("chrome.exe", url) |> ignore

let search (keyWords: string) =
    let url = System.String.Format("https://www.google.com/search?q={0}", System.Uri.EscapeDataString(keyWords))
    browse url

