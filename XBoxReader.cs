using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace KinectTest
{
    public class XBoxReader : IDisposable
    {
        private readonly KinectSensor sensor;
        private INotifyPropertyChanged reader;
        private FrameDescription description;
        private ushort[] data;
        private byte[] bytes;
        private byte[] convertedData;
        private WriteableBitmap bitmap;
        private readonly Body[] bodies = new Body[6];
        private Canvas canvas;
        private Size defaultPointSize = new Size(10, 10);
        private bool showBody = false;

        public bool ShowBody
        {
            get
            {
                return showBody;
            }
            set
            {
                canvas?.Children.Clear();
                showBody = value;
            }
        }



        private Dictionary<JointType, Size> pointSizes = new Dictionary<JointType, Size>
        {
            {JointType.Head, new Size(30,30)}
        };

        public FrameDescription Description => description;
        public KinectSensor Sensor => sensor;

        public WriteableBitmap Bitmap => bitmap;

        public static class Factory
        {
            public static XBoxReader CreateInfraReader(Image image)
            {
                var instance = new XBoxReader();
                instance.OpenInfra(image);
                return instance;
            }

            public static XBoxReader CreateInfraReader(Image image, Canvas canvas)
            {
                var instance = CreateInfraReader(image);
                instance.OpenInfra(image);
                instance.OpenBody(canvas);
                return instance;
            }

            public static XBoxReader CreateColorReader(Image image)
            {
                var instance = new XBoxReader();
                instance.OpenColored(image);
                return instance;
            }
            public static XBoxReader CreateColorReader(Image image, Canvas canvas)
            {
                var instance = new XBoxReader();
                instance.OpenColored(image);
                instance.OpenBody(canvas);
                return instance;
            }

            public static XBoxReader CreateDepthReader(Image image)
            {
                var instance = new XBoxReader();
                instance.OpenDepth(image);
                return instance;
            }

            public static XBoxReader CreateDepthReader(Image image, Canvas canvas)
            {
                var instance = new XBoxReader();
                instance.OpenDepth(image);
                instance.OpenBody(canvas);
                return instance;
            }

            public static XBoxReader CreateBodyReader(Canvas canvas)
            {
                var instance = new XBoxReader();
                instance.OpenBody(canvas);
                instance.showBody = true;
                return instance;
            }

            public static XBoxReader CreateBodyIndexReader(Image image)
            {
                var instance = new XBoxReader();
                instance.OpenBodyIndex(image);
                return instance;
            }

            public static XBoxReader CreateBodyIndexReader(Image image, Canvas canvas)
            {
                var instance = new XBoxReader();
                instance.OpenBodyIndex(image);
                instance.OpenBody(canvas);
                return instance;
            }

        }

        public XBoxReader()
        {
            sensor = KinectSensor.GetDefault();
        }
        public void Dispose()
        {
            sensor?.Close();
        }

        private void OpenInfra(Image image)
        {
            reader = sensor.InfraredFrameSource.OpenReader();
            description = sensor.InfraredFrameSource.FrameDescription;
            Set();
            image.Source = bitmap;
            ((InfraredFrameReader)reader).FrameArrived += XBoxReader_InfraFrameArrived;
        }

        private void OpenColored(Image image)
        {
            reader = sensor.ColorFrameSource.OpenReader();
            description = sensor.ColorFrameSource.FrameDescription;
            Set();
            image.Source = bitmap;
            ((ColorFrameReader)reader).FrameArrived += XBoxReader_ColorFrameArrived;
        }

        private void OpenBodyIndex(Image image)
        {
            reader = sensor.BodyIndexFrameSource.OpenReader();
            description = sensor.BodyIndexFrameSource.FrameDescription;
            Set();
            image.Source = bitmap;
            ((BodyIndexFrameReader)reader).FrameArrived += XBoxReader_BodyIndexFrameArrived;
        }

        private void OpenDepth(Image image)
        {
            reader = sensor.DepthFrameSource.OpenReader();
            description = sensor.DepthFrameSource.FrameDescription;
            Set();
            image.Source = bitmap;
            ((DepthFrameReader)reader).FrameArrived += XBoxReader_DepthFrameArrived;
        }

        private void OpenBody(Canvas canvas)
        {
            this.canvas = canvas;
            reader = sensor.BodyFrameSource.OpenReader();
            this.canvas = canvas;
            ((BodyFrameReader)reader).FrameArrived += XBoxReader_BodyFrameArrived;
        }

        private void Set()
        {
            data = new ushort[description.LengthInPixels];
            bytes = new byte[description.LengthInPixels];
            convertedData = new byte[description.LengthInPixels * 4];
            bitmap = new WriteableBitmap(description.Width, description.Height, 96, 96, PixelFormats.Bgr32, null);
            sensor.Open();
        }

        private void XBoxReader_InfraFrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame == null) return;
                frame.CopyFrameDataToArray(data);
                for (var i = 0; i < data.Length; i++)
                {
                    byte intensity = (byte)(data[i] >> 8);
                    convertedData[i * 4] = intensity;
                    convertedData[i * 4 + 1] = intensity;
                    convertedData[i * 4 + 2] = intensity;
                    convertedData[i * 4 + 3] = 255;
                }
                bitmap.WritePixels(new Int32Rect(0, 0, description.Width, description.Height), convertedData, description.Width * 4, 0, 0);
            }
        }

        private void XBoxReader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame == null) return;
                frame.CopyConvertedFrameDataToArray(convertedData, ColorImageFormat.Rgba);
                bitmap.WritePixels(new Int32Rect(0, 0, description.Width, description.Height), convertedData, description.Width * 4, 0, 0);
            }
        }

        private void XBoxReader_BodyIndexFrameArrived(object sender, BodyIndexFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame == null) return;
                frame.CopyFrameDataToArray(bytes);
                for (var i = 0; i < data.Length; i++)
                {
                    convertedData[i * 4] = 0;
                    convertedData[i * 4 + 1] = bytes[i];
                    convertedData[i * 4 + 2] = 0;
                    convertedData[i * 4 + 3] = 255;
                }
                bitmap.WritePixels(new Int32Rect(0, 0, description.Width, description.Height), convertedData, description.Width * 4, 0, 0);
            }
        }

        private void XBoxReader_DepthFrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame == null) return;
                frame.CopyFrameDataToArray(data);
                for (var i = 0; i < data.Length; i++)
                {
                    convertedData[i * 4] = (byte)data[i];
                    convertedData[i * 4 + 1] = (byte)data[i];
                    convertedData[i * 4 + 2] = (byte)data[i];
                    convertedData[i * 4 + 3] = 255;
                }
                bitmap.WritePixels(new Int32Rect(0, 0, description.Width, description.Height), convertedData, description.Width * 4, 0, 0);
            }
        }

        private void XBoxReader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame == null) return;
                frame.GetAndRefreshBodyData(bodies);
                canvas.Children.Clear();
                foreach (var body in bodies)
                {
                    foreach (var type in Enum.GetValues(typeof(JointType)))
                    {
                        WritePoint(body, (JointType)type);
                    }
                }
            }
        }

        private void WritePoint(Body body, JointType type)
        {
            if (!showBody) return;
            if (!body.IsTracked || body.Joints[type].TrackingState != TrackingState.Tracked) return;
            Joint joint = body.Joints[type];
            var dsp = sensor.CoordinateMapper.MapCameraPointToDepthSpace(joint.Position);
            var size = pointSizes.Keys.Any(t => t == type) ? pointSizes[type] : defaultPointSize;
            var point = new Ellipse
            {
                Width = size.Width,
                Height = size.Height,
                Fill = new SolidColorBrush(Colors.Red)
            };
            canvas.Children.Add(point);
            Canvas.SetLeft(point, dsp.X - size.Width / 2);
            Canvas.SetTop(point, dsp.Y - size.Height / 2);
        }

    }
}
