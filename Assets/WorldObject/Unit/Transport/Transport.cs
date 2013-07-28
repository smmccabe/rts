using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class Transport : Unit {
	public int capacity;
	public Depot fromDepot, toDepot;
	public int loadAmount, depositAmount;
	
	private bool loading = false, emptying = false;
	public int currentLoad = 0;
	private ResourceType loadType;
	
	protected override void Update() {
		base.Update();	
		
		if(!rotating && !moving){
			if(loading || emptying){
				//play animation here
				if(loading){
					Load ();
				}
				else{
					Empty ();
				}
				//did something, set idle to 0
				idle = 0;
			}
			else{
				Debug.Log("looking for depot");
				
				Depot[] depots = player.GetComponentInChildren<Buildings>().GetComponentsInChildren<Depot>();
				int depotAmount = 0;
				int tempAmount;
				Depot tempToDepot = null;
				
				//TODO: we should also calculate distance away, so we don't always go running off to the busiest one far far away
				foreach(Depot depot in depots){
					//change to some sort of tag later maybe?
					if(!depot.UnderConstruction()){
						tempAmount = depot.Amount();
						if(tempAmount > depotAmount){
							//make sure we have somewhere to take the cargo
							tempToDepot = CalculateToDepot(depot);
							if(tempToDepot){
								depotAmount = tempAmount;
								fromDepot = depot;
								toDepot = tempToDepot;
							}
						}
					}
				}	
				
				if(depotAmount > 0){
					loading = true;
					loadType = fromDepot.LoadPriority();
					StartMove (fromDepot.transform.position, fromDepot.gameObject);
				}
			}
		}
	}
	
	private void Load() {
		if(fromDepot){
			int distance = Mathf.FloorToInt(Vector3.Distance(transform.position, fromDepot.transform.position));
			int acceptableDistance = Mathf.CeilToInt(CalculateShiftAmount(fromDepot.gameObject));
			
			if(distance <= acceptableDistance){
				//make sure we have somewhere to take it to!
				SetToDepot();
				if(toDepot){
					int requestAmount = loadAmount;
					if(requestAmount > (capacity - currentLoad)){
						requestAmount = capacity - currentLoad;
					}
					int amount = fromDepot.Load(requestAmount, loadType);
					currentLoad += amount;
					
					//if empty, try another resource type
					if(currentLoad == 0){
						loadType = fromDepot.LoadPriority();
					}
					
					if(currentLoad >= capacity || (amount == 0 && currentLoad > 0)) {
						loading = false;
						emptying = true;
						
						StartMove(toDepot.transform.position, toDepot.gameObject);
					}
					
					Debug.Log("CurrentLoad: " + currentLoad);
				}
			}
			else{
				StartMove (fromDepot.transform.position, fromDepot.gameObject);
			}
		}
	}
	
	private void Empty() {
		if(!toDepot){
			SetToDepot();	
		}
		
		if(toDepot){
			if(AdjacentTo (toDepot.gameObject)){
				Debug.Log("close enough to dropoff point");
				int overflow = toDepot.Deposit(loadType, depositAmount);
				currentLoad -= depositAmount - overflow;
				
				if(overflow > 0){
					Debug.Log("overflow?");
					//we can't unload, should we wait or go somewhere else? 
					//If we go somewhere else, should we load up back to full for efficiency?
					//would just discarding the rest be more efficient?
				}
				
				if(currentLoad <= 0){
					emptying = false;
					//we don't do anything else as we may not want to transport from the same place!
					//in the future we should maybe have some inertia to changing jobs so everyone isn't constantly changing
				}
			}
			else{
				StartMove(toDepot.transform.position, toDepot.gameObject);
			}
		}
	}
	
	private void SetToDepot() {
		toDepot = CalculateToDepot (fromDepot);	
	}
	
	private Depot CalculateToDepot(Depot calculateFromDepot) {
		Depot returnToDepot = null;
		
		//find closest elevator
		SpaceElevator closestElevator = null;
		SpaceElevator[] elevators = player.GetComponentInChildren<Buildings>().GetComponentsInChildren<SpaceElevator>();
		int elevatorDistance = -1;
		int tempDistance;
		
		foreach(SpaceElevator elevator in elevators){
			tempDistance = Mathf.FloorToInt(Vector3.Distance(calculateFromDepot.transform.position, elevator.transform.position));
			if(tempDistance < elevatorDistance || elevatorDistance == -1){
				elevatorDistance = tempDistance;
				closestElevator = elevator;
			}
		}
		
		//if we couldn't find an elevator, don't move, we don't know which way to go
		if(closestElevator){
			Depot[] depots = player.GetComponentInChildren<Buildings>().GetComponentsInChildren<Depot>();
			int depotDistance = -1;
			int depotToElevatorDistance;
			
			foreach(Depot depot in depots){
				if(depot != fromDepot){
					//change to some sort of tag later maybe?
					if(!depot.UnderConstruction()){
						tempDistance = Mathf.FloorToInt(Vector3.Distance(calculateFromDepot.transform.position, depot.transform.position));
						
						//don't transport farther away from an elevator
						if(tempDistance < elevatorDistance){
							depotToElevatorDistance = Mathf.FloorToInt(Vector3.Distance(closestElevator.transform.position, depot.transform.position));
							
							//don't transport to a closer depot in the wrong direction
							if(depotToElevatorDistance < tempDistance) {
								//make sure we haven't already found a closer depot
								if(tempDistance < depotDistance || depotDistance == -1){
									depotDistance = tempDistance;
									returnToDepot = depot;
								}
							}
						}
					}
				}
			}
		}
		
		return returnToDepot;
	}
}
