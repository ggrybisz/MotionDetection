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
        string path;

        VideoProcessor processor;
        VideoFileReader videoReader;

        Thread processMovieThread;

        public MainWindow()
        {
            InitializeComponent();
            path = null;
            setInitialBitmaps();
        }

        private void setInitialBitmaps()
        {
            Bitmap initial = new Bitmap(320, 240);

            for (int x = 0; x < initial.Width; x++)
            {
                for (int y = 0; y < initial.Height; y++)
                {
                    System.Drawing.Color pixelColor = initial.GetPixel(x, y);
                    System.Drawing.Color newColor = System.Drawing.Color.FromArgb(pixelColor.R, 0, 0);
                    initial.SetPixel(x, y, newColor);
                }
            }

            preprocessedImage.Source = VideoProcessor.convertBitmap(initial);
            differenceImage.Source = VideoProcessor.convertBitmap(initial);
            finalImage.Source = VideoProcessor.convertBitmap(initial);

            initial.Dispose();
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
                    this.Dispatcher.Invoke(new Action(() => frameCountLabel.Content = Properties.Resources.FPS_LABEL_BEGIN + (int)(++i / videoReader.FrameRate) + "s"));
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
                restart();
            }
        }

        private void openFile_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.path = dialog.FileName;
                prepareForPlay(this.path);

                pathLabel.Content = Properties.Resources.PATH_LABEL_BEGIN + path;
                botomStatusBarlabel.Content = Properties.Resources.LOADED_STATUS;

                startButton.IsEnabled = true;
                openFileButton.IsEnabled = false;
                abortButton.IsEnabled = true;

                
            }
            else { botomStatusBarlabel.Content = Properties.Resources.WAIT_STATUS; }
        }

        private void prepareForPlay(string path)
        {
            if ((path != null) || (path.Length != 0))
            {
                processMovieThread = new Thread(new ThreadStart(processMovie));
                processMovieThread.Name = Properties.Resources.PROCESS_MOVIE_THREAD_TITLE;

                processor = new VideoProcessor();
                videoReader = new VideoFileReader();

                videoReader.Open(path);
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (startButton.Content.ToString() == Properties.Resources.START_BUTTON_TEXT)
            {
                start();
            }
            else
            {
                pause();
            }
        }

        private void start()
        {

            startButton.Content = Properties.Resources.PAUSE_BUTTON_TEXT;
            botomStatusBarlabel.Content = Properties.Resources.PLAY_STATUS;

            if (processMovieThread.ThreadState == ThreadState.Suspended)
            {
                processMovieThread.Resume();
            }
            else
            {
                processMovieThread.Start();

            }
        }

        private void pause()
        {
            startButton.Content = Properties.Resources.START_BUTTON_TEXT;
            botomStatusBarlabel.Content = Properties.Resources.PAUSED_STATUS;

            processMovieThread.Suspend();
        }

        private void abortButton_Click(object sender, RoutedEventArgs e)
        {
            if (processMovieThread.ThreadState == ThreadState.Suspended)
            {
                processMovieThread.Resume();
            }
            processMovieThread.Abort();

            processMovieThread = null;

            videoReader.Close();
            videoReader.Dispose();
            videoReader = null;

            processor = null;

            openFileButton.IsEnabled = true;
            abortButton.IsEnabled = false;
            startButton.Content = Properties.Resources.START_BUTTON_TEXT;
            startButton.IsEnabled = false;
            pathLabel.Content = Properties.Resources.DEFAULT_PATH_LABEL;
            botomStatusBarlabel.Content = Properties.Resources.WAIT_STATUS;
            frameCountLabel.Content = Properties.Resources.DEFAULT_FPS_LABEL;

            setInitialBitmaps();
        }

        private void restart()
        {
            prepareForPlay(this.path);

            this.Dispatcher.Invoke(new Action(() => pathLabel.Content = Properties.Resources.PATH_LABEL_BEGIN + path));
            this.Dispatcher.Invoke(new Action(() => botomStatusBarlabel.Content = Properties.Resources.LOADED_STATUS));

            this.Dispatcher.Invoke(new Action(() => startButton.Content = Properties.Resources.START_BUTTON_TEXT));

            this.Dispatcher.Invoke(new Action(() => startButton.IsEnabled = true));
            this.Dispatcher.Invoke(new Action(() => openFileButton.IsEnabled = false));
            this.Dispatcher.Invoke(new Action(() => abortButton.IsEnabled = true));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (processMovieThread != null)
            {
                if (processMovieThread.ThreadState == ThreadState.Suspended)
                {
                    processMovieThread.Resume();
                }
                processMovieThread.Abort();
            }
        }
    }
}
