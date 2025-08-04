using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;

#if WINDOWS
using System.IO.Ports;

#elif ANDROID
using Android.Content;
using Android.Hardware.Usb;
using System.Text;
using Android.App;
#endif

namespace Biosensor_pH___MAUI
{
    internal class Arduino
    {
        public static string portName { get; set; } = string.Empty;

#if WINDOWS
        public static SerialPort? serialPort = null;
# elif ANDROID
        public static bool isConnected = false;

        public static string usbDeviceName;
        public static int deviceId;
        public static string manufacturerName;
        public static string productName;

        public static UsbManager usbManager;
        public static UsbDevice usbDevice;
        public static UsbInterface usbDataInterface;
        public static UsbInterface usbControlInterface;
        public static UsbEndpoint usbEndpointOut;
        public static UsbEndpoint usbEndpointIn;
        public static UsbDeviceConnection usbDeviceConnection;

        const string ACTION_USB_PERMISSION = "com.companyname.mauiusbhost.USB_PERMISSION";

        public static Thread? readThread;
#endif


#if WINDOWS
        public Arduino()
        {
        }

        public static string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        public static bool IsConnected()
        {
            if (serialPort != null)
                if (serialPort.IsOpen)
                    return true;

            return false;
        }

        public static bool Connect()
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

        public static void Disconnect()
        {
            if (IsConnected())
            {
                serialPort.WriteLine("END");
                serialPort.DiscardInBuffer();
                serialPort.Close();
            }
        }
        
        public static void Write(string newLine)
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

#elif ANDROID
        public static string[] GetDeviceNames()
        {
            Android.App.Activity act = Platform.CurrentActivity;

            usbManager = (UsbManager)act.GetSystemService(Context.UsbService);

            IDictionary<string, UsbDevice> usbDevices = usbManager.DeviceList;

            if (usbDevices == null)
                return Array.Empty<string>();

            return usbDevices.Keys.ToArray();
        }

        public static bool IsConnected()
        {
            return isConnected;
        }
        
        public static bool Connect()
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

        public static void Disconnect()
        {
            if (usbDeviceConnection != null)
                if (usbDeviceConnection.ReleaseInterface(usbDataInterface))
                    usbDeviceConnection.Close();

            isConnected = false;

            readThread.Join();

            return;
        }

        public static void Write(string newLine)
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
                                    WeakReferenceMessenger.Default.Send(new AddArduinoLine(line));

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

#else
        public static bool IsConnected()
        {
            return false;
        }

        public static bool Connect()
        {
            return false;
        }

        public static void Disconnect()
        {
            return;
        }

        public static void Write(string data)
        {
            return;
        }
#endif

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
                    WeakReferenceMessenger.Default.Send(new NewChartSamplesMessage(chartsample));
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
                WeakReferenceMessenger.Default.Send(new NewChartSamplesMessage(chartsample));
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
                WeakReferenceMessenger.Default.Send(new NewChartSamplesMessage(chartsample));
            }
        }

    }
}
