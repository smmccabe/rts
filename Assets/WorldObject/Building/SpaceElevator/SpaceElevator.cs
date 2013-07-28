using UnityEngine;
using System.Collections;

public class SpaceElevator : Building {

	protected override void Start() {
		base.Start ();
		
		actions = new string[] { "Worker", "Transport" };
	}
}
