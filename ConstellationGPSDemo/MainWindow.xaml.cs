using System;
using System.Windows;
using System.IO.Ports;
using System.Windows.Threading;
using System.Collections.ObjectModel;

using NSConstellationGPS;
using NSConstellationGPS.GPS_Sentences;

namespace NSConstellationGPSDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ConstellationGPSDemoWindow : Window
    {
        // These are the default ports as provided by National Instruments
        private int[] _baudrates = new int[] { 110, 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 56000, 57600, 115200 };

        // Main GPS Device Interface
        private ConstellationGPS GPS;

        // Update Timer
        private DispatcherTimer timer;

        // Visualization Collections
        private ObservableCollection<GPS_Sentence_GPRMC> Collection_GPRMC = new ObservableCollection<GPS_Sentence_GPRMC>();
        private ObservableCollection<GPS_Sentence_GPGGA> Collection_GPGGA = new ObservableCollection<GPS_Sentence_GPGGA>();
        private ObservableCollection<GPS_Sentence_GPGSA> Collection_GPGSA = new ObservableCollection<GPS_Sentence_GPGSA>();

        public ConstellationGPSDemoWindow()
        {
            InitializeComponent();
            
            // Setup UI 
            cb_Port.ItemsSource = SerialPort.GetPortNames();
            cb_Baud.ItemsSource = _baudrates;
            dataGrid.ItemsSource = Collection_GPRMC;

            // Init Timer
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            timer.Tick += new EventHandler(timer_tick);
        }

        private void timer_tick(object obj, EventArgs sender)
        {
            ErrorCode ret = GPS.Update();

            // Update GPS
            if (ret == ErrorCode.None)
            {
                // Only Populate the collections you wish
                Collection_GPRMC.Add(GPS.getGPRMC());
                Collection_GPGGA.Add(GPS.getGPGGA());
                Collection_GPGSA.Add(GPS.getGPGSA());

                // UI Scroll to most recent item
                dataGrid.ScrollIntoView(dataGrid.Items[dataGrid.Items.Count - 1]);
            }
            else
            {
                // ERROR
                ErrorCatch(ret);
            }
        }

        /// <summary>
        /// This event fires on the Open button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Do not open an already open port, close it first to avoid issues
                if (GPS != null)
                    GPS.Close();

                // Reconfigure Port as UI specifies
                GPS = new ConstellationGPS(cb_Port.SelectedItem.ToString(), Convert.ToInt32(cb_Baud.SelectedValue));

                // Open new port
                GPS.Open();

                timer.Start();

                Write_to_OutputWindow("Processing Started...\n");

            }
            catch(Exception ex)
            {
                ErrorCatch(ex);
                timer.Stop();
            }
        }

        /// <summary>
        /// This event fires on the Close button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            // Try closing the port only if it is already open
            try
            {
                if (GPS != null)
                    GPS.Close();
            }
            catch(Exception ex)
            {
                ErrorCatch(ex);
            }

            timer.Stop();

            Write_to_OutputWindow("...Processing Halted\n");
        }

        private void ErrorCatch(Exception ex)
        {
#if DEBUG
            // Debug Mode Error Handling
            // Append message to error window for testing
            Write_to_OutputWindow("\n" + ex.Message);
#else
            // Release Mode Error Handling
            // TBD
#endif
        }

        private void ErrorCatch(ErrorCode ec)
        {
            switch (ec)
            {
                case ErrorCode.None:
                    Write_to_OutputWindow("Success!\n"); // No reason to ever call the error catch with this...
                    break;
                case ErrorCode.NoData:
                    Write_to_OutputWindow("No GPS data in buffer\n");
                    break;
                case ErrorCode.Timeout:
                    Write_to_OutputWindow("GPS Update Timeout Error: Processing Halted\n ");
                    timer.Stop();
                    break;
                case ErrorCode.Undefined:
                default:
                    Write_to_OutputWindow("Undefined Error: Processing Halted\n");
                    timer.Stop();
                    break;
            }
            
        }


        private void rb_GPRMC_Checked(object sender, RoutedEventArgs e)
        {
            dataGrid.ItemsSource = Collection_GPRMC;
        }

        private void rb_GPGGA_Checked(object sender, RoutedEventArgs e)
        {
            dataGrid.ItemsSource = Collection_GPGGA;
        }
        
        private void rb_GPGSA_Checked(object sender, RoutedEventArgs e)
        {
            dataGrid.ItemsSource = Collection_GPGSA;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();

            if (GPS != null)
                GPS.Close();
        }

        private void Write_to_OutputWindow(string s)
        {
            tb_OutputWindow.Text += s;
            tb_OutputWindow.ScrollToEnd();
        }
    }
}
