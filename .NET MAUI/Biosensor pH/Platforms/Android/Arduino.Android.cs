using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using Android.Content;
using Android.Hardware.Usb;
using System.Text;
using Android.App;

namespace Biosensor_pH
{
    public static partial class Arduino
    {
        #region Właściwości

        private static bool _isConnected;

        public static bool IsConnected
        {
            get { return _isConnected; }
        }

        private static string _portName = string.Empty;

        public static string PortName
        {
            get { return _portName; }

            set { _portName = value; }
        }
        #endregion


        public static bool isConnected = false;

        private static string usbDeviceName;
        private static int deviceId;
        public static string manufacturerName = string.Empty;
        public static string productName = string.Empty;

        private static UsbManager? usbManager;
        private static UsbDevice? usbDevice;
        private static UsbInterface? usbDataInterface;
        private static UsbInterface? usbControlInterface;
        private static UsbEndpoint? usbEndpointOut;
        private static UsbEndpoint? usbEndpointIn;
        private static UsbDeviceConnection? usbDeviceConnection;

        // Zmiana nazwy na tą w CSPROJ
        const string ACTION_USB_PERMISSION = "com.maciejpaszek.biosensorph.USB_PERMISSION";

        public static Thread? readThread;

        public static partial string[] GetPortNames()
        {
            Android.App.Activity act = Platform.CurrentActivity;

            usbManager = (UsbManager)act.GetSystemService(Context.UsbService);

            IDictionary<string, UsbDevice> usbDevices = usbManager.DeviceList;

            if (usbDevices == null)
                return Array.Empty<string>();

            return usbDevices.Keys.ToArray();
        }
        
        public static partial bool Connect()
        {
            // Uzyskaj USB Managera
            usbManager = (UsbManager) Android.App.Application.Context.GetSystemService(Context.UsbService);

            // Uzyskaj dostępne urządzenia USB
            IDictionary<string, UsbDevice> usbDevices = usbManager.DeviceList;

            // Sprawdź warunki
            if (usbDevices == null)
                return false;

            if (usbDevices.Count == null)
                return false;

            usbDeviceName = PortName;

            if (usbDeviceName == string.Empty)
                return false;

            // Pobierz urządzenie ze słownika
            if (!usbDevices.TryGetValue(usbDeviceName, out usbDevice))
                return false;

            // Wypełnij pola informacyjne
            deviceId = usbDevice.DeviceId;
            manufacturerName = usbDevice.ManufacturerName;
            productName = usbDevice.ProductName;

            // Jeśli nie ma pozwolenia, to poproś
            if (!usbManager.HasPermission(usbDevice))
            {
                Debug.WriteLine("Nie mamy pozwolenia na to urządzenie.");
                var permissionIntent = PendingIntent.GetBroadcast(Android.App.Application.Context, 0,
                    new Intent(ACTION_USB_PERMISSION), 0);

                Android.App.Application.Context.RegisterReceiver(new UsbPermissionReceiver(usbManager, usbDevice),
                    new IntentFilter(ACTION_USB_PERMISSION));

                usbManager.RequestPermission(usbDevice, permissionIntent);

                return false;
            }

            if (!FindEndpoints())
                return false;

            Debug.WriteLine("Mamy pozwolenie na to urządzenie.");

            Debug.WriteLine("--- OTWIERANIE ---");

            usbDeviceConnection = usbManager.OpenDevice(usbDevice);

            Debug.WriteLine("--- KONFIGURACJA ---");

            if (usbDeviceConnection.ClaimInterface(usbControlInterface, true))
                Debug.WriteLine("Mamy control interfejs.");
            else
                Debug.WriteLine("Nie mamy control interfejsu.");

            SetSerialParameters(9600, 8, 1, 0);

            usbDeviceConnection.ReleaseInterface(usbControlInterface);

            if (usbDeviceConnection.ClaimInterface(usbDataInterface, true))
                Debug.WriteLine("Mamy interfejs.");
            else
                Debug.WriteLine("Nie mamy interfejsu.");

            isConnected = true;

            readThread = new Thread(ReadThread);

            readThread.Start();

            return true;
        }

        public static bool FindEndpoints()
        {
            usbControlInterface = null;
            usbDataInterface = null;
            usbEndpointOut = null;
            usbEndpointIn = null;

            for (int i = 0; i < usbDevice.InterfaceCount; i++)
            {
                var usbInterface = usbDevice.GetInterface(i);

                Debug.WriteLine("Interface " + i + ":"+ usbInterface.InterfaceClass.ToString());

                if (usbInterface.InterfaceClass == UsbClass.Comm)
                    usbControlInterface = usbInterface;

                if (usbInterface.InterfaceClass == UsbClass.CdcData)
                    usbDataInterface = usbInterface;

                for (int j = 0; j < usbInterface.EndpointCount; j++)
                {
                    var usbEndpoint = usbInterface.GetEndpoint(j);

                    if (usbEndpoint.Direction == UsbAddressing.Out && usbEndpoint.Type == UsbAddressing.XferBulk)
                    {
                        usbEndpointOut = usbEndpoint;
                    }

                    if (usbEndpoint.Direction == UsbAddressing.In && usbEndpoint.Type == UsbAddressing.XferBulk)
                    {
                        usbEndpointIn = usbEndpoint;
                    }

                    if (usbEndpointOut != null && usbEndpointIn != null)
                    {
                        Debug.WriteLine("Znaleziono odpowiednie endpointy.");
                        Debug.WriteLine("Interface " + usbInterface.InterfaceClass.ToString());

                        return true;
                    }
                }
            }

            Debug.WriteLine("Nie znaleziono odpowiednich endpointów.");

            return false;
        }

        class UsbPermissionReceiver : BroadcastReceiver
        {
            private readonly UsbManager _usbManager;
            private readonly UsbDevice _device;

            public UsbPermissionReceiver(UsbManager usbManager, UsbDevice device)
            {
                _usbManager = usbManager;
                _device = device;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                if (intent.Action == ACTION_USB_PERMISSION)
                {
                    if (intent.GetBooleanExtra(UsbManager.ExtraPermissionGranted, false))
                    {
                        //SendDataToDevice(_usbManager, _device);
                    }
                    context.UnregisterReceiver(this);
                }
            }
        }

        public static bool SetSerialParameters(int baudRate, int dataBits, int stopBits, int parity)
        {
            if (usbDeviceConnection == null)
                return false;

            // CDC kontrola: SET_LINE_CODING = 0x20
            // Format danych: [baud(4)][stop(1)][parity(1)][data bits(1)]
            byte stop;
            switch (stopBits)
            {
                case 2: stop = 2; break; // 2 stop bits
                case 1: stop = 0; break; // 1 stop bit
                default: stop = 0; break;
            }

            byte[] lineCoding = new byte[7];
            lineCoding[0] = (byte)(baudRate & 0xFF);
            lineCoding[1] = (byte)((baudRate >> 8) & 0xFF);
            lineCoding[2] = (byte)((baudRate >> 16) & 0xFF);
            lineCoding[3] = (byte)((baudRate >> 24) & 0xFF);
            lineCoding[4] = stop;
            lineCoding[5] = (byte)parity;
            lineCoding[6] = (byte)dataBits;

            // Wyślij SET_LINE_CODING
            usbDeviceConnection.ControlTransfer(
                requestType: (UsbAddressing) 0x21, // CLASS | INTERFACE | OUT
                request: 0x20,     // SET_LINE_CODING
                value: 0,
                index: usbControlInterface.Id,
                buffer: lineCoding,
                length: lineCoding.Length,
                timeout: 5000);

            // Wyślij SET_CONTROL_LINE_STATE (0x22) — ustawienie DTR/RTS
            usbDeviceConnection.ControlTransfer(
                requestType: (UsbAddressing) 0x21,
                request: 0x22,
                value: 0x03, // DTR (bit 0) | RTS (bit 1)
                index: usbControlInterface.Id,
                buffer: null,
                length: 0,
                timeout: 5000);

            return true;
        }

        public static partial void Disconnect()
        {
            if (usbDeviceConnection != null)
                if (usbDeviceConnection.ReleaseInterface(usbDataInterface))
                    usbDeviceConnection.Close();

            isConnected = false;

            readThread.Join();

            return;
        }

        public static partial void Write(string newLine)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(newLine + "\r\n");

            Debug.WriteLine("Wysyłanie...");

            int sentBytes = usbDeviceConnection.BulkTransfer(usbEndpointOut, buffer, buffer.Length, 1000);

            Debug.WriteLine("Wysłano " + sentBytes + " bajtów danych.");
        }

        public static void ReadThread()
        {
            byte[] bytes = new byte[64];
            StringBuilder stringBuilder = new StringBuilder();

            while (isConnected)
            {
                if (usbDeviceConnection != null && usbEndpointIn != null)
                {
                    int received = usbDeviceConnection.BulkTransfer(usbEndpointIn, bytes, 64, 100);

                    if(received > 0)
                    {
                        string command = Encoding.ASCII.GetString(bytes, 0 , received);

                        foreach (char c in command)
                        {
                            if (c == '\n')
                            {
                                string line = stringBuilder.ToString().Trim();

                                stringBuilder.Clear();

                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    Debug.WriteLine("Line: " + line);
                                    //WeakReferenceMessenger.Default.Send(new AddArduinoLine(line));

                                    DataReceivedEventArgs dataReceivedEventArgs = new DataReceivedEventArgs(line, DateTime.Now);
                                    OnDataReceived(dataReceivedEventArgs);

                                    Read(line);
                                }
                            }
                            else
                            {
                                stringBuilder.Append(c);
                            }
                        }
                    }
                }
            }
        }
    }
}
