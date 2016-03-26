using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using NSConstellationGPS.GPS_Sentences;

namespace NSConstellationGPS
{
    /// <summary>
    /// Error codes possible from various functions
    /// </summary>
    enum ErrorCode
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
        GPS_Sentence_Master master = new GPS_Sentence_Master();

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
        public int ReadGPS()
        {
            try
            {
                // Read from serial stream
                _bytes_read = _gps_comm.Read(_gps_stream, 0, _bytes_to_read);

            }
            catch (TimeoutException tex)
            {
                ErrorCatch(tex);
                return (int)ErrorCode.Timeout;
            }
            catch(Exception ex)
            {
                ErrorCatch(ex);
                return (int)ErrorCode.Undefined;
            }

            // Add Data read from stream to our local buffer
            _buffer += new string(_gps_stream, 0, _bytes_read);
            
            // Get the index of the START of the last sentence
            int last_index = _buffer.LastIndexOf("$");

            // If no sentence headers exist, nothing to do
            if(last_index < 0)
                return (int)ErrorCode.NoData;

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
            return (int)ErrorCode.None;
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
