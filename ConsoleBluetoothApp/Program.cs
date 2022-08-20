using ConsoleBluetoothApp;
using Windows.Devices.Enumeration;
internal class Program
{
    private static BluetoothManager bluetoothManager;
    private static string connected;
    private static void Main(string[] args){
        Console.WriteLine("Hello");
        Initial();
        while (true) {
            Console.WriteLine("Type 'c' to connect to a device");
            Console.WriteLine("Type 'rm' to remove the current device");
            Console.WriteLine("Type 'exit' to exit the application");
            var input = Console.ReadLine();
            if (input == "exit")
                return;
            else if (input == "rm")
            {
                if (bluetoothManager.connected)
                {
                    bluetoothManager.connected = false;
                    bluetoothManager.ReleaseAudioPlaybackConnection(connected);
                }
                else Console.WriteLine("Wasn't connected to a device.");
            }
            else if (input == "c")
            {
                Console.WriteLine("Select a device to listen to");
                PrintDevices();
                ChangeDevice();
            }
        }
    }
    private static void Initial() {
        bluetoothManager = new BluetoothManager();
        while ((bluetoothManager.devices.Count < 1)) {
            // Wait for devices list to get devices
            Console.WriteLine("Waiting...");
            Thread.Sleep(1000);
        }
        Console.WriteLine("Select a device to listen to");
        PrintDevices();
        Console.WriteLine("Type 'p' to refresh the list.");
        ChangeDevice();
    }
    public static void PrintDevices(){
        int i = 1;
        foreach (DeviceInformation device in bluetoothManager.devices)
        {
            Console.WriteLine(i + " " + device.Name);
            i++;
        }
    }
    private static void ChangeDevice(){
        if (bluetoothManager.connected) { 
            Console.WriteLine("Please disconnect from the current device first"); 
            return; 
        }
        var input = Console.ReadLine();
        if (input == "p") {
            PrintDevices();
            Console.WriteLine("Type 'p' to refresh the list.");
            ChangeDevice();
        }
        try
        {
            DeviceInformation selected = SelectDevice(bluetoothManager, input);
            //Console.WriteLine(bluetoothManager.devices.Count);
            var enabled = bluetoothManager.EnableAudioPlaybackConnection(selected.Id);
            var opened = bluetoothManager.OpenAudioPlaybackConnection(selected.Id);
            //Console.WriteLine("Enabled " + enabled);
            bluetoothManager.connected = enabled && opened;
            //Console.WriteLine("Device ID " + selected.Id);
            Console.WriteLine("Device Name " + selected.Name);
            if (bluetoothManager.connected)
            {
                Console.WriteLine("Connection Established");
                connected = selected.Id;
            }
            else Console.WriteLine("Connection Failed");
        } catch(Exception e)
        {
            Console.WriteLine("Invalid Input, please type a number from the list");
            PrintDevices();
            Console.WriteLine("Type 'p' to refresh the list.");
            ChangeDevice();
        }
    }
    private static DeviceInformation SelectDevice(BluetoothManager bluetoothManager, string line) {
        return bluetoothManager.devices[int.Parse(line)-1];
    }
}