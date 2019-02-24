using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AudioRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<MMDevice> _mmDevices;

        private IWaveIn capture;
        private WaveFileWriter writer;

        private bool _isRecording = false;

        public MainWindow()
        {
            InitializeComponent();

            this._mmDevices = GetMMDevices().ToList();

            DeviceSelectCombobox.ItemsSource = _mmDevices.ToDictionary(d => d.ID, d => d.FriendlyName);
            DeviceSelectCombobox.DisplayMemberPath = "Value";
            DeviceSelectCombobox.SelectedValuePath = "Key";
        }

        private async void StartStopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            _isRecording = !_isRecording;

            if (_isRecording)
            {
                StartStopRecordingButton.Content = "Stop";
                StartRecording();
            }
            else
            {
                StartStopRecordingButton.Content = "Start";
                StopRecording();
            }
        }

        private void StartRecording()
        {
            var selectedDevice = this._mmDevices.Single(d => d.ID == (string)DeviceSelectCombobox.SelectedValue);

            capture = new WasapiLoopbackCapture(selectedDevice);

            capture.RecordingStopped += (s, a) =>
            {
                writer.Dispose();
                writer = null;
                capture.Dispose();
                capture = null;
            };

            capture.DataAvailable += (s, waveInEventArgs) =>
            {
                if (writer == null)
                {
                    writer = new WaveFileWriter("test.wav", capture.WaveFormat);
                }

                writer.Write(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
            };

            capture.StartRecording();
        }

        private void StopRecording()
        {
            capture?.StopRecording();
        }

        private IEnumerable<MMDevice> GetMMDevices()
        {
            var enumerator = new MMDeviceEnumerator();

            foreach (var mmDevice in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                yield return mmDevice;
            }
        }
    }
}
