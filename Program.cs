using System;
using System.IO.Ports; // Add this for SerialPort
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using SharpDX.XInput;

class Program
{
    // Define SerialPort globally
    static SerialPort serialPort;

    static void Main(string[] args)
    {
        // Initialize Xbox controller
        var controller = new Controller(UserIndex.One);
        if (!controller.IsConnected)
        {
            Console.WriteLine("No Xbox controller found.");
            return;
        }

        Console.WriteLine("Xbox controller connected.");

        // Initialize SerialPort
        InitializeSerialPort();

        while (true)
        {
            var state = controller.GetState();
            if (state.Gamepad.Buttons != GamepadButtonFlags.None)
            {
                HandleControllerInput(state.Gamepad.Buttons);
            }

            // Check for exit condition (e.g., press 'Q' to quit)
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
            {
                break;
            }
        }

        // Close SerialPort when done
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    static bool portCheck;
    static void InitializeSerialPort()
    {
        Console.WriteLine("Available COM Ports:");
        foreach (var port in SerialPort.GetPortNames())
        {
            Console.WriteLine(port);
        }

        Console.Write("Enter COM port to connect (e.g., COM1): ");
        string selectedPort = Console.ReadLine().Trim(); // Rename to selectedPort

        serialPort = new SerialPort(selectedPort, 9600); // Adjust baud rate as necessary
        serialPort.DataReceived += RecieveHandler;
        serialPort.Open();
        Console.WriteLine($"Serial port {selectedPort} opened. Checking if valid printer...");
        portCheck = true;
        SendCommandToSerialPort("M115");
    }

    static void RecieveHandler(object sender, SerialDataReceivedEventArgs a)
    {
        SerialPort sp = (SerialPort)sender;
        string data = sp.ReadExisting().Trim();
        if (portCheck)
        {
            
            if (!data.StartsWith("FIRMWARE_VERSION:Marlin") || data == null){
                serialPort.Close();
                portCheck = false;
                Console.WriteLine("That device is not a valid Marlin FDM printer. Please select another port!");
                InitializeSerialPort();
                return;
                
            }
            else
            {
                portCheck = false;
                return;
            }
        }
    }
    static void HandleControllerInput(GamepadButtonFlags buttonFlags)
    {
        string command = "";

        switch (buttonFlags)
        {
            case GamepadButtonFlags.A:
                command = "Extruding";
                break;
            case GamepadButtonFlags.B:
                command = "Retracting";
                break;
            case GamepadButtonFlags.X:
                command = "Heating";
                Thread.Sleep(200);
                break;
            case GamepadButtonFlags.Y:
                command = "Homing Printer";
                Thread.Sleep(200);
                break;
            case GamepadButtonFlags.Start:
                command = "Beeping";
                break;
            case GamepadButtonFlags.Back:
                command = "Levelling";
                Thread.Sleep(200);
                break;
            case GamepadButtonFlags.DPadUp:
                command = "Moving Head Up";
                break;
            case GamepadButtonFlags.DPadDown:
                command = "Moving Head Down";
                break;
            case GamepadButtonFlags.DPadLeft:
                command = "Moving Head Left";
                break;
            case GamepadButtonFlags.DPadRight:
                command = "Moving Head Right";
                break;
            case GamepadButtonFlags.RightShoulder:
                command = "Moving Plate Forward";
                break;
            case GamepadButtonFlags.LeftShoulder:
                command = "Moving Plate Backward";
                break;
        }

        if (!string.IsNullOrEmpty(command))
        {
            Console.WriteLine(command);
            SendCommandToSerialPort(command);
        }
    }

    static void SendCommandToSerialPort(string command)
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.WriteLine(command);
        }
        else
        {
            Console.WriteLine("Serial port is not open.");
        }
    }
}
