using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildingButton : DragButton, IPointerEnterHandler, IPointerExitHandler {

	public BuildingType buildingType;

	public void SetBuildingType(BuildingType bt){
		buildingType = bt;
	}
	public void OnPointerEnter(PointerEventData eventData){
		UIInspector.instance.UpdateInspectorText (buildingType);
	}
	public void OnPointerExit(PointerEventData eventData){
		UIInspector.instance.CloseInspectorText ();
	}

	protected override void OnDragPosChanged(){
		// Update building ghost colour to indicate whether it can be placed or not.
		bool canPlace = GameController.instance.GetCurrentPlanet().CanPlaceBuilding (buildingType, x, y);
		if (canPlace == false) {
			buildingGhostSR.color = new Color(1f, 0.5f, 0.5f);
		} else {
			buildingGhostSR.color = Color.white;
		}

		// Check for adjacency bonuses in the area.
		UIInspector.instance.UnlockAdjacencyVisuals ();
		UIInspector.instance.GenerateAdjacencyVisuals (buildingType, x, y);
		UIInspector.instance.LockAdjacencyVisuals ();
	}

	protected override string GetName(){
		return buildingType.name;
	}

	protected override void StartDrag(){
		base.StartDrag();

		// Add visuals for the building cost.
		foreach (ResQuant resQ in buildingType.buildCost) {
			UIResourceMenu.instance.SetCostVisual (resQ.resource, resQ.amount);
		}

		// Lock adjacency visuals so that the adjacencies from buildings which are already placed don't show up.
		UIInspector.instance.LockAdjacencyVisuals ();
	}

	protected override void PlaceBuilding(){
		GameController.instance.GetCurrentPlanet ().PlaceBuilding (buildingType, x, y);
	}

	protected override void EndPlacementMode(){
		foreach (ResQuant resQ in buildingType.buildCost) {
			UIResourceMenu.instance.SetCostVisual (resQ.resource, 0);
		}
		base.EndPlacementMode ();
	}

	/*public GameObject emptySpritePrefab;

	GameObject buildingGhost = null;
	SpriteRenderer buildingGhostSR = null;

	int x;
	int y;

	bool multiplePlacementMode = false;

	public void SetBuildingType(BuildingType bt){
		buildingType = bt;
	}

	public void OnPointerDown(PointerEventData eventData){
		if (eventData.button == 0) {
			// If already in building placement mode, end it.
			if (multiplePlacementMode == true){
				EndPlacementMode ();
			} else {
				StartDrag ();
			}
		}
	}
	public void OnPointerEnter(PointerEventData eventData){
		UIInspector.instance.UpdateInspectorText (buildingType);
	}
	public void OnPointerExit(PointerEventData eventData){
		UIInspector.instance.CloseInspectorText ();
	}

	void Update(){
		if (buildingGhost != null){

			// Update building ghost position
			Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			mousePos.z = -10f;
			buildingGhost.transform.position = mousePos + new Vector3(-0.5f, -0.5f, 0f);

			int newX = (int) mousePos.x; int newY = (int) mousePos.y;

			// If our x/y pos has changed.
			if (newX != x || newY != y){


				// Update our x/y.
				x = newX; y = newY;

				// Update building ghost colour to indicate whether it can be placed or not.
				bool canPlace = GameController.instance.GetCurrentPlanet().CanPlaceBuilding (buildingType, x, y);
				if (canPlace == false) {
					buildingGhostSR.color = new Color(1f, 0.5f, 0.5f);
				} else {
					buildingGhostSR.color = Color.white;
				}

				// Check for adjacency bonuses in the area.
				UIInspector.instance.UnlockAdjacencyVisuals ();
				UIInspector.instance.GenerateAdjacencyVisuals (buildingType, x, y);
				UIInspector.instance.LockAdjacencyVisuals ();

			}


			if (Input.GetMouseButtonUp(0)){
				// If the user is still over the button, or some other UI, then set it into multiple placement mode.
				if (GameController.IsOverUI()){
					multiplePlacementMode = true;
				}

				// Otherwise we want to place the building and exit out of building placement mode (unless in multiple placement mode).
				else if (multiplePlacementMode == false){
					PlaceBuilding ();
					EndPlacementMode ();
				}
			}

			if (Input.GetMouseButtonDown(0)){
				// The user is in multiple building mode, so place a single building.
				PlaceBuilding ();
			}

			if (Input.GetMouseButtonDown(1)){
				// We don't want to place the building anymore.
				EndPlacementMode ();

				// Don't drag the sreen.
				CameraController.instance.dontDrag = true;
			}
		}
	}

	void StartDrag(){
		// By default, don't start multiple placement mode.
		multiplePlacementMode = false;

		// Instantiate a building sprite, but get it to follow the mouse.
		buildingGhost = Instantiate (emptySpritePrefab);
		buildingGhost.name = buildingType.name;
		buildingGhostSR = buildingGhost.GetComponent<SpriteRenderer> ();
		buildingGhostSR.sprite = this.GetComponent<Image>().sprite;
		x = -999;
		y = -999;

		// Add visuals for the building cost.
		foreach (ResQuant resQ in buildingType.buildCost) {
			UIResourceMenu.instance.SetCostVisual (resQ.resource, resQ.amount);
		}

		// Lock adjacency visuals so that the adjacencies from buildings which are already placed don't show up.
		UIInspector.instance.LockAdjacencyVisuals ();
	}

	void PlaceBuilding(){
		GameController.instance.GetCurrentPlanet ().PlaceBuilding (buildingType, x, y);
	}

	void EndPlacementMode(){
		multiplePlacementMode = false;
		foreach (ResQuant resQ in buildingType.buildCost) {
			UIResourceMenu.instance.SetCostVisual (resQ.resource, 0);
		}
		DestroyBuildingGhost ();
		UIInspector.instance.UnlockAdjacencyVisuals ();
		UIInspector.instance.ResetAdjacencyVisuals ();
	}

	void DestroyBuildingGhost(){
		Destroy (buildingGhost);
		buildingGhost = null;
	}*/

}
