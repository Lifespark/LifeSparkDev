using UnityEngine;
using System.Collections;

// Adds list of background music to MusicManager object across both scenes. - BR
public class MusicManager : MonoBehaviour {
	
	static private MusicManager _instance;
	static public MusicManager Instance {
		get {
			if (_instance == null)
				_instance = FindObjectOfType(typeof(MusicManager)) as MusicManager;
			return _instance;
		}
	}
	
	public AudioClip[] songs;
	
	AudioSource audio;
	
	// Used index array of song choices. Number accordingly with list in
	// Music Manager object (In all impacted scenes) - BR
	public enum SongChoice {
		Airship_Battle = 0,
		Demo_Trailer_TEMP_Take1 = 1,
		Overture_To_Lifespark_Concept1 = 2,
		SummaDatMagickuhlShit = 3,
		Telluria_Texture_Concept1 = 4,
		Texture_Swells = 5
	}
	public SongChoice startUpMusic = SongChoice.SummaDatMagickuhlShit;
	public SongChoice inGameMusic = SongChoice.Airship_Battle;
	private SongChoice backgroundMusic;
	
	private int level = 0;
	
	void Start() {
		audio = GetComponent<AudioSource>();
		backgroundMusic = startUpMusic;
	}
	
	public void setLevelMusic(int level_) {
		audio.Stop();
		level = level_;
		if(level == 0)
			backgroundMusic = startUpMusic;
		else
			backgroundMusic = inGameMusic;
	}
	
	void Update() {
		if(audio.isPlaying == false) {
			audio.clip = songs[(int)backgroundMusic];
			audio.Play();
		}
	}
}
