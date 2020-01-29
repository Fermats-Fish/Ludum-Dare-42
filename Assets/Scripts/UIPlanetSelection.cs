using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlanetSelection : MonoBehaviour {

	public Dict<string, int> load = new Dict<string, int>();

	public GameObject rocketDestinationButtonPrefab;

	public GameObject newPlanetPrefab;

	public GameObject content;

	void Start () {

		// Create a button for each planet.
		foreach (Planet planet in GameController.instance.planets) {
			GameObject go = Instantiate (rocketDestinationButtonPrefab, content.transform);
			go.transform.GetChild (0).GetComponent<Text>().text = planet.name;
			go.GetComponent<Button> ().onClick.AddListener (() => {
				GoToPlanet(planet);
			});
		}

	}

	public void GoToPlanet( Planet p ){
		foreach (string res in load.Keys) {
			p.ChangeAmountOf (res, load.Get (res));
		}
		if (p != GameController.instance.GetCurrentPlanet()){
			GameController.instance.SwitchToPlanet (p);
		}
		Destroy (gameObject);
	}

	public void NewPlanet () {

		GameObject go = Instantiate (newPlanetPrefab, transform.parent);
		go.GetComponent<UIPlanetCreation> ().planetSelect = this;
		gameObject.SetActive (false);

	}
}
