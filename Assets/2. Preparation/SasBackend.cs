using UnityEngine;
using System.Collections.Generic;
using SPStudios.Tools;
public class SasBackend :  MonoSingleton<SasBackend> 
{
	const float mTimeSpwan = 5.0f;
	const float mHeightSpwan = 5.0f;

	private string mCurrent = null;
	private readonly SasLayerData mLayerData = new SasLayerData ();

	public SasLayerActor layerActor = null;
	public SasLayerRes layerBg = null;
	public SasLayerData layerData { get{ return mLayerData; } }

	public string current { 
		get { return mCurrent; }
		set {
			if (LoadDatas (value))
				mCurrent = value;
		}
	}

	protected override void OnInitOrAwake ()
	{
		LoadCSVTable ();
		current = "unnamed";
	}

	private void LoadCSVTable()
	{
		var dataChar = Resources.Load ("[SasRPG] Character table") as TextAsset;
		var dataEquip = Resources.Load ("[SasRPG] Equips") as TextAsset;
		var dataMagic = Resources.Load ("[SasRPG] Magics") as TextAsset;
		var dataSpend = Resources.Load ("[SasRPG] Spend item") as TextAsset;

		{
			Sas.DataSet.characters = Sas.Uti.ConvertCSVToList<Sas.Data.DataChar>(dataChar.text);
			Sas.DataSet.equips = Sas.Uti.ConvertCSVToList<Sas.Data.ItemEquip>(dataEquip.text);
			Sas.DataSet.magics = Sas.Uti.ConvertCSVToList<Sas.Data.ItemMagic>(dataMagic.text);
			Sas.DataSet.spends = Sas.Uti.ConvertCSVToList<Sas.Data.ItemSpend>(dataSpend.text);
			Sas.DataSet.Build ();
		}
	}

	private bool LoadDatas(string filename)
	{
		var file = Resources.Load (filename) as TextAsset;
		if (null == file)
			return false;

		var data = JsonFx.Json.JsonReader.Deserialize<Sas.Protocols.Map> (file.text);

		layerData.serialization = data.tile;

		{
			// set Actor Layer
			layerActor.data = layerData;
			layerActor.seialize = data.actor;
			layerActor.Apply ();
		}

		{
			// set Background Layer
			layerBg.data = layerData;
			layerBg.objMap = data.bgs.ConvertAll ((path) => (GameObject)Resources.Load (path));

			layerBg.Apply ();
		}

		return true;
	}

}
