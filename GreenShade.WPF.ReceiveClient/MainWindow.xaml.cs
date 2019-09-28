
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GreenShade.WPF.ReceiveClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,INotifyPropertyChanged
    {
        public delegate void BitmapAquiredEventHandler(object sender, System.Windows.Media.Imaging.BitmapSource e);
        public event BitmapAquiredEventHandler BitmapAquired;
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            CancellationTokenSource cts = new CancellationTokenSource();
            System.Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            var tasks = new List<Task>();
            tasks.Add(receiveWebSocketMessagesFromDeviceAsync(cts.Token));
        }


        private async Task receiveWebSocketMessagesFromDeviceAsync(CancellationToken ct)
        {
            try
            {
                string wsUri = $"{Globals.WEBSOCKET_ENDPOINT}?device={Globals.DEVICE_ID}";
                var socket = new ClientWebSocket();
                await socket.ConnectAsync(new Uri(wsUri), ct);

                while (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        if (ct.IsCancellationRequested) break;

                        var buffer = new ArraySegment<Byte>(new Byte[40960]);
                        WebSocketReceiveResult rcvResult = await socket.ReceiveAsync(buffer, ct);
                        string b64 = String.Empty;
                        if (rcvResult.MessageType == WebSocketMessageType.Binary)
                        {
                            List<byte> data = new List<byte>(buffer.Take(rcvResult.Count));
                            while (rcvResult.EndOfMessage == false)
                            {
                                rcvResult = await socket.ReceiveAsync(buffer, CancellationToken.None);
                                data.AddRange(buffer.Take(rcvResult.Count));
                            }

                            MemoryStream ms = new MemoryStream(data.ToArray());

                            var image = System.Drawing.Image.FromStream(ms);
                            var oldBitmap = new Bitmap(image);
                            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                        oldBitmap.GetHbitmap(System.Drawing.Color.Transparent),
                                        IntPtr.Zero,
                                        new Int32Rect(0, 0, oldBitmap.Width, oldBitmap.Height),
                                        null);

                            var del = BitmapAquired;
                            if (del != null)
                            {
                                del(this, bitmapSource);
                            }
                            //var picturespath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                           // image.Save(System.IO.Path.Combine(picturespath, "lastimagefromrover.jpg"));
                           
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private BitmapSource mRemoteBitmap;
        public BitmapSource RemoteBitmap
        {
            get { return mRemoteBitmap; }
            set {
                mRemoteBitmap = value;
                NotifyPropertyChanged("RemoteBitmap");
            }
        }
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
               BitmapAquired += EventsReader_BitmapAquired;                
        }
        private void EventsReader_BitmapAquired(object sender, BitmapSource e)
        {
            RemoteBitmap = e;
        }
    }

}
