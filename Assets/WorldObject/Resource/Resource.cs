using UnityEngine;
using System.Collections;
using RTS;

public class Resource : WorldObject {
	public float capacity;
	
	protected float amountLeft;
	protected ResourceType resourceType;
	
	protected override void Start() {
		base.Start();
		
		amountLeft = capacity;
		resourceType = ResourceType.Unknown;
	}
	
	public void Remove(float amount) {
		amountLeft -= amount;
		if(amountLeft < 0){
			amountLeft = 0;		
		}
	}
	
	public bool isEmpty() {
		return amountLeft <= 0;	
	}
	
	public ResourceType GetResourceType() {
		return resourceType;	
	}
	
	protected override void CalculateCurrentHealth(float lowSplit, float highSplit) {
		healthPercentage = amountLeft / capacity;
		healthStyle.normal.background = ResourceManager.GetResourceHealthBar(resourceType);
	}
}
