using UnityEngine;
using System.Collections;
using RTS;

public class Unit : WorldObject {
	public float moveSpeed, rotateSpeed;
	
	protected bool moving, rotating;
	
	private Vector3 destination;
	private Quaternion targetRotation;
	private GameObject destinationTarget;
	
    protected override void Awake() {
        base.Awake();
    }
 
    protected override void Start () {
        base.Start();
    }
 
    protected override void Update () {
        base.Update();
		
		if(rotating) {
			TurnToTarget();
		}
		else if(moving) {
			MakeMove();	
		}
    }
 
    protected override void OnGUI() {
        base.OnGUI();
    }
	
	public virtual void Init(Building creator){
		//unit initialization stuff
	}
	
	public override void SetHoverState(GameObject hoverObject) {
		base.SetHoverState(hoverObject);
		
		if(player && player.human && currentlySelected) {
			if(hoverObject.tag == "Terrain") {
				player.hud.SetCursorState(CursorState.Move);
			}
		}
	}
	
	public override void ActionClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
		base.ActionClick (hitObject, hitPoint, controller);
		
		if(player && player.human && currentlySelected) {
			if(hitObject.tag == "Terrain" && hitPoint != ResourceManager.InvalidPosition) {
				float x = hitPoint.x;
				float y = hitPoint.y; // + player.SelectedObject.transform.position.y;
				float z = hitPoint.z;
				
				Vector3 destination = new Vector3(x, y, z);
				StartMove(destination);
			}
		}
	}
	
	public void StartMove(Vector3 destination) {
		this.destinationTarget = null;
		
		this.destination = destination;
		targetRotation = Quaternion.LookRotation(destination - transform.position);
		rotating = true;
		moving = false;
	}
	
	public void StartMove(Vector3 destination, GameObject destinationTarget) {
		StartMove(destination);
		
		this.destinationTarget = destinationTarget;
	}
	
	private void TurnToTarget() {
		transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed);
		
		Quaternion inverseTargetRotation = new Quaternion(-targetRotation.x, -targetRotation.y, -targetRotation.z, -targetRotation.w);
		if(transform.rotation == targetRotation || transform.rotation == inverseTargetRotation) {
			rotating = false;
			moving = true;
			
			if(destinationTarget) {
				CalculateTargetDestination();
			}
		}
		CalculateBounds();
		
		Debug.Log ("Turning Destination: " + destination);
	}
	
	private void MakeMove() {
		transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * moveSpeed);
		
		if(transform.position == destination) {
			moving = false;
		}
		CalculateBounds();
	}
	
	private void CalculateTargetDestination() {
		Vector3 originalExtents = selectionBounds.extents;
		Vector3 normalExtents = originalExtents;
		normalExtents.Normalize();
		
		float numberOfExtents = originalExtents.x / normalExtents.x;
		int unitShift = Mathf.FloorToInt(numberOfExtents);
		
		WorldObject worldObject = destinationTarget.GetComponent<WorldObject>();
		
		if(worldObject) {
			originalExtents = worldObject.GetSelectionBounds().extents;	
		}
		else {
			originalExtents = new Vector3(0.0f, 0.0f, 0.0f);
		}
		
		normalExtents = originalExtents;
		
		normalExtents.Normalize();
		numberOfExtents = originalExtents.x / normalExtents.x;
		int targetShift = Mathf.FloorToInt(numberOfExtents);
		
		int shiftAmount = targetShift + unitShift;
		
		Vector3 origin = transform.position;
		Vector3 direction = new Vector3(destination.x - origin.x, destination.y - origin.y, destination.z - origin.z);
		
		direction.Normalize();
		
		for(int i = 0; i < shiftAmount; i++){
			destination -= direction;	
		}
		
		//destination.y = destinationTarget.transform.position.y;
	}
}
