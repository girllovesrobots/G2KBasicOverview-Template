using System.Windows;

namespace K4W.BasicOverview.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Initialize the Kinect
            InitializeKinect();

            //Close Kinect when application is closed
            Closing += OnClosing

        }

        private KinectSensor _kinect = null;

        ///<summary>
        ///Function: InitializeKinect()
        ///Description: Turns on the Kinect and sensor streams
        ///<summary>
        private void InitializeKinect()
        {
            //Get Kinect sensor
            _kinect = KinectSensor.Default;

            if (_kinect==null) return;

            //Open connection with the sensor
            _kinect.Open();
            //Initialize Camera, Depth, Infrared and Body streams
            InitializeCamera();
            InitializeDepth();
            InitializeInfrared();
            InitializeBody();    
        }
        private void OnClosing(object sender, SystemComponentModel.CancelEventArgs e)
        {
            //Close Kinect
            if (_kinect != null) _kinect.Close();
        }

        ///<summary>
        ///Camera Visualization variables
        ///</summary>

        //Size info for RGB pixel in bitmap form
        private readonly int _bytePerPixel = (PixelFormats.Bgr32.BitsPerPixel+7)/8;
        //Frame reader for color output
        private ColorFrameReader _colorReader = null;
        //Color pixel array
        private byte[] _colorPixels = null;
        //Writeable Bitmap in color that is linked to our UI
        private WriteableBitmap _colorBitmap = null;

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

        private void OnColorFrameArrived(object sender, ColorFrameArrivedEventArgs)
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
                    _colorBitmap.WritePixels(new Int32Rec(0,0,frameDesc.Width,frameDesc.Height),_colorPixels
                                                frameDesc.Width*_bytePerPixel, 0);
                }
            }
        }

        ///<summary>
        ///Depth indication variables:
        ///FrameReader for our depth output, Array of depth values.
        ///Array of depth pixels used for the output, Depth WriteableBitmap
        ///linked to our UI
        ///</summary>
        private DepthFrameReader _depthReader = null;
        private ushort[] _depthData = null;
        private byte[] _depthPixels = null;
        private WriteableBitmap _depthBitmap = null;

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

        private void OnDepthFrameArrived(object sender, DepthFrameArrivedEventArgs)
        {
            DepthFrameReference refer = e.FrameReference;

            if (refer == null) return;

            DepthFrame frame = refer.AcquireFrame();

            if (frame == null) return;

            using(frame)
            {
                //Copy depth frames
                frame.CopyFrameDataToArray(_depthData);

                //Get min/max depth
                ushort minDepth = frame.DepthMinReliableDistance;
                ushort maxDepth = frame.DepthMaxReliableDistance;

                //Adjust visualization based on depth
                int colorPixelIndex = 0;
                for (int i = 0; i< _depthData.length; ++i)
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
                _depthBitmap.WritePixels(new Int32Rec(0,0,frameDesc.Width, frameDesc.Height),
                                            _depthPixels, frameDesc.Width*_bytePerPixel, 0);
            }
        }

        ///<summary>
        ///IR variables initialized below
        ///Includes IRFrameReader, array of IR data, array of pixels
        //and Writeable Bitmap object linked to UI to output the IR img
        ///</summary>
        private InfraredFrameReader _infraReader = null;
        private ushort[] _infraData = null;
        private byte[] _infraPixels = null;
        private WriteableBitmap _infraBitmap = null;

        private void InitializeInfrared()
        {
            
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
