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
        // Master is the class that holds all of the NMEA sentence data
        private GPS_Sentence_Master Master = new GPS_Sentence_Master();

        // Private Const Vars
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
        public ConstellationGPS(string port, int baud, int timeout = 1000)
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

            // Call the Master struct to parse the given buffer and populate its children.
            Master.Parse(_split_stream);

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

        /// <summary>
        /// Function returns all available GPS headers that have ceom from the sensor
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<string> getAvailableMessages()
        {
            return Master.getAvailableMessages();
        }

        /// <summary>
        /// Gets a copy of the newest Master.GPRMC sentence and returns it to caller 
        /// </summary>
        /// <returns></returns>
        public GPS_Sentence_GPRMC getGPRMC()
        {
            GPS_Sentence_GPRMC tmp = new GPS_Sentence_GPRMC();

            tmp.Checksum = Master.GPRMC.Checksum;
            tmp.Course = Master.GPRMC.Course;
            tmp.Date = Master.GPRMC.Date;
            tmp.Fix = Master.GPRMC.Fix;
            tmp.Latitude = Master.GPRMC.Latitude;
            tmp.Longitude = Master.GPRMC.Longitude;
            tmp.Magnetic = Master.GPRMC.Magnetic;
            tmp.Speed = Master.GPRMC.Speed;
            tmp.Status = Master.GPRMC.Status;
            tmp.Time = Master.GPRMC.Time;
            tmp.Type = Master.GPRMC.Type;
            
            return tmp;
        }

        /// <summary>
        /// Gets a copy of the newest Master.GPGGA sentence and returns it to caller 
        /// </summary>
        /// <returns></returns>
        public GPS_Sentence_GPGGA getGPGGA()
        {
            GPS_Sentence_GPGGA tmp = new GPS_Sentence_GPGGA();

            tmp.Age_From_Last_Update_s = Master.GPGGA.Age_From_Last_Update_s;
            tmp.Altitude = Master.GPGGA.Altitude;
            tmp.Altitude_Unit = Master.GPGGA.Altitude_Unit;
            tmp.Checksum = Master.GPGGA.Checksum;
            tmp.Diff_Reference_Station_ID = Master.GPGGA.Diff_Reference_Station_ID;
            tmp.Geoidal_Separation = Master.GPGGA.Geoidal_Separation;
            tmp.Geoidal_Separation_Unit = Master.GPGGA.Geoidal_Separation_Unit;
            tmp.Horizontal_Dilution = Master.GPGGA.Horizontal_Dilution;
            tmp.Latitude = Master.GPGGA.Latitude;
            tmp.Longitude = Master.GPGGA.Longitude;
            tmp.Quality = Master.GPGGA.Quality;
            tmp.Satellite_Count = Master.GPGGA.Satellite_Count;
            tmp.Time = Master.GPGGA.Time;
            tmp.Type = Master.GPGGA.Type;

            return tmp;
        }
        
        /// <summary>
        /// Gets a copy of the newest Master.GPGSA sentence and returns it to caller 
        /// </summary>
        /// <returns></returns>
        public GPS_Sentence_GPGSA getGPGSA()
        {
            GPS_Sentence_GPGSA tmp = new GPS_Sentence_GPGSA();

            tmp.HDOP    = Master.GPGSA.HDOP;
            tmp.Mode1   = Master.GPGSA.Mode1;
            tmp.Mode2   = Master.GPGSA.Mode2;
            tmp.PDOP    = Master.GPGSA.PDOP;
            tmp.Type    = Master.GPGSA.Type;
            tmp.VDOP    = Master.GPGSA.VDOP;

            tmp.SVID0 = Master.GPGSA.SVID0;
            tmp.SVID1 = Master.GPGSA.SVID1;
            tmp.SVID2 = Master.GPGSA.SVID2;
            tmp.SVID3 = Master.GPGSA.SVID3;
            tmp.SVID4 = Master.GPGSA.SVID4;
            tmp.SVID5 = Master.GPGSA.SVID5;
            tmp.SVID6 = Master.GPGSA.SVID6;
            tmp.SVID7 = Master.GPGSA.SVID7;
            tmp.SVID8 = Master.GPGSA.SVID8;
            tmp.SVID9 = Master.GPGSA.SVID9;
            tmp.SVID10 = Master.GPGSA.SVID10;
            tmp.SVID11 = Master.GPGSA.SVID11;

            return tmp;
        }

        /// <summary>
        /// Gets a copy of the newest Master.GPGSV sentence and returns it to caller 
        /// </summary>
        /// <returns></returns>
        public GPS_Sentence_GPGSV getGPGSV()
        {
            GPS_Sentence_GPGSV tmp = new GPS_Sentence_GPGSV();

            //tmp.Azimuth0 = Master.GPGSV.Azimuth0;
            //tmp.Azimuth1 = Master.GPGSV.Azimuth1;
            //tmp.Azimuth2 = Master.GPGSV.Azimuth2;
            //tmp.Azimuth3 = Master.GPGSV.Azimuth3;

            //tmp.Elevation0 = Master.GPGSV.Elevation0;
            //tmp.Elevation1 = Master.GPGSV.Elevation1;
            //tmp.Elevation2 = Master.GPGSV.Elevation2;
            //tmp.Elevation3 = Master.GPGSV.Elevation3;

            //tmp.PRN0 = Master.GPGSV.PRN0;
            //tmp.PRN1 = Master.GPGSV.PRN1;
            //tmp.PRN2 = Master.GPGSV.PRN2;
            //tmp.PRN3 = Master.GPGSV.PRN3;

            //tmp.SNR0 = Master.GPGSV.SNR0;
            //tmp.SNR1 = Master.GPGSV.SNR1;
            //tmp.SNR2 = Master.GPGSV.SNR2;
            //tmp.SNR3 = Master.GPGSV.SNR3;

            for (int i = 0; i < Master.GPGSV.SVS.Length; i++)
            {
                tmp.SVS[i] = Master.GPGSV.SVS[i];
            }

            tmp.Checksum = Master.GPGSV.Checksum;
            tmp.Total_Messages = Master.GPGSV.Total_Messages;
            tmp.Total_SV = Master.GPGSV.Total_SV;
            tmp.Type = Master.GPGSV.Type;

            return tmp;
        }
    }
}
