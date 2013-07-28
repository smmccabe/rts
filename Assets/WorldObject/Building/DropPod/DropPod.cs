using UnityEngine;
using System.Collections;

public class DropPod : Building {

	protected override void Start() {
		base.Start ();
		
		actions = new string[] { "Worker", "Transport" };
	}
}
