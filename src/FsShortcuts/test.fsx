#r "bin/Debug/FsShortcuts.dll"
open FsShortcuts.Screenshot

let screenshotForm = new Form.ScreenshotForm()
screenshotForm.CaptureScreen()
screenshotForm.Show()

screenshotForm.Dispose()