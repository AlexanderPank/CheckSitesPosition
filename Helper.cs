using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckPosition
{
    internal class Helper
    {
        public static string getStringValue(object value)
        {
            try { return value.ToString(); } catch (Exception) { return ""; }
        }
        public static int getIngValue(object value, int defValue = 0)
        {
            try { return int.Parse(value.ToString()); } catch (Exception) { return defValue; }
        }
    }
}
