using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace SharpGVGP
{
    /// <summary>
    /// This class allows retrieving of a Screenshot.
    /// </summary>
    public class Visor
    {
        public int Xi { get; private set; }
        public int Xf { get; private set; }
        public int Yi { get; private set; }
        public int Yf { get; private set; }

        /// <summary>
        /// Creates an instance of the <c>Visor</c> class with vision rectangle
        /// dimensions 0x0.
        /// </summary>
        public Visor()
        {
            Xi = 0;
            Xf = 0;
            Yi = 0;
            Yf = 0;
        }

        /// <summary>
        /// Creates an instance of the <c>Visor</c> class with vision rectangle
        /// dimensions set by the user.
        /// </summary>
        /// <param name="xi">X coordinate of the top left corner</param>
        /// <param name="xf">X coordinate of the bottom right corner</param>
        /// <param name="yi">Y coordinate of the top left corner</param>
        /// <param name="yf">Y coordinate of the bottom right corner</param>
        public Visor(int xi, int xf, int yi, int yf)
        {
            SetVisionRectangle(xi, xf, yi, yf);
        }

        /// <summary>
        /// Creates an instance of the <c>Visor</c> class with vision rectangle
        /// dimensions set by the user.
        /// </summary>
        /// <param name="coordinates">Vector with the coordinates of the
        /// vision rectangle. {Xi,Xf,Yi,Yf}</param>
        public Visor(int[] coordinates)
        {
            SetVisionRectangle(coordinates[0],
                                coordinates[1],
                                coordinates[2],
                                coordinates[3]);
        }

        /// <summary>
        /// Allows setting the vision rectangle using a vector containing the 
        /// dimensions set by the user.
        /// </summary>
        /// <param name="coordinates">Vector with the coordinates of the
        /// vision rectangle. {Xi,Xf,Yi,Yf}</param>
        /// <returns>Returns if the selected triangle is valid</returns>
        public bool SetVisionRectangle(int[] coordinates)
        {
            return SetVisionRectangle(coordinates[0],
                                coordinates[1],
                                coordinates[2],
                                coordinates[3]);
        }

        /// <summary>
        /// Allows setting of the vision rectangle.
        /// </summary>
        /// <param name="xi">X coordinate of the top left corner</param>
        /// <param name="xf">X coordinate of the bottom right corner</param>
        /// <param name="yi">Y coordinate of the top left corner</param>
        /// <param name="yf">Y coordinate of the bottom right corner</param>
        /// <returns>Returns if the selected triangle is valid</returns>
        public bool SetVisionRectangle(int xi, int xf, int yi, int yf)
        {
            this.Xi = xi;
            this.Xf = xf;
            this.Yi = yi;
            this.Yf = yf;
            return ((xf - xi) > 0) && ((yf - yi) > 0);
        }

        /// <summary>
        /// Returns the maximum resolution available for the complete current
        /// vision area.
        /// </summary>
        /// <returns><c>{X resolution, Y resolution}</c></returns>
        public int[] GetMaxRes()
        {
            return new int[] { Xf - Xi + 1, Yf - Yi + 1 };
        }

        /// <summary>
        /// Returns the maximum resolution available for a displaced sub-section
        /// of the current vision area.
        /// </summary>
        /// <param name="offXi">Displacement of the top left X coordinate in pixels</param>
        /// <param name="offYi">Displacement of the top left Y coordinate in pixels</param>
        /// <param name="spanX">Width of the sub-section in pixels</param>
        /// <param name="spanY">Height of the sub-section in pixels</param>
        /// <returns></returns>
        public int[] GetMaxRes(int offXi, int offYi, int spanX, int spanY)
        {
            int xiO = this.Xi + offXi;
            int yiO = this.Yi + offYi;
            int xfO = xiO + spanX;
            int yfO = yiO + spanY;
            return new int[] { xfO - xiO + 1, yfO - yiO + 1 };
        }

        /// <summary>
        /// Returns the maximum X and Y displacement for the given Witdh and
        /// Height of the vision sub-section.
        /// </summary>
        /// <param name="spanX">Width of the sub-section in pixels</param>
        /// <param name="spanY">Height of the sub-section in pixels</param>
        /// <returns></returns>
        public int[] GetMaxXY(int spanX, int spanY)
        {
            int[] xy = {(Xf-spanX-Xi)+1, (Yf-spanY-Yi)+1};
            return xy;
        }

        /// <summary>
        /// Returns a full-resolution image of the specified subsection of the
        /// vision sub-section
        /// </summary>
        /// <param name="offXi">Displacement of the top left X coordinate in pixels</param>
        /// <param name="offYi">Displacement of the top left Y coordinate in pixels</param>
        /// <param name="spanX">Width of the sub-section in pixels</param>
        /// <param name="spanY">Height of the sub-section in pixels</param>
        /// <returns>Three-dimensional matrix where the indices are
        /// <c>{(R(0), G(1) or B(2)),Y,X}</c></returns>
        public long[,,] GetViewHD(int offXi, int offYi, int spanX, int spanY)
        {
            int xiO = this.Xi + offXi;
            int xfO = xiO + spanX;
            int yiO = this.Yi + offYi;
            int yfO = yiO + spanY;
            long[,,] avgs;
            if ((xfO>this.Xf)||(yfO<this.Yf))
            {
                avgs = new long[1, 1, 1];
                avgs[0, 0, 0] = -1;
            }
            else
            {
                avgs = new long[3, spanY, spanX];

                var bmpScreenshot = new Bitmap(spanY, spanX);
                var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
                gfxScreenshot.CopyFromScreen(xiO, yiO, 0, 0, bmpScreenshot.Size);

                BitmapData srcData = bmpScreenshot.LockBits(
                new Rectangle(0, 0, bmpScreenshot.Width, bmpScreenshot.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

                int stride = srcData.Stride;

                IntPtr Scan0 = srcData.Scan0;

                long[] totals;
                int incJ = 1;
                int incI = 1;
                int width = bmpScreenshot.Width;
                int height = bmpScreenshot.Height;
                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    int fil = 0;
                    for (int j = 0; j < height; j += incJ)
                    {
                        int col = 0;
                        for (int i = 0; i < width; i += incI)
                        {
                            totals = new long[] { 0, 0, 0 };
                            for (int y = j; y < j + incJ; y++)
                            {
                                for (int x = i; x < i + incI; x++)
                                {
                                    for (int color = 0; color < 3; color++)
                                    {
                                        int idx = (y * stride) + x * 4 + color;

                                        totals[color] += p[idx];
                                    }
                                }
                            }
                            avgs[2, fil, col] = totals[0];
                            avgs[1, fil, col] = totals[1];
                            avgs[0, fil, col] = totals[2];
                            col++;
                        }
                        fil++;
                    }
                }
                bmpScreenshot.UnlockBits(srcData);
            }
            return avgs;
        }

        /// <summary>
        /// Returns a full-resolution image of the complete vision area
        /// </summary>
        /// <returns>Three-dimensional matrix where the indices are
        /// <c>{(R(0), G(1) or B(2)),Y,X}</c></returns>
        public long[,,] GetViewHD()
        {
            int[] MaxRes = GetMaxRes();
            return GetViewHD(0, 0, MaxRes[0], MaxRes[1]);
        }

        /// <summary>
        /// Returns a downsampled verion of the complete vision area. Pixels on
        /// the same sub-rectangle are averaged together.
        /// </summary>
        /// <param name="NRows">Number of rows of the downsampled image matrix</param>
        /// <param name="NColumns">Number of columns of the downsampled image matrix</param>
        /// <returns>Three-dimensional matrix where the indices are
        /// <c>{(R(0), G(1) or B(2)),Y,X}</c></returns>
        public long[, ,] GetView(int NRows, int NColumns)
        {
            long[, ,] avgs = new long[3, NRows, NColumns];

            int incI = (int)Math.Floor((double)(this.Xf - this.Xi) / NColumns);
            int incJ = (int)Math.Floor((double)(this.Yf - this.Yi) / NRows);
            var bmpScreenshot = new Bitmap(incI * NRows, incJ * NColumns);
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(this.Xi, this.Yi, 0, 0, bmpScreenshot.Size);
            BitmapData srcData = bmpScreenshot.LockBits(
            new Rectangle(0, 0, bmpScreenshot.Width, bmpScreenshot.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            IntPtr Scan0 = srcData.Scan0;

            long[] totals;

            int width = bmpScreenshot.Width;
            int height = bmpScreenshot.Height;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int fil = 0;
                for (int j = 0; j < height; j += incJ)
                {
                    int col = 0;
                    for (int i = 0; i < width; i += incI)
                    {
                        totals = new long[] { 0, 0, 0 };
                        for (int y = j; y < j + incJ; y++)
                        {
                            for (int x = i; x < i + incI; x++)
                            {
                                for (int color = 0; color < 3; color++)
                                {
                                    int idx = (y * stride) + x * 4 + color;

                                    totals[color] += p[idx];
                                }
                            }
                        }
                        avgs[2, fil, col] = totals[0] / (incI * incJ);
                        avgs[1, fil, col] = totals[1] / (incI * incJ);
                        avgs[0, fil, col] = totals[2] / (incI * incJ);
                        col++;
                    }
                    fil++;
                }
            }
            bmpScreenshot.UnlockBits(srcData);
            return avgs;
        }

        /// <summary>
        /// Returns a downsampled version of the specified sub-section of the
        /// vision area. Pixels on the same sub-rectangle are averaged together.
        /// </summary>
        /// <param name="NRows">Number of rows of the downsampled image matrix</param>
        /// <param name="NColumns">Number of columns of the downsampled image matrix</param>
        /// <param name="OffXi">Displacement of the top left X coordinate in pixels</param>
        /// <param name="OffYi">Displacement of the top left Y coordinate in pixels</param>
        /// <param name="SpanX">Width of the sub-section in pixels</param>
        /// <param name="SpanY">Height of the sub-section in pixels</param>
        /// <returns>Three-dimensional matrix where the indices are
        /// <c>{(R(0), G(1) or B(2)),Y,X}</c></returns>
        public long[,,] GetView(int NRows, int NColumns, int OffXi, int OffYi, int SpanX, int SpanY)
        {
            long[,,] avgs = new long[3, NRows, NColumns];
            int xiO = this.Xi + OffXi;
            int yiO = this.Yi + OffYi;
            int xfO = xiO + SpanX;
            int yfO = yiO + SpanY;
            
            int incI = (int)Math.Floor((double)(xfO - xiO) / NColumns);
            int incJ = (int)Math.Floor((double)(yfO - yiO) / NRows);
            var bmpScreenshot = new Bitmap(incI * NRows, incJ * NColumns);
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(xiO, yiO, 0, 0, bmpScreenshot.Size);

            BitmapData srcData = bmpScreenshot.LockBits(
            new Rectangle(0, 0, bmpScreenshot.Width, bmpScreenshot.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            IntPtr Scan0 = srcData.Scan0;

            long[] totals;

            int width = bmpScreenshot.Width;
            int height = bmpScreenshot.Height;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int fil = 0;
                for (int j = 0; j < height; j += incJ)
                {
                    int col = 0;
                    for (int i = 0; i < width; i += incI)
                    {
                        totals = new long[] { 0, 0, 0 };
                        for (int y = j; y < j + incJ; y++)
                        {
                            for (int x = i; x < i + incI; x++)
                            {
                                for (int color = 0; color < 3; color++)
                                {
                                    int idx = (y * stride) + x * 4 + color;

                                    totals[color] += p[idx];
                                }
                            }
                        }
                        avgs[2, fil, col] = totals[0] / (incI * incJ);
                        avgs[1, fil, col] = totals[1] / (incI * incJ);
                        avgs[0, fil, col] = totals[2] / (incI * incJ);
                        col++;
                    }
                    fil++;
                }
            }
            bmpScreenshot.UnlockBits(srcData);
            return avgs;
        }

    }
}
