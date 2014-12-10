using UnityEngine;
using System.Collections;

public class VictoryUI : MonoBehaviour {

	public UILabel victoryLabel;
	private NetworkManager m_networkManager;
	public UIButton menuButton;
	//private SimpleGUI m_sGui;

	// Use this for initialization
	void Start () {
		GameObject manager = GameObject.Find("Manager");
		Object.DontDestroyOnLoad(manager);
		m_networkManager = (NetworkManager)manager.GetComponent("NetworkManager");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnBackToMainMenu()
	{
		//.SendMessage("StartQuickTest");
		Debug.Log ("Going back to Main Menu.");

		// Destroy any photonView game objects.
		PhotonView[] views = 
			GameObject.FindObjectsOfType<PhotonView>();

		//m_networkManager.LeaveRoom();
		//GameObject.Find("Manager").GetComponent<UIManager>().OnEndGame(true);
		// Whichever client exits first cannot 
		// destroy the objects owned by
		// another client.
		foreach (PhotonView view in views)
		{
			GameObject obj = view.gameObject;


			if (view.isMine)
			{
				// Clients can destroy their 
				// own instantiated objects
				//PhotonNetwork.Destroy (obj);
				GameObject.Destroy (obj);
				//PhotonNetwork.Destroy(view);
				GameObject.Destroy(view);
			}
			else
			{
				// Clients cannot destroy objects 
				// instantiated by other clients.
				// This avoids the error by 
				// removing the instantiated 
				// object before the network view 
				// is destroyed
				if (PhotonNetwork.
				    networkingPeer.
				    instantiatedObjects.
				    ContainsKey(view.instantiationId))
				{
					PhotonNetwork.
						networkingPeer.
							instantiatedObjects.
							Remove (view.instantiationId);
				}
			}
			GameObject.Destroy (obj);
		}
		
		// By pausing and starting the 
		// messages manually I can start
		// any client or server in any 
		// order and all spawn messages
		// are retained.
		PhotonNetwork.SetSendingEnabled(0, false);
		PhotonNetwork.isMessageQueueRunning = false;
		
		this.gameObject.SetActive(false);

		GameObject.Find ("SkillUIForm").gameObject.SetActive (false);
		GameObject.Find ("VirtualControlForm").gameObject.SetActive (false);
		GameObject.Find ("MiniMapForm").gameObject.SetActive (false);
		GameObject.Find ("ChatWindow").gameObject.SetActive (false);
		GameObject.Find ("Player").gameObject.SetActive (false);

		// Leave photon network.
		PhotonNetwork.LeaveRoom();
		PhotonNetwork.Disconnect();

		// Remove those duplicate files including itself.
		GameObject.Destroy(GameObject.Find("SkillLibrary").gameObject);
		GameObject.Destroy(GameObject.Find("ServerManager").gameObject);
		GameObject.Destroy(GameObject.Find("UI Root").gameObject);
		
		Application.LoadLevel ("Startup");
		
	}
}
