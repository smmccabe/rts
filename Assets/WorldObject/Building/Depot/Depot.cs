using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class Depot : Building {
	private Dictionary<ResourceType, int> resources, resourceLimits;
	private const int RESOURCE_LIMIT = 1000;
	
	protected override void Awake() {
		base.Awake();
		
		resources = new Dictionary<ResourceType, int>();
		resourceLimits = new Dictionary<ResourceType, int>();
	}
	
	protected override void Start() {
		base.Start();
		
		InitResource(ResourceType.Ore);
	}
	
	protected void InitResource(ResourceType type) {
		resources.Add(type, 0);
		resourceLimits.Add (type, RESOURCE_LIMIT);
	}
	
	public int Deposit(ResourceType type, int amount) {
		int overflow = 0;
		
		resources[type] += amount;
		
		if(resources[type] > resourceLimits[type]){
			overflow = 	resources[type] - resourceLimits[type];
			resources[type] = resourceLimits[type];
		}
		
		return overflow;
	}
}
