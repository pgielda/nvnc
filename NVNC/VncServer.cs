// NVNC - .NET VNC Server Library
// Copyright (C) 2014 T!T@N
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Drawing;
using System.Threading;

namespace NVNC
{
    /// <summary>
    /// A wrapper class that should be used. It represents a VNC Server, and handles all the RFB procedures and communication.
    /// </summary>
    public class VncServer
    {
        private RfbProtocol host;
        private Framebuffer fb;

        private int _port;
        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
            }
        }

        private string _password;
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
            }
        }

        private string _name;
        /// <summary>
        /// The VNC Server name.
        /// <remarks>The variable value should be non-null.</remarks>
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        /// <summary>
        /// The default constructor using the default values for the parameters.
        /// Port is set to 5900, the Name is set to Default, and there is no password.
        /// </summary>
        public VncServer()
            : this("", 5900, "Default")
        { }

        public VncServer(string password, int port, string name)
        {
            _password = password;
            _port = port;
            _name = name;

            Size screenSize = ScreenSize();
            fb = new Framebuffer(screenSize.Width, screenSize.Height);

            fb.BitsPerPixel = 32;
            fb.Depth = 24;
            fb.BigEndian = false;
            fb.TrueColor = true;
            fb.RedShift = 16;
            fb.GreenShift = 8;
            fb.BlueShift = 0;
            fb.RedMax = fb.GreenMax = fb.BlueMax = 0xFF;
            fb.DesktopName = name;
        }

        public void ConnectAction(Action<Bitmap> action) {
            Console.WriteLine("Connecting action !!!");
            fb.ProcessFrame += action;
        }

        public void Start()
        {
            if (String.IsNullOrEmpty(Name))
                throw new ArgumentNullException("Name", "The VNC Server Name cannot be empty.");
            if (Port == 0)
                throw new ArgumentNullException("Port", "The VNC Server port cannot be zero.");
            Console.WriteLine("Started VNC Server at port: " + Port);

            host = new RfbProtocol(Port, Name);
            
            host.WriteProtocolVersion();
            Console.WriteLine("Wrote Protocol Version");

            host.ReadProtocolVersion();
            Console.WriteLine("Read Protocol Version");

            Console.WriteLine("Awaiting Authentication");
            if (!host.WriteAuthentication(Password))
            {
                Console.WriteLine("Authentication failed !");
                host.Close();
                Start();
            }
            else
            {
                Console.WriteLine("Authentication successfull !");

                bool share = host.ReadClientInit();
                Console.WriteLine("Share: " + share.ToString());

                Console.WriteLine("Server name: " + fb.DesktopName);
                host.WriteServerInit(this.fb);

                while ((host.isRunning))
                {
                    switch (host.ReadServerMessageType())
                    {
                        case RfbProtocol.ClientMessages.SET_PIXEL_FORMAT:
                            Console.WriteLine("Read SetPixelFormat");
                            Framebuffer f = host.ReadSetPixelFormat(fb.Width, fb.Height);
                          //  if (f != null)
                          //      fb = f;
                            break;
                        case RfbProtocol.ClientMessages.READ_COLOR_MAP_ENTRIES:
                            Console.WriteLine("Read ReadColorMapEntry");
                            host.ReadColorMapEntry();
                            break;
                        case RfbProtocol.ClientMessages.SET_ENCODINGS:
                            Console.WriteLine("Read SetEncodings");
                            host.ReadSetEncodings();
                            break;
                        case RfbProtocol.ClientMessages.FRAMEBUFFER_UPDATE_REQUEST:
                            Console.WriteLine("Read FrameBufferUpdateRequest");
                            host.ReadFrameBufferUpdateRequest(fb);
                            break;
                        case RfbProtocol.ClientMessages.KEY_EVENT:
                            Console.WriteLine("Read KeyEvent");      
                            host.ReadKeyEvent();
                            break;
                        case RfbProtocol.ClientMessages.POINTER_EVENT:
                            Console.WriteLine("Read PointerEvent");
                            host.ReadPointerEvent();
                            break;
                        case RfbProtocol.ClientMessages.CLIENT_CUT_TEXT:
                            Console.WriteLine("Read CutText");
                            host.ReadClientCutText();
                            break;
                    }
                }
                if (!host.isRunning)
                    Start();
            }
        }
        /// <summary>
        /// Closes all active connections, and stops the VNC Server from listening on the specified port.
        /// </summary>
        public void Stop()
        {
            this.host.Close();
        }
        private Size ScreenSize()
        {
            Size s = new Size();
            s.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            s.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            return s;
        }

    }
}
