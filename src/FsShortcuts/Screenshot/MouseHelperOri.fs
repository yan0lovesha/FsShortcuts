module FsShortcuts.Screenshot.MouseHelperOri

open System.Windows.Forms
open System.Drawing
open System.Collections.Generic
open System.Threading

let mutable isEnabledToDraw = true
let mutable private monitoredControl: Control = null
let mutable private mouseIsDown = false
let mutable private mouseRect = Rectangle.Empty

type SelectRectangle = obj -> Rectangle -> unit

let setRectangle = new List<SelectRectangle>()

let private drawStart(point: Point) = 
    monitoredControl.Capture <- true
    Cursor.Clip <- monitoredControl.RectangleToScreen(Rectangle(0, 0, monitoredControl.Width, monitoredControl.Height))
    mouseRect <- Rectangle(point.X, point.Y, 0, 0)
    
let private drawRectangle() =
    let _rect = monitoredControl.RectangleToScreen(mouseRect)
    ControlPaint.DrawReversibleFrame(_rect, Color.White, FrameStyle.Thick)

let private resizeToRectangle (point: Point) =
    drawRectangle()
    mouseRect.Width <- point.X - mouseRect.Left
    mouseRect.Height <- point.Y - mouseRect.Top
    drawRectangle()


let private mouseDown (sender: obj) (eventArgs: MouseEventArgs) =
    if isEnabledToDraw then
        mouseIsDown <- true
        drawStart(Point(eventArgs.X, eventArgs.Y))

let private mouseMove (sender: obj) (eventArgs: MouseEventArgs) =
    if mouseIsDown then
        resizeToRectangle(Point(eventArgs.X, eventArgs.Y))
        let g = monitoredControl.CreateGraphics()
        let pen = new Pen(Color.Green)
        g.DrawLine(pen, Point(0, 0), eventArgs.Location)
        g.Dispose()
        
let private mouseUp (sender: obj) (eventArgs: MouseEventArgs) =
    monitoredControl.Capture <- false
    Cursor.Clip <- Rectangle.Empty
    mouseIsDown <- false
    drawRectangle()
    if mouseRect.X = 0 || mouseRect.Y = 0 || mouseRect.Width = 0 || mouseRect.Height = 0 then
        ()
    else
        setRectangle.ForEach(fun f ->
            f (monitoredControl :> obj) mouseRect
        )
    mouseRect <- Rectangle.Empty


let private mouseUpEvent = MouseEventHandler(mouseUp)
let private mouseMoveEvent = MouseEventHandler(mouseMove)
let private mouseDownEvent = MouseEventHandler(mouseDown)

let startMouseMonitor(control: Control) =
    monitoredControl <- control
    monitoredControl.MouseUp.AddHandler mouseUpEvent
    monitoredControl.MouseMove.AddHandler mouseMoveEvent
    monitoredControl.MouseDown.AddHandler mouseDownEvent

let endMouseMonitor(control: Control) =
    monitoredControl.MouseUp.RemoveHandler mouseUpEvent
    monitoredControl.MouseMove.RemoveHandler mouseMoveEvent
    monitoredControl.MouseDown.RemoveHandler mouseDownEvent
    monitoredControl <- null
