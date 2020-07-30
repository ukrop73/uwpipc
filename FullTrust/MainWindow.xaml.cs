using System;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;
using System.Windows.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace FullTrust
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppServiceConnection connection = null;

        private PipeClient pipeClient;

        public MainWindow()
        {
            InitializeComponent();
            pipeClient = new PipeClient();
        }

        /// <summary>
        /// Open connection to UWP app service
        /// </summary>
        private async void InitializeAppServiceConnection()
        {
            connection = new AppServiceConnection();
            connection.AppServiceName = "SampleInteropService";
            connection.PackageFamilyName = Package.Current.Id.FamilyName;
            //MessageBox.Show(Package.Current.Id.FamilyName);
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;

            AppServiceConnectionStatus status = await connection.OpenAsync();
            if (status != AppServiceConnectionStatus.Success)
            {
                // something went wrong ...
                MessageBox.Show(status.ToString());
                this.IsEnabled = false;
            }
            else
            {
                MessageBox.Show("FullTrust::Connected");
            }
        }

        /// <summary>
        /// Handles the event when the app service connection is closed
        /// </summary>
        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            MessageBox.Show("Connection_ServiceClosed");
            // connection to the UWP lost, so we shut down the desktop process
            //Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            //{
            //    Application.Current.Shutdown();
            //}));
        }

        /// <summary>
        /// Handles the event when the desktop process receives a request from the UWP app
        /// </summary>
        private async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // Get a deferral so we can use an awaitable API to respond to the message
            var messageDeferral = args.GetDeferral();

            // retrive the reg key name from the ValueSet in the request
            string key = args.Request.Message["KEY"] as string;


            ValueSet response = new ValueSet();
                response.Add("ERROR", "INVALID REQUEST");
                await args.Request.SendResponseAsync(response);
                messageDeferral.Complete();

            Task t = new Task(() => {
                pipeClient.start(key);
            });
            t.Start();
        }

        /// <summary>
        /// Sends a request to the UWP app
        /// </summary>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            tbResult.Text = "Sending...";

            // ask the UWP to calculate d1 + d2
            ValueSet request = new ValueSet();
            request.Add("D1", tb1.Text);
            request.Add("D2", tb2.Text);
            try {
                AppServiceResponse response = await connection.SendMessageAsync(request);
                tbResult.Text = "Response = " + response + "<<<";
                MessageBox.Show("response.Message" + response.Message.Keys.Count);
                if (response != null && response.Message.ContainsKey("RESULT"))
                {
                    var result = response.Message["RESULT"];
                    tbResult.Text = result.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("error" + ex.Message);
            }
            //tbResult.Text = "Sent";
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            InitializeAppServiceConnection();
        }
    }
}
