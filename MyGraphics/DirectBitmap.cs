using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GK_proj4
{
    // found here:
    // https://stackoverflow.com/questions/24701703/c-sharp-faster-alternatives-to-setpixel-and-getpixel-for-bitmaps-for-windows-f
    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        public float[] Zbuffer { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());

            Zbuffer = new float[width * height];
        }

        public void ResetZBuffer()
        {
            int max = Width * Height;
            for(int i = 0; i < max; i++)
            {
                Zbuffer[i] = float.MaxValue;
            }
        }

        public bool CheckZbuffer(int x, int y, double z)
        {
            int index = x + (y * Width);
            return z < Zbuffer[index];
        }

        public void SetPixel(int x, int y, float z, Color colour)
        {
            int index = x + (y * Width);
            //int index = x + (Height - y) * Width;
            int col = colour.ToArgb();

            if(index >= 0 && index < Width * Height && z < Zbuffer[index])
            {
                Bits[index] = col;
                Zbuffer[index] = z;
            }
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}
