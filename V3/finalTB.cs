using System;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ConsoleApp
{
    class Program
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");

            var enabled = false;
            var frame1 = 0;
            var fps = 1;
            var fps1 = 0;
            var tm = DateTime.Now;
            var bmp = new Bitmap(10, 10);
            var g = Graphics.FromImage(bmp);

            using (var form = new Form())
            {
                var pictureBox = new PictureBox();
                pictureBox.Dock = DockStyle.Fill;
                form.Controls.Add(pictureBox);
                form.TopMost = true;
                form.Show();

                while (true)
                {
                    g.CopyFromScreen(new Point(955, 535), Point.Empty, bmp.Size);

                    var frame = CalculateFrameSum(bmp);

                    if (GetAsyncKeyState(Keys.F) != 0)
                    {
                        Console.WriteLine("Predicting...");
                        enabled = !enabled;
                        Thread.Sleep(200);
                    }

                    if (enabled)
                    {
                        CheckForBotActivity(ref frame1, frame);
                    }

                    UpdateFPS(ref tm, ref fps, ref fps1);
                    UpdateUI(form, bmp, pictureBox, frame, fps1);

                    if (GetAsyncKeyState(Keys.Q) != 0)
                    {
                        break;
                    }
                }
            }
        }

        private static void UpdateUI(Form form, Bitmap bmp, PictureBox pictureBox, int frame, int fps1)
        {
            var resizedBmp = new Bitmap(200, (int)(bmp.Height * 200.0 / bmp.Width));
            using (var g = Graphics.FromImage(resizedBmp))
            {
                g.DrawImage(bmp, 0, 0, resizedBmp.Width, resizedBmp.Height);
            }

            pictureBox.Image = resizedBmp;
            form.Size = resizedBmp.Size;
            Console.WriteLine("colorValue: " + frame + " FPS: " + fps1);
        }

        private static void UpdateFPS(ref DateTime tm, ref int fps, ref int fps1)
        {
            if (DateTime.Now - tm > TimeSpan.FromMilliseconds(1000))
            {
                fps1 = fps;
                tm = DateTime.Now;
                fps = 0;
            }
            fps++;
        }

        private static void CheckForBotActivity(ref int frame1, int frame)
        {
            if (frame1 > (frame + 500) || frame1 < (frame - 500))
            {
                MouseClick();
                Console.WriteLine("Shot");
                frame1 = frame;
            }
            else if (frame1 > (frame + 50) || frame1 < (frame - 50))
            {
                frame1 = frame;
            }
        }

        private static int CalculateFrameSum(Bitmap bmp)
        {
            var frame = 0;
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    var color = bmp.GetPixel(x, y);
                    frame += color.R + color.G + color.B;
                }
            }

            return frame;
        }

        private static void  MouseClick()
        {
            SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
            Mouse.click(MouseButton.left);
            Thread.Sleep(200);
        }
    }
}
