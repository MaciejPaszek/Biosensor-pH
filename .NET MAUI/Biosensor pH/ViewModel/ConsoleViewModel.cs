using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace Biosensor_pH.ViewModel
{
    public class ConsoleViewModel : INotifyPropertyChanged
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

        private string _buttonName;

        public string ButtonName
        {
            get { return _buttonName; }
            private set
            {
                _buttonName = value;
                OnPropertyChanged();
                RefreshCanExecutes();
            }
        }

        private string _outputString;

        public string OutputString
        {
            get { return _outputString; }
            set
            {
                _outputString = value;
                OnPropertyChanged();
            }
        }

        private string _commandString;

        public string CommandString
        {
            get { return _commandString; }
            set
            {
                _commandString = value;
                OnPropertyChanged();
                RefreshCanExecutes();
            }
        }

        private string _portName;

        public string PortName
        {
            get { return _portName; }
            set
            {
                _portName = value;
                OnPropertyChanged();
            }
        }

        private List<string> _portNames;

        public List<string> PortNames
        {
            get { return _portNames; }
            set
            {
                _portNames = value;
                OnPropertyChanged();
            }
        }

        //public ObservableCollection<string> PortNames { get; } = new ObservableCollection<string>()

        #endregion

        #region Prywatne

        private Queue<string> _outputQueue;

        #endregion

        #region Komendy

        public ICommand ArduinoRefresh { get; private set; }
        public ICommand ArduinoConnect { get; private set; }
        public ICommand ArduinoWrite { get; private set; }
        public ICommand ArduinoClear { get; private set; }

        #endregion

        #region Konstruktor

        public ConsoleViewModel()
        {
            _isConnected = false;
            _buttonName = "Połącz";
            _outputString = "Połącz z Arduino, a następnie wyślij komendę 'START'.";
            _commandString = string.Empty;
            _outputQueue = new Queue<string>();
            _portNames = new List<string>(Arduino.GetPortNames());
            if(_portNames.Count > 0)
                _portName = _portNames.First();

            ArduinoRefresh = new Command(
                execute: () =>
                {
                    PortNames = new List<string>(Arduino.GetPortNames());
                    if (PortNames.Count > 0)
                        PortName = PortNames.First();
                    RefreshCanExecutes();
                },
                canExecute: () =>
                {
                    return !IsConnected;
                });

            ArduinoConnect = new Command(
                execute: () =>
                {
                    if (!IsConnected)
                    {
                        Arduino.PortName = PortName;
                        if (Arduino.Connect())
                        {
                            IsConnected = true;
                            ButtonName = "Rozłącz";
                        } 
                    }
                    else
                    {
                        Arduino.Disconnect();
                        IsConnected = false;
                        ButtonName = "Połącz";
                    }

                    RefreshCanExecutes();
                },
                canExecute: () =>
                {
                    return !IsConnected && PortName != null && PortName != String.Empty || IsConnected;
                });

            ArduinoWrite = new Command(
                execute: () =>
                {
                    Arduino.Write(CommandString);

                    DateTime dateTime = DateTime.Now;

                    _outputQueue.Enqueue($"{dateTime.ToString("HH:mm:ss.fff")} [User]\t\t{CommandString}");

                    BuildOutputString();

                    CommandString = String.Empty;

                    RefreshCanExecutes();
                },
                canExecute: () =>
                {
                    return IsConnected && CommandString != String.Empty;
                });

            ArduinoClear = new Command(
                execute: () =>
                {
                    _outputQueue.Clear();
                    OutputString = String.Empty;

                    RefreshCanExecutes();
                },
                canExecute: () =>
                {
                    return true;
                });

            Arduino.ConnectionChanged += Arduino_ConnectionChanged;
            Arduino.DataReceived += Arduino_DataReceived;
        }

        void RefreshCanExecutes()
        {
            (ArduinoRefresh as Command)?.ChangeCanExecute();
            (ArduinoConnect as Command)?.ChangeCanExecute();
            (ArduinoWrite as Command)?.ChangeCanExecute();
            (ArduinoClear as Command)?.ChangeCanExecute();
        }

        #endregion

        private void BuildOutputString()
        {
            if (_outputQueue.Count > 500)
                _outputQueue.Dequeue();

            StringBuilder stringBuilder = new StringBuilder();
            foreach (string item in _outputQueue)
                stringBuilder.Append(item + "\n");

            OutputString = stringBuilder.ToString();
        }

        private void Arduino_ConnectionChanged(object? sender, ConnectionChangedEventArgs e)
        {
            IsConnected = e.ConnectionStatus;
        }

        private void Arduino_DataReceived(object? sender, DataReceivedEventArgs e)
        {
            _outputQueue.Enqueue($"{e.DateTime.ToString("HH:mm:ss.fff")} [Arduino]\t{e.Line}");

            BuildOutputString();
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
