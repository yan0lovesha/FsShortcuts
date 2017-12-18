module FsShortcuts.Screenshot.MouseHelper

open System
open System.Windows.Forms
open System.Drawing
open System.Collections.Generic
open System.Threading
open FsShortcuts.Common

let mutable isEnabledToDraw = true
let mutable private monitoredControl: Control = null
let mutable private mouseIsDown = false
let mutable private mouseRect = Rectangle.Empty
let mutable private oriScreen: Bitmap = Unchecked.defaultof<Bitmap>
let mutable private gdi: Graphics = null
let private pen = new Pen(Color.Green, 1.0f)
let private testPen = new Pen(Color.Red, 1.0f)

type SelectRectangle = obj -> Rectangle -> unit

let setRectangle = new List<SelectRectangle>()

let private drawStart(point: Point) = 
    monitoredControl.Capture <- true
    Cursor.Clip <- monitoredControl.RectangleToScreen(Rectangle(0, 0, monitoredControl.Width, monitoredControl.Height))
    gdi <- monitoredControl.CreateGraphics()
    mouseRect <- Rectangle(point.X, point.Y, 0, 0)
    
let private drawRectangle() =
    let _rect = monitoredControl.RectangleToScreen(mouseRect)
    gdi.DrawRectangle(pen, _rect)
    //ControlPaint.DrawReversibleFrame(_rect, Color.White, FrameStyle.Thick)

let mutable penSwitchNum = 0
let getCurrentPen () =
    if penSwitchNum % 2 = 0 then
        pen
    else
        testPen

let private eraseOldRectangle (oldRect: Rectangle) (newRect: Rectangle) =
    let _oldRect = monitoredControl.RectangleToScreen(oldRect)
    let _newRect = monitoredControl.RectangleToScreen(newRect)
    let oldX0 = _oldRect.X
    let oldX1 = _oldRect.X + _oldRect.Width
    let oldY0 = _oldRect.Y
    let oldY1 = _oldRect.Y + _oldRect.Height
    let newX0 = _newRect.X
    let newX1 = _newRect.X + _newRect.Width
    let newY0 = _newRect.Y
    let newY1 = _newRect.Y + _newRect.Height
    let oldHeight = _oldRect.Height
    let oldWidth = _oldRect.Width
    let newHeight = _newRect.Height
    let newWidth = _newRect.Width
    
    let mutable currentPen = getCurrentPen()

    if oldHeight = 0 && oldWidth = 0 then
        gdi.DrawRectangle(currentPen, _newRect)

    // if mouse is moving to right
    if oldX1 > oldX0 then
        // draw new lines
        gdi.DrawLines(currentPen, [| Point(oldX1, oldY0); Point(newX1, oldY0); Point(newX1, newY1); Point(oldX0, newY1); Point(oldX0, oldY1) |])
        Log.logTrace Log.LogType.Info (String.Format("Draw line: {0}*{1}, {2}*{3}, {4}*{5}, {6}*{7}, {8}*{9}", oldX1, oldY0, newX1, oldY0, newX1, newY1, oldX0, newY1, oldX0, oldY1))

        // remove the verticle line
        if oldX1 <> newX1 then
            let rect1 = Rectangle(oldX1, oldY0 + 1, 1, oldHeight)
            //gdi.DrawRectangle(testPen, rect1)
            gdi.DrawImage(oriScreen, rect1, rect1, GraphicsUnit.Pixel)
            //Log.logTrace Log.LogType.Info (String.Format("Remove vertical line: {0}, {1}, {2}, {3}", rect1.Left, rect1.Right, rect1.Top, rect1.Bottom))
        else
            if newY1 < oldY1 then
                let rect1 = Rectangle(oldX1, newY1 + 1, 1, oldY1 - newY1)
                gdi.DrawImage(oriScreen, rect1, rect1, GraphicsUnit.Pixel)

        // remove the horizental line
        if oldY1 <> newY1 then
            let rect2 = Rectangle(oldX0 + 1, oldY1, oldWidth, 1)
            gdi.DrawImage(oriScreen, rect2, rect2, GraphicsUnit.Pixel)
            //Log.logTrace Log.LogType.Info (String.Format("Remove horizental line: {0}, {1}, {2}, {3}", rect2.Left, rect2.Right, rect2.Top, rect2.Bottom))
        else
            if newX1 < oldX1 then
                let rect2 = Rectangle(newX1 + 1, oldY1, oldX1 - newX1, 1)
                gdi.DrawImage(oriScreen, rect2, rect2, GraphicsUnit.Pixel)
        
        // remove shrinked part of verticle line
        if newY1 < oldY1 then
            let rect3 = Rectangle(oldX0, newY1 + 1, 1, oldY1 - newY1)
            gdi.DrawImage(oriScreen, rect3, rect3, GraphicsUnit.Pixel)
            //Log.logTrace Log.LogType.Info (String.Format("Remove shrinked part of verticle line: {0}, {1}, {2}, {3}", rect3.Left, rect3.Right, rect3.Top, rect3.Bottom))
        
        // remove shrinked part of horizontal line
        if newX1 < oldX1 then
            let rect4 = Rectangle(newX1 + 1, oldY0,  oldX1 - newX1, 1)
            gdi.DrawImage(oriScreen, rect4, rect4, GraphicsUnit.Pixel)
            //Log.logTrace Log.LogType.Info (String.Format("Remove shrinked part of horizontal line: {0}, {1}, {2}, {3}", rect4.Left, rect4.Right, rect4.Top, rect4.Bottom))
        

let private resizeToRectangle (point: Point) =
    let newRect = Rectangle(mouseRect.X, mouseRect.Y, point.X - mouseRect.Left, point.Y - mouseRect.Top)
    eraseOldRectangle mouseRect newRect
    //drawRectangle()
    mouseRect.Width <- point.X - mouseRect.Left
    mouseRect.Height <- point.Y - mouseRect.Top
    //drawRectangle()

let private mouseDown (sender: obj) (eventArgs: MouseEventArgs) =
    if isEnabledToDraw then
        mouseIsDown <- true
        // backup the screen first
        oriScreen <- ((monitoredControl :?> PictureBox).Image :?> Bitmap)

        //let gdit = Graphics.FromImage(oriScreen)
        //gdit.FillRectangle(Brushes.Red, Rectangle(0, 0, oriScreen.Width, oriScreen.Height))
        //gdit.Dispose()
        
        // start draw
        drawStart(Point(eventArgs.X, eventArgs.Y))

let private mouseMove (sender: obj) (eventArgs: MouseEventArgs) =
    if mouseIsDown then
        resizeToRectangle(Point(eventArgs.X, eventArgs.Y))
        //let g = monitoredControl.CreateGraphics()
        //let pen = new Pen(Color.Green)
        //g.DrawLine(pen, Point(0, 0), eventArgs.Location)
        //g.Dispose()
        
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
    gdi.Dispose()


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
