namespace Biosensor_pH___MAUI;

using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Maui.Controls;
using System.Diagnostics;

public class AddArduinoLine : ValueChangedMessage<string>
{
    public AddArduinoLine(string text) : base(text)
    {
    }
}

public partial class ConsolePage : ContentPage
{
    public ConsolePage()
	{
		InitializeComponent();

        RefreshPortNames();

        WeakReferenceMessenger.Default.Register<AddArduinoLine>(this, (recipoent, message) =>
        {
            if (MainThread.IsMainThread)
                ArduinoWrite(message.Value);
            else
                MainThread.BeginInvokeOnMainThread(() => ArduinoWrite(message.Value)); // System.InvalidOperationException: „Unable to find main thread.”

        });
    }

    public void EditorWrite(string writeLine)
    {
        // System.Runtime.InteropServices.COMException: „”
        try
        {
            EditorOutput.Text += writeLine + "\n";
        }
        catch(System.Runtime.InteropServices.COMException e)
        {
            Debug.WriteLine(e.Message);
        }
    }

    public void ArduinoWrite(string writeLine) 
    {
        string command = writeLine;

        command.Trim();

        EditorWrite("[Arduino] " + command);
    }

    private void UserWrite()
    {
        string command = EntryCommand.Text;

        command.Trim();

        EntryCommand.Text = string.Empty;

        if (command != string.Empty)
        {
            Arduino.Write(command);
            EditorWrite("[User] " + command);
        }
    }

    private void EntryCommand_Completed(object sender, EventArgs e)
    {
        UserWrite();
    }

    private void ButtonSend_Clicked(object? sender, EventArgs e)
    {
        UserWrite();
    }

    private void ButtonClear_Clicked(object sender, EventArgs e)
    {
        EditorOutput.Text = string.Empty;
    }

    private void ButtonConnectDisconnect_Clicked(object sender, EventArgs e)
    {
        if (Arduino.IsConnected())
        {
            Arduino.Disconnect();
            EditorWrite("Roz³¹czono.");

            ButtonConnectDisconnect.Text = "Po³¹cz";
            EntryCommand.IsEnabled = false;
            ButtonSend.IsEnabled = false;
            PickerPortName.IsEnabled = true;
            ButtonRefreshPortNames.IsEnabled = true;
        }
        else
        {
            if (Arduino.Connect())
            {
#if WINDOWS
                EditorWrite("Po³¹czono z " +  Arduino.portName + ".");
#elif ANDROID
                EditorWrite("Po³¹czono z " + Arduino.productName + ".");

#endif
                ButtonConnectDisconnect.Text = "Roz³¹cz";
                EntryCommand.IsEnabled = true;
                ButtonSend.IsEnabled = true;
                PickerPortName.IsEnabled = false;
                ButtonRefreshPortNames.IsEnabled = false;
            }
            else
                EditorWrite("Nie uda³o siê po³¹czyæ z urz¹dzeniem");

        }
    }

    private void ButtonRefreshPortNames_Clicked(object sender, EventArgs e)
    {
        RefreshPortNames();
    }

    private void RefreshPortNames()
    {
#if WINDOWS
        PickerPortName.ItemsSource = Arduino.GetPortNames();
#elif ANDROID
        PickerPortName.ItemsSource = Arduino.GetDeviceNames();
#endif
        if (PickerPortName.ItemsSource.Count > 0)
            PickerPortName.SelectedIndex = 0;
    }

    private void PickerPortName_SelectedIndexChanged(object sender, EventArgs e)
    {
#if WINDOWS
        Arduino.portName = (string)PickerPortName.SelectedItem;
#elif ANDROID
        Arduino.usbDeviceName = (string)PickerPortName.SelectedItem;
#endif
    }
}