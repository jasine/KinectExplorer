﻿using System;
using System.Collections.Generic;
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
using Id3Lib;
using Mp3Lib;

namespace KinectExplorer
{
    /// <summary>
    /// DetialWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MusicWindow : Window
    {
        private ImageSource img;
        private double size, w, h;
        Storyboard stdStart, stdEnd, std_MusicFinish, stdEnd2;
        //private Point lastCenter;
        public static MusicWindow Instace { get; private set; }

        public static MusicWindow GetInstance(FileInfo imgSrc)
        {
            if (Instace != null)
                Instace.Close();
            Instace = new MusicWindow(imgSrc);
            return Instace;
        }

        private DispatcherTimer timerLyric, timerPhoto;
        private string fileName;
        private int photoIndex;

        private MusicWindow(FileInfo imgSrc)
        {

            InitializeComponent();
            SpectrumAnalyzer.RegisterSoundPlayer(BassEngine.Instance);
            string file = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) +@"\"+
                          System.IO.Path.GetFileNameWithoutExtension(imgSrc.Name) + ".mp3";
            fileName = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + @"\Lyrics\"
                + System.IO.Path.GetFileNameWithoutExtension(imgSrc.Name) + ".lrc";

            TagHandler _tagHandler = new TagHandler(new Mp3File(file).TagModel);
            Title.Text = _tagHandler.Title != ""
                           ? _tagHandler.Title
                           : System.IO.Path.GetFileNameWithoutExtension(imgSrc.Name);
            OrilTime.Text =_tagHandler.Track!=""?"发源时间："+ _tagHandler.Genre:"";
            
            Kind.Text =_tagHandler.Track!=""?"类别："+ _tagHandler.Artist:"";// != "" ? _tagHandler.Artist : "未知类别";
            OrilLoca.Text =_tagHandler.Track!=""?"起源地："+ _tagHandler.Album:"";// != "" ? "《" + _tagHandler.Album + "》" : "未知专辑";
            Year.Text =_tagHandler.Track!=""? "入遗时间："+_tagHandler.Year:"";
            Rank.Text =_tagHandler.Track!=""?"级别："+ _tagHandler.Track:"";



            timerLyric = new DispatcherTimer();
            timerLyric.Interval = TimeSpan.FromMilliseconds(200);
            timerLyric.Tick += (a, b) =>
                {
                    ShowLyric();
                };
            timerLyric.Start();

            DirectoryInfo phothDir = new DirectoryInfo(imgSrc.Directory.FullName + @"\" + Path.GetFileNameWithoutExtension(imgSrc.Name));
            if (phothDir.Exists)
            {
                photo.ImageUrl = imgSrc.FullName;
                photo.Width = gd.Width;
                photo.Height = gd.Height;
                List<FileInfo> photos = new List<FileInfo>();

                photos.AddRange(phothDir.GetFiles("*.jpg", SearchOption.AllDirectories));
                photos.AddRange(phothDir.GetFiles("*.png", SearchOption.AllDirectories));
                photos.Add(imgSrc);
                timerPhoto = new DispatcherTimer {Interval = TimeSpan.FromSeconds(4)};
                timerPhoto.Tick += (a, b) =>
                    {
                        photo.ImageUrl = photos[photoIndex++].FullName;
                        photoIndex = photoIndex % photos.Count;
                    };
                timerPhoto.Start();

            }

            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            this.Left = 0;
            this.Top = 0;
            // this.Topmost = true;

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
            std_MusicFinish = (Storyboard)this.Resources["MusicFinish"];
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
                process.BeginAnimation(ProgressBar.WidthProperty, datPrs);
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
                    int temp = sttr.IndexOf(sss);
                    if (temp!=-1)
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


        public void CloseThis()
        {
            if (stdEnd != null)
            {
                std_MusicFinish.Begin();
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
                stdEnd = null;
            }
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
                timerPhoto.Stop();
                BassEngine.Instance.Pause();
                playStatue.Opacity = 0.7;               
            }
            else
            {
                playStatue.Source = new BitmapImage(new Uri(System.Environment.CurrentDirectory + @"\pause.png"));
                timerLyric.Start();
                timerPhoto.Start();
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
                TimeSpan.FromMilliseconds(BassEngine.Instance.ChannelLength.TotalMilliseconds * precent);
            ShowLyric();
            //lyric.Text = "";
        }


        public void Forword()
        {
            ChangePlayingTime(BassEngine.Instance.ChannelPosition.TotalMilliseconds /
                BassEngine.Instance.ChannelLength.TotalMilliseconds + 0.1);
        }

        public void Backword()
        {
            ChangePlayingTime(BassEngine.Instance.ChannelPosition.TotalMilliseconds /
                BassEngine.Instance.ChannelLength.TotalMilliseconds - 0.1);
        }



    }
}
