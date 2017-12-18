module FsWork.Screenshot.Form

open System.Windows.Forms
open System.Drawing
open System.Drawing.Imaging
open System
open System.ComponentModel
open FsWork.Screenshot
open FsWork.Common

type ScreenshotForm() as this =
    inherit Form()
    let mutable imgRect: Rectangle = Unchecked.defaultof<Rectangle>
    let mutable hookPointer: IntPtr = Unchecked.defaultof<IntPtr>
    let mutable isDrawed = false
    let mutable screenCatch: Bitmap = null // cloned from fScreen
    let mutable fScreen: Bitmap = null  // the original picture from screen
    let mutable tempScreen: Bitmap = null
    let fullScreenPicture = new PictureBox()
    let hookedKeyboardFunctions = [
        this.KeyStroked
    ]

    member this.CaptureScreen () =
        this.InitScreenshot()
        this.DrawBlackBitmap()
        this.InitializeComponent()
        //todo setup hooks
        hookPointer <- KeyboardHooker.hook KeyboardHooker.WindowsHookCodes.WH_KEYBOARD hookedKeyboardFunctions false

        this.WindowState <- FormWindowState.Maximized
        fullScreenPicture.Height <- this.ScreenHeight
        fullScreenPicture.Width <- this.ScreenWidth
        fullScreenPicture.Image <- fScreen

        //todo mouse monitor
        MouseHelper.setRectangle.Add this.SetRectangle
        MouseHelper.isEnabledToDraw <- true
        MouseHelper.startMouseMonitor fullScreenPicture

    member private this.ScreenWidth with get() = Screen.PrimaryScreen.Bounds.Width
    member private this.ScreenHeight with get() = Screen.PrimaryScreen.Bounds.Height
    member private this.ScreenSize with get() = Screen.PrimaryScreen.Bounds.Size
    member private this.InitScreenshot() =
        // InitScreenshot
        fScreen <- new Bitmap(this.ScreenWidth, this.ScreenHeight)
        let g = Graphics.FromImage(fScreen)
        g.CopyFromScreen(Point(0, 0), Point(0, 0), this.ScreenSize)
        screenCatch <- fScreen.Clone(Rectangle(0, 0, fScreen.Width, fScreen.Height), PixelFormat.DontCare)
        g.Dispose()
    member private this.DrawBlackBitmap () =
        // DrawBlackBitmap
        let g = Graphics.FromImage(fScreen)
        let imageAttribute = new ImageAttributes()
        let colorMatrix = [|
            [| 0.229f;  0.229f; 0.229f; 0.0f;   0.0f |];
            [| 0.587f;  0.587f; 0.587f; 0.0f;   0.0f |];
            [| 0.114f;  0.114f; 0.114f; 0.0f;   0.0f |];
            [| 0.0f;    0.0f;   0.0f;   0.8f;   0.0f |];
            [| 0.0f;    0.0f;   0.0f;   0.0f;   1.0f |]
        |]
        let cm = ColorMatrix(colorMatrix)
        imageAttribute.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap)
        g.DrawImage(fScreen, Rectangle(0, 0, fScreen.Width, fScreen.Height), 0, 0, fScreen.Width, fScreen.Height, GraphicsUnit.Pixel, imageAttribute)
        g.Dispose();
        imageAttribute.Dispose()
        tempScreen <- fScreen.Clone(Rectangle(0, 0, fScreen.Width, fScreen.Height), PixelFormat.DontCare)
    member private this.CopyToTemp () =
        tempScreen <- fScreen.Clone(Rectangle(0, 0, fScreen.Width, fScreen.Height), PixelFormat.DontCare)
    member private this.FullScreenPicture_DoubleClick (e: EventArgs) =
        if isDrawed then
            let x = imgRect.X + 1
            let y = imgRect.Y + 1
            let width = imgRect.Width - 2
            let height = imgRect.Height - 2
            let shotSize = Size(width, height)
            let clipBitmap = new Bitmap(width, height)
            let g = Graphics.FromImage(clipBitmap)
            g.CopyFromScreen(x, y, 0, 0, shotSize)
            g.Dispose()
            Clipboard.SetDataObject(clipBitmap, true)
            clipBitmap.Dispose()
    member private this.InitializeComponent() =
        (fullScreenPicture :> ISupportInitialize).BeginInit()
        base.SuspendLayout()
        fullScreenPicture.Location <- Point(0, 0)
        fullScreenPicture.Size <- Size(500, 433)
        fullScreenPicture.TabIndex <- 0
        fullScreenPicture.TabStop <- false
        fullScreenPicture.DoubleClick.Add(this.FullScreenPicture_DoubleClick)

        this.AutoScaleDimensions <- System.Drawing.SizeF(6.0f, 13.0f)
        this.AutoScaleMode <- AutoScaleMode.Font
        this.ClientSize <- Size(500, 433)
        this.Controls.Add(fullScreenPicture)
        this.FormBorderStyle <- FormBorderStyle.None
        this.MaximizeBox <- false
        this.MinimizeBox <- false
        this.Name <- "Screenshot"
        this.ShowInTaskbar <- false
        this.Text <- "Taking Screenshot"
        this.TopMost <- true
        (fullScreenPicture :> ISupportInitialize).EndInit()
        base.ResumeLayout(false) |> ignore
    member private this.KeyStroked (wParam: int, vkCode: uint32) =
        if vkCode = (uint32 Keys.Escape) then
            if isDrawed then
                isDrawed <- false
                MouseHelper.isEnabledToDraw <- true
                this.CopyToTemp();
                fullScreenPicture.Image <- fScreen;
                this.LastDraw();
            else
                KeyboardHooker.unHook hookPointer |> ignore
                this.Close()
    member private this.DrawSelectBitmap (rect: Rectangle) =
        let g = Graphics.FromImage(tempScreen)
        g.DrawImage(screenCatch, rect, rect.X, rect.Y, rect.Width, rect.Height, GraphicsUnit.Pixel)
        let pen = new Pen(Color.Green)
        g.DrawRectangle(pen, rect)
        pen.Dispose()
        g.Dispose()
    member private this.LastDraw() =
        let rect = fullScreenPicture.RectangleToScreen(imgRect)
        ControlPaint.DrawReversibleFrame(rect, Color.White, FrameStyle.Dashed)
    member private this.SetRectangle (sender: obj) (rectangle: Rectangle) =
        if (not isDrawed) then
            imgRect <- rectangle
            isDrawed <- true
            MouseHelper.isEnabledToDraw <- false
            this.DrawSelectBitmap imgRect
            fullScreenPicture.Image <- tempScreen
            this.LastDraw()