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

using NAudio;
using NAudio.Wave;
using NAudio.Dsp;

namespace Microfono
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WaveIn waveIn; //Conexion con microfono

        WaveFormat formato; //Formato de audio

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnIniciar_Click(object sender, RoutedEventArgs e)
        {
            //Inicializar la conexion
            waveIn = new WaveIn();
            // Establecer el formato
            waveIn.WaveFormat = new WaveFormat(44100, 16, 1);
            formato = waveIn.WaveFormat;
            // Duracion del buffer
            waveIn.BufferMilliseconds = 500;
            //Con que funcion respondemos
            //Cuando se llena el buffer
            waveIn.DataAvailable += WaveIn_DataAvailable;

            waveIn.StartRecording();

            
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;

            int bytesGrabados = e.BytesRecorded;

            int numMuestras = bytesGrabados / 2;

            int exponente = 1;

            int numeroBits = 0;

            do
            {
                exponente++;
                numeroBits = (int)Math.Pow(2, exponente);
            }
            while (numeroBits < numMuestras);

            exponente -= 1;

            numeroBits = (int)Math.Pow(2, exponente);

            Complex[] muestrasComplejas = new Complex[numeroBits];

            //Vamo a analiza cada bait grabado mi negro
            for (int i = 0; i < bytesGrabados; i += 2)
            {
                short muestra = (short)(buffer[i + 1] << 8 | buffer[i]);
                float muestra32bits = (float)muestra / 32768.0f;

                if(i/2 < numeroBits)
                {
                    muestrasComplejas[i / 2].X = muestra32bits;
                }

            }
            FastFourierTransform.FFT(true, exponente, muestrasComplejas);

            float[] ValoresAbsolutos = new float[muestrasComplejas.Length];

            for(int i=0; i < muestrasComplejas.Length; i++)
            {
                ValoresAbsolutos[i] = (float)Math.Sqrt((muestrasComplejas[i].X * muestrasComplejas[i].Y) + (muestrasComplejas[i].X * muestrasComplejas[i].Y));
            }

            int indiceValorMaximo = ValoresAbsolutos.ToList().IndexOf(ValoresAbsolutos.Max());

            float frecuenciaFundamental = (float)(indiceValorMaximo * formato.SampleRate) / (float)ValoresAbsolutos.Length;

            LblHertz.Text = frecuenciaFundamental.ToString("N") + "Hz";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            waveIn.StopRecording();
        }
    }
}
