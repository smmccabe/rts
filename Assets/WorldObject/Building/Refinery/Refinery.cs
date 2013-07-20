using UnityEngine;
using System.Collections;

public class Refinery : Building {

	protected override void Start() {
		base.Start ();
		
		actions = new string[] { "Harvester" };
	}
}
