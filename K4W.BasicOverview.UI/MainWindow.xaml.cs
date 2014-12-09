﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Drawing;

using Microsoft.Kinect;

namespace K4W.BasicOverview.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //-----Image processing initialization variables-----//
        //Size info for RGB pixel in bitmap form
        private readonly int _bytePerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        /// Representation of the Kinect Sensor
        private KinectSensor _kinect = null;
        //Frame reader for color output
        private ColorFrameReader _colorReader = null;
        /// FrameReader for our depth output
        private DepthFrameReader _depthReader = null;
        /// FrameReader for our infrared output
        private InfraredFrameReader _infraReader = null;
        /// Color pixel array
        private byte[] _colorPixels = null;
        /// Depth output pixel array
        private byte[] _depthPixels = null;
        /// Depth value array
        private ushort[] _depthData = null;
        /// Infrared output pixel array
        private byte[] _infraPixels = null;
        /// Infrared data array
        private ushort[] _infraData = null;
        /// Color WriteableBitmap linked to our UI
        private WriteableBitmap _colorBitmap = null;
        /// Color WriteableBitmap linked to our UI
        private WriteableBitmap _depthBitmap = null;
        /// Infrared WriteableBitmap linked to our UI
        private WriteableBitmap _infraBitmap = null;


        //-----Body tracking initialization variables-----//
        //FrameReader for bodies
        private BodyFrameReader _bodyReader = null;
        //Tracked bodies array
        private Body[] bodies = null;

        public MainWindow()
        {

            InitializeComponent();
            //Initialize the Kinect
            InitializeKinect();

            //Close Kinect when application is closed
            Closing += OnClosing;

        }

        ///<summary>
        ///Function: InitializeKinect()
        ///Description: Turns on the Kinect and sensor streams
        ///<summary>
        private void InitializeKinect()
        {
            //Get Kinect sensor
            _kinect = KinectSensor.GetDefault();

            if (_kinect==null) return;

            //Open connection with the sensor
            _kinect.Open();
            //Initialize Camera, Depth, Infrared and Body streams
            InitializeCamera();
            InitializeDepth();
            InitializeInfrared();
            InitializeBody();    
        }

        ///<summary>
        ///Function: OnClosing()
        ///Description: Turns off the Kinect
        ///<summary>
        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Close Kinect
            if (_kinect != null) _kinect.Close();
        }

        ///<summary>
        ///Function: InitializeCamera()
        ///Description: Checks if kinect sensor is detected. Then obtains
        ///metadata for ColorFrameSource of the sensor as a FrameDescription
        ///object. This object is used for allocation of the color pixel array _colorPixels
        ///based on the dimensions of the color output and number of bytes per pixel.
        ///ColorFrameReader object _colorReader is bound to the ColorFrameSource
        ///connected to the FrameArrived-event. Color data is rewritten through 
        ///initialization of a new WriteableBitmap object that is linked to the source
        ///of image control.
        ///</summary>
        private void InitializeCamera()
        {
            if (_kinect==null) return;
            //Get FrameDescrip for color output
            FrameDescription desc = _kinect.ColorFrameSource.FrameDescription;
            //Get FrameReader for color
            _colorReader = _kinect.ColorFrameSource.OpenReader();
            //Allocate pixel array
            _colorPixels = new byte[desc.Width*desc.Height*_bytePerPixel];
            //Create WriteableBitmap to store new color data
            _colorBitmap = new WriteableBitmap(desc.Width,desc.Height,96,96,PixelFormats.Bgr32,null);
            //Link WriteableBitmap to UI
            CameraImage.Source = _colorBitmap;
            //Hook-up event in Camera
            _colorReader.FrameArrived += OnColorFrameArrived;
        }

        ///<summary>
        ///Function: OnColorFrameArrived
        ///Description: Validates and copies color data stream then outputs to bitmap image.
        ///</summary>
        private void OnColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            //Obtain ref to ColorFrame
            ColorFrameReference colorRef = e.FrameReference;
            if (colorRef == null) return;
            //Acquire frame for specific ref
            ColorFrame frame = colorRef.AcquireFrame();
            //if frame skipped/gone
            if (frame==null) return;
            using (frame)
            {
                //Obtain frame desc
                FrameDescription frameDesc = frame.FrameDescription;
                //Check if width/height matches
                if (frameDesc.Width ==_colorBitmap.PixelWidth && frameDesc.Height==_colorBitmap.PixelHeight)
                {
                    //Copy data to array based on format of img
                    if (frame.RawColorImageFormat==ColorImageFormat.Bgra)
                    {
                        frame.CopyRawFrameDataToArray(_colorPixels);
                    }
                    else frame.CopyConvertedFrameDataToArray(_colorPixels,ColorImageFormat.Bgra);

                    //Copy to bitmap img format
                    _colorBitmap.WritePixels(new Int32Rect(0,0,frameDesc.Width,frameDesc.Height),_colorPixels, 
                                                    frameDesc.Width*_bytePerPixel, 0);
                }
            }
        }

        ///<summary>
        ///Function: InitializeDepth()
        ///Description: Initialize FrameDescription, DepthReader, data/pixel arrays and WriteableBitmap objects
        ///<summary>
        private void InitializeDepth()
        {
            if (_kinect == null) return;
            //Get the frame description for the color output
            FrameDescription desc = _kinect.DepthFrameSource.FrameDescription;
            //Get the framereader for Color
            _depthReader = _kinect.DepthFrameSource.OpenReader();
            //Allocate pixel array
            _depthData = new ushort[desc.Width*desc.Height];
            _depthPixels = new byte[desc.Width*desc.Height*_bytePerPixel];
            //Create new WriteableBitmap
            _depthBitmap = new WriteableBitmap(desc.Width, desc. Height, 96,96,PixelFormats.Bgr32, null);
            //Link WBMP to UI
            DepthImage.Source = _depthBitmap;
            //Hook-up event
            _depthReader.FrameArrived += OnDepthFrameArrived;
        }

        ///<summary>
        ///Function: OnDepthFrameArrived
        ///Description: Validates and copies depth data stream then outputs to bitmap image.
        ///</summary>
        private void OnDepthFrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            //Ref to depth frame
            DepthFrameReference refer = e.FrameReference;

            if (refer == null) return;
            //Get Depth frame
            DepthFrame frame = refer.AcquireFrame();

            if (frame == null) return;

            using(frame)
            {
                //Obtain frame desc
                FrameDescription frameDesc = frame.FrameDescription;
                //Copy depth frames
                frame.CopyFrameDataToArray(_depthData);

                //Get min/max depth
                ushort minDepth = frame.DepthMinReliableDistance;
                ushort maxDepth = frame.DepthMaxReliableDistance;

                //Adjust visualization based on depth
                int colorPixelIndex = 0;
                for (int i = 0; i< _depthData.Length; ++i)
                {
                    //get Depth
                    ushort depth = _depthData[i];
                    if (depth == 0)
                    {
                        _depthPixels[colorPixelIndex++] = 41;
                        _depthPixels[colorPixelIndex++] = 239;
                        _depthPixels[colorPixelIndex++] = 242;
                    }
                    else if (depth < minDepth || depth > maxDepth)
                    {
                        _depthPixels[colorPixelIndex++] = 25;
                        _depthPixels[colorPixelIndex++] = 0;
                        _depthPixels[colorPixelIndex++] = 255;
                    }
                    else
                    {
                        double grey = (Math.Floor((double)depth/250)*12.75);
                        _depthPixels[colorPixelIndex++] = (byte)grey;
                        _depthPixels[colorPixelIndex++] = (byte)grey;
                        _depthPixels[colorPixelIndex++] = (byte)grey;
                    }
                //increment color pixel index
                ++colorPixelIndex;
                }
                //output to bitmap
                _depthBitmap.WritePixels(new Int32Rect(0,0,frameDesc.Width, frameDesc.Height),
                                            _depthPixels, frameDesc.Width*_bytePerPixel, 0);
            }
        }

        ///<summary>
        ///Function: InitializeInfrared()
        ///Description: Initialize FrameDescription, DepthReader, data/pixel arrays and WriteableBitmap objects
        ///<summary>
        private void InitializeInfrared()
        {
            if (_kinect == null) return;
            //Get frame desc for color out
            FrameDescription desc = _kinect.InfraredFrameSource.FrameDescription;
            //Get frameread for color
            _infraReader = _kinect.InfraredFrameSource.OpenReader();
            //Allocate array of pixels
            _infraData = new ushort[desc.Width*desc.Height];
            _infraPixels = new byte[desc.Width*desc.Height*_bytePerPixel];
            //Create new WriteableBitmap
            _infraBitmap = new WriteableBitmap(desc.Width,desc.Height,96,96,PixelFormats.Bgr32,null);
            //Linking WriteableBitmap to the user interface
            InfraredImage.Source = _infraBitmap;
            //Hook-up event
            _infraReader.FrameArrived += OnInfraredFrameArrived;
        }

        ///<summary>
        ///Function: OnInfraredFrameArrived
        ///Description: Validates and copies infrared data stream then outputs to bitmap image.
        ///</summary>
        private void OnInfraredFrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
            //Ref to infrared frame
            InfraredFrameReference refer = e.FrameReference;

            if (refer == null) return;

            //Get infrared frame
            InfraredFrame frame = refer.AcquireFrame();

            if (frame == null) return;

            using(frame)
            {
                //Get desc for frame
                FrameDescription frameDesc = frame.FrameDescription;

                if (((frameDesc.Width*frameDesc.Height)==_infraData.Length) && (frameDesc.Width == _infraBitmap.PixelWidth) && (frameDesc.Height ==_infraBitmap.PixelHeight))
                {
                    //Copy data to array
                    frame.CopyFrameDataToArray(_infraData);
                    int colorPixelIndex=0;

                    for (int i = 0; i < _infraData.Length; ++i)
                    {
                        //Get IR value
                        ushort ir = _infraData[i];
                        //Bitshift the data
                        byte intensity = (byte)(ir >> 8);
                        //Assign IR intensity
                        _infraPixels[colorPixelIndex++] = intensity;
                        _infraPixels[colorPixelIndex++] = intensity;
                        _infraPixels[colorPixelIndex++] = intensity;

                        ++colorPixelIndex;
                    }

                    //Copy out to bitmap object
                    _infraBitmap.WritePixels(new Int32Rect(0,0,frameDesc.Width, frameDesc.Height),
                                            _infraPixels, frameDesc.Width*_bytePerPixel, 0);
                }

              
            }
        }

        private void InitializeBody()
        { 
            if (_kinect == null) return;
            
            //Fill array of bodies
            _bodies = new Body[_kinect.BodyFrameSource.BodyCount];
            
            //Open body reader
            _bodyReader = _kinect.BodyFrameSource.OpenReader();

            //Hook-up event
            _bodyReader.FrameArrived += OnBodyFrameArrived;

        }

        private void OnBodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            //Get reference frame
            BodyFrameReference refer = e.FrameReference;

            if (refer == null) return;

            using (frame)
            {
                //Obtain body data
                Frame.GetAndRefreshBodyData(_bodies);
                //Clear canvas
                SkeletonCanvas.Children.Clear();
                //Loop through all bodes
                foreach (Body body in _bodies)
                { 
                    //Process bodies that are being tracked
                    if (body.IsTracked)
                    {
                        DrawBody(body);
                    }
                }
            }
        }

        private void DrawBody(Body body)
        { 
        //Draw points
        foreach (JointType type in body.Joints.Keys)
            //Draw all joints
            switch (type)
            { 
                case JointType.Head:
                case JointType.FootLeft:
                case JointType.FootRight:
                    DrawJoint(body.Joints[type], 20, Brushes.Red, 2, Brushes.Pink);
                    break;
                case JointType.ShoulderLeft:
                case JointType.ShoulderRight:
                case JointType.HipLeft:
                case JointType.HipRight:
                    DrawJoint(body.Joints[type], 20, Brushes.Blue, 2, Brushes.SkyBlue);
                    break;
                case JointType.ElbowLeft:
                case JointType.ElbowLeft:
                case JointType.KneeLeft:
                case JointType.KneeRight:
                    DrawJoint(body.Joints[type], 15, Brushes.Green, 2, Brushes.Honeydew);
                    break;
                case JointType.HandLeft:
                    DrawHandJoint(body.Joints[type], body.HandLeftState, 20, 2, Brushes.Orange);
                    break;
                case JointType.HandRight:
                    DrawHandJoint(body.Joints[type], body.HandRightState, 20, 2, Brushes.Orange);
                    break;
                default:
                    DrawJoint(body.Joints[type], 15, Brushes.Purple, 2, Brushes.Plum);
                    break;
            
            }
        }

        private void DrawJoin(Joint joint, double radius, SolidColorBrush fill, double borderWidth, SolidColorBrush border)
        {
            if (joint.TrackingState != TrackingState.Tracked) return;
            //Map Camera point to Colorspace
            ColorSpacePoint colorPoint = _kinect.CoordinateMapper.MapCameraPointsToColorSpace(joint.position);
            //Create UI object
            Ellipse el = new Ellipse();
            el.Fill = fill;
            el.Stroke = border;
            el.StrokeThickness = borderWidth;
            el.Width = el.Height = radius;
            //Put Ellipse on canvas
            SkeletonCanvas.Children.Add(el);

            //Take care of edge case when point is at infinity
            if (float.IsInfinity(colorPoint.X) || float.IsInfinity(colorPoint.X)) return;
            
            //Align ellipse on canvas
            Canvas.SetLeft(el, colorPoint.X/2.0;)
            Canvas.SetTop(el, colorPoint.Y/2);
        }
        #region UI Methods

        private void OnToggleCamera(object sender, RoutedEventArgs e)
        {
            ChangeVisualMode("Camera");
        }

        private void OnToggleDepth(object sender, RoutedEventArgs e)
        {
            ChangeVisualMode("Depth");
        }

        private void OnToggleInfrared(object sender, RoutedEventArgs e)
        {
            ChangeVisualMode("Infrared");
        }

        /// <summary>
        /// Change the UI based on the mode
        /// </summary>
        /// <param name="mode">New UI mode</param>
        private void ChangeVisualMode(string mode)
        {
            // Invis all
            CameraGrid.Visibility = Visibility.Collapsed;
            DepthGrid.Visibility = Visibility.Collapsed;
            InfraredGrid.Visibility = Visibility.Collapsed;

            switch (mode)
            {
                case "Camera":
                    CameraGrid.Visibility = Visibility.Visible;
                    break;

                case "Depth":
                    DepthGrid.Visibility = Visibility.Visible;
                    break;

                case "Infrared":
                    InfraredGrid.Visibility = Visibility.Visible;
                    break;
            }
        }

        #endregion UI Methods
    }
}
