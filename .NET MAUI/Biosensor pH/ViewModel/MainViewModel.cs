using System;
using System.ComponentModel;
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

        private bool _isToggled;

        public bool IsToggled
        {
            get { return _isToggled; }
            set
            {
                _isToggled = value;
                
                if(value)
                    Application.Current.UserAppTheme = AppTheme.Dark;
                else
                    Application.Current.UserAppTheme = AppTheme.Light;

                OnPropertyChanged();
            }
        }

        private AppTheme _appTheme;

        public AppTheme AppTheme
        {
            get { return _appTheme; }
            set
            {
                _appTheme = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Konstruktor

        public MainViewModel()
        {
            _isConnected = false;
            Application.Current.UserAppTheme = AppTheme.Unspecified;
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
