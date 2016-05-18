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
			return idx << sizeof(ushort) | level;
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
	
		[CSVPositionAttribute(5)] public eParts parts { get; set; }
		[CSVPositionAttribute(6)] public int    hp { get; set; }
		[CSVPositionAttribute(7)] public int    attack { get; set; }
		[CSVPositionAttribute(8)] public float  attack_speed { get; set; }
		[CSVPositionAttribute(9)] public int    shield { get; set; }
		[CSVPositionAttribute(10)] public int    cure { get; set; }
	}

	public class ItemMagic : DataItem
	{
		public enum eType
		{
			Curse, 
			Protection, 
			Attack
		}

		[CSVPositionAttribute(5)] public eType type { get; set; }
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
		private static IList<Data.DataChar>   mChars  = null;
		private static IList<Data.ItemEquip>  mEquip  = null;
		private static IList<Data.ItemSpend>  mSpend  = null;
		private static IList<Data.ItemMagic>  mMagic  = null;
		private static IList<Data.ItemPrefix> mPrefix = null;

		public static IList<Data.DataChar>   characters { set; get; }
		public static IList<Data.ItemEquip>  equip      { set; get; }
		public static IList<Data.ItemSpend>  spend      { set; get; }
		public static IList<Data.ItemMagic>  magic      { set; get; }
		public static IList<Data.ItemPrefix> prefix     { set; get; }


		public static Data.ItemSpend GetSpend(ushort idx)
		{
			int index = mSpend.BinarySearchForMatch ((val) => val.idx.CompareTo (idx));
			return index < mSpend.Count ? mSpend [index] : null;
		}

		public static Data.DataChar GetChar(ushort idx, ushort level)
		{
			int key = Data.DataChar.MakeKey (idx, level);
			int index = mChars.BinarySearchForMatch ((val) => val.key.CompareTo (key));
			return index < mChars.Count ? mChars [index] : null;
		}

		public static Data.ItemEquip GetEquip(ushort idx)
		{
			int index = mEquip.BinarySearchForMatch ((val) => val.idx.CompareTo (idx));
			return index < mEquip.Count ? mEquip [index] : null;
		}

		public static Data.ItemMagic GetMagic(ushort idx)
		{
			int index = mMagic.BinarySearchForMatch ((val) => val.idx.CompareTo (idx));
			return index < mMagic.Count ? mMagic [index] : null;
		}

		public static Data.ItemPrefix GetPrefix(ushort idx)
		{
			int index = mPrefix.BinarySearchForMatch ((val) => val.idx.CompareTo (idx));
			return index < mPrefix.Count ? mPrefix [index] : null;
		}

		public static void Build()
		{
			mChars = characters;
			mEquip = equip;
			mMagic = magic;
			mPrefix = prefix;
			mSpend = spend;

			mEquip.Sort ((lhs, rhs) => lhs.idx.CompareTo (rhs));
			mMagic.Sort ((lhs, rhs) => lhs.idx.CompareTo (rhs));
			mPrefix.Sort ((lhs, rhs) => lhs.idx.CompareTo (rhs));
			mChars.Sort ((lhs, rhs) => lhs.key.CompareTo (rhs.key));
			mSpend.Sort ((lhs, rhs) => lhs.idx.CompareTo (rhs));
		}
	}
}