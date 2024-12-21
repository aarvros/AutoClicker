using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AutoClicker{
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
    public static bool active = false;
    private static CancellationTokenSource? _cancellationTokenSource;
    private static CancellationTokenSource? _cancellationTokenSource2;
    private static Dictionary<string, List<uint>> clickCodeDict = new Dictionary<string, List<uint>>{
        {"Left Click", new List<uint>{0x0002, 0x0004}},     // left down, left up
        {"Right Click", new List<uint>{0x0008, 0x0010}},
    };

    public static async Task RunMacroAsync(uint down, uint up, int downInterval, int upInterval){
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        var stopwatch = new Stopwatch();
        await Task.Run(async () => {
            while (!token.IsCancellationRequested){
                stopwatch.Restart();
                SendDown(down);
                while (stopwatch.ElapsedMilliseconds < downInterval) {await Task.Yield();}
                stopwatch.Restart();
                SendUp(up);
                while (stopwatch.ElapsedMilliseconds < upInterval) {await Task.Yield();}
            }
            active = false;
        }, token);
    }

    public static async Task RunKeyMacroAsync(int downInterval, int upInterval, INPUT inputDown, INPUT inputUp){
        int size = Marshal.SizeOf(typeof(INPUT));
        _cancellationTokenSource2 = new CancellationTokenSource();
        var token = _cancellationTokenSource2.Token;
        var stopwatch = new Stopwatch();
        await Task.Run(async () => {
            while (!token.IsCancellationRequested){
                stopwatch.Restart();
                //keybd_event(key, 0, 0, 0);        // Fallback, works in most cases but not in certain games, key is just char key in args
                _ = SendInput(1, [inputDown], size);
                while (stopwatch.ElapsedMilliseconds < downInterval + upInterval) {await Task.Yield();}
                stopwatch.Restart();
                //keybd_event(key, 0, 2, 0);
                _ = SendInput(1, [inputUp], size);
                while (stopwatch.ElapsedMilliseconds < upInterval) {await Task.Yield();}
            }
            active = false;
        }, token);
    }

    public static void RunMacro(int downInterval, int upInterval, string clickType){
        if (active){
            cancel();
            active = false;
        }else{
            active = true;
            RunMacroAwait(downInterval, upInterval, clickType);
        }
    }

    private static async void RunMacroAwait(int downInterval, int upInterval, string clickType){
        uint down = clickCodeDict[clickType][0];
        uint up = clickCodeDict[clickType][1];
        await RunMacroAsync(down, up, downInterval, upInterval);
    }

    public static void RunHolder(string clickType){
        uint down = clickCodeDict[clickType][0];
        uint up = clickCodeDict[clickType][1];
        SendUp(up);
        SendDown(down);
    }

    public static void RunKeyPress(int downInterval, int upInterval, char key){
        if (active){
            cancel();
            active = false;
        }else{
            active = true;
            RunKeyPressAwait(downInterval, upInterval, key);
        }
    }

    private static async void RunKeyPressAwait(int downInterval, int upInterval, char key){
        uint scanCode = MapVirtualKey(key, 0x00);
        INPUT inputDown = new INPUT();
        inputDown.type = (int)InputType.Keyboard;       // incredibly important do not leave out
        inputDown.u.ki.wVk = 0;
        inputDown.u.ki.wScan = (ushort)scanCode;
        inputDown.u.ki.time = 0;
        inputDown.u.ki.dwFlags = (uint)(KeyEventF.KeyDown | KeyEventF.Scancode);
        inputDown.u.ki.dwExtraInfo = GetMessageExtraInfo();
        INPUT inputUp = new INPUT();
        inputUp.type = (int)InputType.Keyboard;         // incredibly important do not leave out
        inputUp.u.ki.wVk = 0;
        inputUp.u.ki.wScan = (ushort)scanCode;
        inputUp.u.ki.time = 0;
        inputUp.u.ki.dwFlags = (uint)(KeyEventF.KeyUp | KeyEventF.Scancode);
        inputUp.u.ki.dwExtraInfo = GetMessageExtraInfo();
        await RunKeyMacroAsync(downInterval, upInterval, inputDown, inputUp);
    }

    public static void cancel(){
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource2?.Cancel();
        active = false;
    }

    public static void SendDown(uint mb){
        uint x = (uint)Cursor.Position.X;
        uint y = (uint)Cursor.Position.Y;
        mouse_event(mb, x, y, 0, 0);
    }

    public static void SendUp(uint mb){
        uint x = (uint)Cursor.Position.X;
        uint y = (uint)Cursor.Position.Y;
        mouse_event(mb, x, y, 0, 0);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HardwareInput
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)]public MouseInput mi;
        [FieldOffset(0)] public KeyboardInput ki;
        [FieldOffset(0)] public HardwareInput hi;
    } 

    public struct INPUT
    {
        public int type;
        public InputUnion u;
    }

    [Flags]
    public enum InputType
    {
        Mouse = 0,
        Keyboard = 1,
        Hardware = 2
    }

    [Flags]
    public enum KeyEventF
    {
        KeyDown = 0x0000,
        ExtendedKey = 0x0001,
        KeyUp = 0x0002,
        Unicode = 0x0004,
        Scancode = 0x0008
    }
}
}