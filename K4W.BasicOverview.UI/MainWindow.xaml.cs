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
        ///Depth indication variables
        ///</summary>


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
