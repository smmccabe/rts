using UnityEngine;
using System.Collections;
using RTS;

public class Harvester : Unit {
	
	public float capacity;
	public Building resourceStore;
	public float collectionAmount, depositAmount;
	
	private bool harvesting = false, emptying = false;
	public float currentLoad = 0.0f;
	private ResourceType harvestType;
	private Resource resourceDeposit;
	private float currentDeposit = 0.0f;
	
	protected override void Start() {
		base.Start();
		
		harvestType = ResourceType.Unknown;
	}
	
	protected override void Update() {
		base.Update();	
		
		if(!rotating && !moving){
			if(harvesting || emptying){
				//play animation here
				if(harvesting){
					Collect();
					if(currentLoad >= capacity || resourceDeposit.isEmpty()) {
						currentLoad = Mathf.Floor(currentLoad);
						harvesting = false;
						emptying = true;
						//stop harvest animation here
						StartMove(resourceStore.transform.position, resourceStore.gameObject);						
					}
				}
				else{
					Deposit ();
					if(currentLoad <= 0){
						emptying = false;
						
						if(!resourceDeposit.isEmpty ()){
							harvesting = true;
							StartMove (resourceDeposit.transform.position, resourceDeposit.gameObject);
						}
					}
				}
			}
		}
	}
	
	public override void SetBuilding (Building creator){
		base.SetBuilding(creator);
		
		resourceStore = creator;
	}
	
	public override void SetHoverState(GameObject hoverObject){
		base.SetHoverState (hoverObject);
		
		if(player && player.human && currentlySelected){
			if(hoverObject.tag != "Terrain"){
				Resource resource = hoverObject.transform.parent.GetComponent<Resource>();
				
				if(resource && !resource.isEmpty()){
					player.hud.SetCursorState(CursorState.Harvest);
				}
			}
		}
	}
	
	public override void ActionClick(GameObject hitObject, Vector3 hitPoint, Player controller){
		base.ActionClick(hitObject, hitPoint, controller);
		
		if(player && player.human){
			if(hitObject.tag != "Terrain"){
				Resource resource = hitObject.transform.parent.GetComponent<Resource>();
				
				if(resource && !resource.isEmpty()){
					StartHarvest(resource);
				}
			}
			else {
				StopHarvest();	
			}
		}
	}
	
	private void StartHarvest(Resource resource) {
		resourceDeposit = resource;
		
		StartMove (resource.transform.position, resource.gameObject);
		
		if(harvestType == ResourceType.Unknown || harvestType != resource.GetResourceType()){
			harvestType = resource.GetResourceType ();
			currentLoad = 0.0f;
		}
		
		harvesting = true;
		emptying = false;
	}
	
	private void StopHarvest(){
		
	}
	
	private void Collect() {
		float collect = collectionAmount * Time.deltaTime;
		
		if(currentLoad + collect > capacity) {
			collect = capacity - currentLoad;	
		}
		
		resourceDeposit.Remove (collect);
		currentLoad += collect;		
	}
	
	private void Deposit() {
		currentDeposit += depositAmount * Time.deltaTime;
		int deposit = Mathf.FloorToInt(currentDeposit);
		if(deposit >= 1) {
			if(deposit > currentLoad) {
				deposit = Mathf.FloorToInt(currentLoad);
			}
			currentDeposit -= deposit;
			currentLoad -= deposit;
			ResourceType depositType = harvestType;
			if(harvestType == ResourceType.Ore) {
				depositType = ResourceType.Money;
			}
			player.AddResource(depositType, deposit);
		}
	}
	
	protected override void DrawSelectionBox(Rect selectBox) {
		base.DrawSelectionBox(selectBox);
		
		float percentFull = currentLoad / capacity;
		float maxHeight = selectBox.height - 4;
		float height = maxHeight * percentFull;
		float leftPos = selectBox.x + selectBox.width - 7;
		float topPos = selectBox.y + 2 + (maxHeight - height);
		float width = 5;
		Texture2D resourceBar = ResourceManager.GetResourceHealthBar(harvestType);
		
		if(resourceBar) {
			GUI.DrawTexture(new Rect(leftPos, topPos, width, height), resourceBar);	
		}
	}
	
}
