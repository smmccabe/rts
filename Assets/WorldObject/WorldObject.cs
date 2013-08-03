using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class WorldObject : MonoBehaviour {
	public string objectName;
	public Texture2D buildImage;
	public int cost, sellValue, hitPoints, maxHitPoints;
	
	protected Player player;
	protected string[] actions = {};
	protected bool currentlySelected = false;
	protected Bounds selectionBounds;
	protected Rect playingArea = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
	protected GUIStyle healthStyle = new GUIStyle();
	protected float healthPercentage = 1.0f;
	
	private List<Material> oldMaterials = new List<Material>();
	
	public Bounds GetSelectionBounds() { return selectionBounds; }
	
	protected virtual void Awake() {
		selectionBounds = ResourceManager.InvalidBounds;
		CalculateBounds();
	}
	
	protected virtual void Start() {
		SetPlayer();
	}
	
	protected virtual void Update() {
		
	}
	
	protected virtual void OnGUI() {
		if(currentlySelected){
			DrawSelection();
			GUI.depth = -1;
			DrawSelectionInfo();
			GUI.depth = 0;
		}
	}
	
	private void DrawSelection() {
		GUI.skin = ResourceManager.SelectBoxSkin;
		Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
		
		GUI.BeginGroup(playingArea);
		DrawSelectionBox(selectBox);
		GUI.EndGroup();
	}
	
	protected virtual void DrawSelectionBox(Rect selectBox){
		GUI.Box (selectBox, "");
		CalculateCurrentHealth(0.35f, 0.65f);
		DrawHealthBar(selectBox, "");
		GUI.Label (new Rect(selectBox.x, selectBox.y - 7, selectBox.width * healthPercentage, 5), "", healthStyle);
	}
	
	protected virtual void CalculateCurrentHealth(float lowSplit, float highSplit) {
		healthPercentage = (float)hitPoints / (float)maxHitPoints;
		
		if(healthPercentage > highSplit) {
			healthStyle.normal.background = ResourceManager.HealthyTexture;
		}
		else if(healthPercentage > lowSplit) {
			healthStyle.normal.background = ResourceManager.DamagedTexture;
		}
		else {
			healthStyle.normal.background = ResourceManager.CriticalTexture;	
		}
	}
	
	protected void DrawHealthBar(Rect selectBox, string label){
		healthStyle.padding.top = -20;
		healthStyle.fontStyle = FontStyle.Bold;
		GUI.Label(new Rect(selectBox.x, selectBox.y - 7, selectBox.width * healthPercentage, 5), label, healthStyle);
	}
	
	private void DrawSelectionInfo(){
		GUI.BeginGroup(new Rect(Screen.width - 140, Screen.height - 150, 130, 140));
		DrawSelectionInfoBox();
		GUI.EndGroup();	
	}
	
	protected virtual int DrawSelectionInfoBox() {
		int offset = 0;

		if(!objectName.Equals("")) {
			GUI.Label(new Rect(0, 0, 150, 20), objectName);
			offset += 20;
		}
		GUI.Label(new Rect(0, offset, 150, 20), hitPoints + "/" + maxHitPoints + " HP");
		offset += 20;
		
		return offset;
	}
	
	public virtual void SetSelection(bool selected, Rect playingArea) {
		currentlySelected = selected;
		if(selected){
			this.playingArea = playingArea;	
		}
	}
	
	public string[] GetActions() {
		return actions;	
	}
	
	public void CalculateBounds() {
		selectionBounds = new Bounds(transform.position, Vector3.zero);
		foreach(Collider collider in GetComponentsInChildren<Collider>()) {
			selectionBounds.Encapsulate(collider.bounds);
		}
	}
	
	public virtual void PerformAction(string actionToPerform) {
		
	}
	
	public virtual void SelectClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
		if(currentlySelected && hitObject && hitObject.tag != "Terrain") {
			WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
			
			if(worldObject){
				ChangeSelection(worldObject, controller);	
			}
		}
	}
	
	public virtual void ActionClick(GameObject hitObject, Vector3 hitPoint, Player controller){
		
	}
	
	public virtual void SetHoverState(GameObject hoverObject){
		if(player && player.human && currentlySelected) {
			if(hoverObject.tag != "Terrain") {
				player.hud.SetCursorState(CursorState.Select);
			}
		}
	}
	
	private void ChangeSelection(WorldObject worldObject, Player controller){
		SetSelection (false, playingArea);
		
		if(controller.SelectedObject){
			controller.SelectedObject.SetSelection(false, playingArea);
		}
		
		controller.SelectedObject = worldObject;
		worldObject.SetSelection(true, playingArea);
	}
	
	public bool IsOwnedBy(Player owner) {
		if(player && player.Equals(owner)) {
			return true;
		}
		return false;
	}
	
	public void SetColliders(bool enabled){
		Collider[] colliders = GetComponentsInChildren<Collider>();
		
		foreach(Collider collider in colliders){
			collider.enabled = enabled;	
		}
	}
	
	public void SetTransparentMaterial(Material material, bool storeExistingMaterial) {
		if(storeExistingMaterial) {
			oldMaterials.Clear();	
		}
		
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		
		foreach(Renderer renderer in renderers){
			if(storeExistingMaterial) {
				oldMaterials.Add(renderer.material);	
			}
			renderer.material = material;
		}
	}
	
	public void RestoreMaterials() {
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		
		if(oldMaterials.Count == renderers.Length) {
			for(int i=0; i < renderers.Length; i++) {
				renderers[i].material = oldMaterials[i];	
			}
		}
	}
	
	public void SetPlayingArea(Rect playingArea) {
		this.playingArea = playingArea;	
	}
	
	public void SetPlayer() {
		player = transform.root.GetComponentInChildren<Player>();	
	}
}
