using System.Diagnostics;

namespace Biosensor_pH
{

    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            DevicePlatform devicePlatform = DeviceInfo.Current.Platform;
            LabelDeviceType.Text = GetPlatformName();
        }

        private string GetPlatformName()
        {
#if WINDOWS
            return "Windows";
#elif ANDROID
            return "Android";
#else
            return "Other";
#endif
        }
    }
}
