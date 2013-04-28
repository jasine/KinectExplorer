using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using DoubanFM.Bass;
using Fizbin.Kinect.Gestures;
using Fizbin.Kinect.Gestures.Segments;
using ID3;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.SwipeGestureRecognizer;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using LeapHelper;

namespace KinectExplorer
{
    public partial class MainWindow : Window
    {
        private class FileInfoComparer : IComparer<FileInfo>
        {
            #region IComparer<FileInfo> Membres

            public int Compare(FileInfo x, FileInfo y)
            {
                return string.Compare(x.FullName, y.FullName);
            }

            #endregion
        }

        #region Handlers

        private void DoKeyDown(Key key)
        {
            switch (key)
            {
                case Key.Right:
                    flow.GoToNext();
                    break;
                case Key.Left:
                    flow.GoToPrevious();
                    break;
                case Key.PageUp:
                    flow.GoToNextPage();
                    break;
                case Key.PageDown:
                    flow.GoToPreviousPage();
                    break;
                case Key.Escape:
                    CloseThis();
                    break;
            }
            if (flow.Index != Convert.ToInt32(slider.Value))
                slider.Value = flow.Index;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            DoKeyDown(e.Key);
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            flow.Index = Convert.ToInt32(slider.Value);
        }

        #endregion

        #region Private stuff
        LeapMotinn leapMotinn;

        private List<FileInfo> images;
        private int currentIndex = 0;

        DirectoryInfo myMusicDir, myCoverDir, myLyricDir, myVideoDir;

        #endregion

        /// <summary>
        /// 加载图片
        /// </summary>
        public void LoadImages()
        {

            images = new List<FileInfo>();

            var commonPicturesDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures));
            images.AddRange(commonPicturesDir.GetFiles("*.jpg", SearchOption.AllDirectories));
            images.AddRange(commonPicturesDir.GetFiles("*.png", SearchOption.AllDirectories));


            var myPicturesDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            images.AddRange(myPicturesDir.GetFiles("*.jpg", SearchOption.AllDirectories));
            images.AddRange(myPicturesDir.GetFiles("*.png", SearchOption.AllDirectories));

            myMusicDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
            myCoverDir = new DirectoryInfo(myMusicDir.FullName + @"\Covers\");
            if (!myCoverDir.Exists)
            {
                myCoverDir.Create();
            }
            myLyricDir = new DirectoryInfo(myMusicDir.FullName + @"\Lyrics\");
            if (!myLyricDir.Exists)
            {
                myLyricDir.Create();
            }
            FileInfo[] musics = myMusicDir.GetFiles("*.mp3");

            foreach (var music in musics)
            {
                string cover = myCoverDir + @"\" + System.IO.Path.GetFileNameWithoutExtension(music.FullName) + ".jpg";
                if (!File.Exists(cover))
                {
                    ID3Info info = new ID3Info(music.FullName, true);//从ID3V2信息中读取封面
                    info.Load();
                    if (info.ID3v2Info.HaveTag && info.ID3v2Info.AttachedPictureFrames.Items.Length > 0)
                    {
                        MemoryStream ms = info.ID3v2Info.AttachedPictureFrames.Items[0].Data;
                        System.Drawing.Image img = Bitmap.FromStream(ms);
                        img.Save(cover, System.Drawing.Imaging.ImageFormat.Jpeg);

                    }
                    else
                    {
                        File.Copy(System.Environment.CurrentDirectory + @"\cover.jpg", cover, true);
                    }
                }
                images.Add(new FileInfo(cover));
            }

            myVideoDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos));
            DirectoryInfo thumbnailDir = new DirectoryInfo(myVideoDir.FullName + @"\thumbnais");
            if (!thumbnailDir.Exists)
            {
                thumbnailDir = myVideoDir.CreateSubdirectory("thumbnails");
            }
            FileInfo[] videos = myVideoDir.GetFiles("*.mp4");
            foreach (var video in videos)
            {
                string thumbnail = thumbnailDir + @"\" + System.IO.Path.GetFileNameWithoutExtension(video.FullName) + ".jpg";

                if (!File.Exists(thumbnail))
                {
                    if (VideoUnity.CatchImg(video.FullName, thumbnail))
                    {
                        images.Add(new FileInfo(thumbnail));
                    }
                    //images.Add(new FileInfo(thumbnail));
                }
                else
                {
                    images.Add(new FileInfo(thumbnail));
                }
            }


            images.Sort(new FileInfoComparer());
            foreach (FileInfo f in images)
                flow.Add(Environment.MachineName, f.FullName);
        }




        #region Kinect 相关变量


        private volatile DetialWindow detialWindow;
        private volatile MusicWindow musicWindow;
        private volatile VideoWindow videoWindow;

        /// <summary>
        /// The recognizer being used.
        /// </summary>
        private readonly Recognizer activeRecognizer;


        //private readonly MotionHelper motionHelper;



        /// <summary>
        /// Array of arrays of contiguous line segements that represent a skeleton.
        /// </summary>
        private static readonly JointType[][] SkeletonSegmentRuns = new JointType[][]
        {
            new JointType[] 
            { 
                JointType.Head, JointType.ShoulderCenter, JointType.HipCenter 
            },
            new JointType[] 
            { 
                JointType.HandLeft, JointType.WristLeft, JointType.ElbowLeft, JointType.ShoulderLeft,
                JointType.ShoulderCenter,
                JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight
            },
            new JointType[]
            {
                JointType.FootLeft, JointType.AnkleLeft, JointType.KneeLeft, JointType.HipLeft,
                JointType.HipCenter,
                JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight
            }
        };

        /// <summary>
        /// The sensor we're currently tracking.
        /// </summary>
        private KinectSensor nui;

        /// <summary>
        /// There is currently no connected sensor.
        /// </summary>
        private bool isDisconnectedField = true;

        /// <summary>
        /// Any message associated with a failure to connect.
        /// </summary>
        private string disconnectedReasonField;

        /// <summary>
        /// Array to receive skeletons from sensor, resize when needed.
        /// </summary>
        private Skeleton[] skeletons = new Skeleton[0];

        /// <summary>
        /// Time until skeleton ceases to be highlighted.
        /// </summary>
        private DateTime highlightTime = DateTime.MinValue;

        /// <summary>
        /// The ID of the skeleton to highlight.
        /// </summary>
        private int highlightId = -1;

        /// <summary>
        /// The ID if the skeleton to be tracked.
        /// </summary>
        private int nearestId = -1;


        private CoordinateMapper mapper;
        private Storyboard stdStart, stdEnd;
        private GestureController gestureController;
        #endregion


        public MainWindow()
        {
            InitializeComponent();
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            this.Top = 0;
            this.Left = 0;

            flow.Width = this.Width;
            flow.Height = this.Height;
            flow.IndexChanged += flow_IndexChanged;
            flow.CenterCoverClicked += flow_CenterCoverClicked;

            flow.Cache = new ThumbnailManager();
            LoadImages();

            slider.Minimum = 0;
            slider.Maximum = flow.Count - 1;


            // Create the gesture recognizer.
            this.activeRecognizer = this.CreateRecognizer();
            // Wire-up window loaded event.
            Loaded += this.Window_Loaded;

            //var opacityGrid = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0)));
            //var widthx = new DoubleAnimation(10, SystemParameters.PrimaryScreenWidth / 2, new Duration(TimeSpan.FromMilliseconds(500)));
            //var heightx = new DoubleAnimation(10, SystemParameters.PrimaryScreenHeight / 2, new Duration(TimeSpan.FromMilliseconds(500)));

            ////this.BeginAnimation(MainWindow.OpacityProperty, opacityGrid);
            //this.BeginAnimation(MainWindow.WidthProperty, widthx);
            //this.BeginAnimation(MainWindow.HeightProperty, heightx);
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            stdStart = (Storyboard)this.Resources["sb_start"];
            stdStart.Begin();

            InitBassEngine();//初始化播放器

            ChangeFileInfo();//显示文件信息

            leapMotinn = new LeapMotinn();
            leapMotinn.Listener.LeapSwipeReady += ListenerLeapSwipeReady;
            leapMotinn.Listener.LeapFingerReady += Listener_LeapFingerReady;
            leapMotinn.Listener.LeapTapScreenReady += ListenerLeapTapScreenReady;
            leapMotinn.Listener.LeapCircleReady += Listener_LeapCircleReady;
            // Start the Kinect system, this will cause StatusChanged events to be queued.
            this.InitializeNui();

            // Handle StatusChange events to pick the first sensor that connects.
            KinectSensor.KinectSensors.StatusChanged += (s, ee) =>
            {
                switch (ee.Status)
                {
                    case KinectStatus.Connected:
                        if (nui == null)
                        {
                            InitializeNui();
                        }
                        break;
                    default:
                        if (ee.Sensor == nui)
                        {
                            UninitializeNui();
                        }
                        break;
                }
            };
        }

        void Listener_LeapCircleReady(object sender)
        {
            if (musicWindow != null)
            {
                Action action = () => musicWindow.ChangeStatue();
                Dispatcher.BeginInvoke(DispatcherPriority.Send, action);
            }
            else if (videoWindow != null)
            {
                Action action = () => videoWindow.ChangeStatue();
                Dispatcher.BeginInvoke(DispatcherPriority.Send, action);
            }
        }

        void ListenerLeapTapScreenReady(object sender)
        {
            if (detialWindow != null || musicWindow != null || videoWindow != null)
            {
                Action action2 = null;
                if (detialWindow != null)
                {
                    action2 = () => detialWindow.CloseThis();
                }
                else if (musicWindow != null)
                {
                    action2 = () => musicWindow.CloseThis();
                }
                else
                {
                    action2 = () => videoWindow.CloseThis();
                }
                Dispatcher.BeginInvoke(DispatcherPriority.Send, action2).Completed += (a, b) =>
                {
                    detialWindow = null;
                    musicWindow = null;
                    videoWindow = null;
                };
            }
            else
            {
                Action action = () => OpenSubWindow();
                Dispatcher.BeginInvoke(DispatcherPriority.Send, action);
            }

        }

        void Listener_LeapFingerReady(object sender, Leap.Finger first, Leap.Finger second)
        {
            if (detialWindow != null)
            {
                Point3D left, right;
                Action action1, action2;
                left = new Point3D(first.TipPosition.x * 3.5,
                        -3.5 * (first.TipPosition.y - 100) + SystemParameters.PrimaryScreenHeight / 2, first.TipPosition.z);
                right = new Point3D(second.TipPosition.x * 3.5,
                    -3.5 * (second.TipPosition.y - 100) + SystemParameters.PrimaryScreenHeight / 2, second.TipPosition.z);
                if (first.TipPosition.x <= second.TipPosition.x)
                {
                    action1 = () => detialWindow.SetHandLeftPoint3D(left);
                    action2 = () => detialWindow.SetHandRightPoint3D(right);
                }
                else
                {
                    action1 = () => detialWindow.SetHandLeftPoint3D(right);
                    action2 = () => detialWindow.SetHandRightPoint3D(left);
                }
                Dispatcher.BeginInvoke(DispatcherPriority.Send, action1);
                Dispatcher.BeginInvoke(DispatcherPriority.Send, action2);
            }
        }

        void ListenerLeapSwipeReady(object sender, SwipeType type)
        {
            Action action = null;
            switch (type)
            {
                case SwipeType.SwipeLeft:
                    if (musicWindow != null)
                    {
                        action = () => musicWindow.Backword();
                    }
                    else if (videoWindow != null)
                    {
                        action = () => videoWindow.Backword();
                    }
                    else if (detialWindow != null)
                    {
                        action = () => detialWindow.Backword();
                    }
                    else
                    {
                        action = () => flow.GoToNext();
                    }

                    break;
                case SwipeType.SwipeRight:
                    if (musicWindow != null)
                    {
                        action = () => musicWindow.Forword();
                    }
                    else if (videoWindow != null)
                    {
                        action = () => videoWindow.Forword();
                    }
                    else if (detialWindow != null)
                    {
                        action = () => detialWindow.Forword();
                    }
                    else
                    {
                        action = () => flow.GoToPrevious();
                    }

                    break;
                //case  SwipeType.SwipeOut:
                //if (detialWindow == null && musicWindow == null)
                //{
                //    action = OpenSubWindow;
                //}
                //break;
                //case SwipeType.SwpieIn:
                //if (detialWindow != null || musicWindow != null)
                //{
                //    Action action2 = null;
                //    if (detialWindow != null)
                //    {
                //        action2 =()=> detialWindow.CloseThis();
                //        //detialWindow.CloseThis();
                //        //detialWindow = null;
                //    }
                //    else
                //    {
                //        action2=()=>musicWindow.CloseThis();
                //        //musicWindow.CloseThis();
                //        //musicWindow = null;
                //    }
                //    Dispatcher.BeginInvoke(DispatcherPriority.Send, action2).Completed+=(a, b) =>
                //        {
                //            detialWindow = null;
                //            musicWindow = null;
                //        };
                //}
                //break;
            }

            if (action != null)
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }


        /// <summary>
        /// 中间的Cover被点击
        /// </summary>
        /// <param name="sender"></param>
        void flow_CenterCoverClicked(object sender)
        {
            OpenSubWindow();
        }


        /// <summary>
        /// 打开图片展示或者音乐播放窗口
        /// </summary>
        private void OpenSubWindow()
        {
            FileInfo info = CheckIfMediaPath(currentIndex);
            if (info != null)
            {
                if (info.Extension == ".mp3")
                {
                    ID3Info id3Info = new ID3Info(info.FullName, true);
                    id3Info.Load();
                    BassEngine.Instance.OpenFile(info.FullName);
                    musicWindow = MusicWindow.GetInstance(images[currentIndex], id3Info);
                    musicWindow.Show();
                }
                else
                {
                    videoWindow = VideoWindow.GetInstance(images[currentIndex], info);//new VideoWindow(images[currentIndex],info);
                    videoWindow.Show();
                }
            }
            else
            {
                detialWindow = DetialWindow.GetInstance(images[currentIndex]);
                detialWindow.Show();
            }
        }


        /// <summary>
        /// 检查点击的Cover是否是音乐或视频文件
        /// </summary>
        /// <param name="currentIndex"></param>
        /// <returns></returns>
        private FileInfo CheckIfMediaPath(int currentIndex)
        {
            string musicPath = myMusicDir.FullName + @"\" +
                          System.IO.Path.GetFileNameWithoutExtension(images[currentIndex].FullName) + ".mp3";
            if (File.Exists(musicPath))
                return new FileInfo(musicPath);
            string videoPath = myVideoDir.FullName + @"\" +
                          System.IO.Path.GetFileNameWithoutExtension(images[currentIndex].FullName) + ".mp4";
            if (File.Exists(videoPath))
            {
                return new FileInfo(videoPath);
            }
            return null;
        }


        /// <summary>
        /// 中间的Cover发生变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void flow_IndexChanged(object sender, int e)
        {
            currentIndex = e;
            ChangeFileInfo();
        }

        /// <summary>
        /// 显示文件信息
        /// </summary>
        private void ChangeFileInfo()
        {
            FileInfo info = CheckIfMediaPath(currentIndex);
            string strName = info != null ? info.Name : images[currentIndex].Name;
            fileInfo.Text = System.IO.Path.GetFileNameWithoutExtension(strName);
            if (flow.Index != Convert.ToInt32(slider.Value))
                slider.Value = flow.Index;
            if (info == null)
                info = new FileInfo(images[currentIndex].Name);
            string picName;
            switch (info.Extension)
            {
                case ".png":
                case ".jpg":
                    picName = "photo.png";
                    break;
                case ".mp3":
                case ".wav":
                    picName = "music.png";
                    break;
                case ".mp4":
                case ".flv":
                    picName = "video.png";
                    break;
                default:
                    picName = null;
                    break;
            }
            if (picName != null)
                ImgMedia.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + @"\" + picName));

        }


        /// <summary>
        /// Event implementing INotifyPropertyChanged interface.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsDisconnected
        {
            get
            {
                return this.isDisconnectedField;
            }
            private set
            {
                if (this.isDisconnectedField != value)
                {
                    this.isDisconnectedField = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("IsDisconnected"));
                    }
                }
            }
        }

        /// <summary>
        /// Gets any message associated with a failure to connect.
        /// </summary>
        public string DisconnectedReason
        {
            get
            {
                return this.disconnectedReasonField;
            }

            private set
            {
                if (this.disconnectedReasonField != value)
                {
                    this.disconnectedReasonField = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("DisconnectedReason"));
                    }
                }
            }
        }



        /// <summary>
        /// Create a wired-up recognizer for running the slideshow.
        /// </summary>
        /// <returns>The wired-up recognizer.</returns>
        private Recognizer CreateRecognizer()
        {
            // Instantiate a recognizer.
            var recognizer = new Recognizer();

            // Wire-up swipe right to manually advance picture.
            recognizer.SwipeRightDetected += (s, e) =>
            {
                if (e.Skeleton.TrackingId == nearestId)
                {

                    HighlightSkeleton(e.Skeleton);
                    if (musicWindow != null)
                    {
                        musicWindow.Backword();
                    }
                    else if (videoWindow != null)
                    {
                        videoWindow.Backword();
                    }
                    else if (detialWindow != null)
                    {
                        detialWindow.Backword();
                    }
                    else
                    {
                        flow.GoToNext();
                    }

                }
            };

            // Wire-up swipe left to manually reverse picture.
            recognizer.SwipeLeftDetected += (s, e) =>
            {
                if (e.Skeleton.TrackingId == nearestId)
                {

                    HighlightSkeleton(e.Skeleton);
                    if (musicWindow != null)
                    {
                        musicWindow.Forword();
                    }
                    else if (videoWindow != null)
                    {
                        videoWindow.Forword();
                    }
                    else if (detialWindow != null)
                    {
                        detialWindow.Forword();
                    }
                    else
                    {
                        flow.GoToPrevious();
                    }

                }
            };
            return recognizer;
        }

        /// <summary>
        /// Handle insertion of Kinect sensor.
        /// </summary>
        private void InitializeNui()
        {
            this.UninitializeNui();

            var index = 0;
            while (this.nui == null && index < KinectSensor.KinectSensors.Count)
            {
                try
                {
                    this.nui = KinectSensor.KinectSensors[index];

                    // initialize the gesture recognizer
                    gestureController = new GestureController();
                    gestureController.GestureRecognized += OnGestureRecognized;

                    // register the gestures for this demo
                    RegisterGestures();

                    this.nui.Start();
                    mapper = new CoordinateMapper(nui);

                    this.IsDisconnected = false;
                    this.DisconnectedReason = null;
                }
                catch (IOException ex)
                {
                    this.nui = null;

                    this.DisconnectedReason = ex.Message;
                }
                catch (InvalidOperationException ex)
                {
                    this.nui = null;

                    this.DisconnectedReason = ex.Message;
                }

                index++;
            }

            if (this.nui != null)
            {
                this.nui.SkeletonStream.Enable();

                this.nui.SkeletonFrameReady += this.OnSkeletonFrameReady;
            }
        }

        /// <summary>
        /// Handle removal of Kinect sensor.
        /// </summary>
        private void UninitializeNui()
        {
            if (this.nui != null)
            {
                this.nui.SkeletonFrameReady -= this.OnSkeletonFrameReady;

                this.nui.Stop();

                this.nui = null;
            }
            this.IsDisconnected = true;
            this.DisconnectedReason = null;
        }


        /// <summary>
        /// 初始化播放器
        /// </summary>
        private void InitBassEngine()
        {
            //歌曲播放完毕
            //SpectrumAnalyzer.RegisterSoundPlayer(BassEngine.Instance);
            BassEngine.ExplicitInitialize(null);
            BassEngine.Instance.TrackEnded += delegate
            {
                if (musicWindow != null)
                {
                    musicWindow.CloseThis();
                    musicWindow = null;
                }
            };
            //音乐加载成功
            BassEngine.Instance.OpenSucceeded += delegate
            {
                Debug.WriteLine(" 音乐加载成功");
                BassEngine.Instance.Volume = 1;
                BassEngine.Instance.Play();

            };
            //打开音乐失败
            BassEngine.Instance.OpenFailed += delegate
            {
                if (musicWindow != null)
                {
                    musicWindow.CloseThis();
                    musicWindow = null;
                }
            };

            //绑定音量设置
        }


        /// <summary>
        /// Handler for skeleton ready handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            // Get the frame.
            using (var frame = e.OpenSkeletonFrame())
            {
                // Ensure we have a frame.
                if (frame != null)
                {
                    // Resize the skeletons array if a new size (normally only on first call).
                    if (this.skeletons.Length != frame.SkeletonArrayLength)
                    {
                        this.skeletons = new Skeleton[frame.SkeletonArrayLength];
                    }

                    // Get the skeletons.
                    frame.CopySkeletonDataTo(this.skeletons);

                    // Assume no nearest skeleton and that the nearest skeleton is a long way away.
                    var newNearestId = -1;
                    var nearestDistance2 = double.MaxValue;

                    // Look through the skeletons.
                    foreach (var skeleton in this.skeletons)
                    {
                        // Only consider tracked skeletons.
                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            // Find the distance squared.
                            var distance2 = (skeleton.Position.X * skeleton.Position.X) +
                                (skeleton.Position.Y * skeleton.Position.Y) +
                                (skeleton.Position.Z * skeleton.Position.Z);

                            // Is the new distance squared closer than the nearest so far?
                            if (distance2 < nearestDistance2)
                            {
                                // Use the new values.
                                newNearestId = skeleton.TrackingId;
                                nearestDistance2 = distance2;
                            }
                            gestureController.UpdateAllGestures(skeleton);
                            if (detialWindow != null)
                            {
                                if (CheckIfShowHand(skeleton))
                                {
                                    detialWindow.SetHandLeftPoint(CalcScreenPoint(skeleton, JointType.HandLeft));
                                    detialWindow.SetHandRightPoint(CalcScreenPoint(skeleton, JointType.HandRight));
                                }
                            }
                        }
                    }

                    if (this.nearestId != newNearestId)
                    {
                        this.nearestId = newNearestId;
                    }
                    // Pass skeletons to recognizer.
                    this.activeRecognizer.Recognize(sender, frame, this.skeletons);
                    this.DrawStickMen(this.skeletons);
                }
            }
        }

        /// <summary>
        /// Select a skeleton to be highlighted.
        /// </summary>
        /// <param name="skeleton">The skeleton</param>
        private void HighlightSkeleton(Skeleton skeleton)
        {
            // Set the highlight time to be a short time from now.
            this.highlightTime = DateTime.UtcNow + TimeSpan.FromSeconds(0.5);
            // Record the ID of the skeleton.
            this.highlightId = skeleton.TrackingId;
        }

        /// <summary>
        /// Draw stick men for all the tracked skeletons.
        /// </summary>
        /// <param name="skeletons">The skeletons to draw.</param>
        private void DrawStickMen(Skeleton[] skeletons)
        {
            // Remove any previous skeletons.
            StickMen.Children.Clear();
            foreach (var skeleton in skeletons)
            {
                // Only draw tracked skeletons.
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    // Draw a background for the next pass.
                    this.DrawStickMan(skeleton, Brushes.WhiteSmoke, 7);
                }
            }

            foreach (var skeleton in skeletons)
            {
                // Only draw tracked skeletons.
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    // Pick a brush, Red for a skeleton that has recently gestures, black for the nearest, gray otherwise.
                    var brush = DateTime.UtcNow < this.highlightTime && skeleton.TrackingId == this.highlightId ? Brushes.Red :
                        skeleton.TrackingId == this.nearestId ? Brushes.Black : Brushes.Gray;

                    // Draw the individual skeleton.
                    this.DrawStickMan(skeleton, brush, 3);
                }
            }
        }

        /// <summary>
        /// Draw an individual skeleton.
        /// </summary>
        /// <param name="skeleton">The skeleton to draw.</param>
        /// <param name="brush">The brush to use.</param>
        /// <param name="thickness">This thickness of the stroke.</param>
        private void DrawStickMan(Skeleton skeleton, Brush brush, int thickness)
        {
            Debug.Assert(skeleton.TrackingState == SkeletonTrackingState.Tracked, "The skeleton is being tracked.");

            foreach (var run in SkeletonSegmentRuns)
            {
                var next = this.GetJointPoint(skeleton, run[0]);
                for (var i = 1; i < run.Length; i++)
                {
                    var prev = next;
                    next = this.GetJointPoint(skeleton, run[i]);

                    var line = new Line
                    {
                        Stroke = brush,
                        StrokeThickness = thickness,
                        X1 = prev.X,
                        Y1 = prev.Y,
                        X2 = next.X,
                        Y2 = next.Y,
                        StrokeEndLineCap = PenLineCap.Round,
                        StrokeStartLineCap = PenLineCap.Round
                    };

                    StickMen.Children.Add(line);
                }
            }
        }

        /// <summary>
        /// Convert skeleton joint to a point on the StickMen canvas.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <param name="jointType">The joint to project.</param>
        /// <returns>The projected point.</returns>
        private Point GetJointPoint(Skeleton skeleton, JointType jointType)
        {
            var joint = skeleton.Joints[jointType];

            // Points are centered on the StickMen canvas and scaled according to its height allowing
            // approximately +/- 1.5m from center line.
            var point = new Point
            {
                X = (StickMen.Width / 2) + (StickMen.Height * joint.Position.X / 3),
                Y = (StickMen.Width / 2) - (StickMen.Height * joint.Position.Y / 3)
            };

            return point;
        }



        private void RegisterGestures()
        {
            // define the gestures for the demo
            IRelativeGestureSegment[] joinedhandsSegments = new IRelativeGestureSegment[20];
            JoinedHandsSegment1 joinedhandsSegment = new JoinedHandsSegment1();
            for (int i = 0; i < 20; i++)
            {
                // gesture consists of the same thing 10 times 
                joinedhandsSegments[i] = joinedhandsSegment;
            }
            gestureController.AddGesture("JoinedHands", joinedhandsSegments);

            IRelativeGestureSegment[] menuSegments = new IRelativeGestureSegment[15];
            MenuSegment1 menuSegment = new MenuSegment1();
            for (int i = 0; i < 15; i++)
            {
                // gesture consists of the same thing 20 times 
                menuSegments[i] = menuSegment;
            }
            gestureController.AddGesture("Menu", menuSegments);

            IRelativeGestureSegment[] zoomInSegments = new IRelativeGestureSegment[3];
            zoomInSegments[0] = new ZoomSegment1();
            zoomInSegments[1] = new ZoomSegment2();
            zoomInSegments[2] = new ZoomSegment3();
            gestureController.AddGesture("ZoomIn", zoomInSegments);

            IRelativeGestureSegment[] zoomOutSegments = new IRelativeGestureSegment[3];
            zoomOutSegments[0] = new ZoomSegment3();
            zoomOutSegments[1] = new ZoomSegment2();
            zoomOutSegments[2] = new ZoomSegment1();
            gestureController.AddGesture("ZoomOut", zoomOutSegments);

            IRelativeGestureSegment[] pullSegment = new IRelativeGestureSegment[2];
            pullSegment[0] = new PullAndPush3();
            pullSegment[1] = new PullAndPush4();
            gestureController.AddGesture("Pull", pullSegment);

            IRelativeGestureSegment[] pushSegment = new IRelativeGestureSegment[2];
            pushSegment[0] = new PullAndPush5();
            pushSegment[1] = new PullAndPush3();
            gestureController.AddGesture("Push", pushSegment);


            IRelativeGestureSegment[] pullLeftSegment = new IRelativeGestureSegment[2];
            pullLeftSegment[0] = new PullAndPush6();
            pullLeftSegment[1] = new PullAndPush7();
            gestureController.AddGesture("PullLeft", pullLeftSegment);

            IRelativeGestureSegment[] pushLeftSegment = new IRelativeGestureSegment[2];
            pushLeftSegment[0] = new PullAndPush8();
            pushLeftSegment[1] = new PullAndPush6();
            gestureController.AddGesture("PushLeft", pushLeftSegment);

        }



        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Gesture event arguments.</param>
        private void OnGestureRecognized(object sender, GestureEventArgs e)
        {
            switch (e.GestureName)
            {
                case "Menu":
                    if (detialWindow == null && musicWindow == null && videoWindow == null)
                    {
                        HighLightStickMan();
                        this.CloseThis();
                    }
                    break;
                case "JoinedHands":
                    if (detialWindow == null)
                    {
                        if (musicWindow != null)
                        {
                            HighLightStickMan();
                            musicWindow.ChangeStatue();
                        }
                        else
                        {
                            HighLightStickMan();
                            videoWindow.ChangeStatue();
                        }
                    }
                    break;
                case "ZoomIn":
                    //Gesture = "Zoom In";
                    if (detialWindow == null && musicWindow == null && videoWindow == null)
                    {
                        HighLightStickMan();
                        this.Close();
                    }
                    break;
                case "ZoomOut":
                    //Gesture = "Zoom Out";              
                    break;
                case "Pull":
                case "PullLeft":
                    if (detialWindow == null && musicWindow == null && videoWindow == null)
                    {
                        HighLightStickMan();
                        OpenSubWindow();
                    }
                    break;
                case "Push":
                case "PushLeft":
                    if (detialWindow != null || musicWindow != null || videoWindow != null)
                    {
                        HighLightStickMan();
                        if (detialWindow != null)
                        {
                            detialWindow.CloseThis();
                            detialWindow = null;
                        }
                        else if (musicWindow != null)
                        {
                            musicWindow.CloseThis();
                            musicWindow = null;
                        }
                        else
                        {
                            videoWindow.CloseThis();
                            videoWindow = null;
                        }
                    }
                    break;
                default:
                    break;
            }

            //_clearTimer.Start();
        }

        private void HighLightStickMan()
        {
            for (int i = 0; i < skeletons.Length; i++)
            {
                if (skeletons[i].TrackingId == nearestId)
                    HighlightSkeleton(skeletons[i]);
            }
        }


        /// <summary>
        /// 根据显示分辨率与彩色图像帧的分辨率的比例，来调整显示坐标
        /// </summary>
        /// <param name="joint"></param>
        /// <returns></returns>
        private Point TransferSkeletonPoint(Joint joint)
        {

            ColorImagePoint colorPoint = mapper.MapSkeletonPointToColorPoint(joint.Position, nui.ColorStream.Format);
            double xScaleRate = (SystemParameters.PrimaryScreenWidth) / 640;
            double yScaleRate = (SystemParameters.PrimaryScreenHeight) / 480;

            double x = (double)colorPoint.X;
            x *= xScaleRate;
            double y = (double)colorPoint.Y;
            y *= yScaleRate;

            return new Point((int)x, (int)y);
        }

        private Point CalcScreenPoint(Skeleton skeleton, JointType type)
        {
            double adj = type == JointType.HandLeft ? 0.28 : 0.12;
            Joint jointHand = skeleton.Joints[type];
            Joint jointShoulderCenter = skeleton.Joints[JointType.ShoulderCenter];
            float x = jointHand.Position.X - jointShoulderCenter.Position.X;//hX - sX;
            float y = jointShoulderCenter.Position.Y - jointHand.Position.Y;//sY - hY;
            return new Point((int)((x + adj) / 0.35 * SystemParameters.PrimaryScreenWidth - SystemParameters.PrimaryScreenWidth / 2),
                                 (int)(y / 0.35 * SystemParameters.PrimaryScreenHeight) - SystemParameters.PrimaryScreenHeight / 2);
        }



        bool CheckIfShowHand(Skeleton skeleton)
        {
            if (skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.HipLeft].Position.Y &&
                skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HipRight].Position.Y &&
                skeleton.Joints[JointType.HandLeft].Position.Z < skeleton.Joints[JointType.ShoulderLeft].Position.Z &&
                skeleton.Joints[JointType.HandRight].Position.Z < skeleton.Joints[JointType.ShoulderRight].Position.Z

                )
            {
                HighLightStickMan();
                return true;
            }
            detialWindow.timerTrans.Stop();
            return false;

        }



        private void CloseThis()
        {
            stdEnd = (Storyboard)this.Resources["sb_end"];
            stdEnd.Completed += (a, b) =>
                {
                    this.Close();
                };
            stdEnd.Begin();
            if (leapMotinn != null)
            {
                leapMotinn.Close();

            }            //var datWth = new DoubleAnimation(SystemParameters.PrimaryScreenWidth, 1, new Duration(TimeSpan.FromMilliseconds(700)));
            //var datHig = new DoubleAnimation(SystemParameters.FullPrimaryScreenHeight, 1, new Duration(TimeSpan.FromMilliseconds(700)));

            //this.BeginAnimation(MainWindow.WidthProperty, datWth);
            //this.BeginAnimation(MainWindow.HeightProperty, datHig);
        }

    }
}

