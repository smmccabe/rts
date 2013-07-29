using UnityEngine;
using System.Collections;

public class Worker : Unit {
	
	public int buildSpeed;
	
	private Building currentProject;
	private bool building = false;
	private float amountBuilt = 0.0f;
	
	// Use this for initialization
	protected override void Start () {
		base.Start();
		
		actions = new string[] {"Refinery", "Depot", "WarFactory"};
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update();
		
		if(!moving && !rotating){
			if(building && currentProject && currentProject.UnderConstruction()){
				amountBuilt += buildSpeed * Time.deltaTime;
				int amount = Mathf.FloorToInt(amountBuilt);
				
				if(amount > 0){
					amountBuilt -= amount;
					currentProject.Construct(amount);
					if(!currentProject.UnderConstruction()){
						building = false;	
					}
				}
			}
		}
	}
	
	public override void SetBuilding (Building project) {
		base.SetBuilding (project);
		
		currentProject = project;
		StartMove (currentProject.transform.position, currentProject.gameObject);
		building = true;
	}
	
	public override void PerformAction (string actionToPerform) {
		base.PerformAction (actionToPerform);
		CreateBuilding(actionToPerform);
	}
	
	public override void StartMove(Vector3 destination) {
		base.StartMove(destination);
		
		amountBuilt = 0.0f;
		building = false;
	}
	
	private void CreateBuilding(string buildingName) {
		Vector3 buildPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z + 10);
		if(player) {
			player.CreateBuilding(buildingName, buildPoint, this, playingArea);
		}
	}
	
	public override void ActionClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
		bool doBase = true;
		
		if(player && player.human && currentlySelected && hitObject && hitObject.tag != "Terrain"){
			Building building = hitObject.transform.parent.GetComponent<Building>();
			if(building){
				if(building.UnderConstruction()) {
					SetBuilding(building);
					doBase = false;
				}
			}
		}
		
		if(doBase) {
			base.ActionClick(hitObject, hitPoint, controller);	
		}
	}
}
