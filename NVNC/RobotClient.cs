using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using NVNC.Encodings;
using System.Collections;
using System.Threading;

namespace NVNC
{
    public class RobotClient
    {
        public static readonly int CELLS = 4;
        public RfbProtocol.Encoding pe;
        public RfbProtocol rp;
        public Framebuffer pf;
        public int no;
        public Color[] colourMap;
        public Bitmap oldImage;
        public int index = 0;
        public Rectangle[] rects = new Rectangle[16];
        public Bitmap[] oldImages = new Bitmap[16];
        int[] rectRatos = new int[16];
        public Framebuffer defaultPixel;
        int dw = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
        int dh = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
        private int times = 0;

        public RobotClient(RfbProtocol rfp)
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    this.rects[(i * 4 + j)] = new Rectangle();
            rp = rfp;
        }

        protected EncodedRectangle[] getChangedImages(Bitmap paramBitmap)
        {
            int i = paramBitmap.Width;
            int j = paramBitmap.Height;
            int m;
            EncodedRectangle localRect;
            if ((this.oldImage == null) || (this.oldImage.Width != i) || (this.oldImage.Height != j))
            {
                this.index = 0;
                int k = i / 4;
                m = j / 4;
                for (int n = 0; n < 4; n++)
                    for (int i1 = 0; i1 < 4; i1++)
                    {
                        this.rects[(n * 4 + i1)].X = (k * n);
                        this.rects[(n * 4 + i1)].Y = (m * i1);
                        this.rects[(n * 4 + i1)].Width = k;
                        this.rects[(n * 4 + i1)].Height = m;
                    }

                EncodedRectangleFactory factory = new EncodedRectangleFactory(rp, pf);
                localRect = factory.Build(new Rectangle(0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height), this.pe);
                localRect.Encode();

                this.oldImage = paramBitmap;
                //localRect = Rect.encode(this.pe, this.pf, paramBitmap, 0, 0);
                return new EncodedRectangle[] { localRect };
            }
            List<EncodedRectangle> localArrayList = new List<EncodedRectangle>();
            for (m = 0; m < 16; m++)
            {
                localRect = getChangedImages(paramBitmap, m);
                if (localRect == null)
                    continue;
                localArrayList.Add(localRect);
            }
            this.oldImage = paramBitmap;
            if (localArrayList.Count > 0)
            {
                EncodedRectangle[] arrayOfRect = localArrayList.ToArray();
                return arrayOfRect;
            }
            return null;
        }

        private EncodedRectangle getChangedImages(Bitmap paramBitmap, int paramInt)
        {
            Rectangle localRectangle = this.rects[paramInt];
            int i = 0;
            int j = 0;
            int k = localRectangle.X;
            int m = localRectangle.Y;
            int n = localRectangle.X + localRectangle.Width;
            int i1 = localRectangle.Y + localRectangle.Height;
            try
            {
                for (i = k; i < n; i++)
                    for (j = m; j < i1; j++)
                    {
                        if (PixelGrabber.GetRGB(this.oldImage, i, j) == PixelGrabber.GetRGB(paramBitmap, i, j))
                            continue;
                        throw new Exception();
                    }
                return null;
            }
            catch (Exception)
            {
                k = i;
                try
                {
                    for (j = m; j < i1; j++)
                        for (i = k; i < n; i++)
                        {
                            if (PixelGrabber.GetRGB(this.oldImage, i, j) == PixelGrabber.GetRGB(paramBitmap, i, j))
                                continue;
                            throw new Exception();
                        }
                    return null;
                }
                catch (Exception)
                {
                    m = j;
                    try
                    {
                        for (i = n - 1; i > k; i--)
                            for (j = m; j < i1; j++)
                            {
                                if (PixelGrabber.GetRGB(this.oldImage, i, j) == PixelGrabber.GetRGB(paramBitmap, i, j))
                                    continue;
                                throw new Exception();
                            }
                        return null;
                    }
                    catch (Exception)
                    {
                        n = i;
                        try
                        {
                            for (j = i1 - 1; j > m; j--)
                                for (i = n; i > k; i--)
                                {
                                    if (PixelGrabber.GetRGB(this.oldImage, i, j) == PixelGrabber.GetRGB(paramBitmap, i, j))
                                        continue;
                                    throw new Exception();
                                }
                            return null;
                        }
                        catch (Exception)
                        {
                            i1 = j;
                            if ((n - k > 0) && (i1 - m > 0))
                            {
                                int i2 = k % 16;
                                if (i2 != 0)
                                    k -= i2;
                                i2 = m % 16;
                                if (i2 != 0)
                                    m -= i2;
                                i2 = n % 16;
                                if (i2 != 0)
                                    n = n - i2 + 16;
                                i2 = i1 % 16;
                                if (i2 != 0)
                                    i1 = i1 - i2 + 16;
                                try
                                {
                                    Console.WriteLine("" + k + ":" + m + ":" + n + ":" + i1 + ":" + (n - k) + ":" + (i1 - m));
                                    EncodedRectangleFactory factory = new EncodedRectangleFactory(rp, pf);

                                    //Rect.encode(this.pe, this.pf, paramBitmap.getSubimage(k, m, n - k, i1 - m), k, m);
                                    EncodedRectangle localRect = factory.Build(PixelGrabber.GetSubImage(paramBitmap, new Rectangle(k, m, n - k, i1 - m)), k, m, this.pe);
                                    localRect.Encode();
                                    return localRect;
                                }
                                catch (Exception localException5)
                                {
                                    Console.WriteLine(localException5.ToString());
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public void DoShit()
        {
            //new Thread(delegate(){
                int i = this.dw / 4;
                int j = this.dh / 4;
                int m = 0;
                Rectangle lRect = Rectangle.Empty;

                Bitmap localBitmap = PixelGrabber.CreateScreenCapture(new Rectangle(0, 0, this.dw, this.dh));
                for (int k = 0; k < 4; k++)
                    for (m = 0; m < 4; m++)
                    {
                        lRect = new Rectangle();
                        lRect.X = (i * k);
                        lRect.Y = (j * m);
                        lRect.Width = i;
                        lRect.Height = j;
                        lRect = PixelGrabber.AlignRectangle(lRect, this.dw, this.dh);
                        this.rects[(k * 4 + m)] = lRect;
                        this.oldImages[(k * 4 + m)] = PixelGrabber.GetSubImage(localBitmap, new Rectangle(lRect.X, lRect.Y, lRect.Width, lRect.Height));
                    }

                List<EncodedRectangle> rCol = new List<EncodedRectangle>();
                foreach (Rectangle r in rects)
                {
                    try
                    {
                        /*Console.WriteLine("X: " + r.X + "\n" +
                                          "Y: " + r.Y + "\n" +
                                          "W: " + r.Width + "\n" + 
                                          "H: " + r.Height + "\n" + 
                                          rp.DisplayName + "\n" + 
                                          defaultPixel.DesktopName + "\n\n");
                        Console.ReadLine();*/
                        EncodedRectangleFactory factory = new EncodedRectangleFactory(rp, defaultPixel);
                        EncodedRectangle localRect = factory.Build(r, rp.GetPreferredEncoding()); //factory.Build(PixelGrabber.GetSubImage(lBitmap, new Rectangle(localRectangle1.X, localRectangle1.Y, localRectangle1.Width, localRectangle1.Height)), localRectangle1.X + localRectangle2.X, localRectangle1.Y + localRectangle2.Y, rp.GetPreferredEncoding());
                        localRect.Encode();
                        rCol.Add(localRect);
                    }
                    catch (Exception localException)
                    {
                        Console.WriteLine(localException.StackTrace.ToString());
                    }
                }
                EncodedRectangle[] arrEnc = rCol.ToArray();
                if (arrEnc != null)
                    rp.WriteFrameBufferUpdate(arrEnc);

                //for (int ii = 0; ii < oldImages.Length; ii++)
                //oldImages[ii].Save("C:\\IMG" + ii + ".bmp");
            //}).Start();
        }
    }
}
