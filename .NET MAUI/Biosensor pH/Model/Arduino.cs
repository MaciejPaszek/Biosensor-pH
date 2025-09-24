using System.Diagnostics;

namespace Biosensor_pH
{
    public class ConnectionChangedEventArgs : EventArgs
    {
        public readonly bool ConnectionStatus;

        public ConnectionChangedEventArgs(bool connectionStatus)
        {
            ConnectionStatus = connectionStatus;
        }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public readonly string Line;
        public readonly DateTime DateTime;

        public DataReceivedEventArgs(string line, DateTime dateTime)
        {
            Line = line;
            DateTime = dateTime;
        }
    }

    public static partial class Arduino
    {
        public static event EventHandler<ConnectionChangedEventArgs>? ConnectionChanged;

        private static void OnConnected(ConnectionChangedEventArgs e)
        {
            ConnectionChanged?.Invoke(null, e);
        }

        public static event EventHandler<DataReceivedEventArgs>? DataReceived;

        private static void OnDataReceived(DataReceivedEventArgs e)
        {
            DataReceived?.Invoke(null, e);
        }

        #region Właściwości

        #endregion

        #region Metody zależne od platformy sprzętowej

        /// <summary>
        /// Połącz z Arduino
        /// </summary>
        public static partial string[] GetPortNames();

        /// <summary>
        /// Połącz z Arduino
        /// </summary>
        public static partial bool Connect();

        /// <summary>
        /// Rozłącz z Arduino
        /// </summary>
        public static partial void Disconnect();

        /// <summary>
        /// Wyślij nową linię tekstu do Arduino
        /// </summary>
        public static partial void Write(string newLine);

        #endregion

        #region Metody niezależne od platformy sprzętowej

        public static void Read(string data)
        {
            data = data.Replace('.', ',');

            char[] separator = { ' ' };
            string[] values = data.Split(separator);

            float SampleTemperature = 0;
            float AmbientTemperature = 0;
            float AmbientHumidity = 0;

            if (values.Length >= 3)
            {
                try
                {
                    SampleTemperature = Convert.ToSingle(values[0]);
                    AmbientTemperature = Convert.ToSingle(values[1]);
                    AmbientHumidity = Convert.ToSingle(values[2]);
                }
                catch (FormatException formatException)
                {
                    Debug.WriteLine(formatException.Message);
                }
                finally
                {
                    ChartSample chartsample = new ChartSample(SampleTemperature, AmbientTemperature, AmbientHumidity);
                    //WeakReferenceMessenger.Default.Send(new NewChartSamplesMessage(chartsample));
                }
            }
        }

        public static float SampleTemperature = 0;
        public static float AmbientTemperature = 0;
        public static float AmbientHumidity = 0;

        public static void ReadBiosensor(string data)
        {
            data = data.Replace('.', ',');

            char[] separator = { ' ' };
            string[] strings = data.Split(separator);

            if (strings.Length == 0)
                return;

            if (data.StartsWith("Temperatura z DHT11:"))
            {
                AmbientTemperature = 0;
                AmbientHumidity = 0;

                if (strings.Length >= 4)
                    AmbientTemperature = Convert.ToSingle(strings[3]);

                if (strings.Length >= 6)
                    AmbientHumidity = Convert.ToSingle(strings[5]);
            }

            if (data.StartsWith("Temperatura z DS18B20:"))
            {
                SampleTemperature = 0;

                if (strings.Length >= 4)
                    SampleTemperature = Convert.ToSingle(strings[3]);

                ChartSample chartsample = new ChartSample(SampleTemperature, AmbientTemperature, AmbientHumidity);
                //WeakReferenceMessenger.Default.Send(new NewChartSamplesMessage(chartsample));
            }

            if (data.StartsWith("I^2C"))
            {
                AmbientTemperature = -1;
                AmbientHumidity = -1;
            }

            if (data.StartsWith("Brak"))
            {
                SampleTemperature = -1;

                ChartSample chartsample = new ChartSample(SampleTemperature, AmbientTemperature, AmbientHumidity);
                //WeakReferenceMessenger.Default.Send(new NewChartSamplesMessage(chartsample));
            }
        }
        #endregion
    }
}
