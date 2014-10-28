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