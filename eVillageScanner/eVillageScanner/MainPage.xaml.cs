using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace eVillageScanner
{
    public partial class MainPage : ContentPage
    {
        private static readonly bool isLocal = true;
        private static readonly string ApiUrl = isLocal
            ? "http://192.168.1.201:5169"
            : "http://192.168.1.201:5169";

        private ZXingScannerView _scannerView;
        private bool isProcessing = false;

        public class CheckInResponse
        {
            public string status { get; set; }
            public string message { get; set; }
        }

        public MainPage()
        {
            InitializeComponent();
            CreateScanner();
            DeviceDisplay.MainDisplayInfoChanged += (s, e) =>
            {
                DeviceDisplay.KeepScreenOn = true;
            };
        }

        private void OnScanResult(ZXing.Result result)
        {
            if (isProcessing) return;

            Device.BeginInvokeOnMainThread(async () =>
            {
                isProcessing = true;
                ShowLoading(true);
                DisableScanner();

                if (!string.IsNullOrWhiteSpace(result?.Text))
                {
                    await ScannerCheckIn(result.Text);
                }
                
                CreateScanner();
                ShowLoading(false);
                isProcessing = false;
            });
        }

        private async Task ScannerCheckIn(string orderId)
        {
            try
            {
                var client = new HttpClientHandlerService().GetInsecureHttpClient();
                var response = JsonConvert.DeserializeObject<CheckInResponse>(
                    await client.GetStringAsync($"{ApiUrl}/order/checkIn?orderId={orderId}")
                );

                await DisplayAlert("Status", response.message, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void ShowLoading(bool show)
        {
            loadingIndicator.IsVisible = show;
            loadingIndicator.IsRunning = show;
        }

        private void DisableScanner()
        {            
            _scannerView.IsAnalyzing = false;
            _scannerView.IsEnabled = false;
            _scannerView.IsScanning = false;
        }

        private void CreateScanner()
        {
            var scannerView = new ZXingScannerView
            {                
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                IsAnalyzing = true,                
                IsEnabled = true,
                IsScanning = true
            };

            var frame = scannerFrame;
            frame.Content = scannerView;

            scannerView.OnScanResult += OnScanResult;

            _scannerView = scannerView;
        }
    }
}
