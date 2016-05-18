using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sas;
using System;

[Serializable]
public class SasLayerData
{
	public class SeirializableData
	{
		public float  lengthX = 0, lengthY = 0;
		public int    dimeX = 0, dimeY = 0;
		public int[,] datas = null;

		/// <summary>
		/// the Datas Value is not sured data. because it datas can recive another dimensional array.
		/// </summary>
		public int[,] tile {
			get {
				datas = datas.To2DArraySured (dimeX, dimeY);
				return datas;
			}
		}
	}

	public const int DER_MASK_GROUP_OFFSET = 8;
	public const int DER_MASK_RESOUCRE_OFFSET = 0;

	public const int DEF_MASK_RESOUCRE = 0xff << DER_MASK_RESOUCRE_OFFSET;
	public const int DEF_MASK_GROUP = (DEF_MASK_RESOUCRE << DER_MASK_GROUP_OFFSET); // 0xff00
	public const int DEF_MASK_BLOCK = 0x10000;

	public static readonly SasTile.PosAtti Accesser = SasTile.PosAtti.DIRECTION_UP + SasTile.PosAtti.DIRECTION_RIGHT +
		SasTile.PosAtti.DIRECTION_DOWN + SasTile.PosAtti.DIRECTION_LEFT;
	private Vector2 mLength = Vector2.one;
	private SasTile<int> mTile = SasTile<int>.zero;

	public SeirializableData serialization
	{
		get { 
			return new SeirializableData { 
				lengthX = mLength.x, lengthY = mLength.y, 
				dimeX = size.x, dimeY = size.y,
				datas = tile }; 
		}
		set { 
			length = new Vector2 ( value.lengthX, value.lengthY); 
			this.tile = value.tile;
		}
	}

	public Vector2 length 
	{
		get { return mLength; }
		set { mLength = value; Reposition (); }
	}

	public Point size 
	{ 
		get { return mTile.size; } 
		set { if (!size.EqualTo (value)) tile = new int[value.x, value.y]; }
	}
		
	public int[,] tile
	{
		get { return mTile; }
		private set { mTile = value; rects = new Rect[mTile.size.x, mTile.size.y]; Reposition ();}
	}

	public Rect[,] rects { get; private set; }
	public SasTile<int>.Pos  posTile { get { return mTile.MakePos (Point.zero, Accesser); } }
		
	public int  GetIdxRes(Point pos)              { return Sas.Uti.GetMaskVal(tile [pos.x, pos.y], DEF_MASK_RESOUCRE) >> DER_MASK_RESOUCRE_OFFSET; }
	public void SetIdxRes(Point pos, int res)     { tile [pos.x, pos.y] = Sas.Uti.SetMaskVal(tile [pos.x, pos.y], DEF_MASK_RESOUCRE, res << DER_MASK_RESOUCRE_OFFSET); }
	public int  GetIdxGroup(Point pos)            { return Sas.Uti.GetMaskVal(tile [pos.x, pos.y], DEF_MASK_GROUP) >> DER_MASK_GROUP_OFFSET; }
	public void SetIdxGroup(Point pos, int group) { tile [pos.x, pos.y] = Sas.Uti.SetMaskVal(tile [pos.x, pos.y], DEF_MASK_GROUP, group << DER_MASK_GROUP_OFFSET); }

	public bool IsBlockOfPosRight(Point pos) { return IsBlock (new Point { x = (pos.x += 1), y = pos.y }); }
	public bool IsBlockOfPosLeft(Point pos)  { return IsBlock (new Point {x = (pos.x -= 1), y = pos.y}); }
	public bool IsBlockOfPosUp(Point pos)    { return IsBlock (new Point {x = pos.x, y = (pos.y -= 1)}); }
	public bool IsBlockOfPosDown(Point pos)  { return IsBlock (new Point {x = pos.x, y = (pos.y += 1)});	}
	public bool IsBlock(Point pos)           { return ValildPoint(pos) ? !Sas.Uti.IsMaskZero(tile [pos.x, pos.y], DEF_MASK_BLOCK) : true; }
	public void SetBlock(Point pos)          { SetBlock (pos, true); }
	public void UnsetBlock(Point pos)        { SetBlock (pos, false); }

	public void SetBlock(Point pos, bool enable)
	{
		if (!ValildPoint (pos)) {
			Debug.LogWarningFormat ("[SasTile]{0} is overflow data.", pos.ToString() );
			return;
		}

		tile [pos.x, pos.y] = enable ? 
			Sas.Uti.SetMaskAll (tile [pos.x, pos.y], DEF_MASK_BLOCK) :
			Sas.Uti.UnsetMaskAll (tile [pos.x, pos.y], DEF_MASK_BLOCK);
	}
		
	private bool ValildPoint(Point pt)
	{
		return size.Contain (pt);
	}

	private void Reposition()
	{
		Point size = this.size;
		for (int x = 0; x < size.x; ++x)
			for (int y = 0; y < size.y; ++y) {
				Rect rc = new Rect();
				rc.min = ConvertPointToPos (new Point { x = x, y = y });
				rc.max = ConvertPointToPos (new Point { x = x+1, y = y+1 });
				rects [x, y] = rc;
			}
	}

	public Vector2 ConvertPointToPos(Point pos)
	{
		return new Vector2 { x = pos.x * mLength.x, y = pos.y * mLength.y };
	}
}