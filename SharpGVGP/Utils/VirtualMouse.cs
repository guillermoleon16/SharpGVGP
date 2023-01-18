using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpGVGP.Utils
{
    /// <summary>
    /// This class allows mouse control
    /// </summary>
    public static class VirtualMouse
    {        
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        // constants for the mouse_input() API function
        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;


        /// <summary>
        /// Move the mouse relative to its current position
        /// </summary>
        /// <param name="xDelta">Increment in X direction</param>
        /// <param name="yDelta">Increment in Y direction</param>
        public static void Move(int xDelta, int yDelta)
        {
            mouse_event(MOUSEEVENTF_MOVE, xDelta, yDelta, 0, 0);
        }


        /// <summary>
        /// Move to the desired pixel position on the screen.
        /// </summary>
        /// <param name="x">X coordinate of the desired pixel</param>
        /// <param name="y">Y coordinate of the desired pixel</param>
        public static void MoveTo(int x, int y)
        {
            int w = Screen.PrimaryScreen.Bounds.Width;
            int h = Screen.PrimaryScreen.Bounds.Height;
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, (x*65535)/w, (y*65535)/h, 0, 0);
        }


        /// <summary>
        /// Simulates a left click on the mouse at the cursor's position
        /// </summary>
        public static void LeftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
        }

        /// <summary>
        /// Simulates a right click of the mouse at the cursor's position
        /// </summary>
        public static void RightClick()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
        }

        /// <summary>
        /// Simulates a middle click of the mouse at the cursor's position
        /// </summary>
        public static void MiddleClick()
        {
            mouse_event(MOUSEEVENTF_MIDDLEDOWN, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
            mouse_event(MOUSEEVENTF_MIDDLEUP, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
        }

        /// <summary>
        /// Hold and don't release the left mouse button.
        /// </summary>
        public static void LeftPress()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
        }

        /// <summary>
        /// Hold and don't release the right mouse button.
        /// </summary>
        public static void RightPress()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
        }

        /// <summary>
        /// Hold and don't release the middle mouse button.
        /// </summary>
        public static void MiddlePress()
        {
            mouse_event(MOUSEEVENTF_MIDDLEDOWN, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
        }

        /// <summary>
        /// Release the left mouse key.
        /// </summary>
        public static void LeftRelease()
        {
            mouse_event(MOUSEEVENTF_LEFTUP, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
        }

        /// <summary>
        /// Release the right mouse key.
        /// </summary>
        public static void RightRelease()
        {
            mouse_event(MOUSEEVENTF_RIGHTUP, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
        }

        /// <summary>
        /// Release the middle mouse key.
        /// </summary>
        public static void MiddleRelease()
        {
            mouse_event(MOUSEEVENTF_MIDDLEUP, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
        }

        /// <summary>
        /// Release all of the mouse buttons.
        /// </summary>
        public static void ReleaseAll()
        {
            mouse_event(MOUSEEVENTF_RIGHTUP, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
            mouse_event(MOUSEEVENTF_MIDDLEUP, Control.MousePosition.X, Control.MousePosition.Y, 0, 0);
        }
        
    }
}
