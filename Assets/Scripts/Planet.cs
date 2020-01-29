using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet {

	public string name;

	Tile[,] tiles;

	public int sizeX;
	public int sizeY;

	float seedX;
	float seedY;

	const float WAVELENGTH = 4f;
	const float SEALEVEL = 0.5f;

	Dict<string, float> relativeResourceAbundancy;

	float resourceAbundancyTotal = 0f;

	Dict<string, ResQuant> stockpile = new Dict<string, ResQuant>();

	List<Building>[] buildings;

	public Tile GetTileAt(int x, int y){
		if (x < 0 || y < 0 || x >= sizeX || y >= sizeY){
			return null;
		} else {
			return tiles [x, y];
		}
	}

	public int GetAmountOf(string resource){
		return stockpile.Get (resource).amount;
	}

	public void ChangeAmountOf(string resource, int delta){
		SetAmountOf(resource, stockpile.Get (resource).amount + delta);
	}

	public void SetAmountOf(string resource, int newAmount){
		stockpile.Get (resource).amount = newAmount;

		// Update the visuals if this is the currently viewed planet.
		if (GameController.instance.GetCurrentPlanet() == this) {
			UIResourceMenu.instance.SetAmountVisual (resource, GetAmountOf (resource));
		}
	}

	public void UpdateAllResourceVisuals (){
		foreach (string resource in stockpile.Keys) {
			UIResourceMenu.instance.SetAmountVisual (resource, GetAmountOf (resource));
		}
	}

	public bool CanAffordBuilding(BuildingType building){
		return CanAffordCost (building.buildCost);
	}

	public bool CanAffordCost(ResQuant[] cost){

		if (cost == null){
			return true;
		}

		foreach (ResQuant rq in cost) {
			if ( GetAmountOf(rq.resource) < rq.amount ){
				return false;
			}
		}

		return true;
	}

	public Planet(string name, int sizeX, int sizeY, float resourceAbundancy, Dict<string, float> relativeResourceAbundancy){

		this.name = name;

		PlanetSelector.instance.AddPlanet (this);

		foreach (var resource in ResQuant.ALL_RESOURCES) {
			stockpile.Add( resource, new ResQuant (resource, 0));
		}

		buildings = new List<Building>[BuildingType.NO_BUILDING_PRIORITIES];
		for (int i = 0; i < buildings.Length; i++) {
			buildings [i] = new List<Building> ();
		}

		this.relativeResourceAbundancy = relativeResourceAbundancy;

		foreach (float value in relativeResourceAbundancy.Values) {
			resourceAbundancyTotal += value;
		}

		this.sizeX = sizeX;
		this.sizeY = sizeY;

		seedX = Random.Range (0, 1000);
		seedY = Random.Range (0, 1000);

		tiles = new Tile[sizeX, sizeY];

		// Generate all of the tiles and their terrain types (e.g. water/land).
		for (int x = 0; x < sizeX; x++) {
			for (int y = 0; y < sizeY; y++) {
				GenerateTileAt (x, y);
			}
		}

		// Populate the map with resources.
		for (int i = 0; i < (int)(resourceAbundancy * sizeX * sizeY); i++) {
			GenerateResource ();
		}

	}

	public bool CanPlaceBuilding(BuildingType b, int x, int y){

		// First check that the tile type at those coords is okay.
		Tile t = GetTileAt (x, y);

		if (t == null) {
			return false;
		}

		foreach (string tt in b.invalidTileTypes) {
			if (tt == t.tileType) {
				return false;
			}
		}

		// Check there is no other building there.
		if (t.building != null) {
			return false;
		}

		// Now check that any required resources are there.
		if (b.requiredTileResource != "" && b.requiredTileResource != t.resource) {
			return false;
		}

		return true;

	}

	public void AutoPlaceFreeBuilding(BuildingType b){
		int fails = 0;

		int x = 0;
		int y = 0;

		while (true){
			// Get a random tile.
			x = Random.Range (0, sizeX);
			y = Random.Range (0, sizeY);

			if (CanPlaceBuilding(b, x, y)){
				break;
			}

			if (fails > 100){
				return;
			}

			fails += 1;
		}

		PlaceBuildingWithoutCostOrCheck(b, x, y);
	}

	public void PlaceBuilding(BuildingType b, int x, int y){

		// First check you can place the building there.
		if (CanPlaceBuilding(b, x, y) == false){
			return;
		}

		// Check you can afford the resource cost!!!
		if (CanAffordBuilding(b) == false){
			return;
		}

		// Apply resource cost!!!
		foreach (ResQuant rq in b.buildCost) {
			ChangeAmountOf (rq.resource, -rq.amount);
		}

		// Place the building!
		PlaceBuildingWithoutCostOrCheck (b, x, y);
	}

	public void PlaceBuildingWithoutCostOrCheck(BuildingType b, int x, int y){

		// Now place buliding!
		Tile t = GetTileAt (x, y);
		Building newBuilding = new Building(b, 0, 0, t);

		// Add it to the list of buildings.
		buildings [newBuilding.buildingType.priority].Add (newBuilding);

		// Now set the graphics.
		GameController.instance.PlaceBuildingVisuals (x, y);
	}

	public void DestroyBuildingAt(int x, int y){
		Tile t = GetTileAt (x, y);

		if (t == null){
			return;
		}

		Building b = t.building;

		if (b == null){
			return;
		}

		// Remove from list of buildings.
		buildings [b.buildingType.priority].Remove (b);

		// Remove from tile.
		GetTileAt (x, y).building = null;

		// Remove visuals.
		GameController.instance.RemoveBuildingVisuals (x, y);

	}

	// Generates the tile at the given location.
	void GenerateTileAt(int x, int y){

		Tile tile = new Tile (x, y);

		if (Mathf.PerlinNoise (seedX + x / WAVELENGTH, seedY + y / WAVELENGTH) > SEALEVEL) {
			tile.tileType = "land";
		} else {
			tile.tileType = "water";
		}

		tiles [x, y] = tile;

	}

	// Generates the resource at a random location.
	void GenerateResource(){

		// First choose the resource based on the relative abundancies given.
		float rand = Random.Range (0f, resourceAbundancyTotal);
		float c = 0f;
		string selectedResource = "";
		foreach (string resource in relativeResourceAbundancy.Keys) {

			c += relativeResourceAbundancy.Get (resource);
			if (c >= rand){
				selectedResource = resource;
				break;
			}
		}


		// Now find a tile to place the resource on.
		int fails = 0;

		int x = 0;
		int y = 0;

		while (true){
			// Get a random tile.
			x = Random.Range (0, sizeX);
			y = Random.Range (0, sizeY);

			if (GetTileAt(x,y).tileType != "water" || selectedResource == "oil"){
				break;
			}

			if (fails > 100){
				return;
			}

			fails += 1;
		}

		GetTileAt (x, y).resource = selectedResource;
	}

	public float GetAdjacency(Building building){

		float multiplier = 1f;

		foreach (AdjacencyBonus adjBonus in building.buildingType.adjacency) {

			// Loop through all the squares in the range of this adjacency.
			for (int dx = -adjBonus.range; dx <= adjBonus.range; dx++) {
				for (int dy = -adjBonus.range; dy <= adjBonus.range; dy++) {

					// Don't check the tile we are currently on.
					if (dx == 0 && dy == 0){
						continue;
					}

					// See if the tile here matches the adjacency criterion.
					Tile t = GameController.instance.GetCurrentPlanet ().GetTileAt (building.tile.x + dx, building.tile.y + dy);
					if ( t != null && t.MatchesCriterion (adjBonus.source) ){
						multiplier *= adjBonus.multiplier;
					}

				}
			}

		}

		return multiplier;
	}

	public void RunTick(){

		// Run all of the tick's for the buildings.

		// Initialise the list of building production to add to the inventory at the end of the tick.
		List<ResQuant> tickProduction = new List<ResQuant> ();

		int popCapacity = 0;

		foreach (List<Building> buildingList in buildings) {
			foreach (Building building in buildingList) {

				// Only do something if we can afford the building's upkeep.
				if (CanAffordCost(building.buildingType.inputs)){

					// Pay the input.
					if (building.buildingType.inputs != null) {
						foreach (ResQuant rq in building.buildingType.inputs) {
							ChangeAmountOf (rq.resource, -rq.amount);
						}
					}

					// Figure out the adjacency bonuses.
					float adjacencyBonus = GetAdjacency (building);

					// Get the output.
					if (building.buildingType.outputs != null) {
						foreach (ResQuant rq in building.buildingType.outputs) {

							// Add output to the tick production list.
							tickProduction.Add (new ResQuant (rq.resource, (int)(adjacencyBonus * rq.amount)));

						}
					}

					popCapacity += (int)(building.buildingType.maxOccupants * adjacencyBonus);
				}

				else {
					if (GameController.instance.GetCurrentPlanet() == this){
						GameController.instance.PlaceBuildingNotWorking (building.tile.x, building.tile.y);
					}
				}

			}
		}

		// Apply tick production.
		foreach (ResQuant rq in tickProduction) {
			ChangeAmountOf (rq.resource, rq.amount);
		}

		// Update population stats.
		int pop = GetAmountOf ("population");
		SetAmountOf ("population", (int)(Random.Range(0f,1f) + GameController.LINEAR_GROWTH_RATE + pop * (GameController.EXPONENTIAL_POPULATION_GROWTH_RATE)));
		int homeless = pop - popCapacity;
		if (homeless < 0){
			homeless = 0;
		}
		SetAmountOf ("homeless", homeless);
		SetAmountOf ("homes", popCapacity);

		// If Any planet has at least 1000 pops, at least 50% of which are homeless, then you lose.
		if (GetAmountOf("population") >= 1000 && homeless > pop/2 ){
			GameController.instance.DisplayLoseText ();
		}
	}

}

public class Tile {

	public string tileType;
	public string resource;
	public Building building;

	public int x;
	public int y;

	public Tile(int x, int y){
		this.x = x; this.y = y;
	}

	public Sprite GetSprite(){
		return Resources.Load<Sprite> ("Tiles/" + tileType);
	}

	public Sprite GetResourceSprite(){
		return Resources.Load<Sprite> ("Tiles/Resources/" + resource);
	}

	public Sprite GetBuldingSprite(){
		return Resources.Load<Sprite> ("Buildings/" + building.buildingType.name);
	}

	public bool MatchesCriterion(string criterion){
		if (criterion == "empty"){
			return building == null;
		} else if (criterion == tileType) {
			return true;
		} else if (building != null && criterion == building.buildingType.name){
			return true;
		} else {
			return false;
		}
	}

}

public class ResQuant {

	public static string[] ALL_RESOURCES = new string[]{ "energy", "gas", "oil", "metal", "population", "homes", "homeless" };
	public string resource;
	public int amount;
	public ResQuant (string resource, int amount){
		this.resource = resource;
		this.amount = amount;
	}
}

public class Building {
	public BuildingType buildingType;
	public int occupants;
	public int workers;
	public Tile tile;
	public Building(BuildingType buildingType, int occupants, int workers, Tile tile){
		this.buildingType = buildingType;
		this.occupants = occupants;
		this.workers = workers;
		this.tile = tile;
		this.tile = tile;
		tile.building = this;
	}

}

public class AdjacencyBonus {

	public string source;
	public int range;
	public float multiplier;

	public AdjacencyBonus( string source, int range, float multiplier ){
		this.source = source;
		this.range = range;
		this.multiplier = multiplier;
	}

}

public class BuildingType {

	static AdjacencyBonus[][] adj = new AdjacencyBonus[][]{
		/*city      */ new AdjacencyBonus[]{   new AdjacencyBonus("city",  1, 0.95f), new AdjacencyBonus("mine",  1, 0.9f ), new AdjacencyBonus("generator", 1, 1.1f ), new AdjacencyBonus("turbine", 1, 1.03f) },
		/*turbine   */ new AdjacencyBonus[]{   new AdjacencyBonus("water", 1, 1.02f), new AdjacencyBonus("empty", 1, 1.02f)   },
		/*oilRig    */ new AdjacencyBonus[]{      },
		/*mine      */ new AdjacencyBonus[]{      },
		/*gasMine   */ new AdjacencyBonus[]{      },
		/*generator */ new AdjacencyBonus[]{   new AdjacencyBonus("oilRig", 2, 1.1f)   }
	};

	public const int NO_BUILDING_PRIORITIES = 3;

	public static BuildingType[] buildingTypes = new BuildingType[] {
		//            Building Name  | Invalid Tile Types | Required Tile Resource | BuildCost                                     | Inputs                                   | Outputs                                   | w|o | p| adjacency
		new BuildingType( "city",     new string[]{"water"},    "",                  new ResQuant[]{new ResQuant("metal",  600)},     new ResQuant[]{new ResQuant("energy", 10)}, null,                                      0,100,0, adj[0]),
		new BuildingType( "turbine",  new string[]{"water"},    "",                  new ResQuant[]{new ResQuant("metal",  100 )},    null,                                       new ResQuant[]{new ResQuant("energy", 15)},0,0,  1, adj[1]),
		new BuildingType( "oilRig",   new string[]{},           "oil",               new ResQuant[]{new ResQuant("metal",  100 )},    new ResQuant[]{new ResQuant("energy", 10)}, new ResQuant[]{new ResQuant("oil",    10)},0,0,  1, adj[2]),
		new BuildingType( "mine",     new string[]{"water"},    "metal",             new ResQuant[]{new ResQuant("metal",  100 )},    new ResQuant[]{new ResQuant("energy", 10)}, new ResQuant[]{new ResQuant("metal",  10)},0,0,  2, adj[3]),
		new BuildingType( "gasMine",  new string[]{"water"},    "gas",               new ResQuant[]{new ResQuant("metal",  600 )},    new ResQuant[]{new ResQuant("energy", 20)}, new ResQuant[]{new ResQuant("gas",    2 )},0,0,  2, adj[4]),
		new BuildingType( "generator",new string[]{"water"},    "",                  new ResQuant[]{new ResQuant("metal",  100 )},    new ResQuant[]{new ResQuant("oil", 20)},    new ResQuant[]{new ResQuant("energy", 80)},0,0,  1, adj[5])
	};

	public string name;
	public string[] invalidTileTypes;
	public string requiredTileResource;
	public ResQuant[] buildCost;
	public ResQuant[] inputs;
	public ResQuant[] outputs;
	public int maxWorkers;
	public int maxOccupants;
	public int priority;
	public AdjacencyBonus[] adjacency;

	BuildingType ( string name, string[] invalidTileTypes, string requiredTileResource, ResQuant[] buildCost, ResQuant[] inputs, ResQuant[] outputs, int maxWorkers, int maxOccupants, int priority, AdjacencyBonus[] adjacency ){
		this.name = name;
		this.invalidTileTypes = invalidTileTypes;
		this.requiredTileResource = requiredTileResource;
		this.buildCost = buildCost;
		this.inputs = inputs;
		this.outputs = outputs;
		this.maxWorkers = maxWorkers;
		this.maxOccupants = maxOccupants;
		this.priority = priority;
		this.adjacency = adjacency;
	}

	public static BuildingType GetBuildingType(string name ){
		////////////////////////////////// COULD BE O(1) INSTEAD OF O(n) if the building types were implimented as a dictionary instead.
		foreach (var building in buildingTypes) {
			if (building.name == name){
				return building;
			}
		}
		return null;
	}

}


public class Dict<K, V> : Dictionary<K, V> {

	public V Get(K key){
		V value = default(V);
		TryGetValue (key, out value);
		return value;
	}

}