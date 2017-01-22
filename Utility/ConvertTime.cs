using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlusPlayer.Utility
{
    public static class ConvertTime
    {

        public static void NanosecondsToHours(double _ns, out double _nanoseconds, out double _seconds, out double _minutes, out double _hours)
        {
            _seconds = Math.Floor (_ns / 10000000);
            _nanoseconds = _ns - _seconds * 10000000;

            SecondsToHours(_seconds ,out _seconds, out _minutes, out _hours);
            
        }

        public static void SecondsToHours(double _s, out double _seconds, out double _minutes, out double _hours)
        {
            _minutes = Math.Floor(_s / 60);
            _seconds = _s - _minutes * 60;

            MinutesToHours(_minutes, out _minutes, out _hours);
        }

        public static void MinutesToHours(double _m, out double _minutes, out double _hours)
        {
            _hours = Math.Floor(_m / 60);
            _minutes = _m - _hours * 60;
        }

        public static string TimeUnitsToString(double _hours, double _minutes, double _seconds)
        {
            string lengthStr = "";

            if (_hours > 0)
            {
                lengthStr = String.Format("{0}:", _hours);

                if (_minutes > 0)
                {
                    if (_minutes < 10)
                        lengthStr += "0";

                    lengthStr += String.Format("{0}:", _minutes);
                }
                else
                    lengthStr += "00:";
            }
            else if (_minutes > 0)
                lengthStr += String.Format("{0}:", _minutes);


            if (_minutes > 0 && _seconds < 10)
                lengthStr += "0";

             lengthStr += String.Format("{0}", _seconds);

            return lengthStr;
        }
    }
}
