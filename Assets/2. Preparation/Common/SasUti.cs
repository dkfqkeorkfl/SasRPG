using UnityEngine;
using System.Collections;
using Sas;
using JAB.CSV;

namespace Sas
{
	public static class Uti
	{
		public static System.Collections.Generic.List<T> ConvertCSVToList<T>(string str)
		{
			var rows = CsvParser2.Parse (str);
			int size = rows.GetLength (0)-1;
			var datas = new System.Collections.Generic.List<T> ();
			datas.Resize (size);
			for (int i = 0; i < size; ++i)
				datas[i] = rows [i+1].ToObj<T> ();

			return datas;
		}

		public static Detail.Duration<T> MakeDuration<T>(System.Action<T> action, T _old, T _new)
		{
			return new Detail.Duration<T> (action, _old, _new);
		}

		public static bool SaveBin(string filepath, object obj)
		{
			using (var file = System.IO.File.Create (filepath)) {
				var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				formatter.Serialize (file, obj);
			}

			return true;
		}

		public static T LoadBin<T>(string filepath)
		{
			using (var file = System.IO.File.OpenRead (filepath)) {
				var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				return (T)formatter.Deserialize(file);
			}
		}

		public static bool SaveJson(string filepath, object obj)
		{
			string prefix = "Assets/";
			using ( var file = System.IO.File.CreateText (prefix+filepath) ) {
				JsonFx.Json.JsonWriterSettings settings = new JsonFx.Json.JsonWriterSettings();
				settings.PrettyPrint = true;
				JsonFx.Json.JsonWriter jw = new JsonFx.Json.JsonWriter (file,settings);
				jw.Write (obj);
				UnityEditor.AssetDatabase.Refresh ();
			}
			return true;
		}

		public static T LoadJson<T>(string filepath)
		{
			string serialization = System.IO.File.ReadAllText (filepath);
			if (string.IsNullOrEmpty (serialization))
				return default(T);

			var rtn = JsonFx.Json.JsonReader.Deserialize<T> (serialization);
			return rtn;
		}

		public static bool IsMaskVal(int val, int mask, int key)
		{
			return (val & mask) == (key & mask);
		}

		public static bool IsMaskZero(int val, int mask)
		{
			return (val & mask) == 0;
		}

		public static int SetMaskVal(int val, int mask, int key)
		{
			return (~mask & val) | (key & mask);
		}

		public static int SetMaskAll(int val, int mask)
		{
			return val | mask;
		}

		public static int UnsetMaskAll(int val, int mask)
		{
			return ~mask & val;
		}

		public static int GetMaskVal(int val, int mask)
		{
			return val & mask;
		}
	}
}

//public static class SasUti{
//
//	public static bool SaveBin(string filepath, object obj)
//	{
//		using (var file = System.IO.File.Create (filepath)) {
//			var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
//			formatter.Serialize (file, obj);
//		}
//
//		return true;
//	}
//
//	public static T LoadBin<T>(string filepath)
//	{
//		using (var file = System.IO.File.OpenRead (filepath)) {
//			var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
//			return (T)formatter.Deserialize(file);
//		}
//	}
//
//	public static bool SaveJson(string filepath, object obj)
//	{
//		using ( var file = System.IO.File.CreateText (filepath) ) {
//			string serialization = JsonFx.Json.JsonWriter.Serialize (obj);
//			file.Write (serialization);
//		}
//		return true;
//	}
//
//	public static T LoadJson<T>(string filepath)
//	{
//		string serialization = System.IO.File.ReadAllText (filepath);
//		if (string.IsNullOrEmpty (serialization))
//			return default(T);
//		
//		var rtn = JsonFx.Json.JsonReader.Deserialize<T> (serialization);
//		return rtn;
//	}
//
//	public static bool IsMaskVal(int val, int mask, int key)
//	{
//		return (val & mask) == (key & mask);
//	}
//
//	public static bool IsMaskZero(int val, int mask)
//	{
//		return (val & mask) == 0;
//	}
//
//	public static int SetMaskVal(int val, int mask, int key)
//	{
//		return (~mask & val) | (key & mask);
//	}
//
//	public static int SetMaskAll(int val, int mask)
//	{
//		return val | mask;
//	}
//
//	public static int UnsetMaskAll(int val, int mask)
//	{
//		return ~mask & val;
//	}
//
//	public static int GetMaskVal(int val, int mask)
//	{
//		return val & mask;
//	}
//}
