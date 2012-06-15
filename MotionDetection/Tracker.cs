using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows;

namespace MotionDetection
{
    class Tracker
    {
        public int X = 0;
        public int Y = 0;

        Pen markerPen;
        Pen orangePen = new Pen(Color.Orange, 2);

        List<Rectangle> rects;

        int searchBorder = 80;

        public Tracker(int x, int y, Color markerColor)
        {
            this.X = x;
            this.Y = y;
            markerPen = new Pen(markerColor, 4);
            rects = new List<Rectangle>();
        }

        public void checkRectangleProximity(Rectangle rc)
        {
            int [] rectCenter = Tracker.getCenterOfRectangle(rc);

            if ((Math.Abs(rectCenter[0] - X) < searchBorder) && (Math.Abs(rectCenter[1] - Y) < searchBorder))
                rects.Add(rc);
        }

        public void computeCenter()
        {
            Vector TopLeft = new Vector(640, 480);
            Vector BottomRight = new Vector(0, 0);
            
            int centerX = X;
            int centerY = Y;
            
            foreach (Rectangle rc in rects)
            {
                int[] rectCenter = Tracker.getCenterOfRectangle(rc);

                if (rectCenter[0] < TopLeft.X)
                    TopLeft.X = rectCenter[0];
                if (rectCenter[1] < TopLeft.Y)
                    TopLeft.Y = rectCenter[1];

                if (rectCenter[0] > BottomRight.X)
                    BottomRight.X = rectCenter[0];

                if (rectCenter[1] > BottomRight.Y)
                    BottomRight.Y = rectCenter[1];

                centerX = ((int)BottomRight.X - (int)TopLeft.X) / 2 + (int)TopLeft.X;
                centerY = ((int)BottomRight.Y - (int)TopLeft.Y) / 2 + (int)TopLeft.Y;
            }

            tryToSetCoords(centerX, centerY);

            rects.Clear();
        }

        private bool tryToSetCoords(int centerX, int centerY)
        {
            if ((Math.Abs(centerX - X) < searchBorder) && (Math.Abs(centerY - Y) < searchBorder))
            {
                X = centerX;
                Y = centerY;
                return true;
            }

            return false;
        }

        public void drawTracker(Graphics g)
        {
            g.DrawEllipse(markerPen, new Rectangle(X - 10, Y - 10, 20, 20));
            g.DrawRectangle(orangePen, new Rectangle(X - searchBorder, Y - searchBorder, searchBorder * 2, searchBorder * 2));
        }

        public static int[] getCenterOfRectangle(Rectangle rc)
        {
            int x = rc.X + rc.Width / 2;
            int y = rc.Y + rc.Height / 2;
            int[] tab = { x, y };

            return tab;
        }

        public bool compareAndChange(Tracker otherTracker)
        {
            if ((this.X == otherTracker.X) && (this.Y == otherTracker.Y))
            {
                this.X += 30;
                this.Y += 30;
                return true;
            }
            return false;
        }
      
    }



}
