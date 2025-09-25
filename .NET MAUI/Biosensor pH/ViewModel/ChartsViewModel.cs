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

        private Queue<Point> _sampleTemperature;

        public Queue<Point> SampleTemperature
        {
            get { return _sampleTemperature; }
            set
            {
                _sampleTemperature = value;
                OnPropertyChanged();
            }
        }

        private Queue<Point> _ambientTemperature;

        public Queue<Point> AmbientTemperature
        {
            get { return _ambientTemperature; }
            set
            {
                _ambientTemperature = value;
                OnPropertyChanged();
            }
        }

        private Queue<Point> _ambientHumidity;

        public Queue<Point> AmbientHumidity
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

            _sampleTemperature  = new Queue<Point>(100);
            _ambientTemperature = new Queue<Point>(100);
            _ambientHumidity    = new Queue<Point>(100);
            
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
                    SampleTemperature.Enqueue(new Point(0.0, sampleTemperature));
                    AmbientTemperature.Enqueue(new Point(0.0, ambientTemperature));
                    AmbientHumidity.Enqueue(new Point(0.0, ambientHumidity));
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
