using UnityEngine;
using System.Collections.Generic;
using System;
using Sas;

[Serializable]
public class SasTile
{
//	[UP_LEFT]   [UP]     [UP_RIGHT]
//	[LEFT]      [ORIGIN] [RIGHT]
//	[DOWN_LEFT] [DOWN]   [DOWN_RIGHT]

	public enum eDirection
	{
		Up = 0, 
		UpRight = 1, 
		Right = 2, 
		DownRight = 3, 
		Down = 4, 
		DownLeft = 5, 
		Left = 6, 
		UpLeft = 7,
		COUNT = 8
	}

	public struct PosAtti : IEnumerable<int>
	{
		private const int DEF_MASK = 0x07; // 0000 0111
		private const int DEF_MASK_COUNT = 0x0f << ((int)eDirection.COUNT * DEF_MASK_OFFSET_SIZE);
		private const int DEF_MASK_OFFSET_SIZE = 3;

		public  static readonly BuilderArg DIRECTION_UP        = eDirection.Up;
		public  static readonly BuilderArg DIRECTION_UPRIGHT   = eDirection.UpRight;
		public  static readonly BuilderArg DIRECTION_RIGHT     = eDirection.Right;
		public  static readonly BuilderArg DIRECTION_DOWNRIGHT = eDirection.DownRight;
		public  static readonly BuilderArg DIRECTION_DOWN      = eDirection.Down;
		public  static readonly BuilderArg DIRECTION_DOWNLEFT  = eDirection.DownLeft;
		public  static readonly BuilderArg DIRECTION_LEFT      = eDirection.Left;
		public  static readonly BuilderArg DIRECTION_UPLEFT    = eDirection.UpLeft;
		public  static readonly PosAtti POS_ATTI_DIRECTION_ALL = 
			DIRECTION_UP + DIRECTION_UPRIGHT + DIRECTION_RIGHT +
			DIRECTION_DOWNRIGHT + DIRECTION_DOWN + DIRECTION_DOWNLEFT +
			DIRECTION_LEFT + DIRECTION_UPLEFT;

		private int mVal; // = 0;
		public PosAtti(BuilderArg arg)
		{
			mVal = 0;
			Push(arg);
		}

		public PosAtti(BuilderArg arg1, BuilderArg arg2) 
		{
			mVal = 0;
			Push(arg1);
			Push(arg2);
		}

		public int count {
			get { return Sas.Uti.GetMaskVal (mVal, DEF_MASK_COUNT) >> ((int)eDirection.COUNT * DEF_MASK_OFFSET_SIZE); }
			private set { mVal = Sas.Uti.SetMaskVal (mVal, DEF_MASK_COUNT, value << ((int)eDirection.COUNT * DEF_MASK_OFFSET_SIZE)); }
		}

		public void Push(BuilderArg arg)
		{
			int count = this.count;
			if ((int)eDirection.COUNT < count) {
				Debug.LogError ("[SasTile.error] Iterator cannot exceed a size 8.");
				return;
			}

			int mask = DEF_MASK << (DEF_MASK_OFFSET_SIZE * count);
			mVal = Sas.Uti.SetMaskVal( mVal, mask, (int)arg.mDirection << (DEF_MASK_OFFSET_SIZE * count));
			this.count = ++count;

		}

		public static PosAtti operator + (PosAtti b, BuilderArg arg)
		{
			b.Push (arg);
			return b;
		}

		public int GetIdx(int idx)
		{
			int mask = DEF_MASK << (DEF_MASK_OFFSET_SIZE * idx);
			return Sas.Uti.GetMaskVal (mVal, mask) >> (DEF_MASK_OFFSET_SIZE * idx);
		}

		public IEnumerator<int> GetEnumerator()
		{
			int count = this.count;
			for (int i = 0; i < count; ++i) {
				yield return GetIdx(i);
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public struct BuilderArg
	{
		public eDirection mDirection;
		public BuilderArg(eDirection direction) { this.mDirection = direction; }

		public static PosAtti operator + (BuilderArg arg1, BuilderArg arg2) { return new PosAtti(arg1, arg2); }
		public static PosAtti operator + (BuilderArg arg1, eDirection arg2) { return new PosAtti(arg1, arg2); }
		public static implicit operator BuilderArg(eDirection val) { return new BuilderArg { mDirection = val }; }
	}

	protected static readonly Point[] RESERVE_POINT = new Point[(int)eDirection.COUNT];

	static SasTile()
	{
		RESERVE_POINT [(int)eDirection.Up]        = new Point (0, -1);
		RESERVE_POINT [(int)eDirection.UpRight]   = new Point (1, -1);
		RESERVE_POINT [(int)eDirection.Right]     = new Point (1, 0);
		RESERVE_POINT [(int)eDirection.DownRight] = new Point (1, 1);
		RESERVE_POINT [(int)eDirection.Down]      = new Point (0, 1);
		RESERVE_POINT [(int)eDirection.DownLeft]  = new Point (-1, 1);
		RESERVE_POINT [(int)eDirection.Left]      = new Point (-1, 0);
		RESERVE_POINT [(int)eDirection.UpLeft]    = new Point (-1, -1);
	}
}

[Serializable]
public class SasTile<T> : SasTile
{
	public struct Pos : IEnumerable<T>
	{
		private SasTile<T> tile { get; set; }
		public PosAtti builder { get; private set; }
		public Point pt { get; private set; }
		public int   count  { get { return builder.count; } }
		public bool invalid { get { return tile == null; } }

		public Pos(SasTile<T> tile, Point pos, PosAtti builder /* = POS_ATTI_DIRECTION_ALL */ ) 
		{
			this.tile = tile;
			this.pt = pos;
			this.builder = builder;
		}

		public void Move(eDirection dir)
		{
			pt += RESERVE_POINT [(int)dir];
		}

		public bool IsPos(eDirection dir)
		{
			Point pt = this.pt + RESERVE_POINT [(int)dir];
			return tile.size.Contain (pt);
		}

		public eDirection GetDir (int idx)
		{
			return idx < builder.count ? (eDirection)builder.GetIdx (idx) : eDirection.COUNT;
		}
				
		public eDirection TryNext (int idx, out Pos rtn)
		{
			rtn = new Pos (null, Point.zero, builder);
			if (!(idx < builder.count)) 
				return eDirection.COUNT;

			eDirection dir = (eDirection)builder.GetIdx (idx);
			if (!IsPos(dir))
				return eDirection.COUNT;

			rtn = GetPos(dir);
			return dir;
		}

		public IEnumerator<T> GetEnumerator()
		{
			yield return tile [pt.x, pt.y];

			int count = builder.count;
			for (int i = 0; i < count; ++i) {
				Point pt = this.pt + RESERVE_POINT [builder.GetIdx (i)];
				if (!tile.size.Contain (pt)) continue;
				yield return tile [pt.x, pt.y];
			}

		}
			
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private Pos GetPos(eDirection dir)
		{
			Point pt = this.pt + RESERVE_POINT [(int)dir];
			return new Pos (tile, pt, builder);
		}
	}

	public static readonly SasTile<T> zero = new SasTile<T>();

	[SerializeField] private T[,]  mTile = null;

	public Point size { get; private set; }
	public T[,] tile { 
		get { return mTile; }
		set { 
			mTile = value; 
			size = new Point { x = tile.GetLength (0), y = tile.GetLength (1) };
		}
	}

	public T this[Point pos] {
		get { return tile[pos.x, pos.y]; }
		set { tile[pos.x, pos.y] = value; }
	}

	public T this[int x, int y] {
		get { return tile[x, y]; }
		set { tile[x, y] = value; }
	}
		
	public SasTile() {}
	public SasTile(T[,] tile) { this.tile = tile; }
	public SasTile(Point size) { this.tile = new T[size.x,size.y]; }

	public Pos MakePos(Point pos, PosAtti builder)
	{
		return new Pos (this, pos, builder);
	}

	public static implicit operator SasTile<T>(T[,] tile) { return new SasTile<T> (tile); }
	public static implicit operator T[,](SasTile<T> tile) { return tile.tile; }
}
