using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using System.IO.Ports;

namespace Biosensor_pH___MAUI
{
    public partial class Arduino
    {
        public static SerialPort? serialPort = null;

        public Arduino()
        {
        }

        public static partial string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        public static partial bool IsConnected()
        {
            if (serialPort != null)
                if (serialPort.IsOpen)
                    return true;

            return false;
        }

        public static partial bool Connect()
        {
            if (portName == string.Empty)
                return false;

            try
            {
                serialPort = new SerialPort(portName, 9600);
            }
            catch (IOException e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }

            serialPort.DataReceived += SerialPort_DataReceived;

            try
            {
                serialPort.Open();
            }
            catch(Exception exc)
            {
                Debug.WriteLine(exc.Message);
                return false;
            }
            //serialPort.WriteLine("END");
            serialPort.DiscardInBuffer();

            return true;
        }

        public static partial void Disconnect()
        {
            if (IsConnected())
            {
                serialPort.WriteLine("END");
                serialPort.DiscardInBuffer();
                serialPort.Close();
            }
        }
        
        public static partial void Write(string newLine)
        {

            if(IsConnected())
                serialPort.Write(newLine + "\r\n");
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

            //Debug.WriteLine(data);

            WeakReferenceMessenger.Default.Send(new AddArduinoLine(data));

            Read(data);
            //ReadBiosensor(data);
        }
    }
}
