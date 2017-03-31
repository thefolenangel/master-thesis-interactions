using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SW9_Project
{
    public class Gyroscope
    {
        double runningCountZ = 0;
        double runningCountX = 0;
        double absX, absZ;
        double xScale = 0.24f;
        double zScale = 0.26f;
        int xScaleRaw = 110;
        int zScaleRaw = 140;
        long previousTime = 0;
        int screenWidth = Screen.PrimaryScreen.Bounds.Width;
        int screenHeight = Screen.PrimaryScreen.Bounds.Height;
        Cursor cursor = new Cursor(Cursor.Current.Handle); // For test

        public Gyroscope()
        {
            Cursor.Position = new Point(800, 450);
        }

        public void Update(string nTime, string nX, string nY, string nZ)
        {
            try
            {
                double x = Math.Round(double.Parse(nX, CultureInfo.InvariantCulture), 10) / xScaleRaw;
                //double y = Math.Round(double.Parse(nX, CultureInfo.InvariantCulture), 10);
                double z = Math.Round(double.Parse(nZ, CultureInfo.InvariantCulture), 10) / zScaleRaw;
                long currentTime = long.Parse(nTime, CultureInfo.InvariantCulture);

                if (currentTime < previousTime)
                    return;

                //runningCountX += x;
                //runningCountZ += z;
                absX = z;
                absZ = x;
                previousTime = currentTime;
            }
            catch (FormatException)
            {
            }

            //RunningCountLimit(ref runningCountX, ref runningCountZ);

            double cx = absX * ((screenWidth / xScale) / 2.0) + (screenWidth / 2.0);
            double cy = absZ * ((screenHeight / zScale) / 2.0) + (screenHeight / 2.0);

            //Console.WriteLine("RC:" + runningCountX + "\t" + runningCountZ);
            //Console.WriteLine("SC:" + cy + "\t" + cx);
            //Cursor.Position = new Point((int)cx, (int)cy);

            //Console.WriteLine(absX + "\t" + absZ);
            CanvasWindow.GyroPositionX = -absX;
            CanvasWindow.GyroPositionY = -absZ;
            //Console.WriteLine("X:" + -runningCountZ + " Y:" + -runningCountX);
        }

        public void ResetGyroscope()
        {
            runningCountX = 0;
            runningCountZ = 0;
        }

        private void RunningCountLimit(ref double x, ref double z)
        {
            if (x > xScale)
                x = xScale;
            else if (x < -xScale)
                x = -xScale;
            if (z > zScale)
                z = zScale;
            else if (z < -zScale)
                z = -zScale;
        }
    }
}