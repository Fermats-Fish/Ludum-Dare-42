using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRocketLaunch : MonoBehaviour {

	public GameObject resourceInputPrefab;
	public GameObject planetSelectionMenuPrefab;

	Dict<string, int> load = new Dict<string, int>();

	Text fuelCostText;

	void Start () {

		transform.GetChild (3).GetChild (1).GetComponent<Text> ().text = GameController.CONSTANT_METAL_COST.ToString();
		fuelCostText = transform.GetChild (4).GetChild (1).GetComponent<Text> ();

		UpdateFuelCostVisual ();

		Transform resourceInputLayoutTransform = transform.GetChild (1);

		// Place a resource input prefab for each resource.
		for (int i = 0; i < ResQuant.ALL_RESOURCES.Length - 2; i ++) {
			
			string resource = ResQuant.ALL_RESOURCES [i];
			GameObject go = Instantiate (resourceInputPrefab, resourceInputLayoutTransform);
			go.transform.GetChild(0).GetComponent<Image> ().sprite = Resources.Load<Sprite>("Resources/" + resource);

			// Tell the input field what to do when its value is changed.
			go.transform.GetChild(1).GetComponent<InputField>().onValueChanged.AddListener( (string newValue) => {
				int value = 0;
				if (newValue != ""){
					value = int.Parse(newValue);
				}
				UpdateLoad(resource, value);
				UpdateFuelCostVisual();
			});

			UpdateLoad (resource, 0);

		}
	}

	void UpdateFuelCostVisual(){
		fuelCostText.text = GetFuelCost ().ToString ();
	}

	void UpdateLoad(string resource, int amount){
		if (load.ContainsKey(resource)){
			load.Remove (resource);
		}
		load.Add (resource, amount);
	}
	
	int GetFuelCost(){
		float amount = 0;
		foreach (string res in load.Keys) {
			if (res == "population"){
				amount += load.Get (res) * GameController.FUEL_COST_PER_POP;
			} else {
				amount += load.Get (res) * GameController.FUEL_COST_PER_RESOURCE;
			}
		}
		return (int) amount + GameController.CONSTANT_FUEL_COST;
	}

	bool CanLaunch(){

		// Check we have all of the relevant resources.
		foreach (string res in load.Keys) {
			int amountNeeded = load.Get(res);
			if (res == "gas") {
				amountNeeded += GetFuelCost ();
			} else if (res == "metal"){
				amountNeeded += GameController.CONSTANT_METAL_COST;
			}
			if (GameController.instance.GetCurrentPlanet().GetAmountOf(res) < amountNeeded ){
				return false;
			}
		}
		return true;
	}

	public void Launch(){

		// First check we can launch.
		if (CanLaunch()){

			// Remove the correct amount of each resource from the menu.
			foreach (string res in load.Keys) {
				int amountNeeded = load.Get(res);
				if (res == "gas") {
					amountNeeded += GetFuelCost ();
				} else if (res == "metal"){
					amountNeeded += GameController.CONSTANT_METAL_COST;
				}
				GameController.instance.GetCurrentPlanet ().ChangeAmountOf (res, -amountNeeded);
			}

			// Close this menu and open the next!
			GameObject go = Instantiate (planetSelectionMenuPrefab, transform.parent);
			go.GetComponent<UIPlanetSelection> ().load = load;
			gameObject.SetActive (false);

		}

	}

}
