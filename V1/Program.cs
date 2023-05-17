using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);

    [DllImport("user32.dll")]
    static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    static extern bool mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

    const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    const uint MOUSEEVENTF_LEFTUP = 0x0004;

    static void Main()
    {
        string configFile = "config.ini";
        bool configExists = File.Exists(configFile);

        if (!configExists)
        {
            IniWrite("config.ini", "main", "EMCol", "0xA5A528");
            IniWrite("config.ini", "main", "ColVn", "20");
        }

        string EMCol = IniRead("config.ini", "main", "EMCol");
        string ColVn = IniRead("config.ini", "main", "ColVn");

        int toggle = 1;
        int toggle2 = 1;

        int AntiShakeX = Screen.PrimaryScreen.Bounds.Height / 160;
        int AntiShakeY = Screen.PrimaryScreen.Bounds.Height / 128;
        int ZeroX = Screen.PrimaryScreen.Bounds.Width / 2;
        int ZeroY = Screen.PrimaryScreen.Bounds.Height / 2;
        int CFovX = Screen.PrimaryScreen.Bounds.Width / 40;
        int CFovY = Screen.PrimaryScreen.Bounds.Height / 64;
        int ScanL = ZeroX - CFovX;
        int ScanT = ZeroY;
        int ScanR = ZeroX + CFovX;
        int ScanB = ZeroY + CFovY;
        int NearAimScanL = ZeroX - AntiShakeX;
        int NearAimScanT = ZeroY - AntiShakeY;
        int NearAimScanR = ZeroX + AntiShakeX;
        int NearAimScanB = ZeroY + AntiShakeY;

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("F2 to activate. Hold MOUSE4 to shoot!");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("Note that this doesn't always work because it's just color-based");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Default color is PURPLE. Have fun!");

        while (true)
        {
            if (GetKeyState(0x71)) // F2 key
            {
                Console.Beep(750, 500);
                IntPtr foregroundWindow = GetForegroundWindow();
                Rectangle windowRect;
                GetClientRect(foregroundWindow, out windowRect);
                Point clientPoint = new Point(windowRect.Left, windowRect.Top);
                ClientToScreen(foregroundWindow, ref clientPoint);
                int aimPixelX, aimPixelY;

                if (GetKeyState(0x05)) // XButton2 (Mouse button 4)
                {
                    if (PixelSearch(NearAimScanL, NearAimScanT, NearAimScanR, NearAimScanB, EMCol, ColVn, out aimPixelX, out aimPixelY))
                    {
                        while (GetKeyState(0x05)) // Mouse button 4
                        {
                            SetCursorPos(aimPixelX, aimPixelY);
                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                            Thread.Sleep(1);
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        }
                    }
                }

                if (GetKeyState(0x06)) // XButton1 (Mouse button 5)
                {
                    if (PixelSearch(ScanL, ScanT, ScanR, ScanB, EMCol, ColVn, out aimPixelX, out aimPixelY))
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            int aimX = aimPixelX - ZeroX;
                            int aimY = aimPixelY - ZeroY;
                            int dirX = aimX > 0 ? 1 : -1;
                            int dirY = aimY > 0 ? 1 : -1;
                            int aimOffsetX = aimX * dirX;
                            int aimOffsetY = aimY * dirY;
                            int moveX = (int)(Math.Floor(Math.Pow(aimOffsetX, 1.0 / 2)) * dirX);
                            int moveY = (int)(Math.Floor(Math.Pow(aimOffsetY, 1.0 / 2)) * dirY);
                            mouse_event(MOUSEEVENTF_LEFTDOWN, moveX * 2, moveY, 0, 0);
                        }
                    }
                }
            }

            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Home)
                {
                    toggle2 = toggle2 == 0 ? 1 : 0;
                    if (toggle2 == 0)
                        Console.Clear();
                }
                else if (keyInfo.Key == ConsoleKey.F1)
                {
                    toggle = toggle == 0 ? 1 : 0;
                    if (toggle == 0)
                        Console.Clear();
                }
                else if (keyInfo.Key == ConsoleKey.End)
                {
                    break;
                }
            }

            Thread.Sleep(1);
        }
    }

    static bool PixelSearch(int x1, int y1, int x2, int y2, string color1, string color2, out int aimPixelX, out int aimPixelY)
    {
        aimPixelX = 0;
        aimPixelY = 0;
        Bitmap screen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        Graphics g = Graphics.FromImage(screen);
        g.CopyFromScreen(0, 0, 0, 0, screen.Size);

        for (int y = y1; y < y2; y++)
        {
            for (int x = x1; x < x2; x++)
            {
                Color pixelColor = screen.GetPixel(x, y);
                string hexColor = ColorTranslator.ToHtml(pixelColor);
                if (hexColor == color1 || hexColor == color2)
                {
                    aimPixelX = x;
                    aimPixelY = y;
                    return true;
                }
            }
        }

        return false;
    }

    static void IniWrite(string configFile, string section, string key, string value)
    {
        using (StreamWriter writer = new StreamWriter(configFile, true))
        {
            writer.WriteLine($"[{section}]");
            writer.WriteLine($"{key}={value}");
        }
    }

    static string IniRead(string configFile, string section, string key)
    {
        string value = "";
        bool foundSection = false;

        using (StreamReader reader = new StreamReader(configFile))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Trim() == $"[{section}]")
                {
                    foundSection = true;
                }
                else if (foundSection)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2 && parts[0] == key)
                    {
                        value = parts[1];
                        break;
                    }
                }
            }
        }

        return value;
    }

    static bool GetKeyState(int keyCode)
    {
        short state = GetAsyncKeyState(keyCode);
        return (state & 0x8000) != 0;
    }

    [DllImport("user32.dll", SetLastError = true)]
    static extern short GetAsyncKeyState(int vKey);
}
