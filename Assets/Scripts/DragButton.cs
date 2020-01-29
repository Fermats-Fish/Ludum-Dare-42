using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragButton : MonoBehaviour, IPointerDownHandler {

	public GameObject emptySpritePrefab;

	protected GameObject buildingGhost = null;
	protected SpriteRenderer buildingGhostSR = null;

	protected int x;
	protected int y;

	bool multiplePlacementMode = false;

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

				OnDragPosChanged ();

			}


			if (Input.GetMouseButtonUp(0)){
				// If the user is still over the button, or some other UI, then set it into multiple placement mode.
				if (GameController.IsOverUI()){
					multiplePlacementMode = true;

					// Don't drag the sreen on right click.
					CameraController.instance.LockDragging ();
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
			}
		}
	}

	protected virtual string GetName(){
		return "Default - Should Probably be changed.";
	}

	protected virtual void StartDrag(){
		// By default, don't start multiple placement mode.
		multiplePlacementMode = false;

		// Instantiate a building sprite, but get it to follow the mouse.
		buildingGhost = Instantiate (emptySpritePrefab);
		buildingGhost.name = GetName ();
		buildingGhostSR = buildingGhost.GetComponent<SpriteRenderer> ();
		buildingGhostSR.sprite = this.GetComponent<Image>().sprite;
		x = -999;
		y = -999;
	}

	protected virtual void OnDragPosChanged(){
		
	}

	protected virtual void PlaceBuilding(){
		
	}

	protected virtual void EndPlacementMode(){
		multiplePlacementMode = false;
		DestroyBuildingGhost ();
		UIInspector.instance.UnlockAdjacencyVisuals ();
		UIInspector.instance.ResetAdjacencyVisuals ();

		CameraController.instance.UnlockDragging ();
	}

	void DestroyBuildingGhost(){
		Destroy (buildingGhost);
		buildingGhost = null;
	}

}
