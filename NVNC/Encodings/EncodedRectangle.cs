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
using System.IO;
using System.Drawing;

namespace NVNC.Encodings
{
    /// <summary>
    /// Abstract class representing an Rectangle to be encoded and written.
    /// </summary>
    public abstract class EncodedRectangle
    {
        protected RfbProtocol rfb;
        protected Rectangle rectangle;
        protected Framebuffer framebuffer;
        protected byte[] bytes;

        public EncodedRectangle(RfbProtocol rfb, Framebuffer framebuffer, Rectangle rectangle, RfbProtocol.Encoding encoding)
        {
            this.rfb = rfb;
            this.framebuffer = framebuffer;
            this.rectangle = rectangle;
        }

        /// <summary>
        /// Gets the rectangle that needs to be encoded.
        /// </summary>
        public Rectangle UpdateRectangle
        {
            get
            {
                return rectangle;
            }
        }

        /// <summary>
        /// Encode the pixel data from the supplied rectangle and store it in the bytes array.
        /// </summary>
        public abstract void Encode();

        /// <summary>
        /// Writes the generic rectangle data to the stream.
        /// It's coordinates and size.
        /// </summary>
        public virtual void WriteData()
        {
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.X));
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.Y));
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.Width));
            rfb.WriteUInt16(Convert.ToUInt16(rectangle.Height));
        }
        
        protected void WritePixel32(int px)
        {
            int pixel;
            int b = 0;
            byte[] bytes = new byte[4];
            pixel = px;

            bytes[b++] = (byte)(pixel & 0xFF);
            bytes[b++] = (byte)((pixel >> 8) & 0xFF);
            bytes[b++] = (byte)((pixel >> 16) & 0xFF);
            bytes[b++] = (byte)((pixel >> 24) & 0xFF);

            rfb.Write(bytes);
        }
        protected int[] CopyPixels(int[] pixels, int scanline, int x, int y, int w, int h)
        {
            int size = w * h;
            int[] ourPixels = new int[size];
            int jump = scanline - w;
            int s = 0;
            int p = y * scanline + x;
            for (int i = 0; i < size; i++, s++, p++)
            {
                if (s == w)
                {
                    s = 0;
                    p += jump;
                }
                ourPixels[i] = pixels[p];
            }

            return ourPixels;
        }
        protected int GetBackground(int[] pixels, int scanline, int x, int y, int w, int h)
        {
            return pixels[y * scanline + x]; ;
            /*
            int runningX, runningY, k;
            int[] counts = new int[256];

            int maxcount = 0;
            int maxclr = 0;

            if( framebuffer.BitsPerPixel == 16 )
                return pixels[0];
            else if( framebuffer.BitsPerPixel == 32 )
                return pixels[0];

            // For 8-bit
            return pixels[0];

            for( runningX = 0; runningX < 256; runningX++ )
                counts[runningX] = 0;

            for( runningY = 0; runningY < pixels.Length; runningY++ )
            {
                k = pixels[runningY];
                if( k >= counts.Length )
                {
                    return 0;
                }
                counts[k]++;
                if( counts[k] > maxcount )
                {
                    maxcount = counts[k];
                    maxclr = pixels[runningY];
                }
            }
            return maxclr;
            */
        }
    }
}
