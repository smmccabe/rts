using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class HUD : MonoBehaviour {
	public GUISkin resourcesSkin, ordersSkin, selectBoxSkin, mouseCursorSkin;
	public Texture2D buttonHover, buttonClick;
	public Texture2D buildFrame, buildMask;
	public Texture2D activeCursor;
	public Texture2D selectCursor, leftCursor, rightCursor, upCursor, downCursor;
	public Texture2D[] moveCursors, attackCursors, harvestCursors;
	public Texture2D rallyPointCursor;
	public Texture2D[] resources;
	public Texture2D smallButtonHover, smallButtonClick;
	public Texture2D healthy, damaged, critical;
	public Texture2D[] resourceHealthBars;
	
	private Player player;
	private CursorState activeCursorState;
	private int currentFrame = 0;
	private Dictionary<ResourceType,int> resourceValues, resourceLimits;
	private Dictionary<ResourceType,Texture2D> resourceImages;
	private WorldObject lastSelection;
	private float sliderValue;
	private int buildAreaHeight = 0;
	private CursorState previousCursorState;
	
	private const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40;
	private const int SELECTION_NAME_HEIGHT = 30;
	private const int ICON_WIDTH = 32, ICON_HEIGHT = 32, TEXT_WIDTH = 128, TEXT_HEIGHT = 32;
	private const int BUILD_IMAGE_WIDTH = 64, BUILD_IMAGE_HEIGHT = 64, BUILD_IMAGE_PADDING = 8;
	private const int BUTTON_SPACING = 7;
	private const int SCROLL_BAR_WIDTH = 22;	
	
	// Use this for initialization
	void Start () {
		player = transform.parent.GetComponent<Player>();
		ResourceManager.StoreSelectBoxItems(selectBoxSkin, healthy, damaged, critical);
		SetCursorState(CursorState.Select);
		
		resourceValues = new Dictionary<ResourceType, int>();
		resourceLimits = new Dictionary<ResourceType, int>();
		
		resourceImages = new Dictionary<ResourceType, Texture2D>();
		for(int i = 0; i < resources.Length; i++) {
			switch(resources[i].name) {
			case "Money":
				resourceImages.Add(ResourceType.Money, resources[i]);
	            resourceValues.Add(ResourceType.Money, 0);
	            resourceLimits.Add(ResourceType.Money, 0);
	            break;
	        case "Power":
	            resourceImages.Add(ResourceType.Power, resources[i]);
	            resourceValues.Add(ResourceType.Power, 0);
	            resourceLimits.Add(ResourceType.Power, 0);
				break;
			default: break;
			}
		}
		
		buildAreaHeight = Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 * BUTTON_SPACING;
		
		Dictionary<ResourceType, Texture2D> resourceHealthBarTextures = new Dictionary<ResourceType, Texture2D>();
		
		for(int i = 0; i < resourceHealthBars.Length; i++){
			switch(resourceHealthBars[i].name){
			case "ore":
				resourceHealthBarTextures.Add (ResourceType.Ore, resourceHealthBars[i]);
				break;
			default:
				break;
			}
		}
		
		ResourceManager.SetResourceHealthBarTextures(resourceHealthBarTextures);
	}
	
	// Update is called once per frame
	void OnGUI() {
		if(player.human){
			DrawOrdersBar();
			DrawResourceBar();
			DrawMouseCursor();
		}
	}
	
	private void DrawOrdersBar() {
		GUI.skin = ordersSkin;
		GUI.BeginGroup(new Rect(Screen.width - ORDERS_BAR_WIDTH - BUILD_IMAGE_WIDTH, RESOURCE_BAR_HEIGHT, ORDERS_BAR_WIDTH + BUILD_IMAGE_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(BUILD_IMAGE_WIDTH, 0, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT), "");
		
		if(player.SelectedObject){
			if(player.SelectedObject.IsOwnedBy(player)) {
				if(lastSelection && lastSelection != player.SelectedObject) {
					sliderValue = 0.0f;
				}
				DrawActions(player.SelectedObject.GetActions ());
				
				lastSelection = player.SelectedObject;
				
				Building selectedBuilding = lastSelection.GetComponent<Building>();
				if(selectedBuilding) {
					DrawBuildQueue(selectedBuilding.getBuildQueueValues(), selectedBuilding.getBuildPercentages());
					DrawStandardBuildingOptions(selectedBuilding);
				}
			}
		}
		
		GUI.EndGroup();
	}
	
	private void DrawResourceBar() {
		GUI.skin = resourcesSkin;
		GUI.BeginGroup(new Rect(0,0,Screen.width,RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(0,0,Screen.width,RESOURCE_BAR_HEIGHT),"");
		
		int topPos = 4, iconLeft = 4, textLeft = 20;
		DrawResourceIcon(ResourceType.Money, iconLeft, textLeft, topPos);
		iconLeft += TEXT_WIDTH;
		textLeft += TEXT_WIDTH;
		DrawResourceIcon(ResourceType.Power, iconLeft, textLeft, topPos);
		
		GUI.EndGroup();
	}
	
	private void DrawResourceIcon(ResourceType type, int iconLeft, int textLeft, int topPos) {
		Texture2D icon = resourceImages[type];
		string text = resourceValues[type].ToString () + "/" + resourceLimits[type].ToString();
		GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), icon);
		GUI.Label (new Rect(textLeft, topPos, TEXT_WIDTH, TEXT_HEIGHT), text);
	}
	
	private void DrawMouseCursor() {
		bool mouseOverHud = !MouseInBounds() && activeCursorState != CursorState.PanRight && activeCursorState != CursorState.PanUp;
		
		if(mouseOverHud){
			Screen.showCursor = true;
		}
		else {
			Screen.showCursor = false;
			if(!player.IsFindingBuildingLocation()) {
				GUI.skin = mouseCursorSkin;
				GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
				UpdateCursorAnimation();
				Rect cursorPosition = GetCursorDrawPosition();
				GUI.Label (cursorPosition, activeCursor);
				GUI.EndGroup();
			}
		}
	}
	
	private void UpdateCursorAnimation() {
		if(activeCursorState == CursorState.Move){
			currentFrame = (int)Time.time % moveCursors.Length;
			activeCursor = moveCursors[currentFrame];
		}
		else if(activeCursorState == CursorState.Attack){
			currentFrame = (int)Time.time % attackCursors.Length;
			activeCursor = attackCursors[currentFrame];
		}
		else if(activeCursorState == CursorState.Harvest){
			currentFrame = (int)Time.time % harvestCursors.Length;
			activeCursor = harvestCursors[currentFrame];
		}
	}
	
	private Rect GetCursorDrawPosition() {
		float leftPos = Input.mousePosition.x;
		float topPos = Screen.height - Input.mousePosition.y;
		
		if(activeCursorState == CursorState.PanRight) {
			leftPos = Screen.width - activeCursor.width;
		}
		else if(activeCursorState == CursorState.PanDown) {
			topPos = Screen.height - activeCursor.height;	
		}
		else if(activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Harvest) {
			topPos -= activeCursor.height / 2;
			leftPos -= activeCursor.width / 2;
		}
		else if(activeCursorState == CursorState.RallyPoint) {
			topPos -= activeCursor.height;
		}
				
    	return new Rect(leftPos, topPos, activeCursor.width, activeCursor.height);
	}
	
	public bool MouseInBounds() {
		Vector3 mousePos = Input.mousePosition;
		bool insideWidth = false;
		bool insideHeight = false;
		
		if(mousePos.x >= 0 && mousePos.x <= Screen.width - ORDERS_BAR_WIDTH){
			insideWidth = true;	
		}
		if(mousePos.y >= 0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT){
			insideHeight = true;
		}
			
		return insideWidth && insideHeight;
	}
	
	public Rect GetPlayingArea(){
		return new Rect(0, RESOURCE_BAR_HEIGHT, Screen.width - ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT);
	}
	
	public void SetCursorState(CursorState newState) {
		if(activeCursorState != newState) {
			previousCursorState = activeCursorState;	
		}
		activeCursorState = newState;
		
		switch(newState){
		case CursorState.Select:
			activeCursor = selectCursor;
			break;
		case CursorState.Attack:
	        currentFrame = (int)Time.time % attackCursors.Length;
	        activeCursor = attackCursors[currentFrame];
	        break;
	    case CursorState.Harvest:
	        currentFrame = (int)Time.time % harvestCursors.Length;
	        activeCursor = harvestCursors[currentFrame];
	        break;
	    case CursorState.Move:
	        currentFrame = (int)Time.time % moveCursors.Length;
	        activeCursor = moveCursors[currentFrame];
	        break;
	    case CursorState.PanLeft:
	        activeCursor = leftCursor;
	    	break;
	    case CursorState.PanRight:
	        activeCursor = rightCursor;
	        break;
	    case CursorState.PanUp:
	        activeCursor = upCursor;
	        break;
	    case CursorState.PanDown:
	        activeCursor = downCursor;
	        break;
		case CursorState.RallyPoint:
			activeCursor = rallyPointCursor;
			break;
		default: break;
		}
	}
	
	public void SetResourceValues(Dictionary<ResourceType, int> resourceValues, Dictionary<ResourceType, int> resourceLimits) {
		this.resourceValues = resourceValues;
		this.resourceLimits = resourceLimits;
	}
	
	private void DrawActions(string[] actions){
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = buttonHover;
		buttons.active.background = buttonClick;
		GUI.skin.button = buttons;
		int numActions = actions.Length;
		
		GUI.BeginGroup (new Rect(BUILD_IMAGE_WIDTH, 0, ORDERS_BAR_WIDTH, buildAreaHeight));
		
		for(int i = 0; i < numActions; i++) {
			int column = i % 2;
			int row = i / 2;
			Rect pos = GetButtonPos(row, column);
			Texture2D action = ResourceManager.GetBuildImage (actions[i]);
			if(action){
				if(GUI.Button(pos, action)){
					if(player.SelectedObject) {
						player.SelectedObject.PerformAction(actions[i]);
					}
				}
			}
		}
		GUI.EndGroup();
	}
	
	private int MaxNumRows(int areaHeight){
		return areaHeight / BUILD_IMAGE_HEIGHT;	
	}
	
	private Rect GetButtonPos(int row, int column){
		int left = SCROLL_BAR_WIDTH + column * BUILD_IMAGE_WIDTH;
		float top = row * BUILD_IMAGE_HEIGHT - sliderValue * BUILD_IMAGE_HEIGHT;
		
		return new Rect(left, top, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
	}
	
	private void DrawBuildQueue(string[] buildQueue, float buildPercentage){
		for(int i = 0; i < buildQueue.Length; i++){
			float topPos = i * BUILD_IMAGE_HEIGHT - (i+1) * BUILD_IMAGE_PADDING;
			Rect buildPos = new Rect(BUILD_IMAGE_PADDING, topPos, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
			GUI.DrawTexture(buildPos, ResourceManager.GetBuildImage(buildQueue[i]));
			GUI.DrawTexture(buildPos, buildFrame);
			topPos += BUILD_IMAGE_PADDING;
			float width = BUILD_IMAGE_WIDTH - 2 * BUILD_IMAGE_PADDING;
			float height = BUILD_IMAGE_HEIGHT - 2 * BUILD_IMAGE_PADDING;
			
			if(i == 0){
				topPos += height * buildPercentage;
				height *= (1 - buildPercentage);
			}
			GUI.DrawTexture(new Rect(2 * BUILD_IMAGE_PADDING, topPos, width, height), buildMask);
		}
	}
	
	private void DrawStandardBuildingOptions(Building building) {
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = smallButtonHover;
		buttons.active.background = smallButtonClick;
		GUI.skin.button = buttons;
		
		int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH + BUTTON_SPACING;
		int topPos = buildAreaHeight - BUILD_IMAGE_HEIGHT / 2;
		int width = BUILD_IMAGE_WIDTH / 2;
		int height = BUILD_IMAGE_HEIGHT / 2;
		
		if(GUI.Button (new Rect(leftPos, topPos, width, height), building.sellImage)){
			building.Sell();	
		}
		
		leftPos += width + BUTTON_SPACING;
		
		if(building.hasSpawnPoint()) {
			if(GUI.Button (new Rect(leftPos, topPos, width, height), building.rallyPointImage)) {
				if(activeCursorState != CursorState.RallyPoint && previousCursorState != CursorState.RallyPoint) {
					SetCursorState(CursorState.RallyPoint);
				}
				else {
					//dirty hack
					SetCursorState(CursorState.PanRight);
					SetCursorState(CursorState.Select);
				}
			}
		}
	}
	
	public CursorState GetCursorState() {
		return activeCursorState;	
	}
	
	public CursorState GetPreviousCursorState() {
		return previousCursorState;	
	}
}
