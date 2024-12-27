using AutoClickerForm;
using AutoClickerModel;

namespace AutoClickerController{
public class Controller{
    public readonly AutoClicker autoClicker;
    public readonly AutoClickerView form;
    public int interval;
    public RunMode mode;
    public InputType type;

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
        char key = GetKey();
        bool isLetter = char.IsLetter(key);
        if(mode == RunMode.AUTOCLICKER && CheckAndSetInterval()){
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

    public bool CheckAndSetInterval(){
        Control milliIntervalTextBox =  form.Controls.Find("MacromsIntervalTextBox", true)[0];
        Control secondIntervalTextBox = form.Controls.Find("MacrosecsIntervalTextBox", true)[0];
        Control minuteIntervalTextBox = form.Controls.Find("MacrominsIntervalTextBox", true)[0];
        try {
            int milliInterval =  int.Parse(milliIntervalTextBox.Text);
            int secondInterval = int.Parse(secondIntervalTextBox.Text);
            int minuteInterval = int.Parse(minuteIntervalTextBox.Text);
            interval = milliInterval + secondInterval * 1000 + minuteInterval * 60000;
            if(interval < 2){
                MessageBox.Show("Interval value is too small!\n\nThe interval must be a minimum of 2ms", "Interval Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                interval = 2;
                milliIntervalTextBox.Text = "2";
            }
            return true;
        } catch (OverflowException){
            MessageBox.Show("Interval value too large!", "Interval Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            milliIntervalTextBox.Text = "20";
            secondIntervalTextBox.Text = "0";
            minuteIntervalTextBox.Text = "0";
            interval = 20;
            return false;
        }
    }
}
}