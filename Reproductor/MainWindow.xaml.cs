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

using System.Windows.Threading;

namespace Reproductor
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AudioFileReader reader;
        WaveOutEvent output;
        //Hilo
        DispatcherTimer timer;
        VolumeSampleProvider volume;
        FadeInOutSampleProvider fades;

        bool fadingOut = false; 

        bool dragging = false;
        

        public MainWindow()
        {
            InitializeComponent();
            LlenarComboSalida();
            //Inicializar Timer
            //Establecer tiempo entre ejcuciones
            //Establcer lo que se va a ejecutar
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if(reader != null)
            {
                lblTiempoActual.Text = reader.CurrentTime.ToString().Substring(0, 8);
                sldRepro.Value = reader.CurrentTime.TotalSeconds;
            }
            if(!dragging)
            {
                sldRepro.Value = reader.CurrentTime.TotalSeconds;
            }
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
                fades = new FadeInOutSampleProvider(reader, true);
                double milisegundosFadein = Double.Parse(txtFadeIn.Text) * 1000.0;
                fades.BeginFadeIn(milisegundosFadein);
                output = new WaveOutEvent();

                fadingOut = false; 

                output.DeviceNumber = cbSalida.SelectedIndex;

                output.PlaybackStopped += Output_PlaybackStopped;

                volume = new VolumeSampleProvider(fades);

                volume.Volume = (float)sldVolumen.Value;

                output.Init(volume);
                output.Play();

                btnDetener.IsEnabled = true;
                btnPausa.IsEnabled = true;
                btnReproducir.IsEnabled = false;

                lblTiempoTotal.Text = reader.TotalTime.ToString().Substring(0, 8);
                lblTiempoActual.Text = reader.CurrentTime.ToString().Substring(0, 8);
                sldRepro.Maximum = reader.TotalTime.TotalSeconds;
                sldRepro.Value = reader.CurrentTime.TotalSeconds;

                timer.Start();
            }
        }

        private void Output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            reader.Dispose();
            output.Dispose();
            timer.Stop();
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

        private void sldRepro_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            dragging = true;
        }

        private void sldRepro_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            dragging = false;
            if(reader != null && output != null && (output.PlaybackState != PlaybackState.Stopped))
            {
                reader.CurrentTime = TimeSpan.FromSeconds(sldRepro.Value);
            }
        }

        private void sldVolumen_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(volume != null && output != null && output.PlaybackState != PlaybackState.Stopped)
            {
                volume.Volume = (float)sldVolumen.Value;
                
            }
            if(lblVolumen != null)
            {
                lblVolumen.Text = ((int)(sldVolumen.Value * 100)).ToString() + " %";
            }
                    }

        private void btnFadeOut_Click(object sender, RoutedEventArgs e)
        {
        
            if(!fadingOut && fades != null && output != null && output.PlaybackState == PlaybackState.Playing)
            {
                fadingOut = true;
                double milisegundosFadeOut = Double.Parse(txtDuracionFadeOut.Text) * 1000.0;
                fades.BeginFadeOut(milisegundosFadeOut);

            }
        }
    }
}
