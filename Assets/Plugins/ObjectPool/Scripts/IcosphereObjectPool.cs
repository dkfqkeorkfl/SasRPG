using UnityEngine;
using System.Collections;

// By Anish Dhesikan
public class IcosphereObjectPool : BetterObjectPool /* Make sure to inherit from BetterObjectPool */ { 

	/* Set all your properties in the inspector */
	/* Read more about what the properties are in BetterObjectPool.cs */

	// - This is so that you can call IcosphereObjectPool.current from anywhere
	// - Be aware that you can only have one IcosphereObjectPool if you do it this way
	public static IcosphereObjectPool current;

	// Make sure to override the Awake() and Update() methods and call their bases.
	public override void Awake () {
		base.Awake ();

		// Make sure this script is only attached to one GameObject. 
		// Otherwise you may get unwanted behavior.
		current = this;
	}

	public override void Update () {
		base.Update (); // This is necessary to update the pool
	}

	/* See IcosphereGenerator class for how to instantiate/destroy objects from pool */
}
