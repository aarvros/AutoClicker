using System;
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

    public static async Task RunMacroAsync(int milliInterval){
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        await Task.Run(async () => {
            int i = 0;
            while (!token.IsCancellationRequested && i < 100){
                SendClick();
                await Task.Delay(milliInterval); 
                i++;
            }
            active = false;
        }, token);
    }

    public static void RunMacro(int milliInterval){
        if (active){
            cancel();
            active = false;
        }else{
            active = true;
            RunTheMacro(milliInterval);
        }
    }

    private static async void RunTheMacro(int milliInterval){
        await RunMacroAsync(milliInterval);
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