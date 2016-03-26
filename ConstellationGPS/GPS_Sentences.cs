using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace NSConstellationGPS.GPS_Sentences
{
    /// <summary>
    /// This is the master class that holds all of the possible GPS sentences that are coming from the GPS
    /// </summary>
    class GPS_Sentence_Master : INotifyPropertyChanged
    {
        private GPS_Sentence_GPRMC _gprmc;
        public GPS_Sentence_GPRMC GPRMC { get { return _gprmc; } set { _gprmc = value; OnPropertyChanged("Time"); } }
        
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
        }

        public bool Parse(string[] split)
        {
            int error = 0;
            if(_gprmc.Parse(split)) error++;

            return (error == 0);
        }
    }

    /// <summary>
    /// Base class for GPS sentences. 
    /// </summary>
    public class GPS_Sentence_Base : INotifyPropertyChanged
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

        public bool Parse(string[] buffer)
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

    
}
