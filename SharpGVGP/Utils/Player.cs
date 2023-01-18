using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace SharpGVGP
{   
    /// <summary>
    /// This class is what allows the GVGP agent to play o its own. It can simulate keyboard
    /// input, holding and releasing keys.
    /// </summary>
    public class Player
    {
        int NKeys;
        Keys[] AvailableKeys;
        bool[] PressedKeys;

        private const int KEYBDEVENTF_KEYDOWN = 0x0000;
        private const int KEYBDEVENTF_KEYUP = 0x0002;

        /// <summary>
        /// Get an instance of <c>Player</c> class, which allows for keyboard manipulation.
        /// </summary>
        /// <param name="nKeys">Number of usable keys.</param>
        /// <param name="keys">string of the different letters to press</param>
        public Player(int nKeys, String keys)
        {
            this.AvailableKeys = new Keys[nKeys];
            this.NKeys = nKeys;
            char [] KeysToPress = keys.ToCharArray();
            for (int i = 0; i < NKeys; i++)
            {
                this.AvailableKeys[i] = CharToVirtualKey(KeysToPress[i]);
            }
            this.PressedKeys = new bool[NKeys];
        }

        /// <summary>
        /// Simulate the desired combination of key presses.
        /// </summary>
        /// <param name="KeyStatus">Array of states desired for the keys.</param>
        public void PressKeys(bool[] KeyStatus)
        {
            for (int i = 0; i < NKeys; i++)
            {
                if (KeyStatus[i])
                {
                    keybd_event((byte)AvailableKeys[i], 0, KEYBDEVENTF_KEYDOWN, 0);
                    PressedKeys[i] = KeyStatus[i];
                }
                else
                {                
                    if (KeyStatus[i] != PressedKeys[i])
                    {
                        keybd_event((byte)AvailableKeys[i], 0, KEYBDEVENTF_KEYUP, 0);
                        PressedKeys[i] = KeyStatus[i];
                    }              
                }
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Release all the Available Keys.
        /// </summary>
        public void ReleaseAll()
        {
            PressKeys(new bool[NKeys]);
        }

        /// <summary>
        /// Rectify the <c>char</c> to <c>Keys</c> conversion
        /// </summary>
        /// <param name="ch"><c>char</c> for conversion</param>
        /// <returns>The Virtual Key code of <c>ch</c></returns>
        public static Keys CharToVirtualKey(char ch)
        {
            short vkey = VkKeyScan(ch);
            Keys retval = (Keys)(vkey & 0xff);
            int modifiers = vkey >> 8;
            if ((modifiers & 1) != 0) retval |= Keys.Shift;
            if ((modifiers & 2) != 0) retval |= Keys.Control;
            if ((modifiers & 4) != 0) retval |= Keys.Alt;
            return retval;
        }

        /// <summary>
        /// Allows conversion from <c>char</c> to Virtual Key
        /// </summary>
        /// <param name="ch"><c>char</c> to be converted</param>
        /// <returns>Virtual Key associated to <c>ch</c></returns>
        [DllImport("user32.dll")]
        private static extern short VkKeyScan(char ch);

        /// <summary>
        /// Allows for input sending to other applications
        /// </summary>
        /// <param name="bVk">Virtual Key value</param>
        /// <param name="bScan">Set to 0</param>
        /// <param name="dwFlags">Hold or release</param>
        /// <param name="dwExtraInfo">Set to 0</param>
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        /// <summary>
        /// Disposes safely of the <c>Player</c> instance.
        /// </summary>
        ~Player()
        {
            ReleaseAll();
        }
    }
}
