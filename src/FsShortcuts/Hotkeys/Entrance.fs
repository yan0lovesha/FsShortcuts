module FsShortcuts.HotKeys.Entrance

open System.Windows.Forms
open FsShortcuts.Common
open System
open FsShortcuts.HotKeys.Form
open Command

let mutable hookPointer: IntPtr = Unchecked.defaultof<IntPtr>
let mutable form: Form = null

let keyboardHookedFunction (wParam: int, vkCode: uint32) =
    // Ctrl + Shift + M
    if wParam = (int KeyboardHooker.KeyEvent.WM_KeyDown) && vkCode = (uint32 Keys.M) && (int)Control.ModifierKeys = (int Keys.Control) + (int Keys.Shift) then
        if form = null || form.IsDisposed then
            form <- new HotKeysForm()
            form.WindowState <- FormWindowState.Minimized
            form.Show()
            form.WindowState <- FormWindowState.Normal
        else
            form.WindowState <- FormWindowState.Minimized
            form.BringToFront()
            form.WindowState <- FormWindowState.Normal
            //form.BringToFront()
            //form.Focus() |> ignore


let start () =
    hookPointer <- KeyboardHooker.hook KeyboardHooker.WindowsHookCodes.WH_KEYBOARD_LL [ keyboardHookedFunction ] true

let stop () =
    KeyboardHooker.unHook hookPointer |> ignore
    Environment.Exit 1