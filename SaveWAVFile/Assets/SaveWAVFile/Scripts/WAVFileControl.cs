using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
#if UNITY_UWP
using System.Threading.Tasks;
#elif UNITY_EDITOR || UNITY_STANDALONE
#endif

namespace SaveWAVFile
{
    public class WAVFileControl
    {
        // Simple Recoder
        public static IEnumerator StartRecordData(int second, Action<float> progress, Action<AudioClip> complete)
        {
            float current = Time.time;
            var audioClip = Microphone.Start(null, false, second, 44100);
            while (Microphone.IsRecording(null)==true)
            {
                if (progress != null) progress.Invoke((Time.time - current) / second);
                yield return new WaitForSeconds(0.1f);
            }
            if (complete != null) complete.Invoke(audioClip);
        }

        public static void StopRecordData()
        {
            Microphone.End(null);
        }

        // WAV IO
        public static IEnumerator CreateWAVData(AudioClip audioClip, string name, Action complete)
        {
            int frequency = audioClip.frequency;
            short channels = (short)audioClip.channels;
            float audiolength = audioClip.length;
            float[] bufdata = new float[frequency * channels * (int)audiolength];
            audioClip.GetData(bufdata, 0);
            byte[] alldata = new byte[frequency * channels * 2 * (int)audiolength + 44];
#if UNITY_UWP
            Task task = Task.Run(() =>
            {
#elif UNITY_EDITOR || UNITY_STANDALONE
            Thread thread = new Thread(() =>
            {
#endif
                // header
                byte[] riff = Encoding.UTF8.GetBytes("RIFF");
                Array.Copy(riff, 0, alldata, 0, 4);
                byte[] alllength = BitConverter.GetBytes((int)alldata.Length - 8);
                Array.Copy(alllength, 0, alldata, 4, 4);
                byte[] wave = Encoding.UTF8.GetBytes("WAVE");
                Array.Copy(wave, 0, alldata, 8, 4);
                byte[] fmt = Encoding.UTF8.GetBytes("fmt ");
                Array.Copy(fmt, 0, alldata, 12, 4);
                byte[] format = BitConverter.GetBytes((int)16);
                Array.Copy(format, 0, alldata, 16, 4);
                byte[] code = BitConverter.GetBytes((short)1);
                Array.Copy(code, 0, alldata, 20, 2);
                byte[] channel = BitConverter.GetBytes(channels);
                Array.Copy(channel, 0, alldata, 22, 2);
                byte[] sample = BitConverter.GetBytes(frequency);
                Array.Copy(sample, 0, alldata, 24, 4);
                byte[] second = BitConverter.GetBytes(frequency * channels * (int)audiolength * 2);
                Array.Copy(second, 0, alldata, 28, 4);
                byte[] block = BitConverter.GetBytes((short)(16 * channels / 8));
                Array.Copy(block, 0, alldata, 32, 2);
                byte[] bit = BitConverter.GetBytes((short)16);
                Array.Copy(bit, 0, alldata, 34, 2);
                byte[] data = Encoding.UTF8.GetBytes("data");
                Array.Copy(data, 0, alldata, 36, 4);
                byte[] alllength2 = BitConverter.GetBytes((int)alldata.Length - 44);
                Array.Copy(alllength2, 0, alldata, 40, 4);
                //
                byte[] vdata = new byte[frequency * channels * 2 * (int)audiolength];
                for (int i = 0; i < bufdata.Length; i++)
                {
                    var buf = (short)(bufdata[i] * 32767);
                    var bytearr = new byte[2];
                    bytearr = BitConverter.GetBytes(buf);
                    bytearr.CopyTo(vdata, i * 2);
                }
                Array.Copy(vdata, 0, alldata, 44, vdata.Length);
                //
#if UNITY_UWP
            });
            yield return new WaitWhile(() => task.IsCompleted == false);
#elif UNITY_EDITOR || UNITY_STANDALONE
            });
            thread.Start();
            yield return new WaitWhile(() => thread.IsAlive == true);
#endif
            // Save local folder
            yield return FileIOControl.WriteBytesFile(FileIOControl.LocalFolderPath + "\\" + name, alldata);
            if (complete != null) complete.Invoke();
        }

        public static IEnumerator LoadWAVData(string name, Action<AudioClip> action)
        {
            // Load local folder
            var path = "file://" + FileIOControl.LocalFolderPath + "\\" + name;
            using (WWW www = new WWW(path))
            {
                yield return www;
                var audioclip = www.GetAudioClip(true, true);
                if (action != null) action.Invoke(audioclip);
            }
        }

        // Stream Recorder
        private static bool audioStopFlag;
        public static IEnumerator StartStreamRecordData(Action<float[]> action,Action close)
        {
            int frequency = 44100;
            float[] databuffer = new float[frequency];
            float[] processbuffer = new float[256];
            int head = 0;

            AudioClip audioClip = Microphone.Start(null, true, 1, frequency);
            if (!(Microphone.GetPosition(null) > 0)) { }
            audioStopFlag = true;
            while (audioStopFlag)
            {
                var tail = Microphone.GetPosition(null);
                if (tail > 0 && head != tail)
                {
                    audioClip.GetData(databuffer, 0);
                    while (true)
                    {
                        var datalength = DataLength(head, tail, databuffer.Length);
                        if (datalength > processbuffer.Length)
                        {
                            var offset = databuffer.Length - head;
                            if (offset < processbuffer.Length)
                            {
                                Array.Copy(databuffer, head, processbuffer, 0, offset);
                                Array.Copy(databuffer, 0, processbuffer, offset, processbuffer.Length - offset);
                            }
                            else
                            {
                                Array.Copy(databuffer, head, processbuffer, 0, processbuffer.Length);
                            }

                            // processbuffer
                            if (action != null) action.Invoke(processbuffer);

                             head += processbuffer.Length;
                            if (head > databuffer.Length)
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

        public static void StopStreamRecordData()
        {
            audioStopFlag = false;
        }

        private static int DataLength(int head, int tail, int length)
        {
            return (head < tail) ? tail - head : length - (tail - head);
        }
    }
}