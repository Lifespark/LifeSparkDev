using UnityEngine;
using System.Collections;
/// <summary>
/// This is ngui's button message with index
/// </summary>
public class UIButtonIndexMsg : MonoBehaviour {
	public byte index;
	public string MessageName;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public GameObject target;

	void OnClick () 
	{ 
		if(enabled)
		{
			if(target!= null && !string.IsNullOrEmpty(MessageName))
			{
				target.SendMessage(MessageName,index);
			}
		}
	}
}
