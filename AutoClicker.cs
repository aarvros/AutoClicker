using System.Diagnostics;
using System.Runtime.InteropServices;
using AutoClickerController;

namespace AutoClickerModel{
public class AutoClicker{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    [DllImport("user32.dll")]
    private static extern IntPtr GetMessageExtraInfo();
    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);
    private CancellationTokenSource? _cancellationTokenSource;
    private Dictionary<Controller.InputType, List<uint>> clickCodeDict = new Dictionary<Controller.InputType, List<uint>>{
        {Controller.InputType.LEFTLCLICK, new List<uint>{0x0002, 0x0004}},     // left down, left up
        {Controller.InputType.RIGHTCLICK, new List<uint>{0x0008, 0x0010}},
    };
    private readonly long ticksPerMs = Stopwatch.Frequency / 1000;
    private readonly Controller controller;
    public bool active = false;

    public AutoClicker(Controller controller){
        this.controller = controller;
    }

    public void Cancel(){
        _cancellationTokenSource?.Cancel();
        active = false;
    }

    public void RunMacro(int interval, Controller.InputType clickType, char key){
        if (active){
            Cancel();
            active = false;
        }else{
            active = true;
            RunMacroAwait(interval, clickType, key);
        }
    }

     public void RunHolder(Controller.InputType clickType, char key){
        INPUT inputDown;
        INPUT inputUp;
        if(key == '?'){
            inputDown = GetMouseInput(clickCodeDict[clickType][0]);
            inputUp = GetMouseInput(clickCodeDict[clickType][1]);
        }else{
            inputDown = GetKeyInput(key, "down");
            inputUp = GetKeyInput(key, "up");
        }
        _ = SendInput(2, [inputUp, inputDown], Marshal.SizeOf(typeof(INPUT)));
    }

    private async void RunMacroAwait(int interval, Controller.InputType clickType, char key){
        INPUT inputDown;
        INPUT inputUp;
        if(key == '?'){
            inputDown = GetMouseInput(clickCodeDict[clickType][0]);
            inputUp = GetMouseInput(clickCodeDict[clickType][1]);
        }else{
            inputDown = GetKeyInput(key, "down");
            inputUp = GetKeyInput(key, "up");
        }
        await RunMacroAsync(interval, inputDown, inputUp);
    }

    private async Task RunMacroAsync(int interval, INPUT inputDown, INPUT inputUp){
        int size = Marshal.SizeOf(typeof(INPUT));
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        var stopwatch = new Stopwatch();
        long intervalTicks = ticksPerMs * interval;  // interval in ticks
        await Task.Run(async () => {
            while (!token.IsCancellationRequested){
                stopwatch.Restart();
                _ = SendInput(2, [inputDown, inputUp], size);
                while (stopwatch.ElapsedTicks < intervalTicks) {await Task.Yield();}
            }
            stopwatch.Stop();
            active = false;
        }, token);
    }

    private INPUT GetMouseInput(uint type){
        INPUT input = new INPUT();
        input.type = (int)InputType.Mouse;
        input.u.mi.dx = 0;
        input.u.mi.dy = 0;
        input.u.mi.dwFlags = type;
        input.u.mi.mouseData = 0;
        input.u.mi.time = 0;
        input.u.mi.dwExtraInfo = GetMessageExtraInfo();
        return input;
    }

    private INPUT GetKeyInput(char key, string type){
        uint scanCode = MapVirtualKey(key, 0x00);
        KeyEventF keyType = type == "down" ? KeyEventF.KeyDown : KeyEventF.KeyUp;
        INPUT input = new INPUT();
        input.type = (int)InputType.Keyboard;
        input.u.ki.wVk = 0;
        input.u.ki.wScan = (ushort)scanCode;
        input.u.ki.time = 0;
        input.u.ki.dwFlags = (uint)(keyType | KeyEventF.Scancode);
        input.u.ki.dwExtraInfo = GetMessageExtraInfo();
        return input;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HardwareInput
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]public MouseInput mi;
        [FieldOffset(0)] public KeyboardInput ki;
        [FieldOffset(0)] public HardwareInput hi;
    } 

    private struct INPUT
    {
        public int type;
        public InputUnion u;
    }

    [Flags]
    private enum InputType
    {
        Mouse = 0,
        Keyboard = 1,
        Hardware = 2
    }

    [Flags]
    private enum KeyEventF
    {
        KeyDown = 0x0000,
        ExtendedKey = 0x0001,
        KeyUp = 0x0002,
        Unicode = 0x0004,
        Scancode = 0x0008
    }
}
}