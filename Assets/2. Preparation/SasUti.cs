using UnityEngine;
using System.Collections;

public static class SasUti{

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
