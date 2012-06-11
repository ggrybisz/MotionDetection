using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using AForge.Video.FFMPEG;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Concurrent;
using System.Threading;
//dodalem cos:P k.bzowski

namespace MotionDetection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VideoProcessor processor;
        VideoFileReader videoReader;

        Thread processMovieThread;
        Thread updateImagesThread;

        BlockingCollection<BitmapSource> diffrencePictures = new BlockingCollection<BitmapSource>();

        public MainWindow()
        {
            InitializeComponent();

            processMovieThread = new Thread(new ThreadStart(processMovie));
            processMovieThread.Name = "PROCESS MOVIE THREAD";

            updateImagesThread = new Thread(new ThreadStart(updateScreens));
            updateImagesThread.Name = "UPDATE IMAGES THREAD";
        }

        private void processMovie()
        {
            if (videoReader.IsOpen)
            {
                Bitmap videoFrame;

                processor.Background = videoReader.ReadVideoFrame();

                while((videoFrame = videoReader.ReadVideoFrame())!=null)
                {
                    Bitmap diff = processor.findDifference(videoFrame);
                  
                    this.Dispatcher.Invoke(new Action(() => differenceImage.Source = VideoProcessor.convertBitmap(diff)));
                    
                    Bitmap final = processor.showMotion(videoFrame, diff);
                    this.Dispatcher.Invoke(new Action(() => finalImage.Source = VideoProcessor.convertBitmap(final)));
            
                    videoFrame.Dispose();
                    diff.Dispose();
                   // final.Dispose();
                    System.Console.Out.WriteLine("PROCESS MOVIE frame " + diffrencePictures.Count);
                }
               
                videoReader.Close();
                videoReader.Dispose();
            }
        }

        private void updateScreens()
        {
            System.Console.Out.WriteLine("UPDATE SCREEN START");
            while (true)
            {
                
                BitmapSource item = diffrencePictures.Take(); //jeśli kolejka jest pusta, blokuje wątek
                item.Dispatcher.Invoke(new Action(() => differenceImage.Source = item));
                System.Console.Out.WriteLine("UPDATE SCREEN LOOP");
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            processor = new VideoProcessor();
            videoReader = new VideoFileReader();

            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Uri path = new Uri(dialog.FileName);
                videoReader.Open(path.OriginalString);

                originalMediaElement.Source = path;
                originalMediaElement.IsMuted = true;
                originalMediaElement.LoadedBehavior = MediaState.Play;

                processMovieThread.Start();
               // updateImagesThread.Start();
            }
        }
    }
}
