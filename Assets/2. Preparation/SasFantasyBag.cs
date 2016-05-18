using UnityEngine;
using System.Collections.Generic;
using Sas;

public class SasFantasyBag
{
	public class InstancedItem
	{
		private int mSerial = 0;
		private int mSize = 0;

		private System.DateTime mCreated = System.DateTime.MinValue;
		private System.DateTime mUpdated = System.DateTime.MinValue;
		private Sas.Data.ItemPrefix mPrefix = null;
		private Sas.Data.DataItem mData = null;
		private Sas.Data.ItemMagic mMagic = null;
		private Sas.Data.ItemSpend mSpend = null;
		private Sas.Data.ItemEquip mEquip = null;

		public int             serial     { get { return mSerial; } set { mSerial = value; } }
		public System.DateTime created    { get { return mCreated; } set { mCreated = value; } }
		public System.DateTime updated    { get { return mUpdated; } set { mUpdated = value; } }
		public Sas.Data.ItemPrefix prefix { get { return mPrefix; } set { if(equip != null) mPrefix = value; } }
		public Sas.Data.ItemMagic magic   { get { return mMagic; } }
		public Sas.Data.ItemSpend spend   { get { return mSpend; } }
		public Sas.Data.ItemEquip equip   { get { return mEquip; } }
		public int hp { 
			get { return equip != null ? (equip.hp + (prefix != null ? prefix.bonusHP : 0)) : 0; }
		}
		public int attk {
			get { 
				if (equip != null)
					return equip.attack + (prefix != null ? prefix.bonusAttack : 0);
				else if (magic != null)
					return magic.attack;
				else if (spend != null)
					return spend.attack;
				return 0;
			}
		}
		public int shield {
			get {
				if (equip != null)
					return equip.shield + (prefix != null ? prefix.bonusShield : 0);
				else if (magic != null)
					return magic.shield;
				else if (spend != null)
					return spend.shield;
				return 0;
			}
		}
		public int cure {
			get {
				if (equip != null)
					return equip.cure + (prefix != null ? prefix.bonusCure : 0);
				else if (magic != null)
					return magic.cure;
				else if (spend != null)
					return spend.cure;
				return 0;
			}
		}
		public float attkSpeed {
			get {
				if (equip != null)
					return equip.attack_speed + (prefix != null ? prefix.bonusAttackSpeed : 0);
				return 0;
			}
		}
		public int size    
		{ 
			get { return mSize; } 
			set { mSize = value > mData.capacity ? mData.capacity : value; } 
		}
		public Sas.Data.DataItem data 
		{ 
			get { return mData; }
			set { 
				mData = value; 
				mMagic = value as Sas.Data.ItemMagic;
				mSpend = value as Sas.Data.ItemSpend;
				mEquip = value as Sas.Data.ItemEquip;
				size = size;
			} 
		}

		static InstancedItem()
		{
			SasPool<InstancedItem>.Instance.release += InstancedItem.Release;
		}

		public static void Release(InstancedItem item)
		{
			item.mSerial = 0;
			item.mSize = 0;
			item.mCreated = System.DateTime.MinValue;
			item.mUpdated = System.DateTime.MinValue;

			item.mData = null;
			item.mEquip = null;
			item.mMagic = null;
			item.mSpend = null;
			item.mPrefix = null;
		}
	}

	public enum eRtn
	{
		Success, NonExist, Exist, Full, Fail
	}

	private readonly InstancedItem[] mEquips = new InstancedItem[(int)Sas.Data.ItemEquip.eParts.Count];
	private readonly Dictionary<ushort, Sas.Data.ItemMagic> mMagics = new Dictionary<ushort, Sas.Data.ItemMagic> ();
	private readonly LinkedList<InstancedItem> mBag = new LinkedList<InstancedItem>();
	private int capacity { set; get; }

	/// <summary>
	/// first generic val : equiped item
	/// secend generic val : new item
	/// </summary>
	public event System.Action<InstancedItem, InstancedItem> onEquip;
	public event System.Action<Sas.Data.ItemMagic> onLearn;

	static SasFantasyBag()
	{
		SasPool<SasFantasyBag>.Instance.release += SasFantasyBag.Release;
	}

	public InstancedItem this[Sas.Data.ItemEquip.eParts part]
	{
		get { return GetEquip (part); }
	}

	public void Init(IEnumerable<InstancedItem> bag, IEnumerable<int> equips, IEnumerable<ushort> magics)
	{
		Release (this);
		foreach (var item in bag) 
			Push (item);
		foreach (var equip in equips)
			Spend (equip);
		foreach (var magic in magics)
			mMagics.Add (magic, Sas.DataSet.GetMagic (magic));
	}

	public InstancedItem GetEquip(Sas.Data.ItemEquip.eParts part)
	{
		return part != Sas.Data.ItemEquip.eParts.Count ? mEquips [(int)part] : null;
	}

	public Sas.Data.ItemMagic GetMagic(ushort idx)
	{
		Sas.Data.ItemMagic magic = null;
		return mMagics.TryGetValue (idx, out magic) ? magic : null;
	}

	public Sas.Data.DataItem Spend(int serial)
	{
		var iter = mBag.Find ((val) => val.serial == serial);
		if (null == iter)
			return null;
		
		InstancedItem item = iter.Value;
		if (null != item.equip) {
			int index = (int)item.equip.parts;
			InstancedItem equiped = mEquips [index];
			mEquips [index] = mEquips [index] != item ? item : null;
			onEquip (equiped, mEquips [index]);
			return null;
		}
			
		if (null != item.magic && !mMagics.ContainsKey (item.magic.idx)) {
			mMagics.Add (item.magic.idx, item.magic);
			onLearn (item.magic);
			PopImpl (item, 1);
			return null;
		}
			
		if (null != item.spend) {
			Sas.Data.DataItem spend = item.spend;
			PopImpl (item, 1);
			return spend;
		}

		return null;
	}

	public eRtn Pop(int serial, ushort size = ushort.MaxValue /* it means everything */)
	{
		var iter = mBag.Find ((val) => val.serial == serial);
		if (null == iter)
			return eRtn.NonExist;

		PopImpl (iter.Value, size);
		return eRtn.Success;
	}

	public eRtn Push(InstancedItem item)
	{
		int serial = item.serial;
		var iter = mBag.Find ((val) => val.serial == serial);
		if (null != iter)
			return eRtn.Exist;

		eRtn rtn = eRtn.Fail;
		if (item.equip != null) {
			rtn = Push (item);
		} 
		else if (item.spend != null || item.magic != null) {
			iter = mBag.Find ((val) => val.data == item.data);
			if (null != iter) {
				iter.Value.size += item.size;
				iter.Value.updated = item.created;
				rtn = eRtn.Success;
			} 
			else {
				rtn = PushImpl (item);
			}
		}
		return rtn;
	}

	private void PopImpl(InstancedItem item, ushort size)
	{
		if (item.size > size) {
			item.size -= size;
		} 
		else {
			
			if(item.equip != null) {
				int index = System.Array.FindIndex (mEquips, (val) => val.serial == item.serial);
				if (index < mEquips.Length)
					mEquips [index] = null;
			}
				
			mBag.Remove (mBag.Find ((val) => val == item));
			SasPool<InstancedItem>.Instance.Dealloc (item);
		}
	}

	private eRtn PushImpl(InstancedItem item)
	{
		if (mBag.Count < capacity == false)
			return eRtn.Full;

		mBag.AddLast (item);
		return eRtn.Success;

	}

	public static void Release(SasFantasyBag bag)
	{
		var iter = bag.mBag.First;
		while (iter != null) {
			SasPool<InstancedItem>.Instance.Dealloc (iter.Value);
			iter = iter.Next;
		}

		for (int i = 0; i < bag.mEquips.Length; ++i)
			bag.mEquips [i] = null;
		bag.mMagics.Clear ();
	}
}
