using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveWAVFile
{
    public class WAVFileControl
    {
        private const int frequency = 44100;
        // Simple Recoder
        public static IEnumerator StartRecordData(string name,int second, Action<AudioClip> complete)
        {
            var audioClip = Microphone.Start(null, false, second, frequency);
            yield return new WaitWhile(() => Microphone.IsRecording(null));
            yield return CreateWAVData(audioClip, name, null);
            if (complete != null) complete.Invoke(audioClip);
        }

        public static void StopRecordData()
        {
            Microphone.End(null);
        }

        // WAV IO
        public static IEnumerator CreateWAVData(AudioClip audioClip, string name, Action complete)
        {
            // Save local folder
            yield return null;
        }
        public static IEnumerator LoadWAVData(string name, Action<AudioClip> action)
        {
            // Load local folder
            string localpath = "\\";
            var path = "file://" + localpath + name;
            using (WWW www = new WWW(path))
            {
                yield return www;
                var audioclip = www.GetAudioClip(true, true);
                if (action != null) action.Invoke(audioclip);
            }
        }

        // Stream Recorder

    }
}