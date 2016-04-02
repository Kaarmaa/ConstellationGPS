using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NSConstellationGPS.GPS_Sentences
{
    /// <summary>
    /// This is the Master class that holds all of the possible GPS sentences that are coming from the GPS
    /// </summary>
    class GPS_Sentence_Master : INotifyPropertyChanged
    {
        private GPS_Sentence_GPRMC _gprmc;
        public GPS_Sentence_GPRMC GPRMC { get { return _gprmc; } set { _gprmc = value; OnPropertyChanged("GPRMC"); } }

        private GPS_Sentence_GPGGA _gpgga;
        public GPS_Sentence_GPGGA GPGGA { get { return _gpgga; } set { _gpgga = value; OnPropertyChanged("GPGGA"); } }

        private GPS_Sentence_GPGSA _gpgsa;
        public GPS_Sentence_GPGSA GPGSA { get { return _gpgsa; } set { _gpgsa = value; OnPropertyChanged("GPGSA"); } }

        private GPS_Sentence_GPGSV _gpgsv;
        public GPS_Sentence_GPGSV GPGSV { get { return _gpgsv; } set { _gpgsv = value; OnPropertyChanged("GPGSV"); } }

        private GPS_Sentence_GPVTG _gpvtg;
        public GPS_Sentence_GPVTG GPVTG { get { return _gpvtg; } set { _gpvtg = value; OnPropertyChanged("GPVTG"); } }
        
        private List<string> avMsgs = new List<string>();

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// Master GPS Constructor
        /// </summary>
        public GPS_Sentence_Master()
        {
            avMsgs.Clear();

            _gprmc = new GPS_Sentence_GPRMC();
            _gpgga = new GPS_Sentence_GPGGA();
            _gpgsa = new GPS_Sentence_GPGSA();
            _gpgsv = new GPS_Sentence_GPGSV();
            _gpvtg = new GPS_Sentence_GPVTG();
        }

        /// <summary>
        /// Main event to have master GPS sentence begin parsing
        /// </summary>
        /// <param name="split"></param>
        /// <returns></returns>
        public bool Parse(string[] split)
        {
            // Check for any new sentences that are not already known
            foreach (string s in split)
            {
                if (s.Length > 0 && s[0] == '$' && !avMsgs.Contains(s))
                {
                    avMsgs.Add(s);
                }
            }

            
            int error = 0;

            //Individual Sentence Parsing Calls 
            if(_gprmc.Parse(split)) error++;
            if(_gpgga.Parse(split)) error++;
            if(_gpgsa.Parse(split)) error++;
            if(_gpgsv.Parse(split)) error++;
            if(_gpvtg.Parse(split)) error++;


            //TODO: Can check here if error > 0, and throw a better error than a FAIL.
            // Return success
            return (error == 0);
        }

        /// <summary>
        /// Returns a collection of available GPS messages from this sensor
        /// </summary>
        /// <returns></returns>
        public List<string> getAvailableMessages()
        {
            return avMsgs;
        }
    }

    /// <summary>
    /// Base class for GPS sentences. 
    /// </summary>
    public abstract class GPS_Sentence_Base : INotifyPropertyChanged
    {
        private string _type;
        public string Type { get { return _type; } set { _type = value; OnPropertyChanged("Type"); } }
        
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        
        /// <summary>
        /// Holds parsing code for individual sentence types
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public abstract bool Parse(string[] s);

        /// <summary>
        /// Converts string to double, with error checking for null strings
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public double String_to_Double(string s)
        {
            if (s.CompareTo("") != 0)
            {
                return Convert.ToDouble(s);
            }
            return -0;
        }

        /// <summary>
        /// Converts string to int, with error checking for null strings
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public int String_to_Int(string s)
        {
            if (s.CompareTo("") != 0)
            {
                return Convert.ToInt32(s);
            }
            return -0;
        }

        /// <summary>
        /// Converts string to char, with error checking for null strings
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public char String_to_Char(string s)
        {
            if (s.CompareTo("") != 0)
            {
                return Convert.ToChar(s);
            }
            return ' ';
        }
    }

    /// <summary>
    /// GPRMC Sentence Structure
    /// </summary>
    public class GPS_Sentence_GPRMC : GPS_Sentence_Base
    {
        // UTC Time of position fix
        private string _time;
        public string Time { get { return _time; } set { _time = value;  OnPropertyChanged("Time"); } }

        // Data Status/Validity (A = ok, V = invalid)
        private char _status;
        public char Status { get { return _status; } set { _status = value; OnPropertyChanged("Status"); } }

        // Latitude of fix (Decimal Degrees)
        private double _latitude;
        public double Latitude { get { return _latitude; } set { _latitude = value; OnPropertyChanged("Latitude"); } }

        // Longitude of fix (Decimal Degrees)
        private double _longitude;
        public double Longitude { get { return _longitude; } set { _longitude = value; OnPropertyChanged("Longitude"); } }
        
        // Speed (Knots)
        private double _speed;
        public double Speed { get { return _speed; } set { _speed = value; OnPropertyChanged("Speed"); } }

        // Course (Degrees)
        private double _course;
        public double Course { get { return _course; } set { _course = value; OnPropertyChanged("Course"); } }

        // Date (UT)
        private int _date;
        public int Date { get { return _date; } set { _date = value; OnPropertyChanged("Date"); } }

        // Magnetic Variation (Degrees)
        //TODO: GPRMC:Magnetic - Not currently populated
        private double _magnetic;
        public double Magnetic { get { return _magnetic; } set { _magnetic = value; OnPropertyChanged("Magnetic"); } }

        // Fix (A = Autonomous, D = Differential, E = Estimated, N = Not Valid, S = Simulator)
        private char _fix;
        public char Fix { get { return _fix; } set { _fix = value; OnPropertyChanged("Fix"); } }

        // Checksum
        private string _checksum;
        public string Checksum { get { return _checksum; } set { _checksum = value; OnPropertyChanged("Checksum"); } }

        /// <summary>
        /// Parsing Function
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public override bool Parse(string[] buffer)
        {
            bool valid_data = false;
            double buf, degrees, minutes;
            
            // TODO: Use Master's precalculated indicides to know where to parse. No search will be neceessary anymore.
            for (int i = 0; i < buffer.Length - 14; i++)
            {
                if(String.Compare(buffer[i],"$GPRMC") == 0)
                {

                    Type        = "$GPRMC";
                    Time        = buffer[i + 1];
                    Status      = buffer[i + 2][0];

                    buf = String_to_Double(buffer[i + 3]);
                    degrees = Math.Floor(buf / 100.0);
                    minutes = (buf - (degrees * 100)) / 60.0;
                    Latitude = degrees + minutes;
                    if (String_to_Char(buffer[i + 4]) == 'S')
                        Latitude *= -1;

                    buf = String_to_Double(buffer[i + 5]);
                    degrees = Math.Floor(buf / 100.0);
                    minutes = (buf - (degrees * 100)) / 60.0;
                    Longitude = degrees + minutes;
                    if (String_to_Char(buffer[i + 6]) == 'W')
                        Longitude *= -1;

                    Speed       = String_to_Double(buffer[i + 7]);
                    Course      = String_to_Double(buffer[i + 8]);
                    Date        = String_to_Int(buffer[i + 9]);
                    Magnetic    = 0.0; //TODO: Magnetic
                    Fix         = buffer[i + 12][0];
                    Checksum    = "*" + buffer[i + 13];

                    valid_data = true;
                }
            }
            return valid_data;
        }
    }

    /// <summary>
    /// GPGGA Sentence Structure
    /// </summary>
    public class GPS_Sentence_GPGGA : GPS_Sentence_Base
    {
        //UTC of Position
        private string _time;
        public string Time { get { return _time; } set { _time = value; OnPropertyChanged("Time"); } }

        // Latitude of Position
        private double _latitude;
        public double Latitude { get { return _latitude; } set { _latitude = value; OnPropertyChanged("Latitude"); } }

        // Longitude of Position
        private double _longitude;
        public double Longitude { get { return _longitude; } set { _longitude = value; OnPropertyChanged("Longitude"); } }

        //  GPS Quality indicator (0=no fix, 1=GPS fix, 2=Dif. GPS fix) 
        private int _quality;
        public int Quality { get { return _quality; } set { _quality = value; OnPropertyChanged("Quality"); } }

        // Number of Satellites in Use
        private int _satellite_count;
        public int Satellite_Count { get { return _satellite_count; } set { _satellite_count = value; OnPropertyChanged("Satellite_Count"); } }

        // Horizontal Dilution of Precision
        private double _horizontal_dilution;
        public double Horizontal_Dilution { get { return _horizontal_dilution; } set { _horizontal_dilution = value; OnPropertyChanged("Horizontal_Dilution"); } }

        // Altitude above sea level
        private double _altitude;
        public double Altitude { get { return _altitude; } set { _altitude = value; OnPropertyChanged("Altitude"); } }

        // Altitude unit of measure (M = Meters)
        private char _altitude_unit;
        public char Altitude_Unit { get { return _altitude_unit; } set { _altitude_unit = value; OnPropertyChanged("Altitude_unit"); } }

        // Geoidal Separation
        private double _geoidal_separation;
        public double Geoidal_Separation { get { return _geoidal_separation; } set { _geoidal_separation = value; OnPropertyChanged("Geoidal_Separation"); } }

        // Geoidal Separation unit of measure (M = Meters)
        private char _geoidal_separation_unit;
        public char Geoidal_Separation_Unit { get { return _geoidal_separation_unit; } set { _geoidal_separation_unit = value; OnPropertyChanged("Geoidal_Separation_Unit"); } }

        // Age of Differential GPS data (seconds)
        private double _age_from_last_update_s;
        public double Age_From_Last_Update_s { get { return _age_from_last_update_s; } set { _age_from_last_update_s = value; OnPropertyChanged("Age_From_Last_Update_s"); } }

        // Differential Reference Station ID
        private int _diff_reference_station_ID;
        public int Diff_Reference_Station_ID { get { return _diff_reference_station_ID; } set { _diff_reference_station_ID = value; OnPropertyChanged("Diff_Reference_Station_ID"); } }

        // Checksum
        private string _checksum;
        public string Checksum { get { return _checksum; } set { _checksum = value; OnPropertyChanged("Checksum"); } }
        
        /// <summary>
        /// Parsing Function
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public override bool Parse(string[] buffer)
        {
            bool valid_data = false;
            double buf, degrees, minutes;

            // TODO: Use Master's precalculated indicides to know where to parse. No search will be neceessary anymore.
            for (int i = 0; i < buffer.Length - 16; i++)
            {
                if (String.Compare(buffer[i], "$GPGGA") == 0)
                {
                    Type = "$GPGGA";
                    Time = buffer[i + 1];

                    buf = String_to_Double(buffer[i + 2]);
                    degrees = Math.Floor(buf / 100.0);
                    minutes = (buf - (degrees * 100)) / 60.0;
                    Latitude = degrees + minutes;
                    if (String_to_Char(buffer[i + 3]) == 'S')
                        Latitude *= -1;

                    buf = String_to_Double(buffer[i + 4]);
                    degrees = Math.Floor(buf / 100.0);
                    minutes = (buf - (degrees * 100)) / 60.0;
                    Longitude = degrees + minutes;
                    if (String_to_Char(buffer[i + 5]) == 'W')
                        Longitude *= -1;

                    Quality = String_to_Int(buffer[i + 6]);
                    Satellite_Count = String_to_Int(buffer[i + 7]);
                    Horizontal_Dilution = String_to_Double(buffer[i + 8]);
                    Altitude = String_to_Double(buffer[i + 9]);
                    Altitude_Unit = String_to_Char(buffer[i + 10]);
                    Geoidal_Separation = String_to_Double(buffer[i + 11]);
                    Geoidal_Separation_Unit = String_to_Char(buffer[i + 12]);
                    Age_From_Last_Update_s = String_to_Double(buffer[i + 13]);
                    Diff_Reference_Station_ID = String_to_Int(buffer[i + 14]);
                    Checksum = "*" + buffer[i + 15];

                    valid_data = true;
                }
            }
            return valid_data;
        }
    }

    /// <summary>
    /// GPGSA Sentence Structure
    /// </summary>
    public class GPS_Sentence_GPGSA : GPS_Sentence_Base
    {
        // Mode1 (M = Manual, A = Automatic)
        private char _mode1;
        public char Mode1 { get { return _mode1; } set { _mode1 = value; OnPropertyChanged("Mode1"); } }

        // Mode2 (1 = Fix unavailable, 2 = 2D, 3 = 3D)
        private int _mode2;
        public int Mode2 { get { return _mode2; } set { _mode2 = value; OnPropertyChanged("Mode2"); } }
        
        private int[] _svIDs = new int[12];

        // PRN of Satellite 0 used for fix
        public int SVID0 { get { return _svIDs[0]; } set { _svIDs[0] = value; OnPropertyChanged("SVID0"); } }

        // PRN of Satellite 1 used for fix
        public int SVID1 { get { return _svIDs[1]; } set { _svIDs[1] = value; OnPropertyChanged("SVID1"); } }

        // PRN of Satellite 2 used for fix
        public int SVID2 { get { return _svIDs[2]; } set { _svIDs[2] = value; OnPropertyChanged("SVID2"); } }

        // PRN of Satellite 3 used for fix
        public int SVID3 { get { return _svIDs[3]; } set { _svIDs[3] = value; OnPropertyChanged("SVID3"); } }

        // PRN of Satellite 4 used for fix
        public int SVID4 { get { return _svIDs[4]; } set { _svIDs[4] = value; OnPropertyChanged("SVID4"); } }

        // PRN of Satellite 5 used for fix
        public int SVID5 { get { return _svIDs[5]; } set { _svIDs[5] = value; OnPropertyChanged("SVID5"); } }

        // PRN of Satellite 6 used for fix
        public int SVID6 { get { return _svIDs[6]; } set { _svIDs[6] = value; OnPropertyChanged("SVID6"); } }

        // PRN of Satellite 7 used for fix
        public int SVID7 { get { return _svIDs[7]; } set { _svIDs[7] = value; OnPropertyChanged("SVID7"); } }

        // PRN of Satellite 8 used for fix
        public int SVID8 { get { return _svIDs[8]; } set { _svIDs[8] = value; OnPropertyChanged("SVID8"); } }

        // PRN of Satellite 9 used for fix
        public int SVID9 { get { return _svIDs[9]; } set { _svIDs[9] = value; OnPropertyChanged("SVID9"); } }

        // PRN of Satellite 10 used for fix
        public int SVID10 { get { return _svIDs[10]; } set { _svIDs[10] = value; OnPropertyChanged("SVID10"); } }

        // PRN of Satellite 11 used for fix
        public int SVID11 { get { return _svIDs[11]; } set { _svIDs[11] = value; OnPropertyChanged("SVID11"); } }

        // Dilution of Precision
        private double _pDOP;
        public double PDOP { get { return _pDOP; } set { _pDOP = value; OnPropertyChanged("PDOP"); } }

        // Horizontal Dilution of Precision
        private double _hDOP;
        public double HDOP { get { return _hDOP; } set { _hDOP = value; OnPropertyChanged("HDOP"); } }

        // Vertical Dilution of Prevision
        private double _vDOP;
        public double VDOP { get { return _vDOP; } set { _vDOP = value; OnPropertyChanged("VDOP"); } }

        /// <summary>
        /// Parsing Function
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public override bool Parse(string[] buffer)
        {
            bool valid_data = false;

            // TODO: Use Master's precalculated indicides to know where to parse. No search will be neceessary anymore.
            for (int i = 0; i < buffer.Length - 16; i++)
            {
                if (String.Compare(buffer[i], "$GPGSA") == 0)
                {
                    Type = "$GPGSA";
                    Mode1 = String_to_Char(buffer[i + 1]);
                    Mode2 = String_to_Int(buffer[i + 2]);

                    SVID0 = String_to_Int(buffer[i + 3]);
                    SVID1 = String_to_Int(buffer[i + 4]);
                    SVID2 = String_to_Int(buffer[i + 5]);
                    SVID3 = String_to_Int(buffer[i + 6]);
                    SVID4 = String_to_Int(buffer[i + 7]);
                    SVID5 = String_to_Int(buffer[i + 8]);
                    SVID6 = String_to_Int(buffer[i + 9]);
                    SVID7 = String_to_Int(buffer[i + 10]);
                    SVID8 = String_to_Int(buffer[i + 11]);
                    SVID9 = String_to_Int(buffer[i + 12]);
                    SVID10 = String_to_Int(buffer[i + 13]);
                    SVID11 = String_to_Int(buffer[i + 14]);


                    PDOP = String_to_Double(buffer[i + 15]);
                    HDOP = String_to_Double(buffer[i + 16]);
                    VDOP = String_to_Double(buffer[i + 17]);

                    valid_data = true;
                }
            }
            return valid_data;
        }
    }

    /// <summary>
    /// GPGSV Sentence Structure
    /// </summary>
    public class GPS_Sentence_GPGSV : GPS_Sentence_Base
    {
        // NOTE: Message Sub-ID (ex: 1/3) is ignored in this implementation because the multiple sentences are fused into this one message on parse

        // Total number of messages in this block
        private int _total_Messages;
        public int Total_Messages { get { return _total_Messages; } set { _total_Messages = value; OnPropertyChanged("Total_Messages"); } }
        
        // Total number of satellites in view
        private int _total_SV;
        public int Total_SV { get { return _total_SV; } set { _total_SV = value; OnPropertyChanged("Total_SV"); } }

        //TODO: Make the SVS array bindable to the Demo UI
        public SV[] _svs = new SV[12];
        public SV[] SVS { get { return _svs; } set { _svs = value; OnPropertyChanged("SVS"); } }

        public struct SV
        { 
            // SV PRN Number 
            public int PRN { get; set; }

            // Elevation (degrees, 9 max)
            public int Elevation { get; set; }

            // Azimuth (degrees from true N, -359)
            public int Azimuth { get; set; }

            // SNR (dB, -99, null with no tracking)
            public int SNR { get; set; }
        }

        // Checksum
        private string _checksum;
        public string Checksum { get { return _checksum; } set { _checksum = value; OnPropertyChanged("Checksum"); } }

        /// <summary>
        /// Parsing Function
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public override bool Parse(string[] buffer)
        {
            bool valid_data = false;

            // TODO: Use Master's precalculated indicides to know where to parse. No search will be neceessary anymore.
            for (int i = 0; i < buffer.Length - 20; i++)
            {
                if (String.Compare(buffer[i], "$GPGSV") == 0)
                {
                    Type = "$GPGSV";

                    Total_Messages = String_to_Int(buffer[i + 1]);

                    int Current_Message = String_to_Int(buffer[i + 2]);

                    Total_SV = String_to_Int(buffer[i + 3]);

                    int arr_size = Current_Message * 4;
                    if (SVS.Length < arr_size)
                        Array.Resize(ref _svs, arr_size);

                    int local_offset = 0;
                    for (int locind = 0; locind < 4; locind++)
                    {
                        int current_index = ((Current_Message - 1) * 4) + locind;
                        if (current_index >= Total_SV)
                            break;

                        local_offset = (locind + 1) * 4;
                        SVS[current_index].PRN = String_to_Int(buffer[i + local_offset]);
                        SVS[current_index].Elevation = String_to_Int(buffer[i + local_offset + 1]);
                        SVS[current_index].Azimuth = String_to_Int(buffer[i + local_offset + 2]);
                        SVS[current_index].SNR = String_to_Int(buffer[i + local_offset + 3]);
                    }
                    
                    Checksum = "*" + buffer[i + local_offset + 4];

                    valid_data = true;
                }
            }
            return valid_data;
        }
    }

    /// <summary>
    /// GPVTG Sentence Structure
    /// </summary>
    public class GPS_Sentence_GPVTG : GPS_Sentence_Base
    {
        // Track Made Good (Degrees)
        private double _trackMadeGood;
        public double TrackMadeGood { get { return _trackMadeGood; } set { _trackMadeGood = value; OnPropertyChanged("TrackMadeGood"); } }

        // Track made good, relative to true north (Always T)
        private char _fixedText_TrueNorth;
        public char FixedText_TrueNorth { get { return _fixedText_TrueNorth; } set { _fixedText_TrueNorth = value; OnPropertyChanged("RelativetoTrueNorth"); } }

        // Magnetic Track (Degrees)
        private double _magneticTrack;
        public double MagneticTrack { get { return _magneticTrack; } set { _magneticTrack = value; OnPropertyChanged("MagneticTrack"); } }
        
        // Track made good, relative to true north (Always T)
        private char _fixedText_Magnetic;
        public char FixedText_Magnetic { get { return _fixedText_Magnetic; } set { _fixedText_Magnetic = value; OnPropertyChanged("FixedText_Magnetic"); } }

        //  Track Speed in Knots
        private double _trackSpeed_Knots;
        public double TrackSpeed_Knots { get { return _trackSpeed_Knots; } set { _trackSpeed_Knots = value; OnPropertyChanged("TrackSpeed_Knots"); } }
        
        // Track made good, relative to true north (Always T)
        private char _fixedText_Knots;
        public char FixedText_Knots { get { return _fixedText_Knots; } set { _fixedText_Knots = value; OnPropertyChanged("FixedText_Knots"); } }

        // Track Speed in Km per Hour
        private double _trackSpeed_KmH;
        public double TrackSpeed_KmH { get { return _trackSpeed_KmH; } set { _trackSpeed_KmH = value; OnPropertyChanged("TrackSpeed_KmH"); } }

        // Track made good, relative to true north (Always T)
        private char _fixedText_KmH;
        public char FixedText_KmH { get { return _fixedText_KmH; } set { _fixedText_KmH = value; OnPropertyChanged("FixedText_KmH"); } }
        
        // Fix
        private char _fix;
        public char Fix { get { return _fix; } set { _fix = value; OnPropertyChanged("Fix"); } }

        // Checksum
        private string _checksum;
        public string Checksum { get { return _checksum; } set { _checksum = value; OnPropertyChanged("Checksum"); } }

        /// <summary>
        /// Parsing Function
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public override bool Parse(string[] buffer)
        {
            bool valid_data = false;

            // TODO: Use Master's precalculated indicides to know where to parse. No search will be neceessary anymore.
            for (int i = 0; i < buffer.Length - 16; i++)
            {
                if (String.Compare(buffer[i], "$GPVTG") == 0)
                {
                    Type = "$GPVTG";
                    TrackMadeGood = String_to_Double(buffer[i + 1]);
                    FixedText_TrueNorth = String_to_Char(buffer[i + 2]);
                    TrackMadeGood = String_to_Double(buffer[i + 3]);
                    FixedText_Magnetic = String_to_Char(buffer[i + 4]);
                    TrackMadeGood = String_to_Double(buffer[i + 5]);
                    FixedText_Knots = String_to_Char(buffer[i + 6]);
                    TrackMadeGood = String_to_Double(buffer[i + 7]);
                    FixedText_KmH = String_to_Char(buffer[i + 8]);
                    Fix = String_to_Char(buffer[i + 9]);
                    
                    Checksum = "*" + buffer[i + 10];

                    valid_data = true;
                }
            }
            return valid_data;
        }
    }
}
