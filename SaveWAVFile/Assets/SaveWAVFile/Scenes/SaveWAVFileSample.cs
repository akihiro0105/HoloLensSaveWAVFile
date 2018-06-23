using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using SaveWAVFile;

public class SaveWAVFileSample : MonoBehaviour {

    private bool audioStopFlag = true;
    // Use this for initialization
    void Start () {
        StartCoroutine(AudioCoroutine(null));
    }
	
	// Update is called once per frame
	void Update () {

	}

    private IEnumerator AudioCoroutine(Action close)
    {
        int frequency = 44100;
        float[] databuffer = new float[frequency];
        float[] processbuffer = new float[256];
        int head = 0;

        AudioClip audioClip = Microphone.Start(null, true, 1, frequency);
        if (!(Microphone.GetPosition(null) > 0)) { }
        while (audioStopFlag)
        {
            var tail = Microphone.GetPosition(null);
            if (tail > 0&&head!= tail)
            {
                audioClip.GetData(databuffer, 0);
                while (true)
                {
                    var datalength = DataLength(head, tail, databuffer.Length);
                    if (datalength > processbuffer.Length)
                    {
                        var offset = databuffer.Length - head;
                        if (offset<processbuffer.Length)
                        {
                            Array.Copy(databuffer, head, processbuffer, 0, offset);
                            Array.Copy(databuffer, 0, processbuffer, offset, processbuffer.Length - offset);
                        }
                        else
                        {
                            Array.Copy(databuffer, head, processbuffer, 0, processbuffer.Length);
                        }

                        // processbuffer

                        head += processbuffer.Length;
                        if (head>databuffer.Length)
                        {
                            head -= databuffer.Length;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            yield return null;
        }
        Microphone.End(null);
        if (close != null) close.Invoke();
    }

    private int DataLength(int head,int tail,int length)
    {
        if (head<tail)
        {
            return tail - head;
        }
        else
        {
            return length - (tail - head);
        }
    }

    void OnDestroy()
    {
        audioStopFlag = false;
    }
}
