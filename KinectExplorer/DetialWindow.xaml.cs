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
        private double size, w, h;
        Storyboard stdStart, stdEnd, stdMiddle;
        //private Point lastCenter;
        private double lastTimesX = 1;
        private Point gdCenter;

        private Point leftHand, rightHand, leftHandLast, rightHandLast;
        private DispatcherTimer timer;



        public DetialWindow(FileInfo imgSrc)
        {
            InitializeComponent();
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            this.Left = 0;
            this.Top = 0;
            // this.Topmost = true;
            gdCenter=new Point(0,0);

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

            stdStart.Completed += (a, b) =>
            {
                //stdMiddle.Begin();   
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
                      
                        double lenY = rX-lX;
                        double timesX = lenY / gd.ActualWidth ;
                        timesX = timesX*timesX*2;

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
                timer.Start();
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
            //stdMiddle.Begin();
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
            var widthx = new DoubleAnimation(w, w2, new Duration(TimeSpan.FromMilliseconds(500)));
            var heightx = new DoubleAnimation(h, h2, new Duration(TimeSpan.FromMilliseconds(500)));

            gd.BeginAnimation(Grid.OpacityProperty, opacityGrid);
            gd.BeginAnimation(Grid.WidthProperty, widthx);
            gd.BeginAnimation(Grid.HeightProperty, heightx);

        }

        private void main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //ClosingAnmit();

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
            stdEnd.Begin();
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


        }


        private void main_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseThis();
        }


        private static void SetElementPosition(FrameworkElement element, Point point)
        {
            //从对象的（left,top）修正为该对象的质心位置
            Canvas.SetLeft(element, point.X - element.Width / 2);
            Canvas.SetTop(element, point.Y - element.Height / 2);
        }


    }
}
