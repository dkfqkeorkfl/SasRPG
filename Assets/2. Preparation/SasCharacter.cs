using UnityEngine;
using System.Collections.Generic;
using Sas.Data;
using Sas;
using System;

public class SasCharacter
{
	private ushort mCharacter = 0;
	private ushort mLevel = 0;
	private uint   mExp = 0;

	public event Action<ushort> levelUp;
	public DataChar cur         { get; private set; }
	public DataChar next        { get; private set; }
	public bool     hasNext     { get { return next != null; } }
	public uint     expRequired { get { return hasNext ? next.exp - exp : 0; } }
	public ushort   character   { 
		get { return mCharacter; }
		set {
			mCharacter = value;
			level = level;
		}
	}

	public ushort level {
		get { return mLevel; }
		set { 
			cur = next != null && mLevel + 1 == value ? 
				next : Sas.DataSet.GetChar (character, value);
			next = Sas.DataSet.GetChar (character, (ushort)(value + 1));
			mLevel = value; 
		}
	}

	public uint exp 
	{
		get { return mExp; }
		set { 
			mExp = value;
			while (hasNext && mExp >= next.exp) {
				level += 1;
				levelUp	(level);
			}

			if (!hasNext)
				mExp = cur.exp;
		}
	}

	public string resource    { get { return cur.res; } }
	public int    hp          { get { return cur.hp; } }
	public int    attack      { get { return cur.attack; } }
	public int    shield      { get { return cur.shield; } }
	public int    cure        { get { return cur.cure; } }
	public float  attackSpeed { get { return cur.attackSpeed; } }
}

public class SasActorData
{
	/// <summary>
	/// Buff Instanced
	/// </summary>
	public class Buf
	{
		private int mOperation = 0;
		private System.DateTime   mStime = System.DateTime.MinValue;
		private Sas.Data.DataItem mData  = null;

		public int                operation { get { return mOperation; } set { mOperation = value; } }
		public System.DateTime    stime     { get { return mStime; }     set { mStime = value; } }
		public Sas.Data.ItemMagic magic     { private set; get; }
		public Sas.Data.ItemSpend spend     { private set; get; }
		public Sas.Data.DataItem  item      { 
			get { return mData; }
			set { 
				mData = value;
				magic = value as Sas.Data.ItemMagic; 
				spend = value as Sas.Data.ItemSpend; 
			} 
		}

		static Buf()
		{
			SasPool<Buf>.Instance.release += Release;
		}
		public static void Release(Buf buf)
		{
			buf.operation = 0;
			buf.stime = System.DateTime.MinValue;
			buf.item = null;
		}
	} // end buf


	private uint mSerial = 0;
	private int mGroup = 0;

	private SasTile<int>.Pos mPt;
	private Vector2  mOffset;
	private int mCurHP = 0;
	private readonly SasCharacter    mChar = new SasCharacter();
	private readonly SasFantasyBag   mBag  = new SasFantasyBag();
	private readonly LinkedList<Buf> mBufs = new LinkedList<Buf> ();

	/// <summary>
	/// bool : Add or Remove
	/// </summary>
	public event System.Action<Buf, bool> onBuf;
	public event System.Action<SasFantasyBag.InstancedItem, SasFantasyBag.InstancedItem> onEquip 
	{ add { mBag.onEquip += value; } remove { mBag.onEquip -= value; } }
	public event System.Action<Sas.Data.ItemMagic> onLearn 
	{ add { mBag.onLearn += value; } remove { mBag.onLearn -= value; } }

	public uint          serial   { get { return mSerial; } }
	public string        resource { get { return mChar.resource; } }

	public SasTile<int>.Pos pt     { get { return mPt; }       set { mPt = value; } }
	public Vector2          offset { get { return mOffset; }   set { mOffset = value; } }
	public uint             exp    { get { return mChar.exp; } set { mChar.exp = value; } }
	public int              group  { get { return mGroup; }    set { mGroup = value; } }
	public int              curHP  { get { return mCurHP; } }
	public SasFantasyBag    bag    { get { return mBag; } }

	public bool isDead { get { return curHP == 0; } }

	public int   totalHP        { get { return baseHP + bonusHP; } }
	public int   totalCure      { get { return baseCure + bonusCure; } }
	public int   totalAttk      { get { return baseAttk + bonusAttk; } }
	public int   totalShield    { get { return baseShield + bonusShield; } }
	public float totalAttkSpeed { get { return baseAttkSpeed + bonusAttkSpeed; } }

	public int   baseHP         { get { return mChar.hp; }  }
	public int   baseCure       { get { return mChar.hp; }  }
	public int   baseAttk       { get { return mChar.attack; } }
	public int   baseShield     { get { return mChar.shield; } }
	public float baseAttkSpeed  { get { 
			return mBag.GetEquip (ItemEquip.eParts.Weapon) != null ? 
				mBag.GetEquip (ItemEquip.eParts.Weapon).attkSpeed : mChar.attackSpeed; } 
	}

	public int   bonusHP        { get; private set; }
	public int   bonusCure      { get; private set; }
	public int   bonusAttk      { get; private set; }
	public int   bonusShield    { get; private set; }
	public float bonusAttkSpeed { get; private set; }

	public SasActorData()
	{
		onEquip += (oldIem, newItem) => {
			if (oldIem != null) {
				bonusHP -= oldIem.hp;
				bonusCure -= oldIem.cure;
				bonusAttk -= oldIem.attk;
				bonusAttkSpeed -= oldIem.attkSpeed;
				bonusShield -= oldIem.shield;
			}

			if (newItem != null) {
				bonusHP += oldIem.hp;
				bonusCure += oldIem.cure;
				bonusAttk += oldIem.attk;
				bonusAttkSpeed += oldIem.attkSpeed;
				bonusShield += oldIem.shield;
			}
		};

		onBuf += (val, enable) => {
			if (enable) {
				bonusCure += val.spend != null ? val.spend.cure : val.magic.cure;
				bonusAttk += val.spend != null ? val.spend.attack : val.magic.attack;
				bonusShield += val.spend != null ? val.spend.shield : val.magic.shield;
			}
			else {
				bonusCure -= val.spend != null ? val.spend.cure : val.magic.cure;
				bonusAttk -= val.spend != null ? val.spend.attack : val.magic.attack;
				bonusShield -= val.spend != null ? val.spend.shield : val.magic.shield;
			}
		};
	}

	public void Init(uint serial, ushort character, ushort level, uint exp = 0, 
		IEnumerable<Buf> bufs = null, 
		IEnumerable<SasFantasyBag.InstancedItem> bag = null, 
		IEnumerable<int> equips = null, IEnumerable<ushort> magics = null)
	{
		this.mSerial = serial;
		mChar.character = character;
		mChar.level = level;
		mChar.exp = exp;

		this.bag.Init (bag, equips, magics);

		ClearBuf ();
		if (null != bufs) {
			foreach(var buf in bufs){
				mBufs.AddLast (buf);
				onBuf (buf, true);
			}
		}

	}

	public void AddBuf(int operation, Sas.Data.DataItem item)
	{
		var iter = mBufs.Find ((val) => val.item == item);
		Buf buf = iter != null ? iter.Value : SasPool<Buf>.Instance.Alloc ();

		buf.item = item;
		buf.stime = System.DateTime.Now;
		buf.operation = operation;
		mBufs.AddLast (buf);
		onBuf (buf, true);
	}

	public void RemoveBuf_If(System.Predicate<Buf> pred)
	{
		LinkedListNode<Buf> node = mBufs.Find (pred);
		if (node == null)
			return;

		mBufs.Remove (node);
		onBuf (node.Value, false);
		SasPool<Buf>.Instance.Dealloc (node.Value);
	}

	public void ClearBuf()
	{
		var iter = mBufs.First;
		while (iter != null) {
			SasPool<Buf>.Instance.Dealloc (iter.Value);
			iter.Value = null;
			iter = iter.Next;
		}
	}

	public static uint MakeSerial()
	{
		return (uint)DateTime.Now.GetHashCode ();
	}
}
