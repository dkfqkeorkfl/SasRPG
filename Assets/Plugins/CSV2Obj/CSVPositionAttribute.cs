using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// from RUL : http://www.codeproject.com/Articles/14254/Converting-CSV-Data-to-Objects
/// this is be custom - Utility Class to Extension method, remove Try-Catch blocks
/// </summary>
namespace JAB.CSV
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CSVPositionAttribute : System.Attribute 
    {
        public int Position;
        public string DataTransform = string.Empty;

        public CSVPositionAttribute(int position,string dataTransform)
        {
            Position = position;
            DataTransform = dataTransform;
        }

        public CSVPositionAttribute(int position)
        {
            Position = position;
        }

        public CSVPositionAttribute()
        {
        }

    }
}
