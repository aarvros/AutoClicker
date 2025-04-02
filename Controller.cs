using AutoClickerForm;
using AutoClickerModel;

namespace AutoClickerController{
public class Controller{
    public readonly AutoClicker autoClicker;
    public readonly AutoClickerView form;
    public int interval;
    public RunMode mode;
    public InputType type;
    public bool showWarning = true;
    public readonly int warningThreshold = 2;

    public enum RunMode{
        AUTOCLICKER = 1,
        MOUSEHOLDER = 2,
    }

    public enum InputType{
        LEFTLCLICK = 1,
        RIGHTCLICK = 2,
        KEYPRESS =   3,
    }
    
    public Controller(){
        autoClicker = new AutoClicker(this);
        form = new AutoClickerView(this);
        form.FormBorderStyle = FormBorderStyle.FixedSingle;
        form.MaximizeBox = false;
        Application.EnableVisualStyles();
    }

    public void Run(){
        Application.Run(form);
    }

    public void HotkeyPressed(){
        form.ActiveControl = form.Controls.Find("FocusMe", true)[0];
        char key = GetKey();
        bool isLetter = char.IsLetter(key);
        if(mode == RunMode.AUTOCLICKER && CheckAndSetInterval(false)){
            if(type == InputType.KEYPRESS && isLetter){
                autoClicker.RunMacro(interval, type, key);
            }else{
                autoClicker.RunMacro(interval, type, '?');
            }
        }else if (mode == RunMode.MOUSEHOLDER){
            if(type == InputType.KEYPRESS && isLetter){
                autoClicker.RunHolder(type, key);
            }else{
                autoClicker.RunHolder(type, '?');
            }
        }
    }

    public void Cancel(){
        autoClicker.Cancel();
    }

    private char GetKey(){
        Control keyTextBox = form.Controls.Find("KeyboardKeyTextBox", true)[0];
        return keyTextBox.Text != "" ? keyTextBox.Text.ToUpper().First() : '?';
    }

    public bool CheckAndSetInterval(bool suppressShowWarningChange){
        Control milliIntervalTextBox =  form.Controls.Find("MacromsIntervalTextBox", true)[0];
        Control secondIntervalTextBox = form.Controls.Find("MacrosecsIntervalTextBox", true)[0];
        Control minuteIntervalTextBox = form.Controls.Find("MacrominsIntervalTextBox", true)[0];
        try {
            int milliInterval =  int.Parse(milliIntervalTextBox.Text);
            int secondInterval = int.Parse(secondIntervalTextBox.Text);
            int minuteInterval = int.Parse(minuteIntervalTextBox.Text);
            interval = milliInterval + secondInterval * 1000 + minuteInterval * 60000;
            if(interval < warningThreshold && showWarning){
                string msg = String.Join('\n',
                "An interval below 2ms can be dangerous to use!",
                "1ms can cause some applications to struggle.",
                "0ms will send an input every 400μs, causing significant lag.",
                "",
                "Applications WILL struggle to keep up! Using an interval of 2ms is recommended.",
                "",
                "Set interval to 2ms?"
                );
                DialogResult setDefault = MessageBox.Show(msg, "Interval Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if(setDefault == DialogResult.Yes){
                    interval = warningThreshold;
                    milliIntervalTextBox.Text = warningThreshold.ToString();
                    secondIntervalTextBox.Text = "0";
                    minuteIntervalTextBox.Text = "0";
                }else{
                    if(!suppressShowWarningChange){     // when being called from hitting enter on the input box, do not toggle show warning
                        showWarning = false;            // ensures warning is seen on hotkey press, and if shown, does not start the macro
                    }
                }
                return false;
            }else if(interval >= warningThreshold && !showWarning){
                showWarning = true;
            }
            return true;
        } catch (OverflowException){
            MessageBox.Show("Interval value too large!", "Interval Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            interval = 20;
            milliIntervalTextBox.Text = "20";
            secondIntervalTextBox.Text = "0";
            minuteIntervalTextBox.Text = "0";
            return false;
        }
    }
}
}