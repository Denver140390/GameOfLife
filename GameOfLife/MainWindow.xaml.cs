using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        private Int32Rect _sourceRect = new Int32Rect(0, 0, CELLS_IN_ROW, CELLS_IN_COLUMN);
        private int _sourceBufferStride = 0;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            Bitmap = new WriteableBitmap(CELLS_IN_ROW, CELLS_IN_COLUMN, 96, 96, PixelFormats.Gray8, BitmapPalettes.Gray256);
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
            //bytes = new byte[] {0, 0, 0, 255, 255, 0, 0, 255, 0};
            FillBitmap(bytes);

            //Run the game
            while (true)
            {
                //await Task.Delay(10);


                await Task.Run(() => RequestNextGeneration(bytes));
                FillBitmap(bytes);
            }
        }

        private void RequestNextGeneration(byte[] bytes)
        {
            //Transform byte values from 255 to 1
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] /= 255;
            }

            //Transform byte[] into string[]
            string postData = "[";
            for (int i = 0; i < CELLS_IN_COLUMN; i++)
            {
                postData += "[";
                for (int j = 0; j < CELLS_IN_ROW; j++)
                {
                    var b = bytes[j + i * CELLS_IN_ROW];
                    postData += j == CELLS_IN_ROW - 1 ? b + "" : b + ",";

                }
                postData += i == CELLS_IN_COLUMN - 1 ? "]" : "],";
            }
            postData += "]";

            //Request next generation
            byte[,] nextGen = wsc.Request(postData);

            //Uncomment this to see a random behaviour without using web service (to test speed)
//                        byte[,] nextGen = new byte[CELLS_IN_ROW, CELLS_IN_COLUMN];
//                        for (int i = 0; i < CELLS_IN_ROW; i++)
//                        {
//                            for (int j = 0; j < CELLS_IN_COLUMN; j++)
//                            {
//                                nextGen[i, j] = (byte)_random.Next(0, 2);
//                            }
//                        }
            
            //Transform byte values from 1 to 255
            for (int i = 0; i < CELLS_IN_ROW; i++)
            {
                for (int j = 0; j < CELLS_IN_COLUMN; j++)
                {
                    nextGen[i, j] *= 255;
                }
            }

            //Transform byte[,] into byte[]
            nextGen.Cast<byte>().ToArray().CopyTo(bytes, 0);
        }

        private void FillBitmap(byte[] sourceBytes)
        {
            //sourceBytes = new byte[] {0, 255, 0,  255, 0, 255, 0,  255, 0};
            Bitmap.WritePixels(_sourceRect, sourceBytes, _sourceBufferStride, 0);
//            OnPropertyChanged(nameof(Bitmap));
//            UpdateLayout();

//            sourceBytes = new byte[] {0, 255, 0, 0, 255, 0, 255, 0, 0, 255, 0, 0};
//            Bitmap.Lock();
//            Marshal.Copy(sourceBytes, 0, Bitmap.BackBuffer, sourceBytes.Length);
//            Bitmap.AddDirtyRect(_sourceRect);
//            Bitmap.Unlock();
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
