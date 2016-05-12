using UnityEngine;
using System.Collections.Generic;

public class SasLayerActor {

	public class Actor
	{
		public SasTile<int>.Pos pt;
		public Vector3 offset;
		public int hp;
		public int res;
		public int state;
	}

	private readonly LinkedList<Actor> mActors = new LinkedList<Actor>();


}
