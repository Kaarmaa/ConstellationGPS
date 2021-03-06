﻿using System;
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
        private ObservableCollection<GPS_Sentence_GPGSV> Collection_GPGSV = new ObservableCollection<GPS_Sentence_GPGSV>();
        private ObservableCollection<GPS_Sentence_GPVTG> Collection_GPVTG = new ObservableCollection<GPS_Sentence_GPVTG>(); 

        /// <summary>
        /// Demo Window Contructor
        /// </summary>
        public ConstellationGPSDemoWindow()
        {
            InitializeComponent();
            
            // Setup UI 
            cb_Port.ItemsSource = SerialPort.GetPortNames();
            cb_Baud.ItemsSource = _baudrates;

            // Init Timer
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            timer.Tick += new EventHandler(timer_tick);

            GPS = new ConstellationGPS();

            Write_to_OutputWindow("Loaded Constellation GPS v" + GPS.getVersion() + "\n");
        }

        /// <summary>
        /// Main UI timer
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sender"></param>
        private void timer_tick(object obj, EventArgs sender)
        {
            ErrorCode ret = GPS.getErrors();

            // Update GPS
            if (ret == ErrorCode.None)
            {
                // Only Populate the collections you wish
                Collection_GPRMC.Add(GPS.getGPRMC());
                Collection_GPGGA.Add(GPS.getGPGGA());
                Collection_GPGSA.Add(GPS.getGPGSA());
                Collection_GPGSV.Add(GPS.getGPGSV());
                Collection_GPVTG.Add(GPS.getGPVTG());

                // UI Scroll to most recent item
                if (dataGrid.ItemsSource != null)
                    dataGrid.ScrollIntoView(dataGrid.Items[dataGrid.Items.Count - 1]);

                string text = cb_available_msgs.Text;
                cb_available_msgs.ItemsSource = null;
                cb_available_msgs.ItemsSource = GPS.getAvailableMessages();
                cb_available_msgs.Text = text;
                cb_available_msgs.UpdateLayout();
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
                
                if (GPS != null)
                {
                    // Do not open an already open port, close it first to avoid issues
                    GPS.Close();

                    // Reconfigure Port as UI specifies
                    GPS.setBaud(Convert.ToInt32(cb_Baud.SelectedValue));
                    GPS.setPort(cb_Port.SelectedItem.ToString());
                    
                    // Open new port
                    GPS.Open();
                }

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

        /// <summary>
        /// Catch for exceptions
        /// </summary>
        /// <param name="ex"></param>
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

        /// <summary>
        /// Catch for modules ErrorCode objects
        /// </summary>
        /// <param name="ec"></param>
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

        /// <summary>
        /// Event fires when main window is closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();

            if (GPS != null)
                GPS.Close();
        }

        /// <summary>
        /// Writes arguement to the OutputWindow
        /// </summary>
        /// <param name="s"></param>
        private void Write_to_OutputWindow(string s)
        {
            tb_OutputWindow.Text += s;
            tb_OutputWindow.ScrollToEnd();
        }

        /// <summary>
        /// Event that fires when the combobox specify which message to display is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_available_msgs_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cb_available_msgs.SelectedIndex >= 0)
            {
                switch (cb_available_msgs.Items[cb_available_msgs.SelectedIndex].ToString())
                {
                    case "$GPRMC":
                        dataGrid.ItemsSource = Collection_GPRMC;
                        break;
                    case "$GPGGA":
                        dataGrid.ItemsSource = Collection_GPGGA;
                        break;
                    case "$GPGSA":
                        dataGrid.ItemsSource = Collection_GPGSA;
                        break;
                    case "$GPGSV":
                        dataGrid.ItemsSource = Collection_GPGSV;
                        break;
                    case "$GPVTG":
                        dataGrid.ItemsSource = Collection_GPVTG;
                        break;
                    default:
                        Write_to_OutputWindow(cb_available_msgs.Items[cb_available_msgs.SelectedIndex].ToString() + " Parsing unavailable.\n");
                        dataGrid.ItemsSource = null;
                        break;
                }
            }
        }

        private void image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=9G8SW3CXLWUNU");
        }
    }
}
