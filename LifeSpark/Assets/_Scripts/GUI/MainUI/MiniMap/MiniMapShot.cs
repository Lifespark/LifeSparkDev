using UnityEngine;
using System.Collections;
using System.IO;

public class MiniMapShot : MonoBehaviour {

    RenderTexture miniMap;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp("z"))
        {
            Debug.LogWarning("aaa");
            miniMap = gameObject.GetComponent<Camera>().targetTexture;

            string path = System.IO.Directory.GetCurrentDirectory() + "\\Assets\\_Atlas\\MiniMap\\MiniMap.png";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            Texture2D myTexture2D = new Texture2D(miniMap.width+24, miniMap.height+24, TextureFormat.RGB24, false);
            RenderTexture.active = miniMap;
            myTexture2D.ReadPixels(new Rect(0, 0, miniMap.width, miniMap.height), 12, 12);
            myTexture2D.Apply();
            RenderTexture.active = null;

            File.WriteAllBytes(path, myTexture2D.EncodeToPNG());
        }
	}
}
