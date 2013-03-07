using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace Fizbin.Kinect.Gestures.Segments
{
    public class SurrenderSegment1:IRelativeGestureSegment
    {
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {

            //// right hand in front of right shoulder
            //if (skeleton.Joints[JointType.HandRight].Position.Z < skeleton.Joints[JointType.ElbowRight].Position.Z && skeleton.Joints[JointType.HandLeft].Position.Y < skeleton.Joints[JointType.ShoulderCenter].Position.Y)
            //{
            //    // right hand below shoulder height but above hip height
            //    if (skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.Head].Position.Y && skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y)
            //    {
            //        // right hand right of right shoulder
            //        if (skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.ShoulderRight].Position.X)
            //        {
            //            return GesturePartResult.Succeed;
            //        }
            //        return GesturePartResult.Pausing;
            //    }
            //    return GesturePartResult.Fail;
            //}
            //return GesturePartResult.Fail;


            if (skeleton.Joints[JointType.HandLeft].Position.Y - skeleton.Joints[JointType.Head].Position.Y > 0.2 &&
                skeleton.Joints[JointType.HandRight].Position.Y - skeleton.Joints[JointType.Head].Position.Y > 0.2)
            {
                return GesturePartResult.Succeed;

            }
            return GesturePartResult.Fail;
        }
    }
}
