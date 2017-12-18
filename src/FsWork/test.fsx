#r "bin/Debug/FsWork.dll"
open FsWork.Screenshot

let screenshotForm = new Form.ScreenshotForm()
screenshotForm.CaptureScreen()
screenshotForm.Show()

screenshotForm.Dispose()