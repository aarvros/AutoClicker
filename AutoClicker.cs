using System.Diagnostics;
using System.Runtime.InteropServices;
using AutoClickerController;

namespace AutoClickerModel{
public class AutoClicker{
    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    private static extern void keybd_event(uint bVk, uint bScan, uint dwFlags, uint dwExtraInfo);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    [DllImport("user32.dll")]
    private static extern IntPtr GetMessageExtraInfo();
    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);
    private CancellationTokenSource? _cancellationTokenSource;
    private CancellationTokenSource? _cancellationTokenSource2;
    private Dictionary<Controller.InputType, List<uint>> clickCodeDict = new Dictionary<Controller.InputType, List<uint>>{
        {Controller.InputType.LEFTLCLICK, new List<uint>{0x0002, 0x0004}},     // left down, left up
        {Controller.InputType.RIGHTCLICK, new List<uint>{0x0008, 0x0010}},
    };

    private readonly Controller controller;
    public bool active = false;

    public AutoClicker(Controller controller){
        this.controller = controller;
    }

    public void RunMacro(int interval, Controller.InputType clickType, char key){
        if (active){
            Cancel();
            active = false;
        }else{
            active = true;
            if(key == '?'){
                RunClickAwait(interval, clickType);
            }else{
                RunKeyPressAwait(interval, key);
            }
        }
    }

     public void RunHolder(Controller.InputType clickType, char key){
        if(key == '?'){
            uint down = clickCodeDict[clickType][0];
            uint up = clickCodeDict[clickType][1];
            SendUp(up);
            SendDown(down);
        }else{
            INPUT inputDown = GetInput(key, "down");
            INPUT inputUp = GetInput(key, "up");
            int size = Marshal.SizeOf(typeof(INPUT));
            _ = SendInput(1, [inputUp], size);
            _ = SendInput(1, [inputDown], size);
        }
    }

    public void Cancel(){
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource2?.Cancel();
        active = false;
    }

    private async void RunClickAwait(int interval, Controller.InputType clickType){
        uint down = clickCodeDict[clickType][0];
        uint up = clickCodeDict[clickType][1];
        await RunClickMacroAsync(interval, down, up);
    }

    private async void RunKeyPressAwait(int interval, char key){
        INPUT inputDown = GetInput(key, "down");
        INPUT inputUp = GetInput(key, "up");
        await RunKeyMacroAsync(interval, inputDown, inputUp);
    }


    private async Task RunClickMacroAsync(int interval, uint down, uint up){
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        var stopwatch = new Stopwatch();
        await Task.Run(async () => {
            while (!token.IsCancellationRequested){
                stopwatch.Restart();
                SendDown(down);
                while (stopwatch.ElapsedMilliseconds < 1) {await Task.Yield();}
                SendUp(up);
                while (stopwatch.ElapsedMilliseconds < interval) {await Task.Yield();}
            }
            active = false;
        }, token);
    }

    private async Task RunKeyMacroAsync(int interval, INPUT inputDown, INPUT inputUp){
        int size = Marshal.SizeOf(typeof(INPUT));
        _cancellationTokenSource2 = new CancellationTokenSource();
        var token = _cancellationTokenSource2.Token;
        var stopwatch = new Stopwatch();
        await Task.Run(async () => {
            while (!token.IsCancellationRequested){
                stopwatch.Restart();
                //keybd_event(key, 0, 0, 0);        // Fallback, works in most cases but not in certain games, key is just char key in args
                _ = SendInput(1, [inputDown], size);
                while (stopwatch.ElapsedMilliseconds < 1) {await Task.Yield();}
                //keybd_event(key, 0, 2, 0);
                _ = SendInput(1, [inputUp], size);
                while (stopwatch.ElapsedMilliseconds < interval) {await Task.Yield();}
            }
            active = false;
        }, token);
    }

    private void SendDown(uint mb){
        uint x = (uint)Cursor.Position.X;
        uint y = (uint)Cursor.Position.Y;
        mouse_event(mb, x, y, 0, 0);
    }

    private void SendUp(uint mb){
        uint x = (uint)Cursor.Position.X;
        uint y = (uint)Cursor.Position.Y;
        mouse_event(mb, x, y, 0, 0);
    }

    private INPUT GetInput(char key, string type){
        uint scanCode = MapVirtualKey(key, 0x00);
        KeyEventF keyType = type == "down" ? KeyEventF.KeyDown : KeyEventF.KeyUp;
        INPUT input = new INPUT();
        input.type = (int)InputType.Keyboard;       // incredibly important do not leave out
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