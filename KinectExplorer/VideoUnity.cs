using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectExplorer
{
    class VideoUnity
    {
        /// <summary>
        /// 截取视频缩略图
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="imgFile"></param>
        /// <returns></returns>
        public static bool CatchImg(string fileName, string imgFile)
        {
            const string ffmpeg = "ffmpeg.exe";
            //string flvImg = imgFile + ".jpg";
            const string flvImgSize = "640*480";

            System.Diagnostics.ProcessStartInfo ImgstartInfo = new System.Diagnostics.ProcessStartInfo(ffmpeg);
            ImgstartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            ImgstartInfo.Arguments = "   -i   " + fileName + "  -y  -f  image2   -ss 2 -vframes 1  -s   " + flvImgSize + "   " + imgFile;
            try
            {
                System.Diagnostics.Process.Start(ImgstartInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (System.IO.File.Exists(imgFile))
            {
                return true;
            }
            return false;
        }

    }
}
