using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInspector : MonoBehaviour {

	public static UIInspector instance;

	public GameObject adjacencyBonusIndicatorPrefab;

	List<GameObject> adjacencyBonusIndicators = new List<GameObject> ();

	Text text;
	GameObject goToActivate;

	bool adjacencyVisualsLocked = false;

	public void LockAdjacencyVisuals(){
		adjacencyVisualsLocked = true;
	}

	public void UnlockAdjacencyVisuals(){
		adjacencyVisualsLocked = false;
	}

	void Start () {

		if (instance != null){
			Debug.LogError ("There are two inspectors for some reason!");
		} else {
			instance = this;
		}

		text = transform.GetChild (0).GetChild (0).GetComponent<Text> ();
		goToActivate = transform.GetChild (0).gameObject;
	}

	void RemoveText(){
		goToActivate.SetActive (true);
		text.text = "";
	}

	public void ResetAdjacencyVisuals(){

		if (adjacencyVisualsLocked){
			return;
		}

		if (adjacencyBonusIndicators != null){
			foreach (GameObject go in adjacencyBonusIndicators) {
				Destroy (go);
			}
		}
		adjacencyBonusIndicators = new List<GameObject> ();
	}

	public void GenerateAdjacencyVisuals( BuildingType bt, int x, int y ){

		if (adjacencyVisualsLocked){
			return;
		}

		ResetAdjacencyVisuals ();

		foreach (AdjacencyBonus adjBonus in bt.adjacency) {

			// Loop through all the squares in the range of this adjacency.
			for (int dx = -adjBonus.range; dx <= adjBonus.range; dx++) {
				for (int dy = -adjBonus.range; dy <= adjBonus.range; dy++) {

					// Don't check the tile we are currently on.
					if (dx == 0 && dy == 0){
						continue;
					}

					// See if the tile here matches the adjacency criterion.
					Tile t = GameController.instance.GetCurrentPlanet ().GetTileAt (x + dx, y + dy);
					if ( t != null && t.MatchesCriterion (adjBonus.source) ){

						// The criterion for this position is a match! Place a thing to show the user!
						GameObject adjBonusIndicator = Instantiate (adjacencyBonusIndicatorPrefab, new Vector3 ((float)(x + dx), (float)(y + dy)), Quaternion.identity);
						adjacencyBonusIndicators.Add (adjBonusIndicator);

						// If the bonus is negative, change the color of the indicator.
						if (adjBonus.multiplier < 1f){
							adjBonusIndicator.GetComponent<SpriteRenderer> ().color = Color.red;
						}
					}

				}
			}

		}
	}

	void AddGeneralText( BuildingType bt ){
		string t = bt.name.ToUpper() + ":";
		if (bt.buildCost != null){
			t += "\n  Build Cost: " + FormatResQuantArray(bt.buildCost);
		}
		if (bt.inputs != null){
			t += "\n  Upkeep Cost: " + FormatResQuantArray(bt.inputs);
		}
		if (bt.outputs != null){
			t += "\n  Produces: " + FormatResQuantArray(bt.outputs);
		}
		if (bt.maxOccupants != 0){
			t += "\n  Houses: " + bt.maxOccupants;
		}
		if (bt.requiredTileResource != "") {
			t += "\n  Built On: " + bt.requiredTileResource;
		}
		text.text += t;
	}

	string FormatResQuantArray(ResQuant[] resQuants){
		return FormatResQuantArray (resQuants, 1f);
	}

	string FormatResQuantArray(ResQuant[] resQuants, float multiplier){
		string t = "";
		foreach (ResQuant rq in resQuants) {
			t += ((int)(multiplier * rq.amount)).ToString () + " " + rq.resource + ",";
		}
		t = t.Substring (0, t.Length - 1);
		return t;
	}

	void AddBuildingText( Building b ){
		BuildingType bt = b.buildingType;
		float adjacency = GameController.instance.GetCurrentPlanet ().GetAdjacency (b);
		string t = bt.name.ToUpper() + ":";

		t += "\n  Adjacency Bonus: " + (100f * (adjacency - 1)).ToString ("F1") + "%";

		if (bt.inputs != null){
			t += "\n  Upkeep Cost: " + FormatResQuantArray(bt.inputs);
		}
		if (bt.outputs != null){
			t += "\n  Produces: " + FormatResQuantArray(bt.outputs, adjacency);
			t += "\n   (default: " + FormatResQuantArray (bt.outputs) + ")";
		}
		if (bt.maxOccupants != 0){
			t += "\n  Houses: " + ((int)(adjacency * bt.maxOccupants)).ToString();
			t += "\n   (default: " + bt.maxOccupants.ToString () + ")";
		}
		if (bt.requiredTileResource != "") {
			t += "\n  Built On: " + bt.requiredTileResource;
		}
		text.text += t;
	}

	public void UpdateInspectorText(BuildingType buildingType){
		RemoveText ();
		AddGeneralText (buildingType);
	}

	public void UpdateInspectorText(Building building){
		RemoveText ();
		AddBuildingText (building);
	}

	public void CloseInspectorText(){
		goToActivate.SetActive (false);
	}

}
