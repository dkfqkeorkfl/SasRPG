using UnityEngine;
using System.Collections;

namespace Sas
{
	public interface IPool<T> where T : class, new()
	{
		T Alloc ();
		T[] Alloc (int size);
		void Dealloc (T item);

		int capacity { get; set; }
		int inactive { get;  }
	}

	public interface ILayerDeco<T> 
	{
		T data { set; get; }
		void Apply ();
	}
}