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
       // Bitmap backgroundFrame;
        Difference difference = new Difference();
        IFilter threshold = new Threshold(15);
        Grayscale grayscaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
        Erosion erosionFilter = new Erosion();

        public VideoProcessor()
        {
            
        }

        public static BitmapSource convertBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;

            bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
               IntPtr.Zero, Int32Rect.Empty,
               System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            return bs;
        }

        public Bitmap findDifference(Bitmap background, Bitmap currentFrame)
        {
            //Przekształcenie w obrazy czarno-białe
            
            background = grayscaleFilter.Apply(background);
            currentFrame = grayscaleFilter.Apply(currentFrame);

            difference.OverlayImage = background;
            
            //różnica pomiędzy obrazami
            Bitmap tmp1 = difference.Apply(currentFrame);
            //przekształcenie na obraz binarny
            Bitmap tmp2 = threshold.Apply(tmp1);

            //Usunięcie szumu
            Bitmap tmp3 = erosionFilter.Apply(tmp2);
            tmp1.Dispose();
            tmp2.Dispose();
            return tmp3;
        }

        public Bitmap showMotion(Bitmap originalImage, Bitmap motionImage)
        {
            // czerwony kanał z oryginalnego obrazu
            IFilter extrachChannel = new ExtractChannel(RGB.R);
            Bitmap redChannel = extrachChannel.Apply(originalImage);
            
            //  merge red channel with motion regions
            Merge mergeFilter = new Merge();
            mergeFilter.OverlayImage = motionImage;
            Bitmap tmp4 = mergeFilter.Apply(redChannel);
            // replace red channel in the original image
            ReplaceChannel replaceChannel = new ReplaceChannel(RGB.R, tmp4);
           // replaceChannel.ChannelImage = tmp4;
            Bitmap tmp5 = replaceChannel.Apply(originalImage);

            redChannel.Dispose();
            tmp4.Dispose();
            return tmp5;
        }
    }
}
