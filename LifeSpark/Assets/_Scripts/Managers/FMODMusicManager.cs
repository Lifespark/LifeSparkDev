using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FMODMusicManager : MonoBehaviour {


    // DATA
    FMOD.Studio.EventInstance music;
    public bool[] layersActive = new bool[6];
    public float[] layerPresence = new float[6];   // float between [0,1] where 0 = layer not playing, and 1 = layer fully playing (for fading in and out)
    public float fadeInOutRate = 0.5f;             // 1/# of secs it takes for a layer to fully fade in or out
    public float nearbyRange = 150;                // how close enemies/allies must be to affect music


    public bool bossSpawned = false;

    public float musicVolume = 0.8f;    // Change volume of music in inspector



	// Use this for initialization
	void Start () {
        music = FMOD_StudioSystem.instance.GetEvent("{5f8876dd-acff-4ca3-8d7d-79bab4e1c08c}");
        music.start();
		GameObject me = this.gameObject;
	}
	

	// Update is called once per frame
	void Update () {

        // Set volume
        music.setVolume(musicVolume);

        // First determine which layers should/shouldn't be playing
        UpdateLayersActive();


        // Iterate through each layer and update layer presence
        for (int i = 0; i < 6; i++) {
            if (layersActive[i] && layerPresence[i] < 1.0f) {
                // if layer is active and not fully faded in yet, continue to fade in
                layerPresence[i] += Time.deltaTime * fadeInOutRate;
            }
            else if (!layersActive[i] && layerPresence[i] > 0.0f) {
                // if layer IS NOT active and not fully faded out yet, continue to fade out
                layerPresence[i] -= Time.deltaTime * fadeInOutRate;
            }
        }


        // Finally, pass each layers 'presence' to its corresponding FMOD paramater value
		music.setParameterValue("layer2Presence", layerPresence[1]);
        music.setParameterValue("layer3Presence", layerPresence[2]);
        music.setParameterValue("layer4Presence", layerPresence[3]);
        music.setParameterValue("layer5Presence", layerPresence[4]);
        music.setParameterValue("layer6Presence", layerPresence[5]);
	}



    // returns list of all nearby allies
    List<GameObject> FindNearbyAllies() {

        // First determine which player you are
        GameObject myPlayer = PlayerManager.Instance.myPlayer;
		List<GameObject> allies = new List<GameObject>();
		if (myPlayer!= null)
		{
	        int myTeam = myPlayer.GetComponent<Player>().GetTeam();


	        // Iterate through all players and add to list if allied and in range
	        List<Player> allPlayers = PlayerManager.Instance.allPlayers;
	        for (int i = 0; i < allPlayers.Count; i++) {
	            if (allPlayers[i].playerID != myPlayer.GetComponent<Player>().playerID) {
	                if (allPlayers[i].GetTeam() == myTeam && Vector3.Distance(myPlayer.transform.position, allPlayers[i].transform.position) < nearbyRange) {
	                    allies.Add(allPlayers[i].gameObject);
	                }
	            }
	        }

	        // Iterate through all CAPTURED spark points and add to list if allied and within range
//	        List<GameObject> sparkPoints = SparkPointManager.Instance.getAllSparkPoint();
//	        for (int i = 0; i < sparkPoints.Count; i++) {
//	            if (sparkPoints[i].GetComponent<SparkPoint>().sparkPointState == SparkPoint.SparkPointState.CAPTURED) {
//	                if (sparkPoints[i].GetComponent<SparkPoint>().GetOwner() == myTeam && Vector3.Distance(myPlayer.transform.position, sparkPoints[i].transform.position) < nearbyRange) {
//	                    allies.Add(sparkPoints[i]);
//	                }
//	            }
//	        }

	        // Iterate through all lane creeps and add to list if allied and within range
//	        GameObject[] laneCreeps = GameObject.FindGameObjectsWithTag("LaneCreep");
//	        for (int i = 0; i < laneCreeps.Length; i++) {
//	            if (laneCreeps[i].GetComponent<LaneCreep>().owner == myTeam && Vector3.Distance(myPlayer.transform.position, laneCreeps[i].transform.position) < nearbyRange) {
//	                allies.Add(laneCreeps[i]);
//	            }
//	        }
		}
		return allies;
    }


    // Returns list of all nearby enemies
    List<GameObject> FindNearbyEnemies() {

        // First determine which player you are
        GameObject myPlayer = PlayerManager.Instance.myPlayer;
		List<GameObject> enemies = new List<GameObject>();
        if (myPlayer != null)
		{
			int myTeam = myPlayer.GetComponent<Player>().GetTeam();

	        

	        // Iterate through all players and add to list if enemy and in range
	        List<Player> allPlayers = PlayerManager.Instance.allPlayers;
	        for (int i = 0; i < allPlayers.Count; i++) {
	            if (allPlayers[i].playerID != myPlayer.GetComponent<Player>().playerID) {
	                if (allPlayers[i].GetTeam() != myTeam && Vector3.Distance(myPlayer.transform.position, allPlayers[i].transform.position) < nearbyRange) {
	                    enemies.Add(allPlayers[i].gameObject);
	                }
	            }
	        }

	        // Iterate through all CAPTURED spark points and add to list if enemy and within range
//	        List<GameObject> sparkPoints = SparkPointManager.Instance.getAllSparkPoint();
//	        for (int i = 0; i < sparkPoints.Count; i++) {
//	            if (sparkPoints[i].GetComponent<SparkPoint>().sparkPointState == SparkPoint.SparkPointState.CAPTURED) {
//	                if (sparkPoints[i].GetComponent<SparkPoint>().GetOwner() != myTeam && Vector3.Distance(myPlayer.transform.position, sparkPoints[i].transform.position) < nearbyRange) {
//	                    enemies.Add(sparkPoints[i]);
//	                }
//	            }
//	        }

	        // Iterate through all lane creeps and add to list if enemy and within range
//	        GameObject[] laneCreeps = GameObject.FindGameObjectsWithTag("LaneCreep");
//	        for (int i = 0; i < laneCreeps.Length; i++) {
//	            if (laneCreeps[i].GetComponent<LaneCreep>().owner != myTeam && Vector3.Distance(myPlayer.transform.position, laneCreeps[i].transform.position) < nearbyRange) {
//	                enemies.Add(laneCreeps[i]);
//	            }
//	        }

		}
		return enemies;
    }

    


    // Updates which layers are/arent active
    void UpdateLayersActive() {

        // first find nearby allies and enemies
        List<GameObject> nearbyAllies = FindNearbyAllies();
        List<GameObject> nearbyEnemies = FindNearbyEnemies();




        // First layer is active all the time
        layersActive[0] = true;

        // Second layer is active iff there is at least ONE ally nearby
        layersActive[1] = (nearbyAllies.Count >= 1);

        // Third layer is active iff there is at least ONE enemy nearby
        layersActive[2] = (nearbyEnemies.Count >= 1);

        // Fourth layer is active iff there is at least TWO enemies nearby
        layersActive[3] = (nearbyEnemies.Count >= 2);

        // Fifth layer is active iff the boss is alive
        if (GameObject.FindGameObjectWithTag("Boss")) {
            layersActive[4] = true;
            bossSpawned = true;
        } else {
            layersActive[4] = false;
        }

        // Sixth layer is active iff boss has been killed
        if (bossSpawned && !GameObject.FindGameObjectWithTag("Boss")) {
            layersActive[5] = true;
        }
    }
    
}
