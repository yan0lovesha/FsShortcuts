module FsWork.HotKeys.MouseSpeed

open System
open FsWork.Common
open System.Runtime.InteropServices
open FsWork.Common.Log

[<DllImport("User32.dll")>]
//[<return:MarshalAs(UnmanagedType.Bool)>]
extern bool SystemParametersInfo(uint32 uiAction, uint32 uiParam, uint32 pvParam, uint32 fWinIni)

let setSpeed speed =
    let currentSpeed = System.Windows.Forms.SystemInformation.MouseSpeed
    //let newSpeed = if currentSpeed = 2 then 6 else 2
    let succeed = SystemParametersInfo((uint32 113), (uint32 0), (uint32 speed), (uint32 0))
    if succeed then
        Log.logTrace LogType.Success (String.Format("Current mouse speed is changed from {0} to {1}", currentSpeed, speed))
    else
        Log.logTrace LogType.Error (String.Format("Failed to set mouse speed. Current speed is {0}", currentSpeed))