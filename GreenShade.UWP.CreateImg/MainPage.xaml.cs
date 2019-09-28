using GreenShade.UWP.CreateImg.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace GreenShade.UWP.CreateImg
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private WebSocketCamera mCamera;
        public MainPage()
        {
            this.InitializeComponent();
            mCamera = new WebSocketCamera();
            RegisterKeyEvents();
        }
        private void RegisterKeyEvents()
        {
            //http://stackoverflow.com/questions/32781864/get-keyboard-state-in-universal-windows-apps
            Window.Current.CoreWindow.KeyDown += (s, e) =>
            {
              
                if (e.VirtualKey == VirtualKey.M)
                {
                    mCamera?.StartCapture();
                }
                if (e.VirtualKey == VirtualKey.N)
                {
                    mCamera?.StopCapture();
                }
            };
        }
        internal static string GetUniqueDeviceId()
        {
            var deviceInformation = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
            return deviceInformation.Id.ToString();
        }
    }
}
