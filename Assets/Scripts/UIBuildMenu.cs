using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildMenu : MonoBehaviour {

	public GameObject buildingButtonPrefab;

	void Start () {

		foreach (BuildingType bt in BuildingType.buildingTypes) {
			GameObject newButton = Instantiate (buildingButtonPrefab, transform);
			newButton.name = bt.name;
			newButton.GetComponent<Image> ().sprite = Resources.Load<Sprite>("Buildings/" + bt.name);
			newButton.GetComponent<BuildingButton> ().SetBuildingType (bt);
		}

	}
}
