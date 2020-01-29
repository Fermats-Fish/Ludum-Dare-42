using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	public GameObject loseText;

	public const float EXPONENTIAL_POPULATION_GROWTH_RATE = 1.001f;
	public const float LINEAR_GROWTH_RATE = 2f;

	public List<Planet> planets = new List<Planet>();

	// Planet Creation Variables.
	public const int PLANET_SIZE_X_MIN = 10;
	public const int PLANET_SIZE_X_MAX = 20;
	public const int PLANET_SIZE_Y_MIN = 5;
	public const int PLANET_SIZE_Y_MAX = 14;
	public const float RESOURCE_ABUNDANCY_MIN = 0.2f;
	public const float RESOURCE_ABUNDANCY_MAX = 0.5f;

	const int STARTING_PLANET_SIZE_X = 16;
	const int STARTING_PLANET_SIZE_Y = 8;
	const float STARTING_PLANET_RESOURCE_ABUNDANCY = 0.3f;
	Dict<string, float> STARTING_PLANET_RELATIVE_RESOURCE_ABUNDANCY = new Dict<string, float>{
		{"metal", 1f},
		{"oil", 1f},
		{"gas", 0.5f}
	};

	const float STARTING_CITIES_PERCENT = 0.4f;
	const int STARTING_MINES = 3;
	const int STARTING_CITY_CAPACITY_LEFT = 200;

	const int STARTING_METAL = 300;
	const float STARTING_ENERGY_MULTIPLIER = 3f;

	public GameObject planetGO;

	public GameObject tilePrefab;
	public GameObject tileResourcePrefab;

	public GameObject buildingNotWorking;

	public static GameController instance;

	public float clock;

	int mouseX = -999;
	int mouseY = -999;


	// Fuel Costs
	public const float FUEL_COST_PER_RESOURCE = 1f; //0.1f;
	public const float FUEL_COST_PER_POP = 1f; //0.05f;
	public const int CONSTANT_FUEL_COST = 100; //20;
	public const int CONSTANT_METAL_COST = 1000; //20;

	Planet currentPlanet;

	void Start () {

		if (instance != null){
			Debug.LogError ("There are two game controllers for some reason!");
		} else {
			instance = this;
		}

		// Create the starting planet.
		planets.Add (new Planet ("Earth", STARTING_PLANET_SIZE_X, STARTING_PLANET_SIZE_Y, STARTING_PLANET_RESOURCE_ABUNDANCY, STARTING_PLANET_RELATIVE_RESOURCE_ABUNDANCY));
		currentPlanet = planets [0];

		// Load the planet so all the tiles etc. get created.
		LoadPlanet (planets [0]);

		// Figure out how many tiles there are on the planet.
		int noTiles = 0;
		for (int x = 0; x < STARTING_PLANET_SIZE_X; x++) {
			for (int y = 0; y < STARTING_PLANET_SIZE_Y; y++) {
				if (planets[0].GetTileAt(x,y).tileType == "land"){
					noTiles += 1;
				}
			}
		}

		int cityEnergyUsage = BuildingType.GetBuildingType ("city").inputs [0].amount;
		int mineEnergyUsage = BuildingType.GetBuildingType ("mine").inputs [0].amount;
		int oilRigEnergyUsage = BuildingType.GetBuildingType ("oilRig").inputs [0].amount;
		int oilRigProduction = BuildingType.GetBuildingType ("oilRig").outputs [0].amount;
		int generatorOilUsage = BuildingType.GetBuildingType ("generator").inputs [0].amount;
		int generatorEnergyProduction = BuildingType.GetBuildingType ("generator").outputs [0].amount;

		// Calculate the no. of cities to add.
		int noCities = (int)(STARTING_CITIES_PERCENT * noTiles);

		// Place enough generators to sustain the cities and mines, rounded down.
		int noGenerators = (int)(((float)noCities * cityEnergyUsage + STARTING_MINES * mineEnergyUsage) / (generatorEnergyProduction - oilRigEnergyUsage * generatorOilUsage / oilRigProduction));

		// Place enough oil rigs to power the generators rounded down.
		int noOilRigs = (int) (((float)noGenerators) * generatorOilUsage /oilRigProduction);

		// Create a bunch of starting buildings.
		for (int i = 0; i < noOilRigs; i++) {
			planets [0].AutoPlaceFreeBuilding (BuildingType.GetBuildingType("oilRig"));
		}
		for (int i = 0; i < STARTING_MINES; i++) {
			planets [0].AutoPlaceFreeBuilding (BuildingType.GetBuildingType("mine"));
		}
		for (int i = 0; i < noGenerators; i++) {
			planets [0].AutoPlaceFreeBuilding (BuildingType.GetBuildingType("generator"));
		}
		for (int i = 0; i < noCities; i++) {
			planets [0].AutoPlaceFreeBuilding (BuildingType.GetBuildingType("city"));
		}
		for (int i = 0; i < 0; i++) {
			planets [0].AutoPlaceFreeBuilding (BuildingType.GetBuildingType("turbine"));
		}

		// Set the starting population.
		planets [0].SetAmountOf ("population", noCities * BuildingType.GetBuildingType ("city").maxOccupants - STARTING_CITY_CAPACITY_LEFT);

		// Set starting resources.
		planets [0].ChangeAmountOf ("metal", STARTING_METAL);
		planets [0].ChangeAmountOf ("energy", (int)(noCities * cityEnergyUsage * STARTING_ENERGY_MULTIPLIER));

		// Set the current planet variable.
		currentPlanet = planets [0];

		// Set the camera somewhere nice :).
		Camera.main.transform.position = new Vector3 (STARTING_PLANET_SIZE_X / 2f, STARTING_PLANET_SIZE_Y / 2f, -100);
	}

	void Update () {

		clock += Time.deltaTime;

		if (clock >= 1f){
			foreach (Planet planet in planets) {
				planet.RunTick ();
			}

			while(clock >= 1f){
				clock -= 1f;
			}

		}

		// Do Inspector Text Mouse Over, unless over the UI.
		if (IsOverUI() == false) {


			Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			mousePos.z = -10f;

			int newX = (int)mousePos.x;
			int newY = (int)mousePos.y;

			// If our x/y pos has changed.
			if (newX != mouseX || newY != mouseY) {

				mouseX = newX;
				mouseY = newY;

				// If there is a building here.
				Tile t = GetCurrentPlanet ().GetTileAt (mouseX, mouseY);
				bool buildingThere = false;
				if (t != null) {
					Building b = t.building;
					if (b != null) {
						UIInspector.instance.UpdateInspectorText (b);

						// Also do adjacency visuals!
						UIInspector.instance.GenerateAdjacencyVisuals (b.buildingType, mouseX, mouseY);

						buildingThere = true;
					}
				}
				if (buildingThere == false) {
					UIInspector.instance.CloseInspectorText ();

					// Remove any old adjacency visuals.
					UIInspector.instance.ResetAdjacencyVisuals();
				}
			}

		}

	}

	public Planet GetCurrentPlanet (){
		return currentPlanet;
	}

	void LoadPlanet(Planet planet){

		// Load the tiles.
		for (int x = 0; x < planet.sizeX; x++) {
			for (int y = 0; y < planet.sizeY; y++) {

				// Load tile graphics.
				GameObject tile = Instantiate (tilePrefab, planetGO.transform);
				tile.name = "Tile (" + x.ToString () + ", " + y.ToString () + ")";
				tile.GetComponent<SpriteRenderer> ().sprite = planet.GetTileAt(x,y).GetSprite ();
				tile.transform.position = new Vector3 ((float)x, (float)y, 1f);

				// Load resource graphics.
				if (planet.GetTileAt(x,y).resource != null){
					GameObject tileResource = Instantiate (tileResourcePrefab, tile.transform);
					tileResource.name = planet.GetTileAt(x,y).resource;
					tileResource.GetComponent<SpriteRenderer> ().sprite = planet.GetTileAt(x,y).GetResourceSprite ();
					tileResource.transform.position += new Vector3 (0f, 0f, -1f);
				}

				// Load building graphics.
				if (planet.GetTileAt(x,y).building != null){
					PlaceBuildingVisuals (tile, x, y);
				}
			}
		}

		// Load resource numbers.
		planet.UpdateAllResourceVisuals ();

	}

	public void SwitchToPlanet(Planet planet){
		
		currentPlanet = planet;

		for (int i = 0; i < planetGO.transform.childCount; i++) {
			Destroy (planetGO.transform.GetChild (i).gameObject);
		}

		LoadPlanet (planet);
	}

	public void PlaceBuildingVisuals( int x, int y ){
		PlaceBuildingVisuals( GameObject.Find ("Tile (" + x.ToString() + ", " + y.ToString() + ")"), x, y);
	}

	void PlaceBuildingVisuals(GameObject tile, int x, int y){

		// If there is a resource under the bulding, make it small.
		if (tile.transform.childCount > 0){
			tile.transform.GetChild (0).localScale = new Vector3 (0.4f, 0.4f, 0.4f);
		}

		GameObject building = Instantiate (tileResourcePrefab, tile.transform);
		building.name = GetCurrentPlanet().GetTileAt(x,y).building.buildingType.name;
		building.GetComponent<SpriteRenderer> ().sprite = GetCurrentPlanet().GetTileAt(x,y).GetBuldingSprite ();
		building.transform.position += new Vector3 (0f, 0f, -0.5f);
	}

	public void RemoveBuildingVisuals( int x, int y ){
		RemoveBuildingVisuals( GameObject.Find ("Tile (" + x.ToString() + ", " + y.ToString() + ")"), x, y);
	}

	void RemoveBuildingVisuals(GameObject tile, int x, int y){

		// If there is a resource under the bulding, make it big, then destroy the building game object.
		if (tile.transform.childCount > 1){
			tile.transform.GetChild (0).localScale = new Vector3 (1f, 1f, 1f);
			Destroy (tile.transform.GetChild (1).gameObject);
		} else {
			Destroy (tile.transform.GetChild (0).gameObject);
		}

	}

	public void PlaceBuildingNotWorking(int x, int y){
		Instantiate(buildingNotWorking, new Vector3((float) x, (float) y, -1f), Quaternion.identity);
	}

	public void DisplayLoseText (){
		if (loseText.activeInHierarchy == true){
			return;
		}
		loseText.SetActive (true);
		int sum = 0;
		foreach (Planet planet in planets) {
			sum += planet.GetAmountOf ("population");
		}
		loseText.transform.GetChild(0).GetComponent<Text>().text += sum + ".";
	}

	public static bool IsOverUI(){
		return EventSystem.current.IsPointerOverGameObject ();
	}
}
