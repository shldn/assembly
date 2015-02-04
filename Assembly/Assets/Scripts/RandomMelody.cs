using UnityEngine;
using System.Collections;

public class RandomMelody : MonoBehaviour {

    public static RandomMelody Inst = null;

    public AudioClip noteClip = null;
    public float noteFreq = 440f;

    float[] notes = {277.18f, 311.13f, 329.63f, 349.23f, 369.99f, 415.30f, 440f, 493.88f, 554.37f};

    float cooldown = 0f;


    void Awake(){
        Inst = this;
    } // End of Awake().

	// Update is called once per frame
	void Update(){

        cooldown -= Time.deltaTime;

        /*
        // Generate random music
        if(cooldown <= 0f){
            PlayNote();
            cooldown = Random.Range(1f, 5f);
        }
        */

	} // End of Update().


    public void PlayNote(){

        if(cooldown > 0f)
            return;
        cooldown = 0.5f;

    
        GameObject tempGO = new GameObject("TempAudio"); // create the temp object
        tempGO.transform.position = Vector3.zero; // set its position
        AudioSource aSource = tempGO.AddComponent<AudioSource>(); // add an audio source
        aSource.clip = noteClip; // define the clip
        // set other aSource properties here, if desired

        aSource.pitch = (notes[Random.Range(0, notes.Length)] * Random.Range(1, 3)) / noteFreq;
        aSource.volume = 0.4f;

        aSource.Play(); // start the sound
        Destroy(tempGO, noteClip.length); // destroy object after clip duration
    
    } // End of PlayNote().

} // End of RandomMelody.
