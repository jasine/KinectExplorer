using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.IO;
using System.Windows.Threading;

namespace KinectExplorer
{
    /// <summary>
    /// DetialWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DetialWindow : Window
    {
        private ImageSource img;
        private double size,w,h;
        Storyboard stdStart, stdEnd,stdMiddle;
        public DetialWindow(FileInfo imgSrc)
        {
            InitializeComponent();
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
            else if (this.Width<1300&&this.Width>1000)
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
            stdStart.Completed += (a, b) =>
            {
                //this.root.Clip = null;
                //stdMiddle.Begin();
            };
            stdEnd.Completed += (c, d) =>
                {
                    this.Close();
                };
            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {          
            stdStart.Begin();
            double w2=img.Width,h2=img.Height;
            
            if (img.Width > SystemParameters.PrimaryScreenWidth - 50||img.Height>SystemParameters.PrimaryScreenHeight-30)
            {
                double times1=img.Width/(SystemParameters.PrimaryScreenWidth - 60.0);
                double times2=img.Height/(SystemParameters.PrimaryScreenHeight - 60.0);
                double times = times1 > times2 ? times1 : times2;
                w2 = w2/times;
                h2 = h2/times;

            }
            var opacityGrid = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0)));
            var widthx = new DoubleAnimation(w, w2, new Duration(TimeSpan.FromMilliseconds(500)));
            var heightx = new DoubleAnimation(h, h2, new Duration(TimeSpan.FromMilliseconds(500)));

            gd.BeginAnimation(Grid.OpacityProperty,opacityGrid);
            gd.BeginAnimation(Grid.WidthProperty,widthx);
            gd.BeginAnimation(Grid.HeightProperty,heightx);
        }

        private void main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //ClosingAnmit();
        }

        public void Scale(Point center, double length)
        {
            
        }
        public void Rotate(Point center, double angle)
        {
            
        }

        private void ClosingAnmit()
        {
            stdEnd.Begin();

            //var opacityGrid = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0)));
            var widthx = new DoubleAnimation(gd.Width, w, new Duration(TimeSpan.FromMilliseconds(300)));
            var heightx = new DoubleAnimation(gd.Height, h, new Duration(TimeSpan.FromMilliseconds(300)));

            //wb.BeginAnimation(Grid.OpacityProperty, opacityGrid);
            gd.BeginAnimation(Grid.WidthProperty, widthx);
            gd.BeginAnimation(Grid.HeightProperty, heightx);
            DispatcherTimer timer=new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(300);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void main_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ClosingAnmit();
        }

    }
}
