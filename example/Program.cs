using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using NVNC;

namespace VNCTest
{
    class Program
    {
        static void ac(Bitmap bmp) {
                Console.WriteLine("we have bit={0}", bmp);
                 Graphics g = Graphics.FromImage(bmp);
                  g.CopyFromScreen(0, 0, 0, 0, new Size(bmp.Width, bmp.Height));
        }

        static void Main(string[] args)
        {
            VncServer s = new VncServer("", 5900, "VNC");
            s.ConnectAction(ac);
            try
            {
                s.Start();
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            Console.ReadLine();
        }
    }
}
