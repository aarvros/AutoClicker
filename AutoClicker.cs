using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AutoClicker{
public class AutoClicker{
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
    public static bool active = false;
    private static CancellationTokenSource? _cancellationTokenSource;
    private static uint up;
    private static uint down;

    public static async Task RunMacroAsync(int downInterval, int upInterval, string clickType){
        if (clickType == "Left Click"){
            up = MOUSEEVENTF_LEFTUP;
            down = MOUSEEVENTF_LEFTDOWN;
        }else{
            up = MOUSEEVENTF_RIGHTUP;
            down = MOUSEEVENTF_RIGHTDOWN;
        }
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        var stopwatch = new Stopwatch();
        MessageBox.Show($"Down: {downInterval}ms\nUp: {upInterval}ms", "info", MessageBoxButtons.OK);
        return;

        await Task.Run(async () => {
            while (!token.IsCancellationRequested){
                stopwatch.Restart();
                SendDown(down);
                while (stopwatch.ElapsedMilliseconds < downInterval) {await Task.Yield();}  // hold down for half the interval
                stopwatch.Restart();
                SendUp(up);
                while (stopwatch.ElapsedMilliseconds < upInterval) {await Task.Yield();}  // sleep for for half the interval
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
        await RunMacroAsync(downInterval, upInterval, clickType);
    }

    public static void RunHolder(string clickType){
        if (clickType == "Left Click"){
            SendUp(MOUSEEVENTF_LEFTUP);
            SendDown(MOUSEEVENTF_LEFTDOWN);
        }else{
            SendUp(MOUSEEVENTF_RIGHTUP);
            SendDown(MOUSEEVENTF_RIGHTDOWN);
        }
    }

    public static void cancel(){
        _cancellationTokenSource?.Cancel();
        active = false;
    }

    public static void SendClick(){
        uint x = (uint)Cursor.Position.X;
        uint y = (uint)Cursor.Position.Y;
        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, x, y, 0, 0);
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