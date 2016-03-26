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
using System.IO.Ports;
using System.Windows.Threading;
using NSConstellationGPS;
using NSConstellationGPS.GPS_Sentences;
using System.Collections.ObjectModel;

namespace NSConstellationGPSDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ConstellationGPSDemoWindow : Window
    {
        // These are the default ports as provided by National Instruments
        private int[] _baudrates = new int[] { 110, 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 56000, 57600, 115200 };
        private ConstellationGPS GPS;
        private DispatcherTimer timer;
        private ObservableCollection<GPS_Sentence_GPRMC> Collection_GPRMC = new ObservableCollection<GPS_Sentence_GPRMC>();
        private ObservableCollection<GPS_Sentence_GPGGA> Collection_GPGGA = new ObservableCollection<GPS_Sentence_GPGGA>();
        public ConstellationGPSDemoWindow()
        {
            InitializeComponent();
            
            // Setup UI 
            cb_Port.ItemsSource = SerialPort.GetPortNames();
            cb_Baud.ItemsSource = _baudrates;

            dataGrid.ItemsSource = Collection_GPRMC;

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 1);
            timer.Tick += new EventHandler(timer_tick);
        }

        private void timer_tick(object obj, EventArgs sender)
        {
            int ret = GPS.ReadGPS();
            if (ret == 0)
            {
                Collection_GPRMC.Add(GPS.getGPRMC());
                Collection_GPGGA.Add(GPS.getGPGGA());

                dataGrid.ScrollIntoView(dataGrid.Items[dataGrid.Items.Count - 1]);
            }
            else
            {
                // ERROR
            }
        }

        /// <summary>
        /// This event fires on the Open button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Open_Click(object sender, RoutedEventArgs e)
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
        private void btn_Close_Click(object sender, RoutedEventArgs e)
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
        }

        private void ErrorCatch(Exception ex)
        {
#if DEBUG
            // Debug Mode Error Handling
            // Append message to error window for testing
            tb_OutputWindow.Text += "\n" + ex.Message;
#else
            // Release Mode Error Handling
            // TBD
#endif
        }

        private void rb_GPRMC_Checked(object sender, RoutedEventArgs e)
        {
            dataGrid.ItemsSource = Collection_GPRMC;
        }

        private void rb_GPGGA_Checked(object sender, RoutedEventArgs e)
        {
            dataGrid.ItemsSource = Collection_GPGGA;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();

            if (GPS != null)
                GPS.Close();
        }
    }
}
