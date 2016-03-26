using System;
using System.ComponentModel;

namespace NSConstellationGPS.GPS_Sentences
{
    /// <summary>
    /// This is the master class that holds all of the possible GPS sentences that are coming from the GPS
    /// </summary>
    class GPS_Sentence_Master : INotifyPropertyChanged
    {
        private GPS_Sentence_GPRMC _gprmc;
        public GPS_Sentence_GPRMC GPRMC { get { return _gprmc; } set { _gprmc = value; OnPropertyChanged("GPRMC"); } }

        private GPS_Sentence_GPGGA _gpgga;
        public GPS_Sentence_GPGGA GPGGA { get { return _gpgga; } set { _gpgga = value; OnPropertyChanged("GPGGA"); } }

        private GPS_Sentence_GPGSA _gpgsa;
        public GPS_Sentence_GPGSA GPGSA { get { return _gpgsa; } set { _gpgsa = value; OnPropertyChanged("GPGSA"); } }


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

        public GPS_Sentence_Master()
        {
            _gprmc = new GPS_Sentence_GPRMC();
            _gpgga = new GPS_Sentence_GPGGA();
            _gpgsa = new GPS_Sentence_GPGSA();
        }

        public bool Parse(string[] split)
        {
            int error = 0;
            if(_gprmc.Parse(split)) error++;
            if(_gpgga.Parse(split)) error++;
            if(_gpgsa.Parse(split)) error++;


            return (error == 0);
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

        public abstract bool Parse(string[] s);

        public double String_to_Double(string s)
        {
            if (s.CompareTo("") != 0)
            {
                return Convert.ToDouble(s);
            }
            return -0;
        }

        public int String_to_Int(string s)
        {
            if (s.CompareTo("") != 0)
            {
                return Convert.ToInt32(s);
            }
            return -0;
        }

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
        private string _time;
        public string Time { get { return _time; } set { _time = value;  OnPropertyChanged("Time"); } }

        private char _status;
        public char Status { get { return _status; } set { _status = value; OnPropertyChanged("Status"); } }

        private double _latitude;
        public double Latitude { get { return _latitude; } set { _latitude = value; OnPropertyChanged("Latitude"); } }

        private double _longitude;
        public double Longitude { get { return _longitude; } set { _longitude = value; OnPropertyChanged("Longitude"); } }

        private double _speed;
        public double Speed { get { return _speed; } set { _speed = value; OnPropertyChanged("Speed"); } }

        private double _course;
        public double Course { get { return _course; } set { _course = value; OnPropertyChanged("Course"); } }

        private int _date;
        public int Date { get { return _date; } set { _date = value; OnPropertyChanged("Date"); } }

        private double _magnetic;
        public double Magnetic { get { return _magnetic; } set { _magnetic = value; OnPropertyChanged("Magnetic"); } }

        private char _fix;
        public char Fix { get { return _fix; } set { _fix = value; OnPropertyChanged("Fix"); } }

        private string _checksum;
        public string Checksum { get { return _checksum; } set { _checksum = value; OnPropertyChanged("Checksum"); } }

        public override bool Parse(string[] buffer)
        {
            bool valid_data = false;
            double buf, degrees, minutes;

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
        
        public override bool Parse(string[] buffer)
        {
            bool valid_data = false;
            double buf, degrees, minutes;

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

        public override bool Parse(string[] buffer)
        {
            bool valid_data = false;

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

}
