using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;
using ScottPlot.WinForms;
using ScottPlot;

namespace AppForm{
public class AutoClickerForm : Form {
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    private const int HOTKEY_ID = 5;    // chosen at random
    private RadioButton macroRadio;
    private RadioButton holderRadio;
    private RadioButton keyboardRadio;
    private TextBox keyboardKeyBox;
    private RadioButton leftRadio;
    private RadioButton rightRadio;
    private TextBox macroInterval;
    private System.Windows.Forms.Label intervalLabel;
    private Button hotkeyButton;
    private System.Windows.Forms.Label hotkeyLabel;
    private System.Windows.Forms.Label ratioLabel;
    private HScrollBar scrollBar;
    private string mode = "macro";
    private string click = "Left Click";
    private Keys hotkey;
    private bool hotkeyMode = false;
    private int downInterval;
    private int upInterval;
    private FormsPlot dutyCyclePlot;

public AutoClickerForm(){
        Text = "Auto Clicker v1.6";
        ClientSize = new Size(450, 400);
        Stream ico = LoadDLL();
        Icon = new Icon(ico);

        TableLayoutPanel topLevel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 1,RowCount = 4,AutoSize = true};
        topLevel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
        topLevel.RowStyles.Add(new RowStyle(SizeType.Percent, 30f));
        topLevel.RowStyles.Add(new RowStyle(SizeType.Percent, 30f));
        topLevel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

        TableLayoutPanel topPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 3,RowCount = 1,AutoSize = true};
        //topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
        //topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
        //topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
        topLevel.Controls.Add(topPanel, 0, 0);

        GroupBox intervalBox = new GroupBox{Dock = DockStyle.Fill, Text = "Interval", AutoSize = true};
        topPanel.Controls.Add(intervalBox, 0, 0);

        TableLayoutPanel intervalTextPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 2,RowCount = 1,AutoSize = true};
        intervalTextPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80f));
        intervalTextPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
        intervalBox.Controls.Add(intervalTextPanel);
        
        macroInterval = new TextBox{Dock = DockStyle.Fill, Text = "20", AutoSize = true};
        intervalLabel = new System.Windows.Forms.Label{Dock = DockStyle.Fill, Text = "ms", Width = 20};
        macroInterval.Anchor = AnchorStyles.Left;
        macroInterval.KeyPress += new KeyPressEventHandler(IntervalKeyPress);
        macroInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        intervalLabel.Anchor = AnchorStyles.Left;
        intervalLabel.TextAlign = ContentAlignment.MiddleLeft;
        intervalTextPanel.Controls.Add(macroInterval, 0, 0);
        intervalTextPanel.Controls.Add(intervalLabel, 1, 0);

        GroupBox ratioBox = new GroupBox{Dock = DockStyle.Fill, Text = "Duty Cycle Slider", AutoSize = true};
        topPanel.Controls.Add(ratioBox, 1, 0);

        TableLayoutPanel ratioPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 2,RowCount = 1,AutoSize = true};
        ratioPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80f));
        ratioPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
        ratioBox.Controls.Add(ratioPanel);

        scrollBar = new HScrollBar{Dock = DockStyle.Fill};
        scrollBar.Value = 50;
        scrollBar.Minimum = 1;
        scrollBar.Maximum = 108;
        scrollBar.ValueChanged += scrollMoved;
        ratioLabel = new System.Windows.Forms.Label{Dock = DockStyle.Fill, Text = "50%", AutoSize = true};
        ratioLabel.Anchor = AnchorStyles.Left;
        ratioLabel.TextAlign = ContentAlignment.MiddleLeft;
        ratioPanel.Controls.Add(scrollBar, 0, 0);
        ratioPanel.Controls.Add(ratioLabel, 1, 0);

        TableLayoutPanel helpPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 1,RowCount = 2,AutoSize = true};
        helpPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        helpPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        topPanel.Controls.Add(helpPanel, 2, 0);

        Button generalHelpButton = new Button{Dock = DockStyle.Fill, Text = "Help"};
        Button ratioHelpButton = new Button{Dock = DockStyle.Fill, Text = "Cycle Help"};
        generalHelpButton.Click += generalHelpButtonClicked;
        ratioHelpButton.Click += ratioHelpButtonClicked;
        helpPanel.Controls.Add(generalHelpButton, 0, 0);
        helpPanel.Controls.Add(ratioHelpButton, 0, 1);

        GroupBox chartBox = new GroupBox{Dock = DockStyle.Fill, Text = "Duty Cycle", AutoSize = true};
        topLevel.Controls.Add(chartBox, 0, 1);
        
        dutyCyclePlot = new FormsPlot{Dock = DockStyle.Fill, Font = new Font("Arial", 12)};
        chartBox.Controls.Add(dutyCyclePlot);

        TableLayoutPanel typePanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 2,RowCount = 1,AutoSize = true};
        typePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        typePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        topLevel.Controls.Add(typePanel, 0, 2);

        GroupBox macroType = new GroupBox{Dock = DockStyle.Fill, Text = "Macro Type", Width = 100};
        GroupBox clickType = new GroupBox{Dock = DockStyle.Fill, Text = "Click Type", Width = 100};
        typePanel.Controls.Add(macroType, 0, 0);
        typePanel.Controls.Add(clickType, 1, 0);

        TableLayoutPanel macroTypePanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 2,RowCount = 2,AutoSize = true};
        macroTypePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        macroTypePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        macroType.Controls.Add(macroTypePanel);

        TableLayoutPanel clickTypePanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 1,RowCount = 2,AutoSize = true};
        clickTypePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        clickTypePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        clickType.Controls.Add(clickTypePanel);

        macroRadio = new RadioButton{Name = "macro", Dock = DockStyle.Fill, Text = "Auto Clicker", Checked=true, AutoSize = true};
        keyboardRadio = new RadioButton{Name = "keyboard", Dock = DockStyle.Fill, Text = "Key Press", Checked=false, AutoSize = true};
        holderRadio = new RadioButton{Name = "holder", Dock = DockStyle.Fill, Text = "Mouse Holder", Checked=false, AutoSize = true};
        macroRadio.Click += MacroRadioClick;
        keyboardRadio.Click += MacroRadioClick;
        holderRadio.Click += MacroRadioClick;
        macroTypePanel.Controls.Add(macroRadio, 0, 0);
        macroTypePanel.Controls.Add(keyboardRadio, 0, 1);
        macroTypePanel.Controls.Add(holderRadio, 0, 2);

        keyboardKeyBox = new TextBox{Dock = DockStyle.Fill, Text = "t", AutoSize = true, Enabled = false};
        keyboardKeyBox.KeyPress += new KeyPressEventHandler(KeyboardKeyPress);
        macroTypePanel.Controls.Add(keyboardKeyBox, 1, 1);

        leftRadio = new RadioButton{Name = "left", Dock = DockStyle.Fill, Text = "Left Click", Checked=true, AutoSize = true};
        rightRadio = new RadioButton{Name = "right", Dock = DockStyle.Fill, Text = "Right Click", Checked=false, AutoSize = true};
        leftRadio.Click += ClickRadioClick;
        rightRadio.Click += ClickRadioClick;
        clickTypePanel.Controls.Add(leftRadio, 0, 0);
        clickTypePanel.Controls.Add(rightRadio, 0, 1);

        GroupBox hotkeyBox = new GroupBox{Dock = DockStyle.Fill, Text = "Hotkey Settings", AutoSize = true};
        topLevel.Controls.Add(hotkeyBox, 0, 3);

        TableLayoutPanel hotkeyPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 2,RowCount = 1,AutoSize = true};
        hotkeyPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        hotkeyPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        hotkeyBox.Controls.Add(hotkeyPanel);

        LoadHotkey();
        hotkeyButton = new Button{Dock = DockStyle.Fill, Text = "Set Hotkey"};
        hotkeyButton.Click += HotkeyClick;
        hotkeyButton.KeyDown += HotkeyDown;
        hotkeyLabel = new System.Windows.Forms.Label{Dock = DockStyle.Fill, Text = $"Hotkey: {hotkey}"};
        hotkeyLabel.TextAlign = ContentAlignment.MiddleCenter;
        hotkeyPanel.Controls.Add(hotkeyButton, 0, 0);
        hotkeyPanel.Controls.Add(hotkeyLabel, 1, 0);

        Controls.Add(topLevel);
        ActiveControl = intervalLabel;    // remove focus from dropdown on startup
        recalculateIntervals();
        RegisterHotKey(Handle, HOTKEY_ID, 0, (uint)hotkey);
    }

    private void UpdateChart(){
        int interval = downInterval + upInterval;
        int duplicates = 2;
        int pointsPerCycle = 100;
        int totalPoints = pointsPerCycle * duplicates;
        int totalPointsBuffered = totalPoints + 3;

        double[] ys = new double[totalPointsBuffered];
        for(int i = 0; i < totalPointsBuffered; i++){ys[i] = 2;}
        int normalizedSplitPoint = (int)(downInterval * (pointsPerCycle / (interval + 0.0)));

        for(int c = 0; c < duplicates; c++){
            for(int i = normalizedSplitPoint + (c*pointsPerCycle); i < pointsPerCycle*(c+1); i++){
                ys[i] = 1;
            }
        }

        ys[totalPoints] = 1;
        ys[totalPoints+1] = 0.8;  // used to lock dragging the x axis
        ys[totalPoints+2] = 2.2;  // by putting data on both upper and lower bounds
        
        ScottPlot.AxisRules.MaximumBoundary axisRule = new(
            xAxis: dutyCyclePlot.Plot.Axes.Bottom,
            yAxis: dutyCyclePlot.Plot.Axes.Left,
            limits: new AxisLimits(0, totalPoints, 0.8, 2.2)
        );

        dutyCyclePlot.Plot.Clear();
        dutyCyclePlot.Plot.Add.Signal(ys);
        dutyCyclePlot.Plot.HideGrid();
        dutyCyclePlot.Plot.HideLegend();
        dutyCyclePlot.Plot.Axes.Left.SetTicks([1, 2], ["Mouse Up", "Mouse Down"]);
        dutyCyclePlot.Plot.Axes.Bottom.SetTicks([0, 100, 200], ["0ms", $"{interval}", $"{interval*2}"]);

        dutyCyclePlot.Plot.Axes.Rules.Clear();
        dutyCyclePlot.Plot.Axes.Rules.Add(axisRule);
        dutyCyclePlot.Refresh();
    }

    private void HotkeyClick(object? sender, EventArgs e){
        hotkeyMode = true;
        hotkeyButton.Text = $"Press Any Key";
        hotkeyLabel.Text = $"ESC To Cancel";
        hotkeyButton.Focus();
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

        hotkeyButton.Text = "Set Hotkey";
        hotkeyLabel.Text = $"Hotkey: {hotkey}";
        if (mode == "macro"){
            macroRadio.Focus();
        }else{
            holderRadio.Focus();
        }
        hotkeyMode = false;
    }

    private void UncheckAll(){
        macroRadio.Checked = false;
        keyboardRadio.Checked = false;
        holderRadio.Checked = false;
        keyboardKeyBox.Enabled = false;
    }

    private void MacroRadioClick(object? sender, EventArgs e){
        AutoClicker.AutoClicker.cancel();
        RadioButton? clickedButton = sender as RadioButton;
        UncheckAll();
        if (clickedButton!.Name == "macro"){
            macroRadio.Checked = true;
        }else if (clickedButton!.Name == "holder"){
            holderRadio.Checked = true;
        }else if (clickedButton!.Name == "keyboard"){
            keyboardRadio.Checked = true;
            keyboardKeyBox.Enabled = true;
        }else{
            MessageBox.Show("How did you manage to do this???");
        }
        mode = clickedButton.Name;
    }

    private void ClickRadioClick(object? sender, EventArgs e){
        AutoClicker.AutoClicker.cancel();
        RadioButton? clickedButton = sender as RadioButton;
        if (clickedButton!.Name == "left"){
            leftRadio.Checked = true;
            rightRadio.Checked = false;
            click = "Left Click";
        }else if (clickedButton!.Name == "right"){
            leftRadio.Checked = false;
            rightRadio.Checked = true;
            click = "Right Click";
        }
    }

    private void scrollMoved(object? sender, EventArgs e){
        HScrollBar scroll = (sender as HScrollBar)!;
        int value = scroll.Value;
        string prefill = value < 10 ? "0" : "";     // adds a leading 0 to stop autosize
        ratioLabel.Text = prefill + scroll.Value.ToString() + "%";
        recalculateIntervals();
    }

    private bool recalculateIntervals(){
        bool success;
        int value = scrollBar.Value;
        int interval;
        try {
            interval = Int32.Parse(macroInterval.Text);
            success = true;
        } catch (OverflowException){
            MessageBox.Show("Interval value too large!", "Interval Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            macroInterval.Text = "20";
            interval = Int32.Parse(macroInterval.Text);
            success = false;
        }

        if(interval < 2){
            interval = 2;
            macroInterval.Text = "2";
        }

        double resultInterval = interval * (value / 100.0);
        double adjResultInterval = resultInterval < 1 ? 1.0 : resultInterval; // if under 1, set to 1
        downInterval = (int)adjResultInterval;
        upInterval = interval - (int)adjResultInterval;
        UpdateChart();
        return success;
    }

    private void generalHelpButtonClicked(object? sender, EventArgs e){
        string msg = "AutoClicker:\nPress hotkey to activate, press again to deactivate.\nLowest possible interval is 2ms.\n\nMouse Holder:\nPress hotkey to activate, press the selected mouse button to deactivate.";
        MessageBox.Show(msg, "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ratioHelpButtonClicked(object? sender, EventArgs e){
        string msg = """
        The Duty Cycle is the percent of the total interval spent holding down the mouse button.
        The down and up intervals will have a value of at least 1ms.
        
        Examples:
        1000ms at 50% --> Down | wait 500ms --> Up | wait 500ms
        1000ms at 5% -->   Down | wait 50ms   --> Up | wait 950ms
        10ms at 99% -->     Down | wait 9ms     --> Up | wait 1ms
        1ms at 50% -->       Down | wait 1ms     --> Up | wait 1ms

        Low %s are recommended for larger intervals over 500ms.
        50% is recommended for small intervals under 100ms.
        """;
        MessageBox.Show(msg, "Duty Cycle Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void RunProcess(object? sender, EventArgs? e){
        if(recalculateIntervals()){
            if(mode == "macro"){
                AutoClicker.AutoClicker.RunMacro(downInterval, upInterval, click);
            }else if(mode == "keyboard"){
                if(keyboardKeyBox.Text != ""){
                    char key = keyboardKeyBox.Text.ToUpper().First();
                    if(char.IsLetter(key)){
                        AutoClicker.AutoClicker.RunKeyPress(downInterval, upInterval, key);
                    }else{
                        MessageBox.Show("Key must be a letter", "Invalid Key", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        keyboardKeyBox.Text = "t";
                    }
                }
            }
        }else if(mode == "holder"){
            AutoClicker.AutoClicker.RunHolder(click);
        }
    }

    private void IntervalKeyPress(object? sender, KeyPressEventArgs e){
        if (e.KeyChar == (char)Keys.Enter){
            recalculateIntervals();
            ActiveControl = intervalLabel;      // remove focus upon pressing enter
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
        if (e.KeyChar == (char)Keys.Enter){
            ActiveControl = intervalLabel;      // remove focus upon pressing enter
            e.Handled = true;
        }else if(keyboardKeyBox.Text.Length == 1 && e.KeyChar != (char)Keys.Back){
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
            RunProcess(null, null);
        }
        base.WndProc(ref m);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        UnregisterHotKey(this.Handle, HOTKEY_ID);
        AutoClicker.AutoClicker.cancel();
        base.OnFormClosing(e);
    }

    [STAThread]
    public static void Main(string[] args){
        try{
            AutoClickerForm form = new AutoClickerForm();
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
            Application.EnableVisualStyles();
            Application.Run(form);
        } catch (Exception e){
            MessageBox.Show(e.Message, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
}