using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;
using AutoClickerController;

namespace AutoClickerForm{
public class AutoClickerView : Form {
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    private const int HOTKEY_ID = 1;
    private readonly Controller controller;
    public Keys hotkey;
    public bool hotkeyMode = false;

public AutoClickerView(Controller controller){
        this.controller = controller;

        Text = "Auto Clicker v1.8";
        ClientSize = new Size(400, 250);
        Stream ico = LoadDLL();
        Icon = new Icon(ico);

        TableLayoutPanel topLevel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 2,RowCount = 3,AutoSize = true};
        topLevel.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
        topLevel.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
        topLevel.RowStyles.Add(new RowStyle(SizeType.Percent, 30f));
        topLevel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        topLevel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

        InitIntervalPanel(topLevel, 0, 0);
        InitOptionsPanel(topLevel, 0, 1);
        InitHotkeyPanel(topLevel, 0, 2);

        Controls.Add(topLevel);
        FocusLabel();
        RegisterHotKey(Handle, HOTKEY_ID, 0, (uint)hotkey);
    }

    private void InitIntervalPanel(TableLayoutPanel topLevel, int col, int row){
        GroupBox intervalBox = new GroupBox{Dock = DockStyle.Fill, Text = "Interval", AutoSize = true};
        topLevel.SetColumnSpan(intervalBox, 2);
        topLevel.Controls.Add(intervalBox, col, row);

        TableLayoutPanel intervalTextPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 3,RowCount = 1,AutoSize = true};
        intervalTextPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
        intervalTextPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
        intervalTextPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
        intervalBox.Controls.Add(intervalTextPanel);

        intervalTextPanel.Controls.Add(CreateIntervalInput("mins"), 0, 0);
        intervalTextPanel.Controls.Add(CreateIntervalInput("secs"), 1, 0);
        intervalTextPanel.Controls.Add(CreateIntervalInput("ms"), 2, 0);
    }

    private TableLayoutPanel CreateIntervalInput(string timeString){
        string name = $"Macro{timeString}IntervalTextBox";
        TableLayoutPanel panel = new TableLayoutPanel{Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, AutoSize=true};
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));

        string intervalText = timeString == "ms" ? "20" : "0";
        TextBox macroInterval = new TextBox{Dock = DockStyle.Fill, Name = name, Text = intervalText, AutoSize = true};
        Label intervalLabel = new Label{Dock = DockStyle.Fill, Name = "FocusMe", Text = timeString};
        macroInterval.KeyPress += new KeyPressEventHandler(IntervalKeyPress);
        macroInterval.TextAlign = HorizontalAlignment.Right;
        intervalLabel.Anchor = AnchorStyles.Left;
        intervalLabel.TextAlign = ContentAlignment.MiddleLeft;
        panel.Controls.Add(macroInterval, 0, 0);
        panel.Controls.Add(intervalLabel, 1, 0);

        return panel;
    }

    private void InitOptionsPanel(TableLayoutPanel topLevel, int col, int row){
        GroupBox macroType = new GroupBox{Dock = DockStyle.Fill, Text = "Macro Type", Width = 100};
        GroupBox clickType = new GroupBox{Dock = DockStyle.Fill, Text = "Input Type", Width = 100};
        topLevel.Controls.Add(macroType, col, row);
        topLevel.Controls.Add(clickType, col+1, row);

        TableLayoutPanel macroTypePanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 1,RowCount = 2,AutoSize = true};
        macroTypePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        macroTypePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        macroType.Controls.Add(macroTypePanel);

        TableLayoutPanel clickTypePanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 2,RowCount = 3,AutoSize = true};
        clickTypePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33f));
        clickTypePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 34f));
        clickTypePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33f));
        clickType.Controls.Add(clickTypePanel);

        RadioButton macroRadio = new RadioButton{Name = "MacroRadioButton", Dock = DockStyle.Fill, Text = "Auto Clicker", Checked=true, AutoSize = true};
        RadioButton holderRadio = new RadioButton{Name = "HolderRadioButton", Dock = DockStyle.Fill, Text = "Mouse Holder", Checked=false, AutoSize = true};
        macroRadio.Click += MacroRadioClick;
        holderRadio.Click += MacroRadioClick;
        controller.mode = Controller.RunMode.AUTOCLICKER;
        macroTypePanel.Controls.Add(macroRadio, 0, 0);
        macroTypePanel.Controls.Add(holderRadio, 0, 1);

        RadioButton leftRadio = new RadioButton{Name = "LeftClickRadioButton", Dock = DockStyle.Fill, Text = "Left Click", Checked=true, AutoSize = true};
        RadioButton rightRadio = new RadioButton{Name = "RightClickRadioButton", Dock = DockStyle.Fill, Text = "Right Click", Checked=false, AutoSize = true};
        RadioButton keyboardRadio = new RadioButton{Name = "KeyPressRadioButton", Dock = DockStyle.Fill, Text = "Key Press", Checked=false, AutoSize = true};
        leftRadio.Click += ClickRadioClick;
        rightRadio.Click += ClickRadioClick;
        keyboardRadio.Click += ClickRadioClick;
        controller.type = Controller.InputType.LEFTLCLICK;
        clickTypePanel.Controls.Add(leftRadio, 0, 0);
        clickTypePanel.Controls.Add(rightRadio, 0, 1);
        clickTypePanel.Controls.Add(keyboardRadio, 0, 2);

        TextBox keyboardKeyBox = new TextBox{Dock = DockStyle.Fill, Name = "KeyboardKeyTextBox", Text = "t", Width = 50, Enabled = false};
        keyboardKeyBox.KeyPress += new KeyPressEventHandler(KeyboardKeyPress);
        clickTypePanel.Controls.Add(keyboardKeyBox, 1, 2);
    }

    private void InitHotkeyPanel(TableLayoutPanel topLevel, int col, int row){
        GroupBox hotkeyBox = new GroupBox{Dock = DockStyle.Fill, Text = "Hotkey Settings", AutoSize = true};
        topLevel.SetColumnSpan(hotkeyBox, 2);
        topLevel.Controls.Add(hotkeyBox, col, row);

        TableLayoutPanel hotkeyPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 2,RowCount = 1,AutoSize = true};
        hotkeyPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        hotkeyPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        hotkeyBox.Controls.Add(hotkeyPanel);

        LoadHotkey();
        Button hotkeyButton = new Button{Dock = DockStyle.Fill, Name = "HotkeyButton", Text = "Set Hotkey"};
        hotkeyButton.Click += HotkeyClick;
        hotkeyButton.KeyDown += HotkeyDown;
        Label hotkeyLabel = new Label{Dock = DockStyle.Fill, Name = "HotkeyLabel", Text = $"Hotkey: {hotkey}"};
        hotkeyLabel.TextAlign = ContentAlignment.MiddleCenter;
        hotkeyPanel.Controls.Add(hotkeyButton, 0, 0);
        hotkeyPanel.Controls.Add(hotkeyLabel, 1, 0);
    }

    private void FocusLabel(){
        Control focusLabel = Controls.Find("FocusMe", true)[0];
        ActiveControl = focusLabel;
    }

    private void HotkeyClick(object? sender, EventArgs e){
        hotkeyMode = true;
        Control hotkeyButton = Controls.Find("HotkeyButton", true)[0];
        Control hotkeyLabel = Controls.Find("HotkeyLabel", true)[0];
        hotkeyButton.Text = $"Press Any Key";
        hotkeyLabel.Text = $"ESC To Cancel";
        hotkeyButton.Focus();                   // the button needs to be focused to accept a hotkey, maybe find a way to force focus and dont let user click other radio buttons
    }

    private void HotkeyDown(object? sender, KeyEventArgs e){
        if (e.KeyCode != Keys.Escape){
            hotkey = e.KeyCode;
            UnregisterHotKey(Handle, HOTKEY_ID);
            RegisterHotKey(Handle, HOTKEY_ID, 0, (uint)hotkey);
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string filepath = localAppDataPath + "\\AutoClicker\\AutoClickerHotkey.cfg";
            File.WriteAllText(filepath, hotkey.ToString());
        }

        Control hotkeyButton = Controls.Find("HotkeyButton", true)[0];
        Control hotkeyLabel = Controls.Find("HotkeyLabel", true)[0];

        hotkeyButton.Text = "Set Hotkey";
        hotkeyLabel.Text = $"Hotkey: {hotkey}";
        FocusLabel();
        hotkeyMode = false;
    }

    private void MacroRadioClick(object? sender, EventArgs e){
        controller.Cancel();

        RadioButton? clickedButton = sender as RadioButton;
        RadioButton? macroRadio = Controls.Find("MacroRadioButton", true)[0] as RadioButton;
        RadioButton? holderRadio = Controls.Find("HolderRadioButton", true)[0] as RadioButton;
        macroRadio!.Checked = false;
        holderRadio!.Checked = false;

        if (clickedButton!.Name == "MacroRadioButton"){
            macroRadio.Checked = true;
            controller.mode = Controller.RunMode.AUTOCLICKER;
        }else if (clickedButton!.Name == "HolderRadioButton"){
            holderRadio.Checked = true;
            controller.mode = Controller.RunMode.MOUSEHOLDER;
        }else{
            MessageBox.Show("How did you manage to do this???");
        }
    }

    private void ClickRadioClick(object? sender, EventArgs e){
        controller.Cancel();

        RadioButton? clickedButton = sender as RadioButton;
        RadioButton? leftRadio = Controls.Find("LeftClickRadioButton", true)[0] as RadioButton;
        RadioButton? rightRadio = Controls.Find("RightClickRadioButton", true)[0] as RadioButton;
        RadioButton? keyboardRadio = Controls.Find("KeyPressRadioButton", true)[0] as RadioButton;
        Control keyboardKeyBox = Controls.Find("KeyboardKeyTextBox", true)[0];
        leftRadio!.Checked = false;
        rightRadio!.Checked = false;
        keyboardRadio!.Checked = false;
        keyboardKeyBox.Enabled = false;

        if (clickedButton!.Name == "LeftClickRadioButton"){
            leftRadio.Checked = true;
            controller.type = Controller.InputType.LEFTLCLICK;
        }else if (clickedButton!.Name == "RightClickRadioButton"){
            rightRadio.Checked = true;
            controller.type = Controller.InputType.RIGHTCLICK;
        }else if (clickedButton!.Name == "KeyPressRadioButton"){
            keyboardRadio.Checked = true;
            keyboardKeyBox.Enabled = true;
            controller.type = Controller.InputType.KEYPRESS;
        }
    }

    private void RunProcess(){
        controller.HotkeyPressed();
    }

    private void IntervalKeyPress(object? sender, KeyPressEventArgs e){
        TextBox? textBox = sender as TextBox;
        if (e.KeyChar == (char)Keys.Enter){
            if(textBox!.Text == ""){textBox.Text = "0";}
            controller.CheckAndSetInterval(true);   // do not toggle showWarning
            FocusLabel();
            e.Handled = true;
        }
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)){
            e.Handled = true;
        }
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)){
            e.Handled = true;
        }
    }

    private void KeyboardKeyPress(object? sender, KeyPressEventArgs e){
        TextBox? textBox = sender as TextBox;
        if (e.KeyChar == (char)Keys.Enter){
            if(textBox!.Text == ""){textBox.Text = "t";}
            FocusLabel();
            e.Handled = true;
        }else if(textBox!.Text.Length == 1 && e.KeyChar != (char)Keys.Back){
            e.Handled = true;
        }else{
            e.Handled = !(char.IsLetter(e.KeyChar) || e.KeyChar == (char)Keys.Back);
        }
    }

    private Stream LoadDLL(){
        string ico_resource = "AutoClicker.resources.icon.ico";
        Assembly assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream(ico_resource)!;
    }

    private void LoadHotkey(){
        string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string path = localAppDataPath + "\\AutoClicker";
        if (!Directory.Exists(path)){
            Directory.CreateDirectory(path);
        }

        string filepath = path + "\\AutoClickerHotkey.cfg";
        if (!File.Exists(filepath)){
            using (FileStream fs = File.Create(filepath)){
                byte[] info = new UTF8Encoding(true).GetBytes($"{Keys.F8}");
                fs.Write(info, 0, info.Length);
            }
        }
        string keyString = File.ReadAllText(filepath);
        hotkey = (Keys)Enum.Parse(typeof(Keys), keyString);
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_HOTKEY = 0x0312;
        if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID && !hotkeyMode)
        {   
            RunProcess();
        }
        base.WndProc(ref m);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        UnregisterHotKey(Handle, HOTKEY_ID);
        controller.Cancel();
        base.OnFormClosing(e);
    }
}
}