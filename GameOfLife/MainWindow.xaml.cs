using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

        private WebServiceController wsc = new WebServiceController();

        private const int CELLS_IN_ROW = 10;
        private const int CELLS_IN_COLUMN = 10;
        private const int CELLS_AMOUNT = CELLS_IN_COLUMN * CELLS_IN_ROW;
        private Int32Rect sourceRect = new Int32Rect(0, 0, CELLS_IN_ROW, CELLS_IN_COLUMN);
        private int _sourceBufferStride = 0;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            Bitmap = new WriteableBitmap(CELLS_IN_ROW, CELLS_IN_COLUMN, 96, 96, PixelFormats.Gray8, BitmapPalettes.BlackAndWhite);
            RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);
            _sourceBufferStride = CELLS_IN_ROW * Bitmap.Format.BitsPerPixel / 8;

            RunGame();
        }

        private async void RunGame()
        {
            //Generate first generation
            var bytes = new byte[CELLS_AMOUNT];
            for (int i = 0; i < CELLS_AMOUNT; i++)
            {
                bytes[i] = (byte)(_random.Next(0, 2) * 255);
            }
            FillBitmap(bytes);

            //Run the game
            while (true)
            {
                bytes = RequestNextGeneration(bytes);
                FillBitmap(bytes);
                await Task.Delay(10);
            }
        }

        private byte[] RequestNextGeneration(byte[] bytes)
        {
            //Transform byte[] into string[] and add row number in the beginning of each string
            var cellStrings = new string[CELLS_IN_COLUMN]; //Row example: 50011001100
            for (int i = 0; i < CELLS_IN_COLUMN; i++)
            {
                cellStrings[i] = i.ToString();
                for (int j = 0; j < CELLS_IN_ROW; j++)
                {
                    int index = j + i * CELLS_IN_ROW;
                    cellStrings[i] += bytes[index] / 255;
                }
            }

            //Request next generation
            byte[,] nextGen = wsc.Request(cellStrings);

            //Uncomment this to see a random behaviour without using web service
//            byte[,] nextGen = new byte[CELLS_IN_ROW, CELLS_IN_COLUMN + 1];
//            for (int i = 0; i < CELLS_IN_ROW; i++)
//            {
//                for (int j = 0; j < CELLS_IN_COLUMN + 1; j++)
//                {
//                    nextGen[i, j] = (byte)(_random.Next(0, 2)*255);
//                }
//            }

            //Transform byte[,] into byte[] and remove first element from every row
            List<byte> nextList = nextGen.Cast<byte>().ToList();
            for (int i = 0; i < CELLS_IN_ROW; i++)
            {
                nextList.RemoveAt(i * CELLS_IN_ROW);
            }

            return nextList.Cast<byte>().ToArray();
        }

        private void FillBitmap(byte[] sourceBytes)
        {
            Bitmap.WritePixels(sourceRect, sourceBytes, _sourceBufferStride, 0);
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
