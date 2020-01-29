using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlanetCreation : MonoBehaviour {

	public UIPlanetSelection planetSelect;

	public GameObject planetCreationButtonPrefab;

	string pName = "";

	void Start () {
		transform.GetChild (1).GetComponent<InputField> ().onValueChanged.AddListener ((string newValue) => {
			pName = newValue;
		});

		// Create three buttons which the user can press.
		for (int i = 0; i < 3; i++) {
			GameObject go = Instantiate (planetCreationButtonPrefab, transform.GetChild (2));

			// Each planet can have a few variables. size (x/y), resourceAbundancy, relativeResourceAbundancy (metal, oil, gas). Generate them randomly.
			int sizeX = Random.Range (GameController.PLANET_SIZE_X_MIN, GameController.PLANET_SIZE_X_MAX+1);
			int sizeY = Random.Range (GameController.PLANET_SIZE_Y_MIN, GameController.PLANET_SIZE_Y_MAX+1);
			float resourceAbundancy = Random.Range (GameController.RESOURCE_ABUNDANCY_MIN, GameController.RESOURCE_ABUNDANCY_MAX);
			float relMetal = Random.Range (0f, 1f);
			float relOil = Random.Range (0f, 1f);
			float relGas = Random.Range (0f, 1f);

			float total = relMetal + relOil + relGas;

			// Update the text for this button.
			go.transform.GetChild(0).GetComponent<Text>().text = "Size: " + sizeX.ToString() + " x " + sizeY.ToString() + " = " + (sizeX * sizeY).ToString() + "\n" + 
				"Resource Abundancy: " + (100 * resourceAbundancy).ToString("F1") + "%" + "\n" + 
				"Resource Proportions:\nMetal " + (100 * relMetal / total).ToString("F1") + "%, Oil " + (100 * relOil / total).ToString("F1") + "%, Gas " + (100 * relGas / total).ToString("F1") + "%";

			// Set what happes on button click.
			go.GetComponent<Button> ().onClick.AddListener (() => {

				if (pName == ""){
					pName = "Planet " + (GameController.instance.planets.Count+1).ToString ();
				}

				// Create this planet.
				Planet p = new Planet(pName, sizeX, sizeY, resourceAbundancy, new Dict<string,float>{ {"metal", relMetal}, {"oil", relOil}, {"gas", relGas} });

				// Add to list of plaets.
				GameController.instance.planets.Add(p);

				planetSelect.gameObject.SetActive(true);
				planetSelect.GoToPlanet(p);
				Destroy(gameObject);
			});
		}
	}

	void Update () {
		
	}
}
