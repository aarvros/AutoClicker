using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AutoClicker{
public class AutoClicker{
    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void keybd_event(uint bVk, uint bScan, uint dwFlags, uint dwExtraInfo);

    public static bool active = false;
    public static bool active2 = false;
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

    public static async Task RunKeyMacroAsync(int downInterval, int upInterval, char key){
        _cancellationTokenSource2 = new CancellationTokenSource();
        var token = _cancellationTokenSource2.Token;
        var stopwatch = new Stopwatch();
        await Task.Run(async () => {
            while (!token.IsCancellationRequested){
                stopwatch.Restart();
                keybd_event(key, 0, 0, 0);
                while (stopwatch.ElapsedMilliseconds < downInterval + upInterval) {await Task.Yield();}
                stopwatch.Restart();
                keybd_event(key, 0, 2, 0);
                while (stopwatch.ElapsedMilliseconds < upInterval) {await Task.Yield();}
            }
            active2 = false;
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
        if (active2){
            cancel();
            active2 = false;
        }else{
            active2 = true;
            RunKeyPressAwait(downInterval, upInterval, key);
        }
    }

    private static async void RunKeyPressAwait(int downInterval, int upInterval, char key){
        await RunKeyMacroAsync(downInterval, upInterval, key);
    }

    public static void cancel(){
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource2?.Cancel();
        active = false;
        active2 = false;
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
}
}