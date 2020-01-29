using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIResourceMenu : MonoBehaviour {

	public GameObject resourcePrefab;
	Dict<string, Text[]> resourceText = new Dict<string, Text[]>();

	public static UIResourceMenu instance;

	void Start () {

		if (instance != null){
			Debug.LogError ("There are two game UIResourceMenus for some reason!");
		} else {
			instance = this;
		}

		foreach (string resource in ResQuant.ALL_RESOURCES) {
			GameObject newButton = Instantiate (resourcePrefab, transform);
			newButton.name = resource;
			newButton.GetComponent<Image> ().sprite = Resources.Load<Sprite>("Resources/" + resource);
			resourceText.Add (resource, new Text[] {
				newButton.transform.GetChild (0).GetComponent<Text> (),
				newButton.transform.GetChild (1).GetComponent<Text> ()
			});
		}

	}

	public void SetAmountVisual(string resource, int newAmount){
		if (resource == "homeless"){
			resourceText.Get (resource) [0].text = (100f * ( (float) newAmount) / GameController.instance.GetCurrentPlanet().GetAmountOf("population")).ToString ("F1") + "%";
		} else {
			resourceText.Get (resource) [0].text = newAmount.ToString ();
		}
	}

	public void SetCostVisual(string resource, int cost){
		if (cost == 0){
			resourceText.Get (resource) [1].gameObject.SetActive (false);
		} else {
			resourceText.Get (resource) [1].gameObject.SetActive (true);
			resourceText.Get (resource) [1].text = (-cost).ToString ();

			// If we have enough of the resource display the text white, else red.
			if (GameController.instance.GetCurrentPlanet ().GetAmountOf (resource) >= cost) {
				resourceText.Get (resource) [1].color = Color.white;
			} else {
				resourceText.Get (resource) [1].color = new Color(1f, 6f/16f, 0f);
			}
		}
	}

}
