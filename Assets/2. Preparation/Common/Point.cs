using UnityEngine;
using System.Collections;

public struct Point {

	public class Serializable
	{
		public int x,y;
	};

	public static readonly Point one = new Point(1,1);
	public static readonly Point zero = new Point(0,0);
	public int x, y;

	public Point(int x, int y) {
		this.x = x;
		this.y = y;
	}

	public Serializable serialize { get { return new Serializable { x = this.x, y = this.y }; } }

	public bool EqualTo(Point data)
	{
		return this.x == data.x && this.y == data.y;
	}

	public override string ToString()
	{
		return string.Format("Point(x:{0}, y:{1})", x,y);
	}

	public static Point operator +(Point a, Point b) {
		return new Point(a.x + b.x, a.y + b.y);
	}

	public static Point operator +(Point a, Vector2 b) {
		return new Point(a.x + (int)b.x, a.y + (int)b.y);
	}

	public static Point operator -(Point a, Point b) {
		return new Point(a.x - b.x, a.y - b.y);
	}

	public static Point operator -(Point a, Vector2 b) {
		return new Point(a.x - (int)b.x, a.y - (int)b.y);
	}

	public static Point operator *(Point a, int b) {
		return new Point(a.x * b, a.y * b);
	}

	public bool Contain(Point pt) 
	{
		return pt.x >= 0 && pt.x < x && 
			pt.y >= 0 && pt.y < y;  
	}
}
