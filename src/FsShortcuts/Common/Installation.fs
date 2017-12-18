module FsShortcuts.Common.Installation

open System.IO.Compression
open FsShortcuts.Common

type Apps = 
    | LinqPad
    | Fiddler
    | ChromeDriver

let install (app: Apps) =
    let installationFunctions = dict[
                                    Apps.LinqPad, fun () -> 
                                        let targetDir = System.IO.Path.Combine(__SOURCE_DIRECTORY__, @"Tools\LinqPad5")
                                        let zipPath = Utility.downloadFile "http://www.linqpad.net/GetFile.aspx?LINQPad5.zip" "LINQPad5.zip"
                                        if System.IO.Directory.Exists(targetDir) then System.IO.Directory.Delete(targetDir, true)
                                        using (System.IO.Compression.ZipFile.Open(zipPath, ZipArchiveMode.Read)) (fun archive ->
                                            archive.ExtractToDirectory(targetDir)
                                        )
                                        
                                        printfn "Linqpad5 installed in %s" targetDir
                                        System.IO.File.Delete(zipPath)
                                        let appPath = System.IO.Path.Combine(targetDir, "LINQPad.exe")
                                        Utility.runProc appPath "" None |> ignore

                                    Apps.Fiddler, fun _ ->
                                        printfn "%s" "It is not supported."

                                    Apps.ChromeDriver, fun _ ->
                                        let targetDir = @"C:\t"
                                        let zipPath = Utility.downloadFile "https://chromedriver.storage.googleapis.com/2.33/chromedriver_win32.zip" "chromedriver_win32.zip"
                                        using (System.IO.Compression.ZipFile.Open(zipPath, ZipArchiveMode.Read)) (fun archive ->
                                            archive.ExtractToDirectory(targetDir)
                                        )
                                        System.IO.File.Copy(@"C:\t\chromedriver.exe", @"C:\chromedriver.exe", true)
                                        
                                        printfn "ChromeDriver installed in %s" targetDir
                                        System.IO.File.Delete(zipPath)
                                        System.IO.Directory.Delete(targetDir, true)
                                        
                                ]
    let func = installationFunctions.Item(app)
    func()