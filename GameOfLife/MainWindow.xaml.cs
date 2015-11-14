using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GameOfLife.Annotations;

namespace GameOfLife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private WriteableBitmap _bitmap;
        private Random _random = new Random();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            Bitmap = new WriteableBitmap(10, 10, 96, 96, PixelFormats.Gray8, BitmapPalettes.BlackAndWhite);
            RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);

            CalculateFirstGeneration();
        }

        public void CalculateFirstGeneration()
        {
            var bytes = new byte[100];
            for (int i = 0; i < 100; i++)
            {
                bytes[i] = (byte)(_random.Next(0, 256) / 128 * 255);
            }
            FillBitmap(bytes);
        }

        public void FillBitmap(byte[] sourceBytes)
        {
            Int32Rect sourceRect = new Int32Rect(0, 0, 10, 10);
            int sourceBufferStride = 10 * Bitmap.Format.BitsPerPixel / 8;

            Bitmap.WritePixels(sourceRect, sourceBytes, sourceBufferStride, 0);
        }

        public WriteableBitmap Bitmap
        {
            get { return _bitmap; }
            set
            {
                _bitmap = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
