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

        public MainWindow()
        {
            InitializeComponent();

            processMovieThread = new Thread(new ThreadStart(processMovie));
            processMovieThread.Name = "PROCESS MOVIE THREAD";

        }

        private void processMovie()
        {
            if (videoReader.IsOpen)
            {
                Bitmap videoFrame;

                processor.Background = videoReader.ReadVideoFrame();

                int i = 0;

                while ((videoFrame = videoReader.ReadVideoFrame()) != null)
                {
                    Bitmap preprocess = processor.preprocessImage(videoFrame);
                    this.Dispatcher.Invoke(new Action(() => frameCountLabel.Content = "Frame: " + ++i));
                    this.Dispatcher.Invoke(new Action(() => preprocessedImage.Source = VideoProcessor.convertBitmap(preprocess)));

                    Bitmap diff = processor.findDifference(preprocess);
                    preprocess.Dispose();

                    this.Dispatcher.Invoke(new Action(() => differenceImage.Source = VideoProcessor.convertBitmap(diff)));

                    Bitmap final = processor.showMotion(videoFrame, diff);
                    diff.Dispose();

                    this.Dispatcher.Invoke(new Action(() => finalImage.Source = VideoProcessor.convertBitmap(final)));
                    final.Dispose();
                    videoFrame.Dispose();
                }
                i = 0;
                videoReader.Dispose();
                videoReader.Close();

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
                //m
                startButton.Visibility = Visibility.Visible;
                pathLabel.Content = "Ścieżka: " + path.OriginalString;


                //m

            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            processMovieThread.Abort();
        }
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            //m
            firstNumberLabel.Visibility = Visibility.Visible;
            //m
            secondNumberLabel.Visibility = Visibility.Visible;
            //m
            thirdNumberLabel.Visibility = Visibility.Visible;
            //m
            processMovieThread.Start();
            //m
            startButton.Visibility = Visibility.Hidden;
            //m
            stopButton.Visibility = Visibility.Visible;
        }
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            //m
            processMovieThread.Abort();

            //m
            // nie wiem jak wyciscic wszystkie 3 image


            //m
            stopButton.Visibility = Visibility.Hidden;
            //m
            pathLabel.Content = "";
        }
    }
}
