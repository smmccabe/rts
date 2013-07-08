using UnityEngine;
using System.Collections;
using RTS;

public class OreDeposit : Resource {
	private int numBlocks;
	
	protected override void Start() {
		base.Start ();
		
		numBlocks = GetComponentsInChildren().Length;
		resourceType = ResourceType.Ore;
	}
	
	protected override void Update () {
		base.Update ();
		float percentLeft = (float)amountLeft / (float)capacity;
		
		if(percentLeft < 0){
			percentLeft = 0;	
		}
		
		int numBlocksToShow = (int)(percentLeft * numBlocks);
		Ore[] blocks = GetComponentInChildren<Ore>();
		
		if(numBlocksToShow >= 0 && numBlocksToShow < blocks.Length) {
			Ore[] sortedBlocks = new Ore[blocks.Length];
			
			foreach(Ore ore in blocks) {
				sortedBlocks[blocks.length - int.Parse(ore.name)] = Ore;	
			}
			for(int i = numBlocksToShow; i < sortedBlocks.Length; i++) {
				sortedBlocks[i].renderer.enabled = false;
			}
			CalculateBounds();
		}
	}
}
