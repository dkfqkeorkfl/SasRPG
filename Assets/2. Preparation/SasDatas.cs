using UnityEngine;
using System.Collections.Generic;
using JAB.CSV;
namespace Sas.Data
{
	public class DataChar
	{
		[CSVPositionAttribute(0)] public ushort idx { get; set; }
		[CSVPositionAttribute(1)] public ushort level { get; set; }
		[CSVPositionAttribute(2)] public uint   exp { get; set; }

		[CSVPositionAttribute(3)] public int    hp { get; set; }
		[CSVPositionAttribute(4)] public int    attack { get; set; }
		[CSVPositionAttribute(5)] public float  attackSpeed { get; set; }
		[CSVPositionAttribute(6)] public int    shield { get; set; }
		[CSVPositionAttribute(7)] public int    cure { get; set; }

		[CSVPositionAttribute(8)] public string res { get; set; }

		public int key { get { return MakeKey (idx, level); } }

		public static int MakeKey(ushort idx, ushort level)
		{
			return idx << 16 | level;
		}
	}

	public abstract class DataItem
	{
		[CSVPositionAttribute(0)] public ushort idx { get; set; }
		[CSVPositionAttribute(1)] public ushort capacity { get; set; }

		[CSVPositionAttribute(2)] public string name { get; set; }
		[CSVPositionAttribute(3)] public string desc { get; set; }
		[CSVPositionAttribute(4)] public string icon { get; set; }
	}

	public class ItemSpend : DataItem
	{
		[CSVPositionAttribute(5)] public int   attack { get; set; }
		[CSVPositionAttribute(6)] public int   shield { get; set; }
		[CSVPositionAttribute(7)] public int   cure { get; set; }
		[CSVPositionAttribute(8)] public float duration { get; set; }
		[CSVPositionAttribute(9)] public float cooldown { get; set; }
	}
		
	public class ItemEquip : DataItem
	{
		public enum eParts
		{
			Weapon, Armor, Count
		}
	
		public eParts part { get; set; }

		[CSVPositionAttribute(5)] public string spart { set { part = (eParts)System.Enum.Parse (typeof(eParts), value); } }
		[CSVPositionAttribute(6)] public int   hp { get; set; }
		[CSVPositionAttribute(7)] public int   attack { get; set; }
		[CSVPositionAttribute(8)] public float attack_speed { get; set; }
		[CSVPositionAttribute(9)] public int   shield { get; set; }
		[CSVPositionAttribute(10)] public int  cure { get; set; }
		void Sample()
		{
			
		}
	}

	public class ItemMagic : DataItem
	{
		public enum eType
		{
			Curse, 
			Protection, 
			Attack
		}

		public eType type { get; set; }

		[CSVPositionAttribute(5)] public string stype { set { type = (eType)System.Enum.Parse (typeof(eType), value); } }
		[CSVPositionAttribute(6)] public int   attack { get; set; }
		[CSVPositionAttribute(7)] public float attackSpeed { get; set; }
		[CSVPositionAttribute(8)] public int   shield { get; set; }
		[CSVPositionAttribute(9)] public int   cure { get; set; }
		[CSVPositionAttribute(10)] public float duration { get; set; }
		[CSVPositionAttribute(11)] public float cooldown { get; set; }
	}

	public class ItemPrefix
	{
		[CSVPositionAttribute(0)] public ushort idx { get; set; }
		[CSVPositionAttribute(1)] public int    bonusAttack { get; set; }
		[CSVPositionAttribute(2)] public float  bonusAttackSpeed { get; set; }
		[CSVPositionAttribute(3)] public int    bonusShield { get; set; }
		[CSVPositionAttribute(4)] public int    bonusHP { get; set; }
		[CSVPositionAttribute(5)] public int    bonusCure { get; set; }
	}


}

namespace Sas
{
	public static class DataSet
	{
		private static List<Data.DataChar>   mChars   = null;
		private static List<Data.ItemEquip>  mEquips  = null;
		private static List<Data.ItemSpend>  mSpends  = null;
		private static List<Data.ItemMagic>  mMagics  = null;
		private static List<Data.ItemPrefix> mPrefixs = null;
		private static readonly List<Data.DataItem> mItems = new List<Data.DataItem>();

		public static List<Data.DataChar>   characters { set; get; }
		public static List<Data.ItemEquip>  equips     { set; get; }
		public static List<Data.ItemSpend>  spends     { set; get; }
		public static List<Data.ItemMagic>  magics     { set; get; }
		public static List<Data.ItemPrefix> prefixs    { set; get; }

		public static Data.DataItem GetItem(ushort idx)
		{
			int index = mItems.BinarySearchForMatch ((val) => val.idx.CompareTo (idx));
			return index < 0 ? null : mItems [index];
		}

		public static Data.ItemSpend GetSpend(ushort idx)
		{
			int index = mSpends.BinarySearchForMatch ((val) => val.idx.CompareTo (idx));
			return index < 0 ? null : mSpends [index];
		}

		public static Data.DataChar GetChar(ushort idx, ushort level)
		{
			int key = Data.DataChar.MakeKey (idx, level);
			int index = mChars.BinarySearchForMatch ((val) => val.key.CompareTo (key));
			return index < 0 ? null : mChars [index];
		}

		public static Data.ItemEquip GetEquip(ushort idx)
		{
			int index = mEquips.BinarySearchForMatch ((val) => val.idx.CompareTo (idx));
			return index < 0 ? null : mEquips [index];
		}

		public static Data.ItemMagic GetMagic(ushort idx)
		{
			int index = mMagics.BinarySearchForMatch ((val) => val.idx.CompareTo (idx));
			return index < 0 ? null : mMagics [index];
		}

		public static Data.ItemPrefix GetPrefix(ushort idx)
		{
			int index = mPrefixs.BinarySearchForMatch ((val) => val.idx.CompareTo (idx));
			return index < 0 ? null : mPrefixs [index];
		}

		public static void Build()
		{
			mChars = characters;
			mEquips = equips;
			mMagics = magics;
			mPrefixs = prefixs;
			mSpends = spends;
			mItems.AddRange (mEquips.ConvertAll ((val) => val as Data.DataItem));
			mItems.AddRange (mMagics.ConvertAll ((val) => val as Data.DataItem));
			mItems.AddRange (mSpends.ConvertAll ((val) => val as Data.DataItem));

			if(null != mItems) mItems.Sort ((lhs, rhs) => lhs.idx.CompareTo (rhs.idx));
			if(null != mEquips) mEquips.Sort ((lhs, rhs) => lhs.idx.CompareTo (rhs.idx));
			if(null != mMagics) mMagics.Sort ((lhs, rhs) => lhs.idx.CompareTo (rhs.idx));
			if(null != mSpends) mSpends.Sort ((lhs, rhs) => lhs.idx.CompareTo (rhs.idx));
			if(null != mChars) mChars.Sort ((lhs, rhs) => lhs.key.CompareTo (rhs.key));
			if(null != mPrefixs) mPrefixs.Sort ((lhs, rhs) => lhs.idx.CompareTo (rhs.idx));
		}
	}
}