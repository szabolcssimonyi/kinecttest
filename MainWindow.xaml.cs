using Microsoft.Kinect;
using System;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace KinectTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private XBoxReader infraReader;
        private XBoxReader coloredReader;
        private XBoxReader depthReader;
        private XBoxReader bodyReader;
        private XBoxReader bodyIndexReader;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            infraReader.Dispose();
            coloredReader.Dispose();
            depthReader.Dispose();
            bodyReader.Dispose();
            bodyIndexReader.Dispose();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            infraReader = XBoxReader.Factory.CreateInfraReader(InfraImage, InfraCanvas);
            coloredReader = XBoxReader.Factory.CreateColorReader(ColorImage, ColorCanvas);
            depthReader = XBoxReader.Factory.CreateDepthReader(DepthImage, DepthCanvas);
            bodyIndexReader = XBoxReader.Factory.CreateBodyIndexReader(BodyIndexImage, BodyIndexCanvas);
            bodyReader = XBoxReader.Factory.CreateBodyReader(BodyCanvas);
        }

        private void ShowDepthBody(object sender, RoutedEventArgs e)
        {
            depthReader.ShowBody = true;
        }

        private void HideDepthBody(object sender, RoutedEventArgs e)
        {
            depthReader.ShowBody = false;
        }

        private void HideColorBody(object sender, RoutedEventArgs e)
        {

            coloredReader.ShowBody = false;
        }

        private void ShowColorBody(object sender, RoutedEventArgs e)
        {
            coloredReader.ShowBody = true;
        }

        private void HideInfraBody(object sender, RoutedEventArgs e)
        {
            infraReader.ShowBody = false;
        }

        private void ShowInfraBody(object sender, RoutedEventArgs e)
        {
            infraReader.ShowBody = true;
        }

        private void HideBodyIndex(object sender, RoutedEventArgs e)
        {
            bodyIndexReader.ShowBody = false;
        }

        private void ShowBodyIndex(object sender, RoutedEventArgs e)
        {
            bodyIndexReader.ShowBody = true;
        }
    }
}
