using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Imaging.Filters;
using System.Drawing;
using AForge.Video;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Drawing.Imaging;
using AForge.Imaging;

namespace MotionDetection
{
    class VideoProcessor
    {
        Bitmap background;

        public Bitmap Background
        {
            get { return background; }
            set { background = this.filters1.Apply(value);
            height = value.Height;
                    width=value.Width; }
        }
        BitmapData bitmapData;

        Pixellate pixelateFilter;

        Difference differenceFilter;
        Threshold thresholdFilter;
        Grayscale grayscaleFilter;
        Erosion erosionFilter;
        Opening openingFilter;
        Dilatation dilatationFilter;
        Edges edgesFilter;

        MoveTowards moveTowardsFilter;

        IFilter extractChannel;
        Merge mergeFilter;

        FiltersSequence filters1;
        FiltersSequence filters2;

        Vector rat1;
        Vector rat2;

        private int counter;
        private int height;
        private int width;

        public VideoProcessor()
        {
            background = null;

            pixelateFilter = new Pixellate();
            pixelateFilter.PixelSize = 10;

            differenceFilter = new Difference();
            thresholdFilter = new Threshold(15);
            grayscaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
            erosionFilter = new Erosion();
            openingFilter = new Opening();
            dilatationFilter = new Dilatation();
            edgesFilter = new Edges();

            extractChannel = new ExtractChannel(RGB.R);
            mergeFilter = new Merge();
            moveTowardsFilter = new MoveTowards();

            filters1 = new FiltersSequence();
            filters1.Add(pixelateFilter);
            filters1.Add(grayscaleFilter);
            
            filters2 = new FiltersSequence();

            filters2.Add(differenceFilter);
            filters2.Add(thresholdFilter);
            filters2.Add(erosionFilter);

            rat1 = new Vector(0, 0);
            rat2 = new Vector(0, 0);

            counter = 0;
        }

        public static BitmapSource convertBitmap(Bitmap source)
        {
            System.Console.WriteLine("source image width"+source.Width);
            System.Console.WriteLine("source image height"+source.Height);

            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;

            bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
               IntPtr.Zero, Int32Rect.Empty,
               System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            return bs;
        }

        public Bitmap preprocessImage(Bitmap currentFrame)
        {
            return filters1.Apply(currentFrame);
        }

        public Bitmap findDifference(Bitmap currentFrame)
        {

            Bitmap tmpImage = currentFrame;

            if (++counter == 2)
            {
                counter = 0;


                moveTowardsFilter.StepSize = 100;
                moveTowardsFilter.OverlayImage = tmpImage;
                moveTowardsFilter.ApplyInPlace(this.background);

            }


            differenceFilter.OverlayImage = background;

            bitmapData = tmpImage.LockBits(new Rectangle(0, 0, width, height),
              ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            System.Console.WriteLine("background = "+background.PixelFormat.ToString());
            System.Console.WriteLine("image = " + tmpImage.PixelFormat.ToString());

            Bitmap tmpImage2 = filters2.Apply(bitmapData);

            tmpImage.UnlockBits(bitmapData);
            tmpImage.Dispose();

            return tmpImage2;
        }

        public Bitmap showMotion(Bitmap originalImage, Bitmap motionImage)
        {

            BlobCounter blobCounter = new BlobCounter();

            // get object rectangles
            blobCounter.ProcessImage(motionImage);
            Rectangle[] rects = blobCounter.GetObjectsRectangles();
            // create graphics object from initial image
            Graphics g = Graphics.FromImage(originalImage);
            // draw each rectangle

            Vector TopLeft = new Vector(640/2, 480/2);
            Vector BottomRight = new Vector(640/2, 480/2);
            


            using (Pen pen = new Pen(Color.Red, 2))
            {
                Pen bluePen = new Pen(Color.Blue, 1);
                foreach (Rectangle rc in rects)
                {
                    g.DrawRectangle(bluePen, rc);

                    if (rc.X < TopLeft.X)
                        TopLeft.X = rc.X;

                    if (rc.Y < TopLeft.Y)
                        TopLeft.Y = rc.Y;

                    if (rc.X > BottomRight.X)
                        BottomRight.X = rc.X;

                    if (rc.Y > BottomRight.Y)
                        BottomRight.Y = rc.Y;

                    if (rat1.X == 0)
                    {
                        rat1.X = rc.X;
                        rat1.Y = rc.Y;
                    }

                    int centerX = ((int)BottomRight.X - (int)TopLeft.X) / 2;
                    int centerY = ((int)BottomRight.Y - (int)TopLeft.Y) / 2;

                    rat1.X = TopLeft.X;
                    rat1.Y = TopLeft.Y;
                    
                    //if ((rc.Width > 15) && (rc.Height > 15))
                    //{
                   /* if (Math.Abs((rc.X + rc.Width / 2) - rat1.X) < 100)
                        {
                            rat1.X = rc.X+rc.Width/2;
                            
                        }

                        if(Math.Abs((rc.Y+rc.Height/2) - rat1.Y) < 100)
                        {
                            rat1.Y = rc.Y+rc.Height/2;
                        }
                   // }*/
                }

                g.DrawRectangle(pen, new Rectangle((int)rat1.X - 10, (int)rat1.X - 10, 20, 20));
            }

            g.Dispose();

            return originalImage;
        }
    }
}
