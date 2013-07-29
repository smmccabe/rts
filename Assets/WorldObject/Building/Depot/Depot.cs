using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class Depot : Building {
	public Dictionary<ResourceType, int> resources, resourceLimits;
	public const int RESOURCE_LIMIT = 100000;
	
	protected override void Awake() {
		base.Awake();
		
		resources = new Dictionary<ResourceType, int>();
		resourceLimits = new Dictionary<ResourceType, int>();
	}
	
	protected override void Start() {
		base.Start();
		
		InitResource(ResourceType.Ore);
	}
	
	protected override int DrawSelectionInfoBox() {
		int offset = base.DrawSelectionInfoBox();
		
		foreach(KeyValuePair<ResourceType, int> resource in resources){
			GUI.Label(new Rect(0, offset, 150, 20), resource.Key + ": " + resource.Value + "/" + resourceLimits[resource.Key]);
			offset += 20;
		}
		
		return offset;
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
	
	public int Amount() {
		int amount = 0;
		
		foreach(KeyValuePair<ResourceType, int> resource in resources){
			amount += resource.Value;	
		}
		
		return amount;
	}
	
	public int Load(int requestAmount, ResourceType type) {
		int amount;
		
		if(resources[type] >= requestAmount){
			amount = requestAmount;
			resources[type] -= requestAmount;
		}
		else {
			amount = resources[type];
			resources[type] = 0;
		}
		
		return amount;
	}
	
	public ResourceType LoadPriority(){
		float percentageFull = -1;
		float tempPercentage = 0;
		
		//not sure what this should be, should sort of be a null?
		ResourceType type = ResourceType.Money;
		
		foreach(KeyValuePair<ResourceType, int> resource in resources){
			tempPercentage = resource.Value / resourceLimits[resource.Key];
			if(tempPercentage > percentageFull){
				percentageFull = tempPercentage;
				type = resource.Key;	
			}
		}	
		
		return type;
	}
	
	public bool IsFull(ResourceType type) {
		if(resources[type] >= resourceLimits[type]){
			return true;	
		}
		else{
			return false;	
		}
	}
}
