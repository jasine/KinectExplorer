using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.IO;
using System.Windows.Threading;
using DoubanFM.Bass;
using ID3;

namespace KinectExplorer
{
    /// <summary>
    /// DetialWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MusicWindow : Window
    {
        private ImageSource img;
        private double size, w, h;
        Storyboard stdStart, stdEnd, stdMiddle, stdEnd2;
        //private Point lastCenter;
        private double lastTimesX = 1;
        private Point gdCenter;

        private Point leftHand, rightHand, leftHandLast, rightHandLast;
        private DispatcherTimer timer;
        private DispatcherTimer timerLyric;
        private string fileName;

        public MusicWindow(FileInfo imgSrc, ID3Info id3)
        {
           
            InitializeComponent();
            SpectrumAnalyzer.RegisterSoundPlayer(BassEngine.Instance);
            fileName = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + @"\Lyrics\"
                + System.IO.Path.GetFileNameWithoutExtension(imgSrc.Name) + ".lrc";
            if (id3.ID3v2Info.HaveTag)
            {            
                songName.Text = id3.ID3v2Info.GetTextFrame("TIT2") != ""
                               ? id3.ID3v2Info.GetTextFrame("TIT2")
                               : System.IO.Path.GetFileNameWithoutExtension(imgSrc.Name);
                artist.Text = id3.ID3v2Info.GetTextFrame("TPE1") != "" ? id3.ID3v2Info.GetTextFrame("TPE1") : "未知艺术家";
                album.Text = id3.ID3v2Info.GetTextFrame("TALB") != "" ? "《" + id3.ID3v2Info.GetTextFrame("TALB") + "》" : "未知专辑";
                year.Text = id3.ID3v2Info.GetTextFrame("TYER");
                //kind.Text = id3.ID3v2Info.GetTextFrame("TCON");
            }
            else
            {
                songName.Text = id3.ID3v1Info.Title != ""
                               ? id3.ID3v1Info.Title
                               : System.IO.Path.GetFileNameWithoutExtension(imgSrc.Name);
                artist.Text = id3.ID3v1Info.Artist != "" ? id3.ID3v1Info.Artist : "未知艺术家";
                album.Text = id3.ID3v1Info.Album != "" ? "《" + id3.ID3v1Info.Album + "》" : "未知专辑";
                year.Text = id3.ID3v1Info.Year;
                //kind.Text = genre[id3.ID3v1Info.Genre];
            }
            

            timerLyric = new DispatcherTimer();
            timerLyric.Interval = TimeSpan.FromMilliseconds(200);
            timerLyric.Tick +=(a, b) =>
                {
                    ShowLyric();
                };
            timerLyric.Start();



            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            this.Left = 0;
            this.Top = 0;
            // this.Topmost = true;
            gdCenter = new Point(0, 0);

            img = new BitmapImage(new Uri(imgSrc.FullName));

            //根据分辨率不同，调整DetialWindow出现的位置
            if (this.Width > 1300)
            {
                size = SystemParameters.PrimaryScreenWidth * 0.415;
            }
            else if (this.Width < 1300 && this.Width > 1000)
            {
                size = SystemParameters.PrimaryScreenWidth * 0.3;
            }
            gd.Background = new ImageBrush(img);
            if (img.Width >= img.Height)
            {
                w = size;
                h = size / img.Width * img.Height;
            }
            else
            {
                h = size;
                w = size / img.Height * img.Width;
            }




            stdStart = (Storyboard)this.Resources["start"];
            stdMiddle = (Storyboard)this.Resources["middle"];
            stdEnd = (Storyboard)this.Resources["end"];
            stdEnd2 = (Storyboard)this.Resources["end_2"];

            stdStart.Completed += (a, b) =>
            {
                //stdMiddle.Begin();
                MusicInfo.Visibility = Visibility.Visible;
                var datImg = new DoubleAnimation(0, -1 * SystemParameters.PrimaryScreenWidth * 0.2, new Duration(TimeSpan.FromMilliseconds(700)));
                var datInfo = new DoubleAnimation(0, SystemParameters.PrimaryScreenWidth * 0.2, new Duration(TimeSpan.FromMilliseconds(700)));
                var datPrs = new DoubleAnimation(0, 300, new Duration(TimeSpan.FromMilliseconds(2000)));

                tlt.BeginAnimation(TranslateTransform.XProperty, datImg);
                infoTlt.BeginAnimation(TranslateTransform.XProperty, datInfo);
                process.BeginAnimation(ProgressBar.WidthProperty,datPrs);

                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(200);
                timer.Tick += (c, d) =>
                {
                    double lX = Canvas.GetLeft(LeftHand) + LeftHand.ActualWidth / 2;
                    double lY = Canvas.GetTop(LeftHand) + LeftHand.ActualHeight / 2;
                    double rX = Canvas.GetLeft(RightHand) + RightHand.ActualWidth / 2;
                    double rY = Canvas.GetTop(RightHand) + RightHand.ActualHeight / 2;
                    double centerX = (rX - lX) / 2 + lX;
                    double centerY = (rY - lY) / 2 + lY;

                    double lenY = rX - lX;
                    double timesX = lenY / gd.ActualWidth;
                    timesX = timesX * timesX * 2;

                    //double timesX = Math.Abs(rightHand.X - leftHand.X) / 300;

                    double angleY = Math.Atan((rightHand.Y - leftHand.Y) / (rightHand.X - leftHand.X)) * 180 / Math.PI;
                    double angleYLast = Math.Atan((rightHandLast.Y - leftHandLast.Y) / (rightHandLast.X - leftHandLast.X)) * 180 / Math.PI;



                    leftHandLast = leftHand;
                    rightHandLast = rightHand;


                    if (double.IsInfinity(timesX) || double.IsNaN(timesX) ||
                        leftHandLast.X == 0 || rightHandLast.X == 0 ||
                        double.IsNaN(angleYLast) || Math.Abs(angleY - angleYLast) < 1.5)
                        return;
                    //timesX = Math.Sqrt(timesX);

                    //if (Math.Abs(timesX - lastTimesX) > 0.2)
                    //{
                    var das1 = new DoubleAnimation(lastTimesX, timesX, new Duration(TimeSpan.FromMilliseconds(200)));
                    var das2 = new DoubleAnimation(lastTimesX, timesX, new Duration(TimeSpan.FromMilliseconds(200)));
                    sct.CenterX = gd.ActualWidth / 2;
                    sct.CenterY = gd.ActualHeight / 2;
                    sct.BeginAnimation(ScaleTransform.ScaleXProperty, das1);
                    sct.BeginAnimation(ScaleTransform.ScaleYProperty, das2);
                    lastTimesX = timesX;
                    //// }

                    //double tanOffset = gd.RenderSize.Width / 2 + centerX / gd.RenderSize.Height / 2 + centerY;
                    //angleY += Math.Atan(tanOffset) * 180 / Math.PI;
                    var dar1 = new DoubleAnimation(angleYLast, angleY, new Duration(TimeSpan.FromMilliseconds(200)));
                    rt.CenterX = gd.ActualWidth / 2;
                    rt.CenterY = gd.ActualHeight / 2;
                    rt.BeginAnimation(RotateTransform.AngleProperty, dar1);


                    double deltaX = centerX - gd.ActualWidth / 2;
                    double deltaY = centerY - gd.ActualHeight / 2;
                    var dat1 = new DoubleAnimation(gdCenter.X, deltaX * 1.5, new Duration(TimeSpan.FromMilliseconds(200)));
                    var dat2 = new DoubleAnimation(gdCenter.Y, deltaY * 1.5, new Duration(TimeSpan.FromMilliseconds(200)));
                    tlt.BeginAnimation(TranslateTransform.XProperty, dat1);
                    tlt.BeginAnimation(TranslateTransform.YProperty, dat2);
                    gdCenter.X = deltaX;
                    gdCenter.Y = deltaY;

                };
                //timer.Start();

            };
            stdEnd.Completed += (c, d) =>
            {
                CloseAnmit();
                stdEnd2.Begin();

            };
            stdEnd2.Completed += (e, f) =>
                {
                    this.Close();
                };
            this.Loaded += MainWindow_Loaded;
        }


        /// <summary>
        /// 显示歌词
        /// </summary>
        private void ShowLyric()
        {
            string sss = BassEngine.Instance.ChannelPosition.Minutes.ToString("00") + ":" +
                         BassEngine.Instance.ChannelPosition.Seconds.ToString("00");
            if (!System.IO.File.Exists(fileName))
            {
                lyric.Text = "";
            }
            else
            {
                string sttr = "";
                UTF8Encoding encode = new UTF8Encoding();
                StreamReader objFile = new StreamReader(fileName, encode);
                sttr = objFile.ReadLine();
                while (sttr != null)
                {
                    if (sttr.IndexOf(sss) != -1)
                    {
                        lyric.Text = (sttr.Substring(sttr.LastIndexOf("]") + 1, sttr.Length - sttr.LastIndexOf("]") - 1) +
                                      "\r\n");
                    }
                    sttr = objFile.ReadLine();
                }
                objFile.Close();
                objFile.Dispose();
            }
            double a = BassEngine.Instance.ChannelPosition.Seconds + BassEngine.Instance.ChannelPosition.Minutes * 60;
            double b = BassEngine.Instance.ChannelLength.Seconds + BassEngine.Instance.ChannelLength.Minutes * 60;
            process.Value = a / b * 100;
        }


        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            stdStart.Begin();
            //stdMiddle.Begin();
            playStatue.Source = new BitmapImage(new Uri(System.Environment.CurrentDirectory + @"\pause.png")); playStatue.Source = new BitmapImage(new Uri(System.Environment.CurrentDirectory + @"\pause.png"));
            double w2 = img.Width, h2 = img.Height;

            if (img.Width > SystemParameters.PrimaryScreenWidth - 50 || img.Height > SystemParameters.PrimaryScreenHeight - 30)
            {
                double times1 = img.Width / (SystemParameters.PrimaryScreenWidth - 60.0);
                double times2 = img.Height / (SystemParameters.PrimaryScreenHeight - 60.0);
                double times = times1 > times2 ? times1 : times2;
                w2 = w2 / times;
                h2 = h2 / times;

            }
            var opacityGrid = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0)));
            var widthx = new DoubleAnimation(w, 400, new Duration(TimeSpan.FromMilliseconds(500)));
            var heightx = new DoubleAnimation(h, 400, new Duration(TimeSpan.FromMilliseconds(500)));

            gd.BeginAnimation(Grid.OpacityProperty, opacityGrid);
            gd.BeginAnimation(Grid.WidthProperty, widthx);
            gd.BeginAnimation(Grid.HeightProperty, heightx);

            var datImg = new DoubleAnimation(0, -1 * SystemParameters.PrimaryScreenHeight * 0.1, new Duration(TimeSpan.FromMilliseconds(500)));
            //var datInfo = new DoubleAnimation(0, deltaY * 1.5, new Duration(TimeSpan.FromMilliseconds(200)));
            tlt.BeginAnimation(TranslateTransform.YProperty, datImg);
            infoTlt.BeginAnimation(TranslateTransform.YProperty, datImg);

        }

        private void main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {


        }



        public void SetHandLeftPoint(Point point)
        {

            SetElementPosition(LeftHand, point);
            leftHand = point;

        }
        public void SetHandRightPoint(Point point)
        {
            SetElementPosition(RightHand, point);
            rightHand = point;

        }


        public void CloseThis()
        {
            playStatue.Opacity = 0;
            timerLyric.Stop();
            //timerLyric = null;
            lyric.Text = "";
            var datImg = new DoubleAnimation(-1 * SystemParameters.PrimaryScreenWidth * 0.2, 0, new Duration(TimeSpan.FromMilliseconds(700)));
            var datInfo = new DoubleAnimation(SystemParameters.PrimaryScreenWidth * 0.2, 0, new Duration(TimeSpan.FromMilliseconds(700)));
            tlt.BeginAnimation(TranslateTransform.XProperty, datImg);
            infoTlt.BeginAnimation(TranslateTransform.XProperty, datInfo);
            BassEngine.Instance.Stop();
            stdEnd.Begin();
        }

        private void CloseAnmit()
        {
            MusicInfo.Visibility = Visibility.Hidden;
            //var opacityGrid = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0)));
            var widthx = new DoubleAnimation(gd.ActualWidth, w, new Duration(TimeSpan.FromMilliseconds(300)));
            var heightx = new DoubleAnimation(gd.ActualHeight, h, new Duration(TimeSpan.FromMilliseconds(300)));

            //wb.BeginAnimation(Grid.OpacityProperty, opacityGrid);
            gd.BeginAnimation(Grid.WidthProperty, widthx);
            gd.BeginAnimation(Grid.HeightProperty, heightx);

            if (lastTimesX != 0 && !double.IsNaN(lastTimesX))
            {
                var das1 = new DoubleAnimation(lastTimesX, 1, new Duration(TimeSpan.FromMilliseconds(300)));
                var das2 = new DoubleAnimation(lastTimesX, 1, new Duration(TimeSpan.FromMilliseconds(300)));
                sct.CenterX = gd.ActualWidth / 2;
                sct.CenterY = gd.ActualHeight / 2;
                sct.BeginAnimation(ScaleTransform.ScaleXProperty, das1);
                sct.BeginAnimation(ScaleTransform.ScaleYProperty, das2);
            }

            double angleYLast = Math.Atan((rightHandLast.Y - leftHandLast.Y) / (rightHandLast.X - leftHandLast.X)) * 180 / Math.PI;
            if (!double.IsNaN(angleYLast))
            {
                var dar1 = new DoubleAnimation(angleYLast, 0, new Duration(TimeSpan.FromMilliseconds(100)));
                rt.CenterX = gd.ActualWidth / 2;
                rt.CenterY = gd.ActualHeight / 2;
                rt.BeginAnimation(RotateTransform.AngleProperty, dar1);
            }

            var dat1 = new DoubleAnimation(gdCenter.X, 0, new Duration(TimeSpan.FromMilliseconds(200)));
            var dat2 = new DoubleAnimation(gdCenter.Y, 0, new Duration(TimeSpan.FromMilliseconds(200)));
            tlt.BeginAnimation(TranslateTransform.XProperty, dat1);
            tlt.BeginAnimation(TranslateTransform.YProperty, dat2);

            var datImg = new DoubleAnimation(-1 * SystemParameters.PrimaryScreenHeight * 0.1, 0, new Duration(TimeSpan.FromMilliseconds(300)));
            //var datInfo = new DoubleAnimation(0, deltaY * 1.5, new Duration(TimeSpan.FromMilliseconds(200)));
            tlt.BeginAnimation(TranslateTransform.YProperty, datImg);
        }


        private void main_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(gd);
            if (p.X < gd.ActualWidth && p.Y < gd.ActualHeight && p.X > 0 && p.Y > 0)
                return;
            Point q = e.GetPosition(process);
            if (q.X < process.ActualWidth + 5 && q.Y < process.ActualHeight + 5 && q.X > -5 && q.Y > -5)
                return;            
            CloseThis();


        }


        private static void SetElementPosition(FrameworkElement element, Point point)
        {
            //从对象的（left,top）修正为该对象的质心位置
            Canvas.SetLeft(element, point.X - element.Width / 2);
            Canvas.SetTop(element, point.Y - element.Height / 2);
        }

        private void gd_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void gd_MouseEnter(object sender, MouseEventArgs e)
        {
            if (BassEngine.Instance.ChannelPosition.Seconds >= 1)
            {
                Visibility vi = gd.Visibility;
                //var opacityImg = new DoubleAnimation(07, 0, new Duration(TimeSpan.FromSeconds(1000)));
                //playStatue.BeginAnimation(Image.OpacityProperty, opacityImg);
                playStatue.Opacity = 0.7;
            }

        }

        private void gd_MouseLeave(object sender, MouseEventArgs e)
        {
            if (BassEngine.Instance.CanPlay)
                return;
            //var opacityImg = new DoubleAnimation(0, 0.7, new Duration(TimeSpan.FromSeconds(1000)));
            //playStatue.BeginAnimation(Image.OpacityProperty, opacityImg);       
            playStatue.Opacity = 0;
        }

        private void playStatue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeStatue();
        }

        public void ChangeStatue()
        {
            if (BassEngine.Instance.CanPause == true)
            {
                playStatue.Source = new BitmapImage(new Uri(System.Environment.CurrentDirectory + @"\play.png"));
                timerLyric.Stop();
                BassEngine.Instance.Pause();
                playStatue.Opacity = 0.7;
            }
            else
            {
                playStatue.Source = new BitmapImage(new Uri(System.Environment.CurrentDirectory + @"\pause.png"));
                timerLyric.Start();
                BassEngine.Instance.Play();
                playStatue.Opacity = 0;
            }
        }

        private void process_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(process);
            double precent = p.X / process.Width;
            ChangePlayingTime(precent);
        }

        private void ChangePlayingTime(double precent)
        {
            precent = precent > 1 ? 1 : precent;
            precent = precent < 0 ? 0 : precent;
            BassEngine.Instance.ChannelPosition =
                TimeSpan.FromMilliseconds(BassEngine.Instance.ChannelLength.TotalMilliseconds*precent);
            ShowLyric();
            //lyric.Text = "";
        }


        public void Forword()
        {
            ChangePlayingTime(BassEngine.Instance.ChannelPosition.TotalMilliseconds/
                BassEngine.Instance.ChannelLength.TotalMilliseconds+0.1);
        }

        public void Backword()
        {
            ChangePlayingTime(BassEngine.Instance.ChannelPosition.TotalMilliseconds /
                BassEngine.Instance.ChannelLength.TotalMilliseconds - 0.1);
        }



    }
}
