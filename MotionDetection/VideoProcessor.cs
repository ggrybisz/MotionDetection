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

        IFilter pixelateFilter;

        Difference differenceFilter;
        Threshold thresholdFilter;
        Grayscale grayscaleFilter;
        Erosion erosionFilter;
        Opening openingFilter;
        Dilatation dilatationFilter;
        Edges edgesFilter;

        Morph morphFilter;
        MoveTowards moveTowardsFilter;

        IFilter extractChannel;
        Merge mergeFilter;

        FiltersSequence filters1;
        FiltersSequence filters2;

        private int counter;
        private int height;
        private int width;

        public VideoProcessor()
        {
            background = null;

            pixelateFilter = new Pixellate();

            differenceFilter = new Difference();
            thresholdFilter = new Threshold(15);
            grayscaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
            erosionFilter = new Erosion();
            openingFilter = new Opening();
            dilatationFilter = new Dilatation();
            edgesFilter = new Edges();

            morphFilter = new Morph();
            morphFilter.SourcePercent = 90.0;

            extractChannel = new ExtractChannel(RGB.R);
            mergeFilter = new Merge();
            moveTowardsFilter = new MoveTowards();

            filters1 = new FiltersSequence();
          //  filters1.Add(pixelateFilter);
            filters1.Add(grayscaleFilter);
            
            filters2 = new FiltersSequence();

            filters2.Add(differenceFilter);
            filters2.Add(thresholdFilter);
            //filters.Add(dilatationFilter);
           // filters.Add(edgesFilter);

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

        public Bitmap findDifference(Bitmap currentFrame)
        {
            /*
            if (this.background == null)
            {
                //stwórz tło za po raz pierwszy;
                Bitmap tmp = this.pixelateFilter.Apply(currentFrame);
                background = tmp;
                height = background.Height;
                width = background.Width;
                tmp.Dispose();
                return background;
            }*/

            Bitmap tmpImage = filters1.Apply(currentFrame);

            if (++counter == 2)
            {
                counter = 0;

                //morphFilter.OverlayImage = tmpImage;
                //morphFilter.ApplyInPlace(this.background);
                moveTowardsFilter.StepSize = 50;
                moveTowardsFilter.OverlayImage = tmpImage;
                moveTowardsFilter.ApplyInPlace(this.background);
              //  this.background = (Bitmap) tmpImage.Clone();
            }


            differenceFilter.OverlayImage = background;

            bitmapData = tmpImage.LockBits(new Rectangle(0, 0, width, height),
              ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            System.Console.WriteLine("background = "+background.PixelFormat.ToString());
            System.Console.WriteLine("image = " + tmpImage.PixelFormat.ToString());

            differenceFilter.ApplyInPlace(bitmapData);
            thresholdFilter.ApplyInPlace(bitmapData);

            Bitmap tmpImage2 = dilatationFilter.Apply(bitmapData);

            tmpImage.UnlockBits(bitmapData);
            tmpImage.Dispose();

            return tmpImage2;
        }

        public Bitmap showMotion(Bitmap originalImage, Bitmap motionImage)
        {
            // czerwony kanał z oryginalnego obrazu
            //IFilter extrachChannel = new ExtractChannel(RGB.R);
            Bitmap redChannel = extractChannel.Apply(originalImage);
            
            //  merge red channel with motion regions
            //Merge mergeFilter = new Merge();
            mergeFilter.OverlayImage = motionImage;
            Bitmap tmp4 = mergeFilter.Apply(redChannel);
           
            // replace red channel in the original image
           
            ReplaceChannel replaceChannel = new ReplaceChannel(RGB.R, tmp4);
            
            Bitmap tmp5 = replaceChannel.Apply(originalImage);

            redChannel.Dispose();
            tmp4.Dispose();
            return tmp5;
        }
    }
}
