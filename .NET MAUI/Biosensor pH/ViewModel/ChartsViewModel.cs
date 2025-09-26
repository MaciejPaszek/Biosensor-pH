using Biosensor_pH.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Biosensor_pH.ViewModel
{
    public class ChartsViewModel : INotifyPropertyChanged
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

        private const int maxQueueCapacity = 600;

        private Queue<DataPoint> _sampleTemperature;

        public Queue<DataPoint> SampleTemperature
        {
            get { return _sampleTemperature; }
            set
            {
                _sampleTemperature = value;
                OnPropertyChanged();
            }
        }

        private Queue<DataPoint> _ambientTemperature;

        public Queue<DataPoint> AmbientTemperature
        {
            get { return _ambientTemperature; }
            set
            {
                _ambientTemperature = value;
                OnPropertyChanged();
            }
        }

        private Queue<DataPoint> _ambientHumidity;

        public Queue<DataPoint> AmbientHumidity
        {
            get { return _ambientHumidity; }
            set
            {
                _ambientHumidity = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Konstruktor

        public ChartsViewModel()
        {
            _isConnected = false;

            SampleTemperature  = new Queue<DataPoint>();
            AmbientTemperature = new Queue<DataPoint>();
            AmbientHumidity    = new Queue<DataPoint>();

            Arduino.ConnectionChanged += Arduino_ConnectionChanged;
            Arduino.DataReceived += Arduino_DataReceived;
        }

        #endregion

        private void Arduino_ConnectionChanged(object? sender, ConnectionChangedEventArgs e)
        {
            IsConnected = e.ConnectionStatus;
        }

        private void Arduino_DataReceived(object? sender, DataReceivedEventArgs e)
        {
            string line = e.Line;

            line.Trim();
            line = line.Replace('.', ',');

            char[] separator = { ' ' };
            string[] values = line.Split(separator);

            double sampleTemperature = 0.0;
            double ambientTemperature = 0.0;
            double ambientHumidity = 0.0;

            if (values.Length >= 3)
            {
                try
                {
                    sampleTemperature  = Convert.ToSingle(values[0]);
                    ambientTemperature = Convert.ToSingle(values[1]);
                    ambientHumidity    = Convert.ToSingle(values[2]);
                }
                catch (FormatException formatException)
                {
                    Debug.WriteLine(formatException.Message);
                }
                finally
                {
                    SampleTemperature.Enqueue(new DataPoint(sampleTemperature, e.DateTime));
                    AmbientTemperature.Enqueue(new DataPoint(ambientTemperature, e.DateTime));
                    AmbientHumidity.Enqueue(new DataPoint(ambientHumidity, e.DateTime));
                }
            }
        }

        #region Interfejs INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "")
        { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
