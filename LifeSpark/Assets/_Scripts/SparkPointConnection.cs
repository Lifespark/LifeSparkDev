using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SparkPointConnection : LSMonoBehaviour {

	LineRenderer m_connection;
	public Vector3 start;
	public Vector3 end;
	//public List<Transform> targets = new List<Transform>();
	public int waveVerts = 12;
	public float waveAmplitude = 0.35f;

	Vector3 startToEndVector;
	float wavelen;

	public bool isConnected = false;

	// Use this for initialization
	void Awake () {
		m_connection = this.GetComponent<LineRenderer>();
		m_connection.SetWidth(5,5);
        GetComponent<LineRenderer>().material.SetColor("_TintColor", (int)photonView.instantiationData[0] == 1 ? new Vector4(66 / 255.0f, 177 / 255.0f, 187 / 255.0f, 1) : new Vector4(165 / 255.0f, 49 / 255.0f, 225 / 255.0f, 1) );
	}

	[RPC]
	public void RPC_startSparkPointConnection(Vector3 self, Vector3 target) {
		isConnected = true;
		start = self;
		//end = target;
		//waveVerts = ((int)((end - start).magnitude))*6/10;
        StartCoroutine(StartConnection(self, target));
	}

    IEnumerator StartConnection(Vector3 self, Vector3 target) {
        float startTime = Time.time;

        Vector3 dir = target - self;
        float length = dir.magnitude;

        dir /= length;
        float marchDistance = 0;

        while (marchDistance < length) {
            marchDistance += Time.time * 0.5f;
            marchDistance = Mathf.Min(marchDistance, length);
            end = self + dir * marchDistance;
            waveVerts = ((int)((end - start).magnitude)) * 6 / 10;
            yield return null;
        }
        end = target;
    }

	// Update is called once per frame
	void Update () {
		//Debug.Log(connection.name);
		if(isConnected) {

			m_connection.SetVertexCount(waveVerts+2);

			m_connection.SetPosition(0,start);

			startToEndVector = (end-start);
			wavelen = startToEndVector.magnitude / (waveVerts+2);
			startToEndVector.Normalize();

			for(int i = 1; i <= waveVerts; i++) {
				m_connection.SetPosition(i,
				                         start + startToEndVector*(wavelen*i)
                                         + Vector3.up * Random.Range(-waveAmplitude, waveAmplitude) + Vector3.right*Random.Range(-waveAmplitude, waveAmplitude));
               
			}


			//r1 = p1.position + p1p2*len + Vector3.up*Random.Range(-2,2);
			//r2 = p1.position + p1p2*len*2 + Vector3.up*Random.Range(-2,2);

			//m_connection.SetPosition(1, r1);
			//m_connection.SetPosition(2, r2);

			m_connection.SetPosition(waveVerts+1,end);
		
		}
	}
}
