using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

/// <summary>
/// from RUL : http://www.codeproject.com/Articles/14254/Converting-CSV-Data-to-Objects
/// this is be custom - Utility Class to Extension method, remove Try-Catch blocks
/// </summary>
namespace JAB.CSV
{
    public static class ClassLoader
    {
        public static T ToObj<T>(this string[] fields)
        {           
            T tempObj = (T) Activator.CreateInstance(typeof(T));
			fields.Load(tempObj);
            return tempObj;
        }

		public static void Load(this string[] fields, object target)
        {
            Type targetType = target.GetType();
            PropertyInfo[] properties = targetType.GetProperties();

            // Loop through properties
            foreach (PropertyInfo property in properties)  
            {
				// Make sure the property is writeable (has a Set operation)
				if (property.CanWrite == false)
					continue;
                
                // find CSVPosition attributes assigned to the current property
                object[] attributes = property.GetCustomAttributes(typeof(CSVPositionAttribute), false);
                
                // if Length is greater than 0 we have at least one CSVPositionAttribute
				if (attributes.Length > 0 == false)
					continue;

                // We will only process the first CSVPositionAttribute
                CSVPositionAttribute positionAttr = (CSVPositionAttribute)attributes[0];
                
                //Retrieve the postion value from the CSVPositionAttribute
                int position = positionAttr.Position;

                // get the CSV data to be manipulate and written to object
                object data = fields[position];

                // check for a Tranform operation that needs to be executed
                if (positionAttr.DataTransform != string.Empty)
                {
                    // Get a MethodInfo object pointing to the method declared by the
                    // DataTransform property on our CSVPosition attribute
                    MethodInfo method = targetType.GetMethod(positionAttr.DataTransform);
                    
                    // Invoke the DataTransform method and get the newly formated data
                    data = method.Invoke(target, new object[] { data });
                }
                // set the ue on our target object with the data
                property.SetValue(target, Convert.ChangeType(data, property.PropertyType), null);
                    
            }
        }

    } // end class 
} // end namespace
