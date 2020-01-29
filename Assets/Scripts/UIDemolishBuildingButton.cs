using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIDemolishBuildingButton : DragButton {

	protected override void PlaceBuilding(){
		GameController.instance.GetCurrentPlanet ().DestroyBuildingAt (x, y);
	}

	protected override string GetName(){
		return "Demolisher!!";
	}

	/*public GameObject emptySpritePrefab;

	GameObject buildingGhost = null;
	SpriteRenderer buildingGhostSR = null;

	int x;
	int y;

	public void OnPointerDown(PointerEventData eventData){
		if (eventData.button == 0) {
			StartDrag ();
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

			}


			if (Input.GetMouseButtonUp(0)){
				// We want to demolish the building.
				PlaceBuilding ();
			}

			if (Input.GetMouseButtonDown(1)){
				// We don't want to demolish a building anymore.
				EndDrag ();
			}
		}
	}

	void StartDrag(){
		// Instantiate a building sprite, but get it to follow the mouse.
		buildingGhost = Instantiate (emptySpritePrefab);
		buildingGhost.name = "Demolisher!!";
		buildingGhostSR = buildingGhost.GetComponent<SpriteRenderer> ();
		buildingGhostSR.sprite = this.GetComponent<Image>().sprite;
		x = -999;
		y = -999;
	}

	void PlaceBuilding(){
		GameController.instance.GetCurrentPlanet ().DestroyBuildingAt (x, y);
		EndDrag ();
	}

	void EndDrag(){
		DestroyBuildingGhost ();
	}

	void DestroyBuildingGhost(){
		Destroy (buildingGhost);
		buildingGhost = null;
	}*/

}
