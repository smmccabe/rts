using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class Building : WorldObject {
	public float maxBuildProgress;
	public Texture2D rallyPointImage;
	public Texture2D sellImage;
	
	protected Queue<string> buildQueue;
	protected Vector3 rallyPoint;
	
	private float currentBuildProgress = 0.0f;
	private Vector3 spawnPoint;
	private bool needsBuilding = false;
	
	protected override void Awake() {
	    base.Awake();
		
		buildQueue = new Queue<string>();
		SetSpawnPoint();
	}
	 
	protected override void Start () {
	    base.Start();
	}
	 
	protected override void Update () {
	    base.Update();
		
		ProcessBuildQueue();
	}
	 
	protected override void OnGUI() {
	    base.OnGUI();
		
		if(needsBuilding){
			DrawBuildProgress();	
		}
	}
	
	public override void PerformAction(string actionToPerform){
		base.PerformAction(actionToPerform);
		CreateUnit (actionToPerform);
	}
	
	protected void CreateUnit(string unitName) {
		buildQueue.Enqueue(unitName);
		
		//have to create a unit just to get price? not sure this is the best way
		//this seems super duper hacky
		GameObject newObject = (GameObject)Instantiate(ResourceManager.GetUnit(unitName), spawnPoint, transform.rotation);
		Unit newUnit = newObject.GetComponent<Unit>();
		player.AddResource(ResourceType.Money, -newUnit.cost);
		Destroy(newUnit);
		Destroy(newObject);
	}
	
	protected void ProcessBuildQueue() {
		if(buildQueue.Count > 0){
			currentBuildProgress += Time.deltaTime * ResourceManager.BuildSpeed;	
			if(currentBuildProgress > maxBuildProgress) {
				if(player) {
					player.AddUnit(buildQueue.Dequeue(), spawnPoint, rallyPoint, transform.rotation, this);
				}
				currentBuildProgress = 0.0f;
			}
		}
	}
	
	public string[] getBuildQueueValues() {
		string[] values = new string[buildQueue.Count];
		int pos = 0;
		foreach(string unit in buildQueue) {
			values[pos++] = unit;
		}
		
		return values;
	}
	
	public float getBuildPercentages() {
		return currentBuildProgress / maxBuildProgress;	
	}
	
	public override void SetSelection(bool selected, Rect playingArea) {
		base.SetSelection(selected, playingArea);	
		
		if(player){
			RallyPoint flag = player.GetComponentInChildren<RallyPoint>();
			if(selected) {
				if(flag && player.human && spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition) {
					flag.transform.localPosition = rallyPoint;
					flag.transform.forward = transform.forward;
					flag.Enable();
				}
			}
			else{
				if(flag && player.human){
					flag.Disable();	
				}
			}
		}
	}
	
	public bool hasSpawnPoint() {
		return spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition;
	}
	
	public override void SetHoverState(GameObject hoverObject) {
		base.SetHoverState(hoverObject);
		
		if(player && player.human && currentlySelected){
			if(hoverObject.tag == "Terrain"){
				if(player.hud.GetPreviousCursorState() == CursorState.RallyPoint) {
					player.hud.SetCursorState(CursorState.RallyPoint);
				}
			}
		}
	}
	
	public override void ActionClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
		base.ActionClick(hitObject, hitPoint, controller);
		
		if(player && player.human && currentlySelected && hitPoint != ResourceManager.InvalidPosition) {
			if(hitObject.tag == "Terrain"){
				SetRallyPoint(hitPoint);	
			}
		}
	}
	
	public void SetRallyPoint(Vector3 position) {
		rallyPoint = position;
		
		if(player && player.human && currentlySelected){
			RallyPoint flag = player.GetComponentInChildren<RallyPoint>();
			if(flag) {
				flag.transform.localPosition = rallyPoint;	
			}
		}
	}
	
	public void Sell() {
		if(player) {
			player.AddResource(ResourceType.Money, sellValue);
		}
		if(currentlySelected) {
			SetSelection(false, playingArea);	
		}
		Destroy(this.gameObject);
	}
	
	public void StartConstruction() {
		CalculateBounds();
		SetSpawnPoint();
		needsBuilding = true;
		hitPoints = 0;
	}
	
	private void DrawBuildProgress() {
		GUI.skin = ResourceManager.SelectBoxSkin;
		Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
		
		GUI.BeginGroup(playingArea);
		CalculateCurrentHealth(0.5f, 0.99f);
		DrawHealthBar(selectBox, "Building...");
		GUI.EndGroup();
	}
	
	private void SetSpawnPoint() {
		float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
		float spawnZ = selectionBounds.center.z + transform.forward.z * selectionBounds.extents.z + transform.forward.z * 10;
		spawnPoint = new Vector3(spawnX, 0.0f, spawnZ);
		
		rallyPoint = spawnPoint;	
	}
	
	public bool UnderConstruction() {
		return needsBuilding;	
	}
	
	public void Construct(int amount) {
		hitPoints += amount;
		if(hitPoints >= maxHitPoints) {
			hitPoints = maxHitPoints;
			needsBuilding = false;
			RestoreMaterials();
		}
	}
}
