using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGVGP.Utils
{
    /// <summary>
    /// This class allows retrieving of Screenshots of different resolutions.
    /// </summary>
    public class VisionManager
    {
        public int Xi { get; private set; }
        public int Xf { get; private set; }
        public int Yi { get; private set; }
        public int Yf { get; private set; }

        /// <summary>
        /// Creates an instance of the <c>VisionManager</c> class with vision rectangle
        /// dimensions 0x0.
        /// </summary>
        public VisionManager()
        {
            Xi = 0;
            Xf = 0;
            Yi = 0;
            Yf = 0;
        }

        /// <summary>
        /// Creates an instance of the <c>VisionManager</c> class with vision rectangle
        /// dimensions set by the user.
        /// </summary>
        /// <param name="xi">X coordinate of the top left corner</param>
        /// <param name="xf">X coordinate of the bottom right corner</param>
        /// <param name="yi">Y coordinate of the top left corner</param>
        /// <param name="yf">Y coordinate of the bottom right corner</param>
        public VisionManager(int xi, int xf, int yi, int yf)
        {
            SetVisionRectangle(xi, xf, yi, yf);
        }

        /// <summary>
        /// Creates an instance of the <c>VisionManager</c> class with vision rectangle
        /// dimensions set by the user.
        /// </summary>
        /// <param name="coordinates">Vector with the coordinates of the
        /// vision rectangle. {Xi,Xf,Yi,Yf}</param>
        public VisionManager(int[] coordinates)
        {
            SetVisionRectangle(coordinates[0],
                                coordinates[1],
                                coordinates[2],
                                coordinates[3]);
        }

        /// <summary>
        /// Allows setting the vision region using the mouse and a screenshot. When calling
        /// from another <c>Form</c>, consider if it should be hidden before calling this
        /// method.
        /// </summary>
        /// <returns>Success of the operation</returns>
        public bool SetVisionRectangle()
        {
            SnippetTool sn = new SnippetTool();
            sn.ShowDialog();
            return SetVisionRectangle(sn.GetVisionBoundaries());
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
            int[] xy = { (Xf - spanX - Xi) + 1, (Yf - spanY - Yi) + 1 };
            return xy;
        }

        /// <summary>
        /// Returns a screenshot of the selected region at full resolution
        /// </summary>
        /// <returns>Bitmap of full resolution</returns>
        public Bitmap GetView()
        {
            Bitmap screen = new Bitmap(Xf - Xi, Yf - Yi);
            Graphics gs = Graphics.FromImage(screen);
            gs.CopyFromScreen(Xi, Yi, 0, 0, screen.Size);
            return screen;
        }

        /// <summary>
        /// Returns a screenshot of a sub-region of the selected region at full resolution
        /// </summary>
        /// <param name="xOffset">Displacement in X of the region</param>
        /// <param name="yOffset">Displacement in Y of the region</param>
        /// <param name="xSpan">Width of the sub-region</param>
        /// <param name="ySpan">Height of the sub-region</param>
        /// <returns>Screenshot of the sub-region</returns>
        public Bitmap GetView(int xOffset, int yOffset, int xSpan, int ySpan)
        {
            Bitmap screen = new Bitmap(xSpan, ySpan);
            Graphics gs = Graphics.FromImage(screen);
            gs.CopyFromScreen(Xi+xOffset, Yi+yOffset, 0, 0, screen.Size);
            return screen;
        }

        /// <summary>
        /// Returns a downsampled bitmap of the selected resolution
        /// </summary>
        /// <param name="xResolution">Number of rows</param>
        /// <param name="yResolution">Number of columns</param>
        /// <returns>Returns the downsampled screenshot</returns>
        public Bitmap GetViewDownsampled(int xResolution, int yResolution)
        {
            Bitmap flag = new Bitmap(xResolution, yResolution);
            Graphics gf = Graphics.FromImage(flag);
            gf.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            gf.DrawImage(GetView(), 0, 0, xResolution, yResolution);
            return flag;
        }

        /// <summary>
        /// Returns a downsampled screenshot of a selected sub-region
        /// </summary>
        /// <param name="xResolution">Number of rows</param>
        /// <param name="yResolution">Number of columns</param>
        /// <param name="xOffset">Displacement in X of the region</param>
        /// <param name="yOffset">Displacement in Y of the region</param>
        /// <param name="xSpan">Width of the sub-region</param>
        /// <param name="ySpan">Height of the sub-region</param>
        /// <returns>Downsampled screenshot of the sub-region</returns>
        public Bitmap GetViewDownsampled(int xResolution, int yResolution,
            int xOffset, int yOffset, int xSpan, int ySpan)
        {
            if(xSpan<xResolution || ySpan < yResolution)
            {
                return null;
            }
            Bitmap flag = new Bitmap(xResolution, yResolution);
            Graphics gf = Graphics.FromImage(flag);
            gf.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            gf.DrawImage(GetView(xOffset,yOffset,xSpan,ySpan), 0, 0, xResolution, yResolution);
            return flag;
        }
    }
}
