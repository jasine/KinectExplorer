using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Fizbin.Kinect.Gestures;
using Fizbin.Kinect.Gestures.Segments;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.SwipeGestureRecognizer;
using KinectHelper;

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
                    this.Close();
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

        private List<FileInfo> images;
        private int currentIndex = 0;

        public void LoadImages()
        {
            //flow.Add();
            //var imageDir = new DirectoryInfo(imagePath);

            //images = new List<FileInfo>(imageDir.GetFiles("*.jpg"));
            //var images2 = new List<FileInfo>(imageDir.GetFiles("*.png"));
            //images.AddRange(images2);
            //images.Sort(new FileInfoComparer());
            //foreach (FileInfo f in images)
            //    flow.Add(Environment.MachineName, f.FullName);

            images = new List<FileInfo>();

            var commonPicturesDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures));
            images.AddRange(commonPicturesDir.GetFiles("*.jpg", SearchOption.AllDirectories));
            images.AddRange(commonPicturesDir.GetFiles("*.png", SearchOption.AllDirectories));


            var myPicturesDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            images.AddRange(myPicturesDir.GetFiles("*.jpg", SearchOption.AllDirectories));
            images.AddRange(myPicturesDir.GetFiles("*.png", SearchOption.AllDirectories));

            images.Sort(new FileInfoComparer());
            foreach (FileInfo f in images)
                flow.Add(Environment.MachineName, f.FullName);
        }

        #endregion


        #region Kinect 相关变量


        private DetialWindow detial;


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


        #endregion



        public MainWindow()
        {
            InitializeComponent();
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            this.Top = 0;
            this.Left = 0;
            //this.Topmost = true;
            flow.Width = this.Width;
            flow.Height = this.Height;
            flow.IndexChanged += flow_IndexChanged;
            flow.CenterCoverClicked += flow_CenterCoverClicked;
            // flow.GoToNextPage();
            flow.Cache = new ThumbnailManager();
            LoadImages();
            //Load(@"C:\Users\Administrator\Desktop\CoverFlowDemo\CoverFlowDemo\bin\Debug\Pictures");
            slider.Minimum = 0;
            slider.Maximum = flow.Count - 1;



            // Create the gesture recognizer.
            this.activeRecognizer = this.CreateRecognizer();
            //this.motionHelper = this.CreateMotion();
            // Wire-up window loaded event.
            Loaded += this.Window_Loaded;
            //var opacityGrid = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0)));
            //var widthx = new DoubleAnimation(10, SystemParameters.PrimaryScreenWidth / 2, new Duration(TimeSpan.FromMilliseconds(500)));
            //var heightx = new DoubleAnimation(10, SystemParameters.PrimaryScreenHeight / 2, new Duration(TimeSpan.FromMilliseconds(500)));

            ////this.BeginAnimation(MainWindow.OpacityProperty, opacityGrid);
            //this.BeginAnimation(MainWindow.WidthProperty, widthx);
            //this.BeginAnimation(MainWindow.HeightProperty, heightx);

        }

        private MotionHelper CreateMotion()
        {
            throw new NotImplementedException();
        }




        /// <summary>
        /// 中间的Cover被点击
        /// </summary>
        /// <param name="sender"></param>
        void flow_CenterCoverClicked(object sender)
        {
            detial = new DetialWindow(images[currentIndex]);
            detial.Show();
        }

        /// <summary>
        /// 中间的Cover发生变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void flow_IndexChanged(object sender, int e)
        {
            currentIndex = e;
            if (flow.Index != Convert.ToInt32(slider.Value))
                slider.Value = flow.Index;
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
                    if (detial == null)
                    {
                        flow.GoToNext();
                        HighlightSkeleton(e.Skeleton);
                    }                   
                }
            };

            // Wire-up swipe left to manually reverse picture.
            recognizer.SwipeLeftDetected += (s, e) =>
            {
                if (e.Skeleton.TrackingId == nearestId)
                {
                    if (detial == null)
                    {
                        flow.GoToPrevious();
                        HighlightSkeleton(e.Skeleton);
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
        Storyboard stdStart;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            stdStart = (Storyboard)this.Resources["sb_start"];
            stdStart.Begin();
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
                            Debug.WriteLine("New Kinect connected");

                            InitializeNui();
                        }
                        else
                        {
                            Debug.WriteLine("Existing Kinect signalled connection");
                        }

                        break;
                    default:
                        if (ee.Sensor == nui)
                        {
                            Debug.WriteLine("Existing Kinect disconnected");

                            UninitializeNui();
                        }
                        else
                        {
                            Debug.WriteLine("Other Kinect event occurred");
                        }

                        break;
                }
            };
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
                            if (detial != null)
                            {
                                //if (
                                //    false)//Math.Abs(skeleton.Joints[JointType.HandLeft].Position.X-skeleton.Joints[JointType.ShoulderLeft].Position.X) <0.2&&
                                //    //skeleton.Joints[JointType.ElbowLeft].Position.X < skeleton.Joints[JointType.ShoulderLeft].Position.X&&
                                //    //Math.Abs(skeleton.Joints[JointType.ShoulderLeft].Position.Z-skeleton.Joints[JointType.HandLeft].Position.Z)<0.15)
                                //    //skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y)
                                //    //skeleton.Joints[JointType.HandLeft].Position.X>skeleton.Joints[JointType.ShoulderLeft].Position.X)
                                //{
                                //    //detial.SetHandLeftPoint(TransferSkeletonPoint(skeleton.Joints[JointType.HandLeft]));
                                //    //detial.SetHandRightPoint(TransferSkeletonPoint(skeleton.Joints[JointType.HandRight]));
                                //    //detial.allowMove = true;
                                //}
                                //else
                                //{
                                    //detial.allowMove = false;
                                    if (CheckIfShowHand(skeleton))
                                    {
                                        detial.SetHandLeftPoint(TransferSkeletonPoint(skeleton.Joints[JointType.HandLeft]));
                                    }
                                    if (CheckIfShowHand(skeleton))
                                    {
                                        detial.SetHandRightPoint(TransferSkeletonPoint(skeleton.Joints[JointType.HandRight]));
                                    }
                                //}
                                
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






        private GestureController gestureController;

        private void RegisterGestures()
        {
            // define the gestures for the demo

            IRelativeGestureSegment[] joinedhandsSegments = new IRelativeGestureSegment[15];
            JoinedHandsSegment1 joinedhandsSegment = new JoinedHandsSegment1();
            for (int i = 0; i < 15; i++)
            {
                // gesture consists of the same thing 10 times 
                joinedhandsSegments[i] = joinedhandsSegment;
            }
            gestureController.AddGesture("JoinedHands", joinedhandsSegments);

            IRelativeGestureSegment[] menuSegments = new IRelativeGestureSegment[20];
            MenuSegment1 menuSegment = new MenuSegment1();
            for (int i = 0; i < 20; i++)
            {
                // gesture consists of the same thing 20 times 
                menuSegments[i] = menuSegment;
            }
            gestureController.AddGesture("Menu", menuSegments);

            IRelativeGestureSegment[] swipeleftSegments = new IRelativeGestureSegment[3];
            swipeleftSegments[0] = new SwipeLeftSegment1();
            swipeleftSegments[1] = new SwipeLeftSegment2();
            swipeleftSegments[2] = new SwipeLeftSegment3();
            gestureController.AddGesture("SwipeLeft", swipeleftSegments);

            IRelativeGestureSegment[] swiperightSegments = new IRelativeGestureSegment[3];
            swiperightSegments[0] = new SwipeRightSegment1();
            swiperightSegments[1] = new SwipeRightSegment2();
            swiperightSegments[2] = new SwipeRightSegment3();
            gestureController.AddGesture("SwipeRight", swiperightSegments);

            IRelativeGestureSegment[] waveRightSegments = new IRelativeGestureSegment[6];
            WaveRightSegment1 waveRightSegment1 = new WaveRightSegment1();
            WaveRightSegment2 waveRightSegment2 = new WaveRightSegment2();
            waveRightSegments[0] = waveRightSegment1;
            waveRightSegments[1] = waveRightSegment2;
            waveRightSegments[2] = waveRightSegment1;
            waveRightSegments[3] = waveRightSegment2;
            waveRightSegments[4] = waveRightSegment1;
            waveRightSegments[5] = waveRightSegment2;
            gestureController.AddGesture("WaveRight", waveRightSegments);

            IRelativeGestureSegment[] waveLeftSegments = new IRelativeGestureSegment[6];
            WaveLeftSegment1 waveLeftSegment1 = new WaveLeftSegment1();
            WaveLeftSegment2 waveLeftSegment2 = new WaveLeftSegment2();
            waveLeftSegments[0] = waveLeftSegment1;
            waveLeftSegments[1] = waveLeftSegment2;
            waveLeftSegments[2] = waveLeftSegment1;
            waveLeftSegments[3] = waveLeftSegment2;
            waveLeftSegments[4] = waveLeftSegment1;
            waveLeftSegments[5] = waveLeftSegment2;
            gestureController.AddGesture("WaveLeft", waveLeftSegments);

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

            IRelativeGestureSegment[] swipeUpSegments = new IRelativeGestureSegment[3];
            swipeUpSegments[0] = new SwipeUpSegment1();
            swipeUpSegments[1] = new SwipeUpSegment2();
            swipeUpSegments[2] = new SwipeUpSegment3();
            gestureController.AddGesture("SwipeUp", swipeUpSegments);

            IRelativeGestureSegment[] swipeDownSegments = new IRelativeGestureSegment[3];
            swipeDownSegments[0] = new SwipeDownSegment1();
            swipeDownSegments[1] = new SwipeDownSegment2();
            swipeDownSegments[2] = new SwipeDownSegment3();
            gestureController.AddGesture("SwipeDown", swipeDownSegments);


            IRelativeGestureSegment[] swipePullSegment = new IRelativeGestureSegment[2];

            swipePullSegment[0] = new PullAndPush3();
            swipePullSegment[1] = new PullAndPush4();
            gestureController.AddGesture("Pull", swipePullSegment);

            IRelativeGestureSegment[] swipePushSegment = new IRelativeGestureSegment[2];

            swipePushSegment[0] = new PullAndPush5();
            swipePushSegment[1] = new PullAndPush3();
            gestureController.AddGesture("Push", swipePushSegment);



            IRelativeGestureSegment[] swipeSurrenderSegment = new IRelativeGestureSegment[5];

            for (int i = 0; i < 5; i++)
            {
                swipeSurrenderSegment[i] = new SurrenderSegment1();
            }
            gestureController.AddGesture("Surrender", swipePullSegment);


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
                    if (detial == null)
                    {
                        this.Close();
                        HighLightStickMan();
                    }
                    break;
                case "WaveRight":
                    break;
                case "WaveLeft":
                    break;
                case "JoinedHands":
                    //if (detial == null)
                    //{
                    //    HighLightStickMan();
                    //    this.Close();
                    //}
                    //Gesture = "Joined Hands";
                    break;
                case "SwipeLeft":
                    // Gesture = "Swipe Left";
                    if (detial == null)
                    {
                        //flow.GoToNext();
                    }
                    break;
                case "SwipeRight":
                    //Gesture = "Swipe Right";
                    if (detial == null)
                    {
                        //flow.GoToPrevious();
                    }                   
                    break;
                case "SwipeUp":
                    // Gesture = "Swipe Up";
                    break;
                case "SwipeDown":
                    //Gesture = "Swipe Down";
                    break;
                case "ZoomIn":
                    //Gesture = "Zoom In";
                    if (detial == null)
                    {
                        HighLightStickMan();
                        this.Close();
                    }
                    break;
                case "ZoomOut":
                    //Gesture = "Zoom Out";              
                    break;
                case "Pull":
                    if (detial == null)
                    {
                        HighLightStickMan();
                        detial = new DetialWindow(images[currentIndex]);                      
                        detial.Show();

                    }    
                    break;
                case "Push":
                    if (detial != null)
                    {
                        HighLightStickMan();
                        detial.CloseThis();
                        detial = null;                     
                    }
                    break;
                case "Surrender":
                    //Gesture = "Surrender";
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
            double yScaleRate = (SystemParameters.PrimaryScreenHeight)/ 480;

            double x = (double)colorPoint.X;
            x *= xScaleRate;
            double y = (double)colorPoint.Y;
            y *= yScaleRate;

            return new Point((int)x,(int)y);
        }


        bool CheckIfShowHand(Skeleton skeleton,JointType jointType)
        {
            if (Math.Abs(skeleton.Joints[JointType.Head].Position.Z - skeleton.Joints[jointType].Position.Z) > 0.3&&
                
                skeleton.Joints[jointType].Position.Y >skeleton.Joints[JointType.HipCenter].Position.Y-0.1              
                )
                return true;
            return false;

        }
        bool CheckIfShowHand(Skeleton skeleton)
        {
            if (Math.Abs(skeleton.Joints[JointType.Head].Position.Z - skeleton.Joints[JointType.HandLeft].Position.Z) > 0.25 &&
                Math.Abs(skeleton.Joints[JointType.Head].Position.Z - skeleton.Joints[JointType.HandRight].Position.Z)>0.25&&
                skeleton.Joints[JointType.HandLeft].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y - 0.1&&
                skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y - 0.1
                )
                return true;
            return false;

        }
       // bool CheckIfControl(Ske)

    }
}

