module FsWork.Common.KeyboardHooker


open System
open System.Runtime.InteropServices
open FsWork.Common
open System.Windows.Forms

[<StructLayout(LayoutKind.Sequential)>]
type KeyboardMsg =
    struct
        val vkCode      : uint32
        val scanCode    : uint32
        val flags       : uint32
        val time        : uint32
        val dwExtraInfo : nativeint
    end

type KeyEvent = 
    | WM_KeyDown = 256
    | WM_KeyUp = 257

type WindowsHookCodes =
    | WH_MSGFILTER = -1
    | WH_JOURNALRECORD = 0
    | WH_JOURNALPLAYBACK = 1
    | WH_KEYBOARD = 2
    | WH_GETMESSAGE = 3
    | WH_CALLWNDPROC = 4
    | WH_CBT = 5
    | WH_SYSMSGFILTER = 6
    | WH_MOUSE = 7
    | WH_HARDWARE = 8
    | WH_DEBUG = 9
    | WH_SHELL = 10
    | WH_FOREGROUNDIDLE = 11
    | WH_CALLWNDPROCRET = 12
    | WH_KEYBOARD_LL = 13
    | WH_MOUSE_LL = 14

type HookProc = delegate of int * nativeint * nativeint -> nativeint

[<DllImport("User32.dll")>]
extern nativeint SetWindowsHookEx(int idHook, [<MarshalAs(UnmanagedType.FunctionPtr)>]HookProc pfnhook, nativeint hInst, int threadid)

[<DllImport("User32.dll")>]
extern bool UnhookWindowsHookEx(nativeint hhook);

[<DllImport("Kernel32.dll")>]
extern int GetCurrentThreadId();

[<DllImport("User32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)>]
extern nativeint CallNextHookEx(nativeint hhook, int nCode, nativeint wParam, nativeint lParam);

let hook (idHook: WindowsHookCodes) (hookedProcedures: List<(int * uint32) -> unit>) (isGlobal: bool) =
    let mutable hookPointer: nativeint = Unchecked.defaultof<nativeint>
    let keyBoardHookProc (nCode: int) (wParam: nativeint) (lParam: nativeint) =
        if nCode = 0 then
            let kbMessage = System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typedefof<KeyboardMsg>) :?> KeyboardMsg
            //Log.logTrace Log.LogType.Info (String.Format("keyBoardHookProc get triggered. nCode: {0}, wParam: {1}, vkCode: {2}.", nCode, wParam, kbMessage.vkCode))
            hookedProcedures |> List.iter (fun f -> f(wParam.ToInt32(), kbMessage.vkCode))
            IntPtr.Zero
        else if nCode > 0 then
            IntPtr.Zero
        else
            CallNextHookEx(hookPointer, nCode, wParam, lParam)
        

    let keyboardProc = HookProc(keyBoardHookProc)
    let intIdHook: System.Int32 = LanguagePrimitives.EnumToValue idHook
    let threadId = if isGlobal then 0 else GetCurrentThreadId()
    hookPointer <- SetWindowsHookEx(intIdHook, keyboardProc, IntPtr.Zero, threadId)
    hookPointer

let unHook hookPointer =
    if UnhookWindowsHookEx(hookPointer) then
        IntPtr.Zero
    else
        failwith "Failed to unhook the keyboard hook."

