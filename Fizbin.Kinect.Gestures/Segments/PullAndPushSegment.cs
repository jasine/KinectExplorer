using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace Fizbin.Kinect.Gestures.Segments
{
    /// <summary>
    /// The first part of the swipe right gesture
    /// </summary>
    public class PullAndPush1 : IRelativeGestureSegment
    {
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            // //left hand in front of left Shoulder
            if (
                Math.Abs(skeleton.Joints[JointType.HandLeft].Position.Z - skeleton.Joints[JointType.HandRight].Position.Z) < 0.1&&
                Math.Abs(skeleton.Joints[JointType.HandLeft].Position.Y - skeleton.Joints[JointType.ShoulderLeft].Position.Y) < 0.2 &&
                Math.Abs(skeleton.Joints[JointType.HandRight].Position.Y - skeleton.Joints[JointType.ShoulderRight].Position.Y) < 0.2
                )
            {
                    if (skeleton.Joints[JointType.Head].Position.Z - skeleton.Joints[JointType.HandRight].Position.Z > 0.35 )
                    {
                        // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - PASS");
                        return GesturePartResult.Succeed;
                    }
                    // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - UNDETERMINED");
                    return GesturePartResult.Pausing;
            }

            // Debug.WriteLine("GesturePart 0 - left hand in front of left Shoulder - FAIL");
            return GesturePartResult.Fail;
        }
    }


    public class PullAndPush2 : IRelativeGestureSegment
    {
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            if (Math.Abs(skeleton.Joints[JointType.HandLeft].Position.Z - skeleton.Joints[JointType.HandRight].Position.Z) < 0.1&&
                Math.Abs(skeleton.Joints[JointType.ShoulderLeft].Position.Y - skeleton.Joints[JointType.HandLeft].Position.Y )< 0.3 &&
                Math.Abs(skeleton.Joints[JointType.ShoulderRight].Position.Y - skeleton.Joints[JointType.HandRight].Position.Y) < 0.3
                )
            {
                if (
                skeleton.Joints[JointType.HandLeft].Position.X> skeleton.Joints[JointType.ElbowLeft].Position.X &&
                skeleton.Joints[JointType.HandRight].Position.X< skeleton.Joints[JointType.ElbowRight].Position.X)
                {
                    if (skeleton.Joints[JointType.Head].Position.Z - skeleton.Joints[JointType.HandRight].Position.Z < 0.27)
                    {
                        // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - PASS");
                        return GesturePartResult.Succeed;
                    }

                    // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - UNDETERMINED");
                    return GesturePartResult.Pausing;
                }

                // Debug.WriteLine("GesturePart 0 - left hand below shoulder height but above hip height - FAIL");
                return GesturePartResult.Pausing;
            }

            // Debug.WriteLine("GesturePart 0 - left hand in front of left Shoulder - FAIL");
            return GesturePartResult.Fail;
        }
    }


    public class PullAndPush3 : IRelativeGestureSegment
    {
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            // //left hand in front of left Shoulder
            if (Math.Abs(skeleton.Joints[JointType.HandLeft].Position.X - skeleton.Joints[JointType.ShoulderLeft].Position.X) < 0.15 &&
                Math.Abs(skeleton.Joints[JointType.HandLeft].Position.Z - skeleton.Joints[JointType.ShoulderLeft].Position.Z) < 0.15 &&
                skeleton.Joints[JointType.HandLeft].Position.Y<skeleton.Joints[JointType.HipLeft].Position.Y&&
                skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y &&
                skeleton.Joints[JointType.HandRight].Position.Z < skeleton.Joints[JointType.ElbowRight].Position.Z
                )
            {
                if (skeleton.Joints[JointType.HandRight].Position.X < skeleton.Joints[JointType.ElbowRight].Position.X &&
                    skeleton.Joints[JointType.Spine].Position.Y < skeleton.Joints[JointType.HandRight].Position.Z)
                {
                    if (skeleton.Joints[JointType.Head].Position.Z - skeleton.Joints[JointType.HandRight].Position.Z > 0.4)
                    {
                        // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - PASS");
                        return GesturePartResult.Succeed;
                    }
                    return GesturePartResult.Pausing;
                }

                // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - UNDETERMINED");
                return GesturePartResult.Fail;
            }

            // Debug.WriteLine("GesturePart 0 - left hand in front of left Shoulder - FAIL");
            return GesturePartResult.Fail;
        }
    }


    public class PullAndPush4 : IRelativeGestureSegment
    {
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            if (Math.Abs(skeleton.Joints[JointType.HandLeft].Position.X - skeleton.Joints[JointType.ShoulderLeft].Position.X) < 0.15 &&
                Math.Abs(skeleton.Joints[JointType.HandLeft].Position.Z - skeleton.Joints[JointType.ShoulderLeft].Position.Z) < 0.15&&
                skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.HipLeft].Position.Y

                )
            {
                if (
                skeleton.Joints[JointType.HandRight].Position.Z < skeleton.Joints[JointType.ElbowRight].Position.Z &&
                skeleton.Joints[JointType.HandRight].Position.X < skeleton.Joints[JointType.ElbowRight].Position.X &&
                    skeleton.Joints[JointType.Spine].Position.Y < skeleton.Joints[JointType.HandRight].Position.Z)
                {
                    if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y &&
                        skeleton.Joints[JointType.Head].Position.Z - skeleton.Joints[JointType.HandRight].Position.Z < 0.2 &&
                        skeleton.Joints[JointType.HandRight].Position.X >= skeleton.Joints[JointType.HipCenter].Position.X - 0.05)
                    {
                        // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - PASS");
                        return GesturePartResult.Succeed;
                    }
                    return GesturePartResult.Pausing;
                }

                // Debug.WriteLine("GesturePart 0 - left hand below shoulder height but above hip height - FAIL");
                return GesturePartResult.Fail;
            }

            // Debug.WriteLine("GesturePart 0 - left hand in front of left Shoulder - FAIL");
            return GesturePartResult.Fail;
        }
    }


    public class PullAndPush5 : IRelativeGestureSegment
    {
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            if (Math.Abs(skeleton.Joints[JointType.HandLeft].Position.X - skeleton.Joints[JointType.ShoulderLeft].Position.X) < 0.15 &&
                Math.Abs(skeleton.Joints[JointType.HandLeft].Position.Z - skeleton.Joints[JointType.ShoulderLeft].Position.Z) < 0.15&&
                skeleton.Joints[JointType.HandLeft].Position.Y<skeleton.Joints[JointType.HipLeft].Position.Y
                )
            {
                if (
                skeleton.Joints[JointType.HandRight].Position.Z < skeleton.Joints[JointType.ElbowRight].Position.Z - 0.1 &&
                skeleton.Joints[JointType.HandRight].Position.X < skeleton.Joints[JointType.ElbowRight].Position.X - 0.1 &&
                    skeleton.Joints[JointType.Spine].Position.Y < skeleton.Joints[JointType.HandRight].Position.Z)
                {
                    if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y &&
                        skeleton.Joints[JointType.Head].Position.Z - skeleton.Joints[JointType.HandRight].Position.Z < 0.25 &&
                        skeleton.Joints[JointType.HandRight].Position.X >= skeleton.Joints[JointType.HipCenter].Position.X - 0.05)
                    {
                        // Debug.WriteLine("GesturePart 0 - left hand left of left Shoulder - PASS");
                        return GesturePartResult.Succeed;
                    }
                    return GesturePartResult.Pausing;
                }

                // Debug.WriteLine("GesturePart 0 - left hand below shoulder height but above hip height - FAIL");
                return GesturePartResult.Fail;
            }

            // Debug.WriteLine("GesturePart 0 - left hand in front of left Shoulder - FAIL");
            return GesturePartResult.Fail;
        }
    }


}
