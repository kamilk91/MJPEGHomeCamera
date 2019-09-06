using System;
using AForge.Video;
using System.Drawing;
using System.Threading;
using Accord.Video.FFMPEG;

namespace Whatever
{

class WebCamOperator
    {
        public WebCamOperator(string streamURL, string Path)
        {
            this.streamUrl = streamURL;
            this.stream = new MJPEGStream();
            this.subPath = Path;
            StartEvents();
        }
        private string streamUrl;
        private MJPEGStream stream;
        private bool enableRecording = true;
        private Bitmap lastFrame;
        private bool amIStartedRecordingYet = false;
        VideoFileWriter writer;
        private int frameRate;
        string subPath;


        Timer videoTimer;


        public void StartStream()
        {
            stream.Source = streamUrl;
            stream.Start();
            Thread.Sleep(1000);
            this.frameRate = FrameRateMaths();

            if (!System.IO.Directory.Exists(subPath))
                System.IO.Directory.CreateDirectory(subPath);
        }

        private void StartEvents()
        {
            stream.NewFrame += Stream_NewFrame;
            stream.PlayingFinished += Stream_PlayingFinished;
            stream.VideoSourceError += Stream_VideoSourceError;

        }

        private void Stream_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {

            lastFrame = (Bitmap)eventArgs.Frame.Clone();
            if (enableRecording && writer != null && writer.IsOpen)
                ReceiveFrameToVideo(lastFrame);

        }


        private void Stream_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
            Console.WriteLine("Hey, do something i got error, omg what now.");
        }

        private void Stream_PlayingFinished(object sender, ReasonToFinishPlaying reason)
        {
            Console.WriteLine("Thank you! Stream finished.");
        }
        /*
         * 
         * 
        * Image and Video management
        * 
        * 
        */
        private int FrameRateMaths()
        {

            int start = stream.FramesReceived;
            Thread.Sleep(1000);
            int stop = stream.FramesReceived;
            return stop - start;

        }

        private void StartRecording()
        {
            if (!amIStartedRecordingYet)
                amIStartedRecordingYet = true;
            writer = new VideoFileWriter();
            if (writer.IsOpen)
                while (writer.IsOpen) { }
            writer.Open(subPath + DateTime.Now.ToString("MMddyyyy HHmmss") + ".avi", lastFrame.Size.Width, lastFrame.Size.Height, 25, VideoCodec.Default, 5000000);

        }

        private void ReceiveFrameToVideo(Bitmap frame)
        {
            try
            {
                writer.WriteVideoFrame(frame);

            }
            catch (Exception e)
            {
                Console.WriteLine("To fast? I have no access to file. " + e.Message);
            }
        }

        private void FinishRecording()
        {
            writer.Close();
        }

        public void RunRecordingMachine()
        {
            if (enableRecording)
            {
                int checkedTimes = 0;
                int timeSleep = 1000;
                while (!stream.IsRunning)
                {
                    Console.WriteLine("Waiting for streaming..");
                    if (checkedTimes > 10)
                        timeSleep = 10000;
                    Thread.Sleep(timeSleep);
                }

                this.videoTimer = new Timer((x) =>
                   {
                       RecordNow();
                   }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1.0));
            }

        }

        private void RecordNow()
        {
            if (!amIStartedRecordingYet)
            {
                StartRecording();
            }
            else
            {
                FinishRecording();
                while (writer.IsOpen)
                {
                    Console.WriteLine("Waiting to writer close himself");
                }
                StartRecording();
            }

        }



    }
}
