using System;
using System.IO.Ports;
using System.Collections.ObjectModel;

using NSConstellationGPS.GPS_Sentences;

namespace NSConstellationGPS
{
    /// <summary>
    /// Error codes possible from various functions
    /// </summary>
    public enum ErrorCode
    {
        None = 0,
        Timeout = -1,
        Undefined = -2,
        NoData = -3,
    }

    /// <summary>
    /// This class retrieves data from a serial GPS stream
    /// </summary>
    public class ConstellationGPS
    {
        // master is the class that holds all of the NMEA sentence data
        private GPS_Sentence_Master master = new GPS_Sentence_Master();

        // Const Vars
        private const int BUFFER_LENGTH = 10000; // Bytes
        private readonly string[] _delimiters = { ",", "\r\n", "*" };

        // Private Vars
        private SerialPort _gps_comm;
        private string[] _split_stream;
        private char[] _gps_stream;
        private int _bytes_to_read;
        private int _bytes_read;
        private string _buffer;
        private bool _initialized = false;
        private VersionControl version = new VersionControl();

        /// <summary>
        /// Default constructor for Constellation GPS
        /// </summary>
        /// <param name="port">The COM post for the GPS unit. Ex: COM6 </param>
        /// <param name="baud">Baud rate in kbps for port</param>
        /// <param name="timeout">"Timeout param for how frequently to check the buffer"</param>
        public ConstellationGPS(string port, int baud, int timeout = 100)
        {
            _gps_stream = new char[BUFFER_LENGTH];
            Array.Clear(_gps_stream, 0, BUFFER_LENGTH);

            _bytes_to_read = 1000;

            _buffer = "";

            _gps_comm = new SerialPort(port, baud);
            _gps_comm.DataBits = 8;
            _gps_comm.StopBits = StopBits.One;
            _gps_comm.Parity = Parity.None;
            _gps_comm.ReadTimeout = timeout;
            
            _initialized = true;
        }

        /// <summary>
        /// Opens the port to communicate to GPS
        /// </summary>
        /// <returns>true if successful, false if error</returns>
        public bool Open()
        {
            if (_initialized)
            {
                try
                {
                    // Do not open an already open port, close it first to avoid issues
                    if (_gps_comm.IsOpen)
                        _gps_comm.Close();

                    // Open new port
                    _gps_comm.Open();

                }
                catch (Exception ex)
                {
                    ErrorCatch(ex);
                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Closes GPS COM port
        /// </summary>
        /// <returns>true if successful, false if error</returns>
        public bool Close()
        {
            if (_initialized)
            {
                // Try closing the port only if it is already open
                try
                {
                    if (_gps_comm.IsOpen)
                        _gps_comm.Close();
                }
                catch (Exception ex)
                {
                    ErrorCatch(ex);
                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Main function to retrieve serial GPS data stream
        /// </summary>
        /// <returns></returns>
        public ErrorCode Update()
        {
            try
            {
                // Read from serial stream
                _bytes_read = _gps_comm.Read(_gps_stream, 0, _bytes_to_read);

            }
            catch (TimeoutException)
            {
                return ErrorCode.Timeout;
            }
            catch(Exception)
            {
                return ErrorCode.Undefined;
            }

            // Add Data read from stream to our local buffer
            _buffer += new string(_gps_stream, 0, _bytes_read);
            
            // Get the index of the START of the last sentence
            int last_index = _buffer.LastIndexOf("$");

            // If no sentence headers exist, nothing to do
            if(last_index < 0)
                return ErrorCode.NoData;

            // Split the stream for easier parsing
            _split_stream = _buffer.Substring(0, last_index).Split(_delimiters, StringSplitOptions.None);

            /* Move current buffer start pointer to the last index. Doing
             * this will keep any partial data and append new data to it. 
             * This way we do not lose data. 
             */
            _buffer = _buffer.Substring(last_index);

            // Call the master struct to parse the given buffer and populate its children.
            master.Parse(_split_stream);

            // Return no error
            return ErrorCode.None;
        }

        /// <summary>
        /// Informational Function
        /// Returns the internal version of the module for display if wanted.
        /// </summary>
        /// <returns></returns>
        public string getVersion()
        {
            return version.Version;
        }

        public ObservableCollection<string> getAvailableMessages()
        {
            return master.getAvailableMessages();
        }

        /// <summary>
        /// Gets a copy of the newest master.GPRMC sentence and returns it to caller 
        /// </summary>
        /// <returns></returns>
        public GPS_Sentence_GPRMC getGPRMC()
        {
            GPS_Sentence_GPRMC tmp = new GPS_Sentence_GPRMC();

            tmp.Checksum = master.GPRMC.Checksum;
            tmp.Course = master.GPRMC.Course;
            tmp.Date = master.GPRMC.Date;
            tmp.Fix = master.GPRMC.Fix;
            tmp.Latitude = master.GPRMC.Latitude;
            tmp.Longitude = master.GPRMC.Longitude;
            tmp.Magnetic = master.GPRMC.Magnetic;
            tmp.Speed = master.GPRMC.Speed;
            tmp.Status = master.GPRMC.Status;
            tmp.Time = master.GPRMC.Time;
            tmp.Type = master.GPRMC.Type;
            
            return tmp;
        }

        /// <summary>
        /// Gets a copy of the newest master.GPGGA sentence and returns it to caller 
        /// </summary>
        /// <returns></returns>
        public GPS_Sentence_GPGGA getGPGGA()
        {
            GPS_Sentence_GPGGA tmp = new GPS_Sentence_GPGGA();

            tmp.Age_From_Last_Update_s = master.GPGGA.Age_From_Last_Update_s;
            tmp.Altitude = master.GPGGA.Altitude;
            tmp.Altitude_Unit = master.GPGGA.Altitude_Unit;
            tmp.Checksum = master.GPGGA.Checksum;
            tmp.Diff_Reference_Station_ID = master.GPGGA.Diff_Reference_Station_ID;
            tmp.Geoidal_Separation = master.GPGGA.Geoidal_Separation;
            tmp.Geoidal_Separation_Unit = master.GPGGA.Geoidal_Separation_Unit;
            tmp.Horizontal_Dilution = master.GPGGA.Horizontal_Dilution;
            tmp.Latitude = master.GPGGA.Latitude;
            tmp.Longitude = master.GPGGA.Longitude;
            tmp.Quality = master.GPGGA.Quality;
            tmp.Satellite_Count = master.GPGGA.Satellite_Count;
            tmp.Time = master.GPGGA.Time;
            tmp.Type = master.GPGGA.Type;

            return tmp;
        }
        
        /// <summary>
        /// Gets a copy of the newest master.GPGSA sentence and returns it to caller 
        /// </summary>
        /// <returns></returns>
        public GPS_Sentence_GPGSA getGPGSA()
        {
            GPS_Sentence_GPGSA tmp = new GPS_Sentence_GPGSA();

            tmp.HDOP    = master.GPGSA.HDOP;
            tmp.Mode1   = master.GPGSA.Mode1;
            tmp.Mode2   = master.GPGSA.Mode2;
            tmp.PDOP    = master.GPGSA.PDOP;
            tmp.Type    = master.GPGSA.Type;
            tmp.VDOP    = master.GPGSA.VDOP;

            tmp.SVID0 = master.GPGSA.SVID0;
            tmp.SVID1 = master.GPGSA.SVID1;
            tmp.SVID2 = master.GPGSA.SVID2;
            tmp.SVID3 = master.GPGSA.SVID3;
            tmp.SVID4 = master.GPGSA.SVID4;
            tmp.SVID5 = master.GPGSA.SVID5;
            tmp.SVID6 = master.GPGSA.SVID6;
            tmp.SVID7 = master.GPGSA.SVID7;
            tmp.SVID8 = master.GPGSA.SVID8;
            tmp.SVID9 = master.GPGSA.SVID9;
            tmp.SVID10 = master.GPGSA.SVID10;
            tmp.SVID11 = master.GPGSA.SVID11;

            return tmp;
        }

        /// <summary>
        /// Custom Error catching function
        /// </summary>
        /// <param name="ex"></param>
        private void ErrorCatch(Exception ex)
        {
#if DEBUG
            // Debug Mode Error Handling
            // Append message to error window for testing
            //tb_OutputWindow.Text += "\n" + ex.Message;
#else
            // Release Mode Error Handling
            // TBD
#endif
        }
    }
}
