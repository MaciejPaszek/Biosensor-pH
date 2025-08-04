using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;

namespace Biosensor_pH___MAUI
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
