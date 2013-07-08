using UnityEngine;
using System.Collections;
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
	
	protected virtual void Awake() {
		selectionBounds = ResourceManager.InvalidBounds;
		CalculateBounds();
	}
	
	protected virtual void Start() {
		player = transform.root.GetComponentInChildren<Player>();
	}
	
	protected virtual void Update() {
		
	}
	
	protected virtual void OnGUI() {
		if(currentlySelected){
			DrawSelection();
		}
	}
	
	protected virtual void DrawSelectionBox(Rect selectBox){
		GUI.Box (selectBox, "");	
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
		foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
			selectionBounds.Encapsulate(r.bounds);
		}
	}
	
	public virtual void PerformAction(string actionToPerform) {
		
	}
	
	public virtual void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
		if(currentlySelected && hitObject && hitObject.name != "Ground") {
			WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
			
			if(worldObject){
				ChangeSelection(worldObject, controller);	
			}
		}
	}
	
	public virtual void SetHoverState(GameObject hoverObject){
		if(player && player.human && currentlySelected) {
			if(hoverObject.name != "Ground") {
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
	
	private void DrawSelection() {
		GUI.skin = ResourceManager.SelectBoxSkin;
		Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
		
		GUI.BeginGroup(playingArea);
		DrawSelectionBox(selectBox);
		GUI.EndGroup();
	}
	
	public bool IsOwnedBy(Player owner) {
		if(player && player.Equals(owner)) {
			return true;
		}
		return false;
	}
}
