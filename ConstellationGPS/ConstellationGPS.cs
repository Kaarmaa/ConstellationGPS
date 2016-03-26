using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace NSConstellationGPS
{
    public class ConstellationGPS
    {
        // Const Vars
        private const int BUFFER_LENGTH = 10000; // Bytes

        // Private Vars
        private readonly string[] _delimiters = { ",", "\r\n" };
        private SerialPort _gps_comm;
        private char[] _gps_stream;
        private int _buffer_offset;
        private int _bytes_to_read;
        private string _buffer;
        private bool _initialized = false;
        private VersionControl version = new VersionControl();

        public ConstellationGPS(string port, int baud, int timeout = 100)
        {
            _gps_stream = new char[BUFFER_LENGTH];
            Array.Clear(_gps_stream, 0, BUFFER_LENGTH);

            _buffer_offset = 0;
            _bytes_to_read = 1000;

            _buffer = "";

            _gps_comm = new SerialPort(port, baud);
            _gps_comm.DataBits = 8;
            _gps_comm.StopBits = StopBits.One;
            _gps_comm.Parity = Parity.None;
            _gps_comm.ReadTimeout = timeout;

            _initialized = true;
        }

        public ConstellationGPS()
        {}

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

        public string getVersion()
        {
            return version.Version;
        }

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
