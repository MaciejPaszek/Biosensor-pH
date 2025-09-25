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
        /// <summary>
        /// Zmiana statusu połączenia z Arduino
        /// </summary>
        public static event EventHandler<ConnectionChangedEventArgs>? ConnectionChanged;

        private static void OnConnected(ConnectionChangedEventArgs e)
        {
            ConnectionChanged?.Invoke(null, e);
        }

        /// <summary>
        /// Arduino otrzymuje nową linię danych
        /// </summary>
        public static event EventHandler<DataReceivedEventArgs>? DataReceived;

        private static void OnDataReceived(DataReceivedEventArgs e)
        {
            DataReceived?.Invoke(null, e);
        }

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
    }
}
