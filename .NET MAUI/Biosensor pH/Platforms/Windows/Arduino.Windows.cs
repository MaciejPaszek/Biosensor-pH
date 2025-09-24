using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using System.IO.Ports;

namespace Biosensor_pH
{
    public static partial class Arduino
    {
        #region Właściwości

        public static bool IsConnected
        {
            get {
                if (_serialPort == null)
                    return false;

                if (!_serialPort.IsOpen)
                    return false;

                return true;
            }
        }

        private static string _portName = string.Empty;

        public static string PortName
        {
            get { return _portName; }
            set { _portName = value; }
        }

#endregion

#region Pola Prywatne

        private static SerialPort? _serialPort = null;
        private const int _baudRate = 9600;

        #endregion

        public static partial string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        public static partial bool Connect()
        {
            if (_portName == null)
                return false;

            if (_portName == string.Empty)
                return false;

            try
            {
                _serialPort = new SerialPort(_portName, _baudRate, Parity.None, 8, StopBits.One);
            }
            catch (IOException e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }

            _serialPort.DataReceived += SerialPort_DataReceived;

            try
            {
                _serialPort.Open();
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }

            _serialPort.DiscardInBuffer();

            return true;
        }

        public static partial void Disconnect()
        {
            if (!IsConnected)
                return;

            _serialPort?.Close();
        }
        
        public static partial void Write(string newLine)
        {
            if(IsConnected)
                _serialPort?.Write(newLine + "\r\n");
        }

        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort)sender;

            string data;

            try
            {
                data = serialPort.ReadLine();
            }
            catch(OperationCanceledException operationCancelledException)
            {
                Debug.WriteLine(operationCancelledException.Message);
                return;
            }

            data.Trim();

            DataReceivedEventArgs dataReceivedEventArgs = new DataReceivedEventArgs(data, DateTime.Now);
            OnDataReceived(dataReceivedEventArgs);
        }
    }
}
