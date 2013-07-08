using UnityEngine;
using System.Collections;
using RTS;

public class Harvester : Unit {
	
	public float capacity;
	
	private bool harvesting = false, emptying = false;
	private float currentLoad = 0.0f;
	private ResourceType harvestType;
	
	protected override void Start() {
		base.Start();
		
		harvestType = ResourceType.Unknown;
	}
	
	protected override void Update() {
		base.Update();	
	}
	
	public override void SetHoverState(GameObject hoverObject){
		base.SetHoverState (hoverObject);
		
		if(player && player.human && currentlySelected){
			if(hoverObject.name != "Ground"){
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
			if(hitObject.name != "Ground"){
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
			
	}
	
	private void StopHarvest(){
		
	}
}
