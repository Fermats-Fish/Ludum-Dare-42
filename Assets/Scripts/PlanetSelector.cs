using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetSelector : MonoBehaviour {

	public GameObject planetSelectButtonPrefab;

	public static PlanetSelector instance;

	void Start () {

		if (instance != null){
			Debug.LogError ("There are two planet selectors for some reason!");
		} else {
			instance = this;
		}

	}

	public void AddPlanet(Planet p){
		GameObject go = Instantiate (planetSelectButtonPrefab, transform);
		go.GetComponent<Button> ().onClick.AddListener (() => {
			GameController.instance.SwitchToPlanet(p);
		});
		go.transform.GetChild (0).GetComponent<Text> ().text = p.name;
	}
}
