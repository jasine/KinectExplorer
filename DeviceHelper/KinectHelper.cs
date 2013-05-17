using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Fizbin.Kinect.Gestures;
using Fizbin.Kinect.Gestures.Segments;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.SwipeGestureRecognizer;

namespace DeviceHelper
{
    public class KinectHelper
    {
        #region Kinect 相关变量


        /// <summary>
        /// The recognizer being used.
        /// </summary>
        private Recognizer activeRecognizer;




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
        /// Array to receive Skeletons from sensor, resize when needed.
        /// </summary>
        public Skeleton[] Skeletons  {get;private set;}

    
        /// <summary>
        /// The ID if the skeleton to be tracked.
        /// </summary>
        public int NearestId { get;private set; }

        private GestureController gestureController;

        public delegate void GestureDetectedEventHandler(object sender, GestureType gestureType);

        public event GestureDetectedEventHandler GestureDetected;

        public delegate void SkeletonReadyEventHandler(object sender, Skeleton skeleton);

        public event SkeletonReadyEventHandler SkeletonReady;
     
        #endregion   


        public KinectHelper ()
        {
            Skeletons=new Skeleton[0];
            NearestId = -1;

            InitializeNui();
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
                    this.nui.Start();

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
                    // Resize the Skeletons array if a new size (normally only on first call).
                    if (this.Skeletons.Length != frame.SkeletonArrayLength)
                    {
                        this.Skeletons = new Skeleton[frame.SkeletonArrayLength];
                    }

                    // Get the Skeletons.
                    frame.CopySkeletonDataTo(this.Skeletons);

                    // Assume no nearest skeleton and that the nearest skeleton is a long way away.
                    var newNearestId = -1;
                    var nearestDistance2 = double.MaxValue;

                    // Look through the Skeletons.
                    foreach (var skeleton in this.Skeletons)
                    {
                        // Only consider tracked Skeletons.
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
                            if (gestureController != null)
                            {
                                gestureController.UpdateAllGestures(skeleton);
                            }
                            if (SkeletonReady!= null)
                            {
                                SkeletonReady(this, skeleton);
                            }  

                            //if (detialWindow != null)
                            //{
                            //    if (CheckIfShowHand(skeleton))
                            //    {
                            //        detialWindow.SetHandLeftPoint(CalcScreenPoint(skeleton, JointType.HandLeft));
                            //        detialWindow.SetHandRightPoint(CalcScreenPoint(skeleton, JointType.HandRight));
                            //    }
                            //}
                        }
                    }

                    if (this.NearestId != newNearestId)
                    {
                        this.NearestId = newNearestId;
                    }
                    if (activeRecognizer != null)
                    {
                        this.activeRecognizer.Recognize(sender, frame, this.Skeletons);
                    }                               
                }
            }
        }

        public void RegisterGestures(List<GestureType> gestureList)
        {
            if (gestureList.Contains(GestureType.JoinedHands) || gestureList.Contains(GestureType.LeftHandHalfLift) ||
                gestureList.Contains(GestureType.Pull) || gestureList.Contains(GestureType.Push) ||
                gestureList.Contains(GestureType.ZoomIn) || gestureList.Contains(GestureType.ZoomOut))
            {
                gestureController = new GestureController();
                gestureController.GestureRecognized += OnGestureRecognized;
                if (gestureList.Contains(GestureType.JoinedHands))
                {
                    IRelativeGestureSegment[] joinedhandsSegments = new IRelativeGestureSegment[15];
                    JoinedHandsSegment1 joinedhandsSegment = new JoinedHandsSegment1();
                    for (int i = 0; i < 15; i++)
                    {
                        // gestureType consists of the same thing 10 times 
                        joinedhandsSegments[i] = joinedhandsSegment;
                    }
                    gestureController.AddGesture("JoinedHands", joinedhandsSegments);
                }
                if (gestureList.Contains(GestureType.LeftHandHalfLift))
                {
                    IRelativeGestureSegment[] menuSegments = new IRelativeGestureSegment[15];
                    MenuSegment1 menuSegment = new MenuSegment1();
                    for (int i = 0; i < 15; i++)
                    {
                        // gestureType consists of the same thing 20 times 
                        menuSegments[i] = menuSegment;
                    }
                    gestureController.AddGesture("Menu", menuSegments);
                }
                if (gestureList.Contains(GestureType.Pull))
                {
                    IRelativeGestureSegment[] pullLeftSegment = new IRelativeGestureSegment[2];
                    pullLeftSegment[0] = new PullAndPush6();
                    pullLeftSegment[1] = new PullAndPush7();
                    gestureController.AddGesture("PullLeft", pullLeftSegment);

                    IRelativeGestureSegment[] pullSegment = new IRelativeGestureSegment[2];
                    pullSegment[0] = new PullAndPush3();
                    pullSegment[1] = new PullAndPush4();
                    gestureController.AddGesture("Pull", pullSegment);
                }
                if (gestureList.Contains(GestureType.Push))
                {
                    IRelativeGestureSegment[] pushLeftSegment = new IRelativeGestureSegment[2];
                    pushLeftSegment[0] = new PullAndPush8();
                    pushLeftSegment[1] = new PullAndPush6();
                    gestureController.AddGesture("PushLeft", pushLeftSegment);

                    IRelativeGestureSegment[] pushSegment = new IRelativeGestureSegment[2];
                    pushSegment[0] = new PullAndPush5();
                    pushSegment[1] = new PullAndPush3();
                    gestureController.AddGesture("Push", pushSegment);
                }
                if (gestureList.Contains(GestureType.ZoomIn))
                {
                    IRelativeGestureSegment[] zoomInSegments = new IRelativeGestureSegment[3];
                    zoomInSegments[0] = new ZoomSegment1();
                    zoomInSegments[1] = new ZoomSegment2();
                    zoomInSegments[2] = new ZoomSegment3();
                    gestureController.AddGesture("ZoomIn", zoomInSegments);
                }
                if (gestureList.Contains(GestureType.ZoomOut))
                {
                    IRelativeGestureSegment[] zoomOutSegments = new IRelativeGestureSegment[3];
                    zoomOutSegments[0] = new ZoomSegment3();
                    zoomOutSegments[1] = new ZoomSegment2();
                    zoomOutSegments[2] = new ZoomSegment1();
                    gestureController.AddGesture("ZoomOut", zoomOutSegments);
                }
            }
           
            if (gestureList.Contains(GestureType.SwipLeft)||gestureList.Contains(GestureType.SwipRight))
            {
               activeRecognizer  = new Recognizer();
                if (gestureList.Contains(GestureType.SwipLeft))
                {
                    activeRecognizer.SwipeLeftDetected += (s, e) =>
                    {
                        if (e.Skeleton.TrackingId == NearestId)
                        {
                            GestureDetected(this,GestureType.SwipLeft);
                        }
                    };
                }
                if (gestureList.Contains(GestureType.SwipRight))
                {
                    activeRecognizer.SwipeRightDetected += (s, e) =>
                    {
                        if (e.Skeleton.TrackingId == NearestId)
                        {
                            GestureDetected(this,GestureType.SwipRight);
                        }
                    };
                }                           
            }
        }

        /// <summary>
        /// Event implementing INotifyPropertyChanged interface.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsDisconnected
        {
            get { return this.isDisconnectedField; }
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
            get { return this.disconnectedReasonField; }

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

        private void OnGestureRecognized(object sender, GestureEventArgs e)
        {
            if (GestureDetected != null)
            {
                switch (e.GestureName)
                {
                    case "Menu":
                        GestureDetected(this, GestureType.LeftHandHalfLift);
                        break;
                    case "JoinedHands":
                        GestureDetected(this, GestureType.JoinedHands);
                        break;
                    case "ZoomIn":
                        GestureDetected(this, GestureType.ZoomIn);
                        break;
                    case "ZoomOut":
                        GestureDetected(this, GestureType.ZoomOut);
                        break;
                    case "Pull":
                    case "PullLeft":
                        GestureDetected(this, GestureType.Pull);
                        break;
                    case "Push":
                    case "PushLeft":
                        GestureDetected(this, GestureType.Push);
                        break;
                }
            }
            
        }
       
        public void Close()
        {
            UninitializeNui();
            if (nui != null)
            {
                nui.Dispose();
            }
        }
    }
}
