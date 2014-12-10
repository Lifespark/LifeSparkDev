using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FogOfWarShader : MonoBehaviour
{
    public Camera xzCamera;
    public Texture2D controllTex;

    //private RenderTexture visionTex;
    public RenderTexture worldXZTex;
    public RenderTexture visionFogTex;
    public RenderTexture formerFogTex;
    private RenderTexture visionFogTexTmp;
    private RenderTexture formerFogTexTmp;
    private RenderTexture mergeResult;
    private Material visionMat = null;
    private Material visionFogMat = null;
    private Material mergeMat = null;
    public Shader visionFogShader;
    public Shader visionShader;
    public Shader blackShader;
    public Shader worldXZShader;
    public Shader mergeShader;
    
    private Texture2D inputTexture;

	// Use this for initialization
	void Start () {

        visionMat = new Material(visionShader);
        visionFogMat = new Material(visionFogShader);
        mergeMat = new Material(mergeShader);

        inputTexture = new Texture2D(128, 128);
        worldXZTex = new RenderTexture(Screen.width, Screen.height, 32);

        visionFogTexTmp = new RenderTexture(128, 128, 0);
        formerFogTexTmp = new RenderTexture(128, 128, 0);
        mergeResult = new RenderTexture(128, 128, 0);

        Material blackMat = new Material(blackShader);
        Graphics.Blit(inputTexture, formerFogTex, blackMat);
        Graphics.Blit(inputTexture, formerFogTexTmp, blackMat);
        Graphics.Blit(inputTexture, visionFogTexTmp, blackMat);

	}

    // Update is called once per frame
    void Update()
    {
        Vector3 pos;
        float pointX;
        float pointY;
        float dis;
        int teamID;
        if (PlayerManager.Instance.myPlayer != null)
        {
            //Debug.Log("find my player!");
            teamID = PlayerManager.Instance.myPlayer.GetComponent<Player>().team;

            List<Player> friendlyPlayers = PlayerManager.Instance.GetFriendlyPlayers(teamID);
            //Debug.LogWarning(friendlyPlayers.Count);

            int index = 0;
            for (int i = 0; i < friendlyPlayers.Count; i++)
            {
                if (index == 0)
                {
                    pos = friendlyPlayers[i].gameObject.transform.position;
                    pointX = (float)(pos.x + 368.6581) / 750;
                    pointY = (float)(pos.z + 370.654) / 750;
                    dis = 100.0f * 2 / 750.0f;

                    visionFogMat.SetFloat("_PointX", pointX);
                    visionFogMat.SetFloat("_PointY", pointY);
                    visionFogMat.SetFloat("_Range", dis * dis);
                    visionFogMat.SetTexture("_ControlTex", controllTex);
                    visionFogMat.SetTexture("_Former", formerFogTex);

                    Graphics.Blit(inputTexture, visionFogTex, visionFogMat);

                    Graphics.Blit(visionFogTex, formerFogTex);

                    index++;
                }
                else
                {
                    pos = friendlyPlayers[i].gameObject.transform.position;
                    pointX = (float)(pos.x + 368.6581) / 750;
                    pointY = (float)(pos.z + 370.654) / 750;
                    dis = 100.0f * 2 / 750.0f;

                    visionFogMat.SetFloat("_PointX", pointX);
                    visionFogMat.SetFloat("_PointY", pointY);
                    visionFogMat.SetFloat("_Range", dis * dis);
                    visionFogMat.SetTexture("_ControlTex", controllTex);
                    visionFogMat.SetTexture("_Former", formerFogTexTmp);

                    Graphics.Blit(inputTexture, visionFogTexTmp, visionFogMat);

                    Graphics.Blit(visionFogTexTmp, formerFogTexTmp);
                }
            }
        }


        mergeMat.SetTexture("_Second", visionFogTexTmp);
        Graphics.Blit(visionFogTex, mergeResult, mergeMat);

    }

    void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
    {

        xzCamera.CopyFrom(Camera.main);
        xzCamera.enabled = false;
        xzCamera.targetTexture = worldXZTex;
        xzCamera.SetReplacementShader(worldXZShader, "RenderType");
        xzCamera.Render();
        

        visionMat.SetTexture("_worldUVTex", worldXZTex);
        visionMat.SetTexture("_ControlTex", mergeResult);
        Graphics.Blit(sourceTexture, destTexture, visionMat);
    }
}
