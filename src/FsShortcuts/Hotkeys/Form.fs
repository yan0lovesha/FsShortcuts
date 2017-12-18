module FsShortcuts.HotKeys.Form

open System.Windows.Forms
open System.Drawing
open System
open FsShortcuts.HotKeys.Command
open System.Text.RegularExpressions



type HotKeysForm() as this =
    inherit Form()

    let mutable textBox: TextBox = null
    let mutable listBox: ListBox = null
    let mutable customSource = AutoCompleteStringCollection()
    let locker = obj()

    do
        this.InitializeComponent()

    member private this.hideListBox() =
        listBox.Visible <- false
        this.ClientSize <- Size(this.ClientSize.Width, 21)

    member private this.updateListResult() =
        let regexString = ".*" + (textBox.Text |> Seq.map (fun c -> c.ToString()) |> String.concat ".*") + ".*"
        let regex = Regex(regexString, RegexOptions.IgnoreCase)
        let filteredResults = Command.commands |> Seq.filter (fun command -> regex.Match(command.Command).Success)
        listBox.Items.AddRange(filteredResults |> Seq.cast<obj> |> Seq.toArray)
        if listBox.Items.Count = 0 then
            this.hideListBox()
        else
            listBox.Visible <- true
            listBox.SelectedIndex <- 0
            listBox.Height <- listBox.PreferredHeight
            this.ClientSize <- Size(this.ClientSize.Width, 21 + listBox.Height)

    member private this.form_KeyDown (eventArgs: KeyEventArgs) =
        if eventArgs.KeyCode = Keys.Escape then
            this.Close()
            textBox.Text <- ""
            this.hideListBox()
        if eventArgs.KeyCode = Keys.Enter && listBox.SelectedIndex <> -1 then
            this.Close()
            let selectedCommand = listBox.SelectedItem :?> HotkeyCommand
            selectedCommand.Action()
        if eventArgs.KeyCode = Keys.Up then
            if listBox.SelectedIndex > 0 then
                listBox.SelectedIndex <- listBox.SelectedIndex - 1
                eventArgs.Handled <- true
        if eventArgs.KeyCode = Keys.Down then
            if listBox.SelectedIndex < listBox.Items.Count - 1 then
                listBox.SelectedIndex <- listBox.SelectedIndex + 1
                eventArgs.Handled <- true

    member private this.textBox_TextChanged(eventArgs: EventArgs) =
        listBox.Items.Clear()
        if textBox.Text.Length = 0 then
            this.hideListBox()
        else
            lock locker (fun _ -> 
                this.updateListResult()
            )
            
    member private this.InitializeComponent() =
        textBox <- new TextBox()
        listBox <- new ListBox()
        this.SuspendLayout()
        
        textBox.Location <- System.Drawing.Point(0, 0)
        textBox.Name <- "InputTextBox"
        textBox.Size <- Size(500, 21)
        textBox.TabIndex <- 1
        textBox.AutoCompleteMode <- AutoCompleteMode.None
        textBox.AutoCompleteSource <- AutoCompleteSource.CustomSource
        textBox.AutoCompleteCustomSource <- customSource
        textBox.KeyDown.Add(this.form_KeyDown)
        textBox.TextChanged.Add(this.textBox_TextChanged)

        listBox.FormattingEnabled <- true
        listBox.Location <- Point(0, 21)
        listBox.Name <- "CommandListBox"
        listBox.Width <- 500
        listBox.TabIndex <- 2
        listBox.DisplayMember <- "DisplayText"

        this.AutoScaleDimensions <- SizeF(6.0f, 13.0F)
        this.AutoScaleMode <- AutoScaleMode.Font
        this.ClientSize <- Size(500, 21)
        this.ControlBox <- false
        this.Controls.Add(textBox)
        this.Controls.Add(listBox)
        this.FormBorderStyle <- FormBorderStyle.FixedToolWindow
        this.MaximizeBox <- false
        this.MinimizeBox <- false
        this.Name <- "Command dialog"
        this.ShowIcon <- false
        this.ShowInTaskbar <- false
        this.StartPosition <- FormStartPosition.CenterScreen
        this.Text <- "Command"
        this.KeyDown.Add(this.form_KeyDown)
        this.Shown.Add(fun _ -> textBox.Focus() |> ignore)
        this.TopMost <- true
        this.ResumeLayout(false) |> ignore
