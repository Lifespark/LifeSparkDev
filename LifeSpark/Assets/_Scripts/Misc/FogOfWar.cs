using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FogOfWar : MonoBehaviour {

    public int m_visionRadius = 21;
    public float m_initialAlpha = 0.9f;
    public float m_updateInterval = 2.0f;

    private Material m_mat;
    private Texture2D m_fogTex;
    private Color[] m_brush;

    private GameObject[] m_players;
    private List<GameObject> m_teammates = new List<GameObject>();
    private bool m_fogOn = false;

	// Use this for initialization
	void Awake () {
       

	}

    public void SetupFog() {
        m_fogTex = new Texture2D(120, 120, TextureFormat.Alpha8, false);
        m_mat = renderer.material;

        Color[] colors = new Color[120 * 120];

        for (int i = 0; i < 120 * 120; i++) {
            colors[i] = new Color(0, 0, 0, m_initialAlpha);
        }

        m_fogTex.SetPixels(colors);
        m_fogTex.Apply();
        renderer.material.mainTexture = m_fogTex;

        m_brush = new Color[m_visionRadius * m_visionRadius];

        for (int i = 0; i < m_visionRadius; i++) {
            for (int j = 0; j < m_visionRadius; j++) {
                m_brush[i * m_visionRadius + j] = new Color(0, 0, 0, (Mathf.Sqrt((Mathf.Abs(i - m_visionRadius / 2) * Mathf.Abs(i - m_visionRadius / 2) + Mathf.Abs(j - m_visionRadius / 2) * Mathf.Abs(j - m_visionRadius / 2)))) / (m_visionRadius / 2.0f) * m_initialAlpha * 2 - 1);
            }
        }

        m_players = GameObject.FindGameObjectsWithTag("Player");


        for (int i = 0; i < m_players.Length; i++) {
            if (m_players[i].GetComponent<Player>().team == PlayerManager.Instance.myPlayer.GetComponent<Player>().team) {
                m_teammates.Add(m_players[i]);
            }
        }
    }

	// Update is called once per frame
	void Update () {

        if (!m_fogOn) {
            if (PlayerManager.Instance.myPlayer != null) {
                SetupFog();
                m_fogOn = true;
                InvokeRepeating("UpdateFog", 0, m_updateInterval);
            }
        }
	}

    void UpdateFog() {
        for (int i = 0; i < m_teammates.Count; i++) {
            Vector2 relativePlayerPos = new Vector2(m_teammates[i].transform.position.x + 60, m_teammates[i].transform.position.z + 60);
            //fogTex.SetPixels((int)relativePlayerPos.x - 5, (int)relativePlayerPos.y - 5, 10, 10, brush);
            for (int iRow = 0; iRow < m_visionRadius; iRow++) {
                for (int iCol = 0; iCol < m_visionRadius; iCol++) {
                    if (m_fogTex.GetPixel((int)relativePlayerPos.x - m_visionRadius / 2 + iRow, (int)relativePlayerPos.y - m_visionRadius / 2 + iCol).a > m_brush[iRow * m_visionRadius + iCol].a) {
                        m_fogTex.SetPixel((int)relativePlayerPos.x - m_visionRadius / 2 + iRow, (int)relativePlayerPos.y - m_visionRadius / 2 + iCol, m_brush[iRow * m_visionRadius + iCol]);
                    }
                }
            }
        }

        // whoa what a shitty for loop
        for (int i = 0; i < CreepManager.Instance.creepDict.Count; i++) {
            if (CreepManager.Instance.creepDict.ElementAt(i).Key.GetComponent<SparkPoint>().owner == PlayerManager.Instance.myPlayer.GetComponent<Player>().team) {
                for (int j = 0; j < CreepManager.Instance.creepDict.ElementAt(i).Value.Count; j++) {
                    Vector2 relativePlayerPos = new Vector2(CreepManager.Instance.creepDict.ElementAt(i).Value[j].transform.position.x + 60, CreepManager.Instance.creepDict.ElementAt(i).Value[j].transform.position.z + 60);
                    for (int iRow = 0; iRow < m_visionRadius; iRow++) {
                        for (int iCol = 0; iCol < m_visionRadius; iCol++) {
                            if (m_fogTex.GetPixel((int)relativePlayerPos.x - m_visionRadius / 2 + iRow, (int)relativePlayerPos.y - m_visionRadius / 2 + iCol).a > m_brush[iRow * m_visionRadius + iCol].a) {
                                m_fogTex.SetPixel((int)relativePlayerPos.x - m_visionRadius / 2 + iRow, (int)relativePlayerPos.y - m_visionRadius / 2 + iCol, m_brush[iRow * m_visionRadius + iCol]);
                            }
                        }
                    }
                }
            }
        }

        m_fogTex.Apply();
    }

}
