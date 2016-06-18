using UnityEngine;
using System.Collections;

namespace Sas.Detail
{
	public class Duration<T> : System.IDisposable 
	{
		private T o = default(T);
		private System.Action<T> action = null;

		public  Duration(System.Action<T> action, T _old, T _new)
		{
			this.action = action;
			action (_new);
		}

		public void Dispose ()
		{
			action (o);
		}
	}
}
