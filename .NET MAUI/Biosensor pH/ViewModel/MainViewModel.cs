using Microsoft.Maui.Devices;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace Biosensor_pH.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Właściwości

        private bool _isConnected;

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }

        private string _platformName;

        public string PlatformName
        {
            get { return _platformName; }
            set
            {
                _platformName = value;
                OnPropertyChanged();
            }
        }

        private int _selectedIndex;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                Debug.WriteLine(value);
                //if (Application.Current != null)
                //{
                if (value == 0)
                    //SetAppTheme(AppTheme.Unspecified);
                    Application.Current.UserAppTheme = AppTheme.Unspecified;

                if (value == 1)
                    Application.Current.UserAppTheme = AppTheme.Light;

                if (value == 2)
                    Application.Current.UserAppTheme = AppTheme.Dark;
                //}

                OnPropertyChanged();
            }
        }

        #endregion

        #region Konstruktor

        public MainViewModel()
        {
            _isConnected = false;
            _platformName = "Nieokreślono";

            DevicePlatform devicePlatform = DeviceInfo.Current.Platform;

            if (devicePlatform == DevicePlatform.WinUI)
                PlatformName = "Windows";

            if (devicePlatform == DevicePlatform.Android)
                PlatformName = "Android";

            SetAppTheme(AppTheme.Light);

            SelectedIndex = 0;

        }

        private void SetAppTheme(AppTheme theme)
        {
            if (Application.Current != null)
                Application.Current.UserAppTheme = theme;
        }

        #endregion

        #region Interfejs INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "")
        { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
