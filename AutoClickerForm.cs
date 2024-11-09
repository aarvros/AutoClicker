using System;
using System.IO;
using System.Diagnostics;
using AutoClicker;
using WindowsInput;
using WindowsInput.Native;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using System.Reflection;

namespace AppForm{
public class AutoClickerForm : Form {
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    private const int HOTKEY_ID = 5;
    private RadioButton macro;
    private RadioButton holder;
    private TextBox macroInterval;
    private Label intervalLabelPre;
    private Label intervalLabelPost;
    private ComboBox mbChoice;
    private string mode = "macro";

    public AutoClickerForm(){
        Text = "Auto Clicker v1.0";
        ClientSize = new Size(320, 100);
        Stream ico = LoadDLL();
        Icon = new Icon(ico);

        macro = new RadioButton{Name = "macro", Dock = DockStyle.Fill, Text = "Auto Clicker (F8)", Checked=true};
        holder = new RadioButton{Name = "holder", Dock = DockStyle.Fill, Text = "Mouse Holder (F8)", Checked=false};
        macro.Click += RadioClick;
        holder.Click += RadioClick;

        mbChoice = new ComboBox{TabIndex = 0, Text = "Left Click"};
        string[] buttons = ["Left Click", "Right Click"];
        mbChoice.Items.AddRange(buttons);
        mbChoice.Anchor = AnchorStyles.None;

        intervalLabelPre = new Label{Dock = DockStyle.Fill, Text = "Left Click Every"};
        macroInterval = new TextBox{Dock = DockStyle.Fill, Text = "5", Width=50};
        intervalLabelPost = new Label{Dock = DockStyle.Fill, Text = "ms"};

        macroInterval.Anchor = AnchorStyles.Right;
        macroInterval.KeyPress += new KeyPressEventHandler(IntervalKeyPress);

        macroInterval.TextAlign = HorizontalAlignment.Right;
        intervalLabelPre.TextAlign = ContentAlignment.MiddleRight;
        intervalLabelPost.TextAlign = ContentAlignment.MiddleLeft;

        TableLayoutPanel topLevel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 1,RowCount = 3,AutoSize = true};
        TableLayoutPanel autoClickerPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 2,RowCount = 1,AutoSize = true, BorderStyle=BorderStyle.FixedSingle};
        TableLayoutPanel intervalPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 3,RowCount = 1,AutoSize = true};
        TableLayoutPanel holderPanel = new TableLayoutPanel{Dock = DockStyle.Fill,ColumnCount = 4,RowCount = 1,AutoSize = true, BorderStyle=BorderStyle.FixedSingle};

        topLevel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        topLevel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

        autoClickerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        autoClickerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));

        holderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
        holderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

        intervalPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        autoClickerPanel.Controls.Add(macro, 0, 0);
        autoClickerPanel.Controls.Add(intervalPanel, 1, 0);
        intervalPanel.Controls.Add(intervalLabelPre, 0, 0);
        intervalPanel.Controls.Add(macroInterval, 1, 0);
        intervalPanel.Controls.Add(intervalLabelPost, 2, 0);
        holderPanel.Controls.Add(holder, 0, 0);
        holderPanel.Controls.Add(mbChoice, 1, 0);

        topLevel.Controls.Add(autoClickerPanel, 0, 0);
        topLevel.Controls.Add(holderPanel, 0, 1);

        Controls.Add(topLevel);
        RegisterHotKey(Handle, HOTKEY_ID, 0, (uint)Keys.F8);
    }

    private void RadioClick(object? sender, EventArgs e){
        RadioButton? clickedButton = sender as RadioButton;
        if (clickedButton!.Name == "macro"){
            macro.Checked = true;
            holder.Checked = false;
            mode = "macro";
        }else if (clickedButton!.Name == "holder"){
            macro.Checked = false;
            holder.Checked = true;
            mode = "holder";
        }
    }

    private void RunProcess(object? sender, EventArgs? e){
        if (mode == "macro"){
            int interval = Int32.Parse(macroInterval.Text);
            AutoClicker.AutoClicker.RunMacro(interval);
        }else{
            AutoClicker.AutoClicker.RunHolder(mbChoice.Text);
        }
    }

    private void IntervalKeyPress(object? sender, KeyPressEventArgs e){
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)){
            e.Handled = true;
        }
    }

    private Stream LoadDLL(){
        string ico_resource = "AutoClicker.resources.spongebob.ico";
        Assembly assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream(ico_resource)!;
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_HOTKEY = 0x0312;
        if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
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