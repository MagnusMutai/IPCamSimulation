using System;
using Emgu.CV;
using System.Diagnostics;

namespace WebcamToRTSP
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize the capture device (webcam)
            VideoCapture capture = new VideoCapture(0); // 0 means default webcam

            if (!capture.IsOpened)
            {
                Console.WriteLine("Webcam not detected!");
                return;
            }

            // Define video properties (size and framerate)
            capture.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, 640);
            capture.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, 480);
            capture.Set(Emgu.CV.CvEnum.CapProp.Fps, 30);

            // FFmpeg command to stream video over RTSP
            string ffmpegArgs = "-f rawvideo -pix_fmt bgr24 -s 640x480 -r 30 -i - -c:v libx264 -preset ultrafast -f rtsp rtsp://localhost:8554/webcam";

            // Set up the FFmpeg process
            Process ffmpeg = new Process();
            ffmpeg.StartInfo.FileName = "ffmpeg";
            ffmpeg.StartInfo.Arguments = ffmpegArgs;
            ffmpeg.StartInfo.RedirectStandardInput = true; // Capture from stdin
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.CreateNoWindow = true;
            ffmpeg.Start();

            // Frame buffer for webcam capture
            // Holds the image frame from the webcam
            Mat frame = new Mat();

            Console.WriteLine("Streaming started... Press Ctrl+C to stop.");

            while (true)
            {
                // Capture frame from webcam
                capture.Read(frame);

                // If the frame is not empty, send it to FFmpeg stdin
                if (!frame.IsEmpty)
                {
                    byte[] frameData = frame.ToImage<Emgu.CV.Structure.Bgr, byte>().Bytes;
                    ffmpeg.StandardInput.BaseStream.Write(frameData, 0, frameData.Length);
                }
            }
        }
    }
}