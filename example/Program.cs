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

        static int[] ac2() {
                Console.WriteLine("we got some question!!!");
                int[] data = new int[640*480];
                // do some lines
                for (int i = 0; i < 10000; i += 10) {
                        unchecked {
                                data[i] = (int)0xFFFFFF00;
                        }
                }
                return data;
        }

        static void keyev (bool pressed, uint code) {
                Console.WriteLine("KEY EVENT, pressed = {0}, code = {1}", pressed, code);
        }

        static void mouseev (byte buttons, ushort x, ushort y) {
                Console.WriteLine("MOUSE EVENT, buttons = {0}, x={1}, y={2}", buttons,x,y);
        }

        static void Main(string[] args)
        {
            VncServer s = new VncServer("", 5900, "VNC", 640, 480);
            //s.ConnectAction(ac);
            s.ConnectActionRaw(ac2);
            s.ConnectInputs(keyev, mouseev);
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
