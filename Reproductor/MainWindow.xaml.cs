using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
namespace Reproductor
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AudioFileReader reader;
        WaveOutEvent output;
        
        public MainWindow()
        {
            InitializeComponent();
            LlenarComboSalida();
        }

        private void LlenarComboSalida()
        {
            cbSalida.Items.Clear();
            for(int i =  0; i < WaveOut.DeviceCount; i++)
            {
                WaveOutCapabilities capcidades = WaveOut.GetCapabilities(i);
                cbSalida.Items.Add(capcidades.ProductName);
            }
            cbSalida.SelectedIndex = 0;
        }

        private void btnElegirArchivo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                txtRutaArchivo.Text = openFileDialog.FileName;
            }
        }

        private void btnReproducir_Click(object sender, RoutedEventArgs e)
        {
            if (output != null &&  output.PlaybackState == PlaybackState.Paused)
            {
                output.Play();
                btnDetener.IsEnabled = true;
                btnPausa.IsEnabled = true;
                btnReproducir.IsEnabled = false;

            }
            else
            {
                reader = new AudioFileReader(txtRutaArchivo.Text);
                output = new WaveOutEvent();

                output.DeviceNumber = cbSalida.SelectedIndex;

                output.PlaybackStopped += Output_PlaybackStopped;

                output.Init(reader);
                output.Play();

                btnDetener.IsEnabled = true;
                btnPausa.IsEnabled = true;
                btnReproducir.IsEnabled = false;

                lblTiempoTotal.Text = reader.TotalTime.ToString().Substring(0, 8);

            }
        }

        private void Output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            reader.Dispose();
            output.Dispose();

        }

        private void btnPausa_Click(object sender, RoutedEventArgs e)
        {
            if(output != null)
            {
                output.Pause();
                btnDetener.IsEnabled = true;
                btnPausa.IsEnabled = false;
                btnReproducir.IsEnabled = true;
            }
        }

        private void btnDetener_Click(object sender, RoutedEventArgs e)
        {
            if(output != null)
            {
                output.Stop();
                btnReproducir.IsEnabled = true;
                btnPausa.IsEnabled = false;
                btnDetener.IsEnabled = false;
            }
        }
    }
}
