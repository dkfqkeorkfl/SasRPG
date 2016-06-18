using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

namespace Sas
{
    public static class Extension
	{
		public static void Sort<T>(this IList<T> list, System.Comparison<T> comp)
		{
			var array = list as T[];
			if (null != array) {
				System.Array.Sort (array, comp);
				return;
			}

			var vlist = list as List<T>;
			vlist.Sort (comp);
		}

		public static T[,] To2DArraySured<T>(this T[,] a, int first, int second)
		{
			if( a.Rank == 2 && first == a.GetLength(0) && second == a.GetLength(1) )
				return (T[,])a;

			T[] array = a as T[];
			T[,] rtn = new T[first, second];
			int i = 0;
			for (int x = 0; x < first; ++x)
				for (int y = 0; y < second; ++y, ++i) {
					if (i < array.Length)
						rtn [x, y] = array[i];
					else
						return rtn;
				}
			return rtn;
		}

		public static bool Contain<T>(this LinkedList<T> list, Predicate<T> pred)
		{
			return list.Find (pred) != null;
		}

		public static LinkedListNode<T> Find<T>(this LinkedList<T> list, Predicate<T> pred)
		{
			LinkedListNode<T> node = list.First;

			while(node != null){
				if (pred (node.Value))
					return node;

				node = node.Next;
			}
			return null;
		}
		public static Transform GetPrefabOrigin(this GameObject obj)
		{
			return PrefabUtility.GetPrefabObject (obj.transform) as Transform;
		}
		public static bool IsPrefabInstance(this GameObject obj)
		{
			return PrefabUtility.GetPrefabParent(obj) != null && 
				PrefabUtility.GetPrefabObject(obj.transform) != null;
		}

		public static bool IsPrefabOriginal(this GameObject obj)
		{
			return PrefabUtility.GetPrefabParent(obj) == null && 
				PrefabUtility.GetPrefabObject(obj.transform) != null;
		}

		public static bool IsPrefabDisconnected(this GameObject obj)
		{
			return PrefabUtility.GetPrefabParent(obj) != null && 
				PrefabUtility.GetPrefabObject(obj.transform) == null;
		}

		public static int BinarySearchForMatch<T>(this IList<T> list,
			Func<T,int> comparer)
		{
			int min = 0;
			int max = list.Count-1;

			while (min <= max)
			{
				int mid = (min + max) / 2;
				int comparison = comparer(list[mid]);
				if (comparison == 0)
				{
					return mid;
				}
				if (comparison < 0)
				{
					min = mid+1;
				}
				else
				{
					max = mid-1;
				}
			}
			return ~min;
		}

		public static T[] ToArray<T>(this IEnumerable<T> e, int size)
		{
			T[] array = new T[size];
			var iter = e.GetEnumerator ();
			for( int i = 0; i < size && iter.MoveNext (); ++i) {
				array [i] = iter.Current;
			}
			return array;
		}

		public static List<T> ToList<T>(this IEnumerable<T> e, int size)
		{
			var list = new List<T> ();
			list.Resize (size);
			var iter = e.GetEnumerator ();
			for( int i = 0; i < list.Count && iter.MoveNext (); ++i) {
				list [i] = iter.Current;
			}
			return list;
		}

		public static List<T> ToList<T>(this IEnumerable<T> e)
		{
			var list = new List<T> ();
			foreach (var data in e)
				list.Add (data);
			return list;
		}

		public static void Resize<T>(this List<T> list, int sz, T c)
		{
			int cur = list.Count;
			if(sz < cur)
				list.RemoveRange(sz, cur - sz);
			else if(sz > cur)
			{
				//this bit is purely an optimisation, to avoid multiple automatic capacity changes.
				if(sz > list.Capacity)
					list.Capacity = sz;
				list.AddRange(Enumerable.Repeat<T>(c, sz - cur));
			}
		}

		public static void Resize<T>(this List<T> list, int sz)
		{
			Resize(list, sz, default(T) );
		}

		public static void Foreach<T>(this IList<T> aList, Action<T> aAction)
		{
			int count = aList.Count;
			for (int i = 0; i < count; ++i) 
			{
				aAction (aList[i]);			
			}
		}

		public static T GetComponentExactly<T>(this GameObject obj) where T : Component
		{
			T t = obj.GetComponent<T> ();
			t = ((t != null) ? t : obj.AddComponent<T> ());
			if (t == null)
				Debug.LogErrorFormat ("[SasExtension.Error] don't make {0}. Plase, Check it that can be instanced", typeof(T).Name );
			return t;
		}

        public static T[] GetComponentsInChildrenWithoutMe<T>(this Transform tm) where T : Component
        {
            T[] compsChild = tm.GetComponentsInChildren<T>();
            T[] compsSelf = tm.GetComponents<T>();

            var rtn = compsChild.Where((value) => { return !compsSelf.Contains(value); });
            return rtn.ToArray();
        }

        public static T GetComponentInChildrenWithoutMe<T>(this Transform tm) where T : Component
        {
            T rtn = null;
            int count = tm.childCount;
            for (int i = 0; i < count && rtn == null; ++i)
            {
                Transform child = tm.GetChild(i);
                rtn = child.GetComponent<T>();
                if (rtn == null)
                    rtn = child.GetComponentInChildrenWithoutMe<T>();
            }

            return rtn;
        }

        public static void ForeachChild(this Transform tm, Action<Transform> a)
        {
            int count = tm.childCount;
            for (int i = 0; i < count; ++i)
            {
                Transform child = tm.GetChild(i);
                a(child);
                child.ForeachChild(a);
            }
        }

        /// <summary>
        /// Search depth-first Traversal
        /// </summary>
        public static Transform FindDST(this Transform tm, Predicate<Transform> pred)
        {
            Transform rtn = null;
            int count = tm.childCount;
            for (int i = 0; i < count && rtn == null; ++i)
            {
                Transform child = tm.GetChild(i);

                if (pred(child))
                    rtn = child;
                else
                    rtn = child.FindDST(pred);
            }
            return rtn;
        }

        public static int CountChildTotal(this Transform tm)
        {
            int total = 0;
            CountChildTotal(tm, ref total);
            return total;
        }

        public static void CountChildTotal(this Transform tm, ref int total)
        {
            int count = tm.childCount;
            total += count;
            for (int i = 0; i < count; ++i)
            {
                CountChildTotal(tm.GetChild(i), ref total);
            }
        }
    }

}