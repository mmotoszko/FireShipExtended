using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FireShip
{
    public struct Vector
    {
        public double X { set; get; }
        public double Y { set; get; }

        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector(Vector old, double x, double y)
        {
            X = old.X + x;
            Y = old.Y + y;
        }
    }

    static class Program
    {
        public static int consoleHeight = 80;
        public static int consoleWidth = 80;
        public static int screenHeight = consoleHeight * 12;
        public static int screenWidth = consoleWidth * 8;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            ScreenView screen = new ScreenView();
            screen.BackColor = Color.Black;
            screen.Width = screenWidth;
            screen.Height = screenHeight;
            screen.FormBorderStyle = FormBorderStyle.FixedSingle;

            Application.Run(screen);
        }
    }
}
