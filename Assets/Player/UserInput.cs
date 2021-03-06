using UnityEngine;
using System.Collections;
using RTS;

public class UserInput : MonoBehaviour {
	private Player player;
	
	// Use this for initialization
	void Start () {
		player = transform.root.GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () {
		if(player.human){
			MoveCamera();
			RotateCamera();
			
			MouseActivity();
			KeyboardActivity();
		}
	}
	
	private void MoveCamera() {
		float xpos = Input.mousePosition.x;
		float ypos = Input.mousePosition.y;
		Vector3 movement = new Vector3(0, 0, 0);
		bool mouseScroll = false;
		
		if(xpos >= 0 && xpos < ResourceManager.ScrollWidth) {
		    movement.x -= ResourceManager.ScrollSpeed;
		    player.hud.SetCursorState(CursorState.PanLeft);
		    mouseScroll = true;
		} else if(xpos <= Screen.width && xpos > Screen.width - ResourceManager.ScrollWidth) {
		    movement.x += ResourceManager.ScrollSpeed;
		    player.hud.SetCursorState(CursorState.PanRight);
		    mouseScroll = true;
		}

		if(ypos >= 0 && ypos < ResourceManager.ScrollWidth) {
		    movement.z -= ResourceManager.ScrollSpeed;
		    player.hud.SetCursorState(CursorState.PanDown);
		    mouseScroll = true;
		} else if(ypos <= Screen.height && ypos > Screen.height - ResourceManager.ScrollWidth) {
		    movement.z += ResourceManager.ScrollSpeed;
		    player.hud.SetCursorState(CursorState.PanUp);
		    mouseScroll = true;
		}
		
		movement = Camera.mainCamera.transform.TransformDirection(movement);
		movement.y = 0;
		
		movement.y -= ResourceManager.ScrollSpeed * Input.GetAxis("Mouse ScrollWheel");
		
		Vector3 origin = Camera.mainCamera.transform.position;
		Vector3 destination = origin;
		destination.x += movement.x;
		destination.y += movement.y;
		destination.z += movement.z;
		
		if(destination.y > ResourceManager.MaxCameraHeight){
			destination.y = ResourceManager.MaxCameraHeight;
		}
		else if(destination.y < ResourceManager.MinCameraHeight){
			destination.y = ResourceManager.MinCameraHeight;
		}
		
		if(destination != origin){
			Camera.mainCamera.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.ScrollSpeed);
		}
		
		if(!mouseScroll){
			player.hud.SetCursorState(CursorState.Select);	
		}
	}
	
	private void RotateCamera() {
		Vector3 origin = Camera.mainCamera.transform.eulerAngles;
		Vector3 destination = origin;
		
		if(Input.GetMouseButton(2)){
			destination.x -= Input.GetAxis("Mouse Y") * ResourceManager.RotateAmount;
			destination.y += Input.GetAxis("Mouse X") * ResourceManager.RotateAmount;
			
			if(destination != origin){
				Camera.mainCamera.transform.eulerAngles = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.RotateSpeed);
			}
		}	
	}
	
	private void MouseActivity(){
		if(Input.GetMouseButtonDown(0)){
			LeftMouseClick();
		}
		else if(Input.GetMouseButtonDown(1)){
			RightMouseClick();
		}
		MouseHover();
	}
	
	//used for selection
	private void LeftMouseClick() {
		if(player.hud.MouseInBounds()) {
			if(player.IsFindingBuildingLocation()) {
				if(player.CanPlaceBuilding()){
					player.StartConstruction();	
				}
			}
			else{
				GameObject hitObject = WorkManager.FindHitObject(Input.mousePosition);
				Vector3 hitPoint = WorkManager.FindHitPoint(Input.mousePosition);
				if(hitObject && hitPoint != ResourceManager.InvalidPosition){
					if(hitObject.tag != "Terrain"){
						WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
						if(worldObject){
							//deselect current object if already selected
							if(player.SelectedObject){
								player.SelectedObject.SetSelection(false, player.hud.GetPlayingArea());
							}
							player.SelectedObject = worldObject;
							worldObject.SetSelection(true, player.hud.GetPlayingArea());
							player.SelectedObject.SelectClick (hitObject, hitPoint, player);
						}
					}
					else if(player.SelectedObject){
						player.SelectedObject.SetSelection(false, player.hud.GetPlayingArea());
	        			player.SelectedObject = null;	
					}
				}
			}
		}
	}
	
	//used for actions
	private void RightMouseClick() {
		if(player.hud.MouseInBounds() && player.SelectedObject) {
			if(player.IsFindingBuildingLocation()){
				player.CancelBuildingPlacement();
			}
			else{
				GameObject hitObject = WorkManager.FindHitObject(Input.mousePosition);
				Vector3 hitPoint = WorkManager.FindHitPoint(Input.mousePosition);
				if(hitObject && hitPoint != ResourceManager.InvalidPosition){
					player.SelectedObject.ActionClick(hitObject, hitPoint, player);
				}
			}
		}
	}
	
	private void MouseHover() {
		if(player.hud.MouseInBounds()){
			if(player.IsFindingBuildingLocation()){
				player.FindBuildingLocation();
			}
			else {
				GameObject hoverObject = WorkManager.FindHitObject(Input.mousePosition);
				if(hoverObject){
					if(player.SelectedObject){
						player.SelectedObject.SetHoverState(hoverObject);
					}
					else if(hoverObject.tag != "Terrain"){
						Player owner = hoverObject.transform.root.GetComponent<Player>();
						if(owner){
							Unit unit = hoverObject.transform.parent.GetComponent<Unit>();
							Building building = hoverObject.transform.parent.GetComponent<Building>();
							if(owner.username == player.username && (unit || building)){
								player.hud.SetCursorState(CursorState.Select);
							}
						}
					}
				}
			}
		}
	}
	
	private void KeyboardActivity() {
		KeyboardMove();
	}
	
	private void KeyboardMove() {
		Vector3 movement = new Vector3(0, 0, 0);
		
		if(Input.GetKey(KeyCode.UpArrow)){			
			movement.z += ResourceManager.ScrollSpeed;
		}
		if(Input.GetKey (KeyCode.DownArrow)){
			movement.z -= ResourceManager.ScrollSpeed;
		}
		if(Input.GetKey(KeyCode.LeftArrow)){			
			movement.x -= ResourceManager.ScrollSpeed;
		}
		if(Input.GetKey (KeyCode.RightArrow)){
			movement.x += ResourceManager.ScrollSpeed;
		}
		
		movement = Camera.mainCamera.transform.TransformDirection(movement);
		movement.y = 0;
			
		Vector3 origin = Camera.mainCamera.transform.position;
		Vector3 destination = origin;
		destination.x += movement.x;
		destination.y += movement.y;
		destination.z += movement.z;
			
		if(destination != origin){
			Camera.mainCamera.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.ScrollSpeed);
		}
	}
}
