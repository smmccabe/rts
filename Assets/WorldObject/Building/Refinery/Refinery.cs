using UnityEngine;
using System.Collections;

public class Refinery : Depot {

	protected override void Start() {
		base.Start ();
		
		actions = new string[] { "Harvester" };
	}
}
