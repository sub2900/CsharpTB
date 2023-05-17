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
            
            // setting up view parameters
            var enabled = false;
            var tm = DateTime.Now;
            var fps = 1;
            var fps1 = 0;

            // start a while loop in order to loop infinitely and as fast as possible
            while (true)
            {
                // get the image at the position 955, 535 with 10 by 10 pixels
                var bmp = new Bitmap(10, 10);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(new Point(955, 535), Point.Empty, bmp.Size);
                }

                // convert it to an array and calculate the sum of all values
                var frame = 0;
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        var color = bmp.GetPixel(x, y);
                        frame += color.R + color.G + color.B;
                    }
                }

                // if you press f you activate the bot or deactivate if it is already running and take the first value
                if (GetAsyncKeyState(Keys.F) != 0)
                {
                    Console.WriteLine("Predicting...");
                    var frame1 = 0;
                    if (enabled)
                    {
                        enabled = false;
                    }
                    else
                    {
                        enabled = true;
                    }

                    // wait 0.2 seconds in order to prevent accidentally double clicking
                    Thread.Sleep(200);
                }

                // checks if the bot is active
                if (enabled)
                {
                    // if the picture value is different, the bot will shoot and deactivate itself
                    if (frame1 > (frame + 500) || frame1 < (frame - 500)) // cs:go 1000, Valorant 500
                    {
                        MouseClick();
                        enabled = false;
                        Console.WriteLine("Shot");
                    }

                    // if the picture value is not changing too much -> update it
                    if (frame1 > (frame + 50) || frame1 < (frame - 50)) // cs:go 100, Valorant 50
                    {
                        frame1 = frame;
                    }
                }

                // do some math in order to get the fps value
                if (DateTime.Now - tm > TimeSpan.FromMilliseconds(1000))
                {
                    fps1 = fps;
                    tm = DateTime.Now;
                    fps = 0;
                }
                fps++;

                // scale the image up
                var resizedBmp = new Bitmap(200, (int)(bmp.Height * 200.0 / bmp.Width));
                using (var g = Graphics.FromImage(resizedBmp))
                {
                    g.DrawImage(bmp, 0, 0, resizedBmp.Width, resizedBmp.Height);
                }

                // convert the image to RGB and show it until you press q for quit
                using (var form = new Form())
                {
                    form.Size = resizedBmp.Size;
                    var pictureBox = new PictureBox();
                    pictureBox.Image = resizedBmp;
                    pictureBox.Dock = DockStyle.Fill;
                    form.Controls.Add(pictureBox);
                    form.TopMost = true;
                    form.Show();

                    if (GetAsyncKeyState(Keys.Q) != 0)
                    {
                        break;
                    }
                }

                // print the colorValue and fps and then repeat the whole procedure
                Console.WriteLine("colorValue: " + frame + " FPS: " + fps1);
            }
        }

        private static void MouseClick()
        {
            SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
            // Perform mouse click here using your preferred method
            // For example
            // Mouse.Click(MouseButton.Left);
            // Thread.Sleep(200);
            Mouse.click(MouseButton.left);
            Thread.Sleep(200);
        }
    }
}
