using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpGVGP.Utils
{
    /// <summary>
    /// Allows for selection of a region of the screen.
    /// <example>
    /// This sample shows how to use <see cref="SnippetTool"/> to select a region of 
    /// the screen.
    /// <code>
    /// Type method()
    /// {
    ///     SnippetTool S = new SnippetTool();
    ///     S.ShowDialog();
    ///     int[] Boundaries = S.GetVisionBoundaries();
    /// }
    /// </code> 
    /// </example>
    /// </summary>
    public partial class SnippetTool : Form
    {
        int selectX;
        int selectY;
        int selectWidth;
        int selectHeight;
        /// <summary>
        /// Pen selector for the cursor
        /// </summary>
        public Pen selectPen;
        bool start = false;
        
        /// <summary>
        /// Gets an instance of the <c>SnippetTool</c> class.
        /// Call the <c>ShowDialog()</c> method to select a region of the screen.
        /// Then, call the <c>GetVisionBoundaries()</c> method to get a vector
        /// with the corresponding values.
        /// </summary>
        public SnippetTool()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Returns the coordinates of the two edges of the vision rectangle (Xi,Yi),
        /// (Xf,Yf) as [Xi,Xf,Yi,Yf]
        /// </summary>
        /// <returns>Vector of values [Xi,Xf,Yi,Yf]</returns>
        public int[] GetVisionBoundaries()
        {
            return new int[] { selectX, selectX + selectWidth - 1, selectY, selectY + selectHeight - 1 };
        } 

        private void SnippetTool_Load(object sender, EventArgs e)
        {
            //Hide the Form
            this.Hide();
            //Create the Bitmap
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                     Screen.PrimaryScreen.Bounds.Height);
            //Create the Graphic Variable with screen Dimensions
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            //Copy Image from the screen
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            //Create a temporal memory stream for the image
            using (MemoryStream s = new MemoryStream())
            {
                //save graphic variable into memory
                printscreen.Save(s, ImageFormat.Bmp);
                pictureBox1.Size = new System.Drawing.Size(this.Width, this.Height);
                //set the picture box with temporary stream
                pictureBox1.Image = Image.FromStream(s);
            }
            //Show Form
            this.Show();
            //Cross Cursor
            Cursor = Cursors.Cross;
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //validate if there is an image
            if (pictureBox1.Image == null)
                return;
            //validate if right-click was trigger
            if (start)
            {
                //refresh picture box
                pictureBox1.Refresh();
                //set corner square to mouse coordinates
                selectWidth = e.X - selectX;
                selectHeight = e.Y - selectY;
                //draw dotted rectangle
                pictureBox1.CreateGraphics().DrawRectangle(selectPen,
                          selectX, selectY, selectWidth, selectHeight);
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            //validate when user right-click
            if (!start)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    //starts coordinates for rectangle
                    selectX = e.X;
                    selectY = e.Y;
                    selectPen = new Pen(Color.Red, 1)
                    {
                        DashStyle = DashStyle.DashDotDot
                    };
                }
                //refresh picture box
                pictureBox1.Refresh();
                //start control variable for draw rectangle
                start = true;
            }
            else
            {
                //validate if there is image
                if (pictureBox1.Image == null)
                    return;
                //same functionality when mouse is over
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    pictureBox1.Refresh();
                    selectWidth = e.X - selectX;
                    selectHeight = e.Y - selectY;
                    pictureBox1.CreateGraphics().DrawRectangle(selectPen, selectX,
                             selectY, selectWidth, selectHeight);

                }
                start = false;
                //send to form 1
                if (selectWidth > 0)
                {
                    MessageBox.Show("Vision set");
                }
                this.Close();
            }
        }
    }
}
