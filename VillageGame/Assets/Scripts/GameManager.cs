using UnityEngine;
using System.Collections;
//including some .NET for dynamic arrays called List in C#
using System.Collections.Generic;

[System.Serializable]
public class GameManager : MonoBehaviour
{
	// weight parameters are set in editor and used by all villagers 
	// if they are initialized here, the editor will override settings	 
	// weights used to arbitrate btweeen concurrent steering forces 
	public float alignmentWt;
	public float separationWt;
	public float cohesionWt;
	public float avoidWt;
	public float inBoundsWt;

	// these distances modify the respective steering behaviors
	public float avoidDist;
	public float separationDist;

	private int deadVillagers;
	public int DeadVillagers { get { return deadVillagers; } set { deadVillagers = value; } }

	private int savedVillagers;
	public int SavedVillagers { get { return savedVillagers; } set { savedVillagers = value; } }

	// set in editor to promote reusability.
	public int numberOfvillagers;
	public int numberOfWerewolves;
	public Object villagerPrefab;
	public Object werewolfPrefab;
	public Object obstaclePrefab;
	public Object followerPrefab;

	public GameObject[] VillagerPath;
	public GameObject[] WerewolfPath;
	

	//values used by all villagers that are calculated by controller on update
	private Vector3 flockDirection;
	private Vector3 centroid;
	
	//accessors
	private static GameManager instance;
	public static GameManager Instance { get { return instance; } }

	public Vector3 FlockDirection {
		get { return flockDirection; }
	}
	
	public Vector3 Centroid { get { return centroid; } }
	public GameObject centroidContainer;
		
	//mayor and accessor
	private GameObject mayor;
	public GameObject Mayor {get{return mayor;}}

	// list of villagers with accessor
	private List<Villager> mfollowers = new List<Villager>();
	public List<Villager> Followers { get { return mfollowers; } }
	
	//list of werewolves with accessor 
	private List<GameObject> werewolves = new List<GameObject>();
	public List<GameObject> Werewolves { get { return werewolves; } }
	
	// list of villagers with accessor
	private List<GameObject> villagers = new List<GameObject>();
	public List<GameObject> Villagers { get { return villagers; } }
	
	//list of villager followers
	public List<GameObject> VillageFollowers = new List<GameObject>();
	public List<GameObject> vFollowers { get { return VillageFollowers; } }
	
	//list of werewolf followers
	public List<GameObject> WerewolfFollowers = new List<GameObject>();
	public List<GameObject> wFollowers { get { return WerewolfFollowers; } }

	public GameObject Cart;

	// array of obstacles with accessor
	private  GameObject[] obstacles;
	public GameObject[] Obstacles { get { return obstacles; } }
	
	// this is a 2-dimensional array for distances between villagers
	// it is recalculated each frame on update
	private float[,] distances;


	// GameObjects to hold our GUITexts
	// These are set in the UNITY INSPECTOR
	public GameObject DeadVillagersText;
	public GameObject SavedVillagersText;

	private int villagerCap = 10;

		
	//Set stage for game, creating characters and the simple GUI implemented.
	public void Start ()
	{
		instance = this;
		//construct our 2d array based on the value set in the editor
		distances = new float[numberOfvillagers, numberOfvillagers];
		
		// Get all obstacles
		obstacles = GameObject.FindGameObjectsWithTag ("Obstacle");

		// Get the mayor
		mayor = GameObject.FindGameObjectWithTag ("Mayor");
		Cart = GameObject.FindGameObjectWithTag("Cart");
		
		GenerateVillagers ();
		GenerateWerewolves ();
		SetUpPathsForNPCs();
	}

	private void SetUpPathsForNPCs()
	{
		//pull in path nodes as an array for villagers
		GameObject[] tempArray = GameObject.FindGameObjectsWithTag("VillagerPath");

		//instanstiate the pathArray with a length = to tempArray
		VillagerPath = new GameObject[tempArray.Length];

		//loop and plop the buggers in the right place
		for(int i = 0; i <tempArray.Length; i++)
		{
			// Get index and stuff by naming convention
			// Protip: Don't change the path node name
			string[] splitName = tempArray[i].name.Split('-');
			int index;

			// Sidenote: While out is useful in some cases, it's also garbage when you don't know the default fail-case return
			int.TryParse(splitName[1].ToString(), out index);//only way to get the damned int properly apparently...thanks Unity? -- Clay <3
			VillagerPath[index] = tempArray[i];
		}

		//pull in path nodes as an array for villagers
		GameObject[] tempArray2 = GameObject.FindGameObjectsWithTag("WerewolfPath");
		
		//instanstiate the pathArray with a length = to tempArray
		WerewolfPath = new GameObject[tempArray2.Length];
		
		//loop and plop the buggers in the right place
		for(int i = 0; i <tempArray2.Length; i++)
		{
			// Get index and stuff by naming convention
			// Protip: Don't change the path node name
			string[] splitName = tempArray2[i].name.Split('-');
			int index;
			
			// Sidenote: While out is useful in some cases, it's also garbage when you don't know the default fail-case return
			int.TryParse(splitName[1].ToString(), out index);//only way to get the damned int properly apparently...thanks Unity? -- Clay <3
			WerewolfPath[index] = tempArray2[i];
		}
	}

	// Creates a new villager, duh
	public void CreateNewVillager()
	{
		Villager villager;
		Follow follower;

		GameObject newVillager = (GameObject)Instantiate (villagerPrefab, 
		                                                  new Vector3 (371 + UnityEngine.Random.Range (0, 10), 5, 365), Quaternion.identity);
		villagers.Add (newVillager);
		//grab a component reference
		villager = newVillager.GetComponent<Villager> ();
		
		villager.GetComponent<MeshRenderer> ().material.SetColor ("_Color", Color.green);
		
		//set values in the Vehicle script
		villager.Index = villagers.Count - 1;
		
		VillageFollowers.Add ((GameObject)Instantiate (followerPrefab, 
		                                               new Vector3 (371 + UnityEngine.Random.Range (0, 10), 5, 365), Quaternion.identity));
		
		//Create a follower for the minimap
		follower = VillageFollowers [VillageFollowers.Count - 1].GetComponent<Follow> ();
		follower.ToFollow = villagers [villagers.Count - 1];
		VillageFollowers [VillageFollowers.Count - 1].GetComponent<MeshRenderer> ().material.SetColor ("_Color", Color.green);
		villager.Follower = follower;
	}

	private void GenerateVillagers()
	{
		Villager villager;
		Follow follower;

		for (int i = 0; i < numberOfvillagers; i++) {
			//Instantiate a flocker prefab, catch the reference, cast it to a GameObject
			//and add it to our list all in one line.
			villagers.Add ((GameObject)Instantiate (villagerPrefab, 
			                                        new Vector3 (600 + 5 * i, 5, 400), Quaternion.identity));
			//grab a component reference
			villager = villagers [i].GetComponent<Villager> ();
			villagers[i].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
			//set values in the Vehicle script
			villager.Index = i;
			
			VillageFollowers.Add((GameObject)Instantiate(followerPrefab, 
			                                             new Vector3(600 + 5 * i, 150,400), Quaternion.identity));
			
			//Create a follower for the minimap
			follower = VillageFollowers[i].GetComponent<Follow> ();
			follower.ToFollow = villagers[i];
			VillageFollowers[i].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
			villager.Follower = follower;
			
		}
	}

	private void GenerateWerewolves()
	{
		Werewolf werewolf;
		Follow follower;

		for (int i=0; i < numberOfWerewolves; i++)
		{
			if(i == 0)
			{
				werewolves.Add( (GameObject)Instantiate(werewolfPrefab, 
				                                        new Vector3(491, 35, 931), Quaternion.identity));
			}
			else if(i == 1)
			{
				werewolves.Add( (GameObject)Instantiate(werewolfPrefab, 
				                                        new Vector3(930, 35, 441), Quaternion.identity));
			}
			else if(i == 2)
			{
				werewolves.Add( (GameObject)Instantiate(werewolfPrefab, 
				                                        new Vector3(385, 35, 50), Quaternion.identity));
			}
			else if(i == 3)
			{
				werewolves.Add( (GameObject)Instantiate(werewolfPrefab, 
				                                        new Vector3(89, 35, 489), Quaternion.identity));	
			}
			else
			{
				werewolves.Add( (GameObject)Instantiate(werewolfPrefab, 
				                                        new Vector3(700 + 5 * i, 5, 600), Quaternion.identity));
			}
			
			//grab a component reference
			werewolf = werewolves [i].GetComponent<Werewolf>();
			werewolves[i].GetComponent<MeshRenderer>().material.SetColor("_Color",Color.red);
			//set value in the Vehicle script
			werewolf.Index = i;
			
			WerewolfFollowers.Add((GameObject)Instantiate(followerPrefab, 
			                                              new Vector3(600 + 5 * i, 150,400), Quaternion.identity));
			follower = WerewolfFollowers[i].GetComponent<Follow> ();
			follower.ToFollow = werewolves[i];
			WerewolfFollowers[i].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
		}
	}

	public void SaveVillager(Villager villager)
	{
		Villagers.Remove(villager.gameObject);
		vFollowers.Remove(villager.Follower.gameObject);
		Followers.Remove(villager);
		Destroy(villager.Follower.gameObject);
		Destroy(villager.Follower);
		Destroy(villager.gameObject);
		
		//CreateNewVillager();
		savedVillagers++;
		SavedVillagersText.guiText.text = "Villagers Saved: " + savedVillagers;
	}


	public void KillVillager(Villager villager)
	{
		//Debug.Log (villager);
		// Remove the Villager from everything and stuff
		Villagers.Remove(villager.gameObject);
		vFollowers.Remove(villager.Follower.gameObject);
		Followers.Remove(villager);
		Destroy(villager.Follower.gameObject);
		Destroy(villager.Follower);
		Destroy (villager.gameObject);

		// create new villager
		//CreateNewVillager();

		// increment
		deadVillagers++;

		// Update text
		DeadVillagersText.guiText.text = "Villagers Killed: " + deadVillagers;
	}
	
	public void Update( )
	{
		numberOfvillagers = villagers.Count;
		//calcCentroid( );//find average position of each flocker 
		calcFlockDirection( );//find average "forward" for each flocker
		if(numberOfvillagers < villagerCap)
		{
			CreateNewVillager();
		}
		//calcDistances( );
	}
	
	private void calcFlockDirection ()
	{
		
		flockDirection = new Vector3();
		
		// calculate the average heading of the flock
		// use transform.
		for(int i = 0; i < numberOfvillagers; i++)
		{	
			flockDirection = flockDirection + villagers[i].transform.forward; 
		}
		
	}

	void calcDistances( )
	{
		float dist;
		for(int i = 0 ; i < numberOfvillagers; i++)
		{
			for( int j = i+1; j < numberOfvillagers; j++)
			{
				dist = Vector3.Distance(villagers[i].transform.position, villagers[j].transform.position);
				distances[i, j] = dist;
				distances[j, i] = dist;
			}
		}
	}
	
	public float getDistance(int i, int j)
	{
		return distances[i, j];
	}
		
	private void calcCentroid ()
	{
		// calculate the current centroid of the flock
		// use transform.position
		
		centroid = new Vector3();
		
		for(int i = 0; i < villagers.Count; i++)
		{
			centroid = centroid + villagers[i].transform.position;	
		}
		
		centroid = centroid / villagers.Count;
		
		centroidContainer.transform.position = new Vector3(100,100,100);
	}
	

}