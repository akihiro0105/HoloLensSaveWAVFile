using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using SaveWAVFile;

public class SaveWAVFileSample : MonoBehaviour {

    // Use this for initialization
    void Start () {
        
        StartCoroutine(RecordTest());
    }

    private IEnumerator RecordTest()
    {
        AudioClip audioClip = null;
        AudioSource audioSource = GetComponent<AudioSource>();
        Debug.Log("Start record");
        yield return WAVFileControl.StartRecordData(5, (progress) => { Debug.Log(progress); }, (clip) => { audioClip = clip; });
        yield return WAVFileControl.CreateWAVData(audioClip, "voice.wav", ()=> { Debug.Log("Create WAV"); });
        yield return WAVFileControl.LoadWAVData("voice.wav", (clip) => {
            audioSource.clip = clip;
            audioSource.Play();
        });
        Debug.Log("Loaded file");
    }
	
	// Update is called once per frame
	void Update () {

	}
}
