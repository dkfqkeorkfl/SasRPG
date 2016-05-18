using UnityEngine;
using System.Collections.Generic;
using SPStudios.Tools;
using JAB.CSV;
using Sas;
public class SasBackend :  MonoSingleton<SasBackend> 
{

	protected override void OnInitOrAwake ()
	{
		var dataChar = Resources.Load ("[SasRPG] Character table") as TextAsset;
		var dataEquip = Resources.Load ("[SasRPG] Equips") as TextAsset;
		var dataMagic = Resources.Load ("[SasRPG] Magics") as TextAsset;
		var dataSpend = Resources.Load ("[SasRPG] Spend item") as TextAsset;

		{
			Sas.DataSet.characters = ConvertCSVToList<Sas.Data.DataChar>(dataChar.text);
			Sas.DataSet.equip = ConvertCSVToList<Sas.Data.ItemEquip>(dataEquip.text);
			Sas.DataSet.magic = ConvertCSVToList<Sas.Data.ItemMagic>(dataMagic.text);
			Sas.DataSet.spend = ConvertCSVToList<Sas.Data.ItemSpend>(dataSpend.text);
			Sas.DataSet.Build ();
		}
	}

	private List<T> ConvertCSVToList<T>(string str)
	{
		
		var rows = CsvParser2.Parse (str);
		int size = rows.GetLength (0);
		var datas = new List<T> ();
		datas.Resize (size);
		for (int i = 1; i < size; ++i)
			datas[i] = rows [i].ToObj<T> ();

		return datas;
	}
}
