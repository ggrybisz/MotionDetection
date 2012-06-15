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
                    this.Dispatcher.Invoke(new Action(() => frameCountLabel.Content = "Czas: " + (int)(++i /videoReader.FrameRate )+ "s"));
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
                startButton.Visibility = Visibility.Visible;
                pathLabel.Content = "Ścieżka: " + path.OriginalString;
                botomStatusBarlabel.Content = "Ok";

                openFile.IsEnabled = false;
                resetButton.IsEnabled = true;
            }
            else { botomStatusBarlabel.Content = "W8"; }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (processMovieThread.ThreadState == ThreadState.Suspended)
            {
                processMovieThread.Resume();
            }
            processMovieThread.Abort();
        }
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            firstNumberLabel.Visibility = Visibility.Visible;
            secondNumberLabel.Visibility = Visibility.Visible;
            thirdNumberLabel.Visibility = Visibility.Visible;

            startButton.Visibility = Visibility.Hidden;
            pauseButton.Visibility = Visibility.Visible;

            botomStatusBarlabel.Content = "PLAY";
            if (processMovieThread.ThreadState == ThreadState.Suspended)
            {
                processMovieThread.Resume();
            }
            else
            {
                processMovieThread.Start();

            }

        }
        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            processMovieThread.Suspend();
            
            pauseButton.Visibility = Visibility.Hidden;
            pathLabel.Content = "";

           botomStatusBarlabel.Content = "STOP";
           startButton.Visibility = Visibility.Visible;
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            if (processMovieThread.ThreadState == ThreadState.Suspended)
            {
                processMovieThread.Resume();
            }
            processMovieThread.Abort();
            openFile.IsEnabled = true;
            resetButton.IsEnabled=false;
            
        }
    }
}
