using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveWAVFile;

/// <summary>
/// 音声の録音，保存，再生を行うサンプル
/// </summary>
public class SaveWAVFileSample : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    // Use this for initialization
    void Start () {
        
        StartCoroutine(RecordTest());
    }

    private IEnumerator RecordTest()
    {
        AudioClip audioClip = null;
        Debug.Log("Start record");
        // 5秒間録音
        yield return WAVFileControl.StartRecordData(5, (progress) => Debug.Log(progress), (clip) => audioClip = clip);
        // 録音されたデータを保存
        yield return WAVFileControl.CreateWAVData(audioClip, "voice.wav", () => Debug.Log("Create WAV"));
        // 保存されたデータを再生
        yield return WAVFileControl.LoadWAVData("voice.wav", (clip) => audioSource.PlayOneShot(clip));
        Debug.Log("Loaded file");
    }
	
	// Update is called once per frame
	void Update () {

	}
}
