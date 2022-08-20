using System.Collections.ObjectModel;
using System.Data;
using Windows.Devices.Enumeration;
using Windows.Media.Audio;

namespace ConsoleBluetoothApp
{
    internal class BluetoothManager {
        private Dictionary<string, AudioPlaybackConnection> audioInputDevices;
        private DeviceWatcher deviceWatcher;
        public bool connected = false;
        public List<Windows.Devices.Enumeration.DeviceInformation> devices = new List<Windows.Devices.Enumeration.DeviceInformation>();
        public BluetoothManager(){
            audioInputDevices = new Dictionary<string, AudioPlaybackConnection>();
            var e = AudioPlaybackConnection.GetDeviceSelector();
//            Console.WriteLine(e);
            deviceWatcher = DeviceInformation.CreateWatcher(AudioPlaybackConnection.GetDeviceSelector());
            deviceWatcher.Added += OnDeviceAdded;
            deviceWatcher.Removed += OnDeviceRemoved;
            deviceWatcher.Start();

        }

        public bool EnableAudioPlaybackConnection(string selectedDeviceId) {
            if (!this.audioInputDevices.ContainsKey(selectedDeviceId)){
                // Create the audio playback connection from the selected device id and add it to the dictionary. 
                // This will result in allowing incoming connections from the remote device. 
                AudioPlaybackConnection playbackConnection = AudioPlaybackConnection.TryCreateFromId(selectedDeviceId);

                if (playbackConnection != null){
                    // The device has an available audio playback connection. 
                    playbackConnection.StateChanged += this.onStateChanged;
                    this.audioInputDevices.Add(selectedDeviceId, playbackConnection);
                    playbackConnection.Start();
                    return true;
                }
            }
            return false;
        }

        public bool OpenAudioPlaybackConnection(string selectedDevice) {
            AudioPlaybackConnection selectedConnection;
            try
            {

                if (this.audioInputDevices.TryGetValue(selectedDevice, out selectedConnection))
                {
                    if (selectedConnection.Open().Status == AudioPlaybackConnectionOpenResultStatus.Success)
                    {
                        // Notify that the AudioPlaybackConnection is connected. 
                        return true;
                    }

                    else
                    {
                        // Notify that the connection attempt did not succeed. 
                        return false;
                    }
                }
                else return false;
            }
            catch (Exception e) {
                Console.WriteLine("Failed to connect to device "+ selectedDevice);
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public bool ReleaseAudioPlaybackConnection(string selectedDevice) {
            if (audioInputDevices.ContainsKey(selectedDevice)){
                AudioPlaybackConnection connectionToRemove = audioInputDevices[selectedDevice];
                connectionToRemove.Dispose();
                this.audioInputDevices.Remove(selectedDevice);

                // Notify that the media device has been deactivated. 
                Console.WriteLine("Disconnected");
                return true;
            }
            return false;
        }




        private void onStateChanged(AudioPlaybackConnection sender, Object e){
            if (sender.State == AudioPlaybackConnectionState.Closed)
            {
                Console.WriteLine("Disconnected");
            }
            else if (sender.State == AudioPlaybackConnectionState.Opened)
            {
                Console.WriteLine("Connected");
            }
            else
            {
                Console.WriteLine("Unexpected Connection State");
            }
        }
        /// <summary>
        /// Open the device that the user wanted to open if it hasn't been opened yet and auto reconnect is enabled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInfo"></param>
        private async void OnDeviceAdded(DeviceWatcher sender, DeviceInformation deviceInfo) {
            devices.Add(deviceInfo);

        }
        /// <summary>
        /// Close the device that is opened so that all pending operations are canceled properly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInformationUpdate"></param>
        private async void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceInformationUpdate) {
                // Find the device for the given id and remove it from the list. 
                foreach (DeviceInformation device in this.devices)
                {
                    if (device.Id == deviceInformationUpdate.Id)
                    {
                        this.devices.Remove(device);
                        break;
                    }
                }

                if (audioInputDevices.ContainsKey(deviceInformationUpdate.Id))
                {
                    AudioPlaybackConnection connectionToRemove = audioInputDevices[deviceInformationUpdate.Id];
                    connectionToRemove.Dispose();
                    this.audioInputDevices.Remove(deviceInformationUpdate.Id);
                }
        }
    }
}
