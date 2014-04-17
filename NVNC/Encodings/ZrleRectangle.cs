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

namespace NVNC.Encodings
{
    /// <summary>
    /// Implementation of ZRLE encoding, as well as drawing support. See RFB Protocol document v. 3.8 section 6.6.5.
    /// </summary>
    public sealed class ZrleRectangle : EncodedRectangle
    {
        private const int TILE_WIDTH = 64;
        private const int TILE_HEIGHT = 64;
        private int[] pixels;
        public ZrleRectangle(RfbProtocol rfb, Framebuffer framebuffer, int[] pixels, Rectangle rectangle)
            : base(rfb, framebuffer, rectangle, RfbProtocol.Encoding.ZRLE_ENCODING)
        {
            this.pixels = pixels;
        }

        public override void Encode()
        {
            int x = rectangle.X;
            int y = rectangle.Y;
            int w = rectangle.Width;
            int h = rectangle.Height;

            //Console.WriteLine("Landed at ZRLE start!");

            int rawDataSize = w * h * (this.framebuffer.BitsPerPixel / 8);
            byte[] data = new byte[rawDataSize];
            int currentX, currentY;
            int tileW, tileH;

            //Bitmap bmp = PixelGrabber.GrabImage(rectangle.Width, rectangle.Height, pixels);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                for (currentY = y; currentY < y + h; currentY += TILE_HEIGHT)
                {
                    tileH = TILE_HEIGHT;
                    tileH = Math.Min(tileH, y + h - currentY);
                    for (currentX = x; currentX < x + w; currentX += TILE_WIDTH)
                    {
                        tileW = TILE_WIDTH;
                        tileW = Math.Min(tileW, x + w - currentX);

                        int[] pixelz = CopyPixels(pixels, w, currentX, currentY, tileW, tileH);

                        byte subencoding = 0;
                        ms.WriteByte(subencoding);
                        //PixelGrabber.GrabPixels(pixels, new Rectangle(currentX, currentY, tileW, tileH), this.framebuffer);

                        for (int i = 0; i < pixelz.Length; ++i)
                        {
                            int bb = 0;

                            //The CPixel structure (Compressed Pixel) has 3 bytes, opposed to the normal pixel which has 4.
                            byte[] bytes = new byte[3];
                            int pixel = pixelz[i];

                            bytes[bb++] = (byte)(pixel & 0xFF);
                            bytes[bb++] = (byte)((pixel >> 8) & 0xFF);
                            bytes[bb++] = (byte)((pixel >> 16) & 0xFF);
                            //bytes[b++] = (byte)((pixel >> 24) & 0xFF);

                            ms.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                byte[] uncompressed = ms.ToArray();
                this.bytes = uncompressed;
            }
        }

        public override void WriteData()
        {
            base.WriteData();
            rfb.WriteUint32(Convert.ToUInt32(RfbProtocol.Encoding.ZRLE_ENCODING));

            //ZrleRectangle exclusively uses a ZlibWriter to compress the bytes
            rfb.ZlibWriter.Write(this.bytes);
            rfb.ZlibWriter.Flush();
        }
    }
}
