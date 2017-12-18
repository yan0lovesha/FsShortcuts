module FsShortcuts.Screenshot.Main

open System.Runtime.InteropServices


[<DllImport("PrScrn.dll", EntryPoint = "PrScrn", CallingConvention = CallingConvention.Cdecl)>]
extern int PrScrn()