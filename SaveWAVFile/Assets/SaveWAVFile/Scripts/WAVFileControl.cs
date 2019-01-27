using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using HoloLensModule.Environment;
using UnityEngine;
#if UNITY_UWP
using System.Threading.Tasks;
#elif UNITY_EDITOR || UNITY_STANDALONE
#endif

namespace SaveWAVFile
{
    /// <summary>
    /// 音声データの録音，保存，再生，ストリーミングを行う
    /// </summary>
    public class WAVFileControl
    {
        /// <summary>
        /// 指定時間録音を行う
        /// </summary>
        /// <param name="second"></param>
        /// <param name="progress"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public static IEnumerator StartRecordData(int second, Action<float> progress, Action<AudioClip> complete,
            int frequency = 44100)
        {
            var current = Time.time;
            var audioClip = Microphone.Start(null, false, second, frequency);
            while (Microphone.IsRecording(null) == true)
            {
                if (progress != null) progress.Invoke((Time.time - current) / second);
                yield return new WaitForSeconds(0.1f);
            }

            if (complete != null) complete.Invoke(audioClip);
        }

        /// <summary>
        /// 録音の停止
        /// </summary>
        public static void StopRecordData()
        {
            Microphone.End(null);
        }

        /// <summary>
        /// WAVファイルでAudioClipデータを保存する
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="name"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public static IEnumerator CreateWAVData(AudioClip audioClip, string name, Action complete)
        {
            // 周波数
            var frequency = audioClip.frequency;
            // チャンネル
            var channels = (short)audioClip.channels;
            // 再生時間
            var audiolength = audioClip.length;
            // 音声データ
            var bufdata = new float[frequency * channels * (int)audiolength];
            audioClip.GetData(bufdata, 0);
            // WAV保存データ
            var alldata = new byte[frequency * channels * 2 * (int)audiolength + 44];
            // 保存処理は非同期で行う
#if UNITY_UWP
            Task task = Task.Run(() =>
            {
#elif UNITY_EDITOR || UNITY_STANDALONE
            Thread thread = new Thread(() =>
            {
#endif
                // WAV header
                var riff = Encoding.UTF8.GetBytes("RIFF");
                Array.Copy(riff, 0, alldata, 0, 4);
                var alllength = BitConverter.GetBytes((int)alldata.Length - 8);
                Array.Copy(alllength, 0, alldata, 4, 4);
                var wave = Encoding.UTF8.GetBytes("WAVE");
                Array.Copy(wave, 0, alldata, 8, 4);
                var fmt = Encoding.UTF8.GetBytes("fmt ");
                Array.Copy(fmt, 0, alldata, 12, 4);
                var format = BitConverter.GetBytes((int)16);
                Array.Copy(format, 0, alldata, 16, 4);
                var code = BitConverter.GetBytes((short)1);
                Array.Copy(code, 0, alldata, 20, 2);
                var channel = BitConverter.GetBytes(channels);
                Array.Copy(channel, 0, alldata, 22, 2);
                var sample = BitConverter.GetBytes(frequency);
                Array.Copy(sample, 0, alldata, 24, 4);
                var second = BitConverter.GetBytes(frequency * channels * (int)audiolength * 2);
                Array.Copy(second, 0, alldata, 28, 4);
                var block = BitConverter.GetBytes((short)(16 * channels / 8));
                Array.Copy(block, 0, alldata, 32, 2);
                var bit = BitConverter.GetBytes((short)16);
                Array.Copy(bit, 0, alldata, 34, 2);
                var data = Encoding.UTF8.GetBytes("data");
                Array.Copy(data, 0, alldata, 36, 4);
                var alllength2 = BitConverter.GetBytes((int)alldata.Length - 44);
                Array.Copy(alllength2, 0, alldata, 40, 4);

                // WAV Raw
                var vdata = new byte[frequency * channels * 2 * (int)audiolength];
                for (var i = 0; i < bufdata.Length; i++)
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
            // LocalFolderに保存
            yield return FileIOControl.WriteBytesFile(FileIOControl.LocalFolderPath + "\\" + name, alldata);
            if (complete != null) complete.Invoke();
        }

        /// <summary>
        /// WAVファイルの読み込み
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerator LoadWAVData(string name, Action<AudioClip> action)
        {
            // Load local folder
            var path = "file://" + FileIOControl.LocalFolderPath + "\\" + name;
            using (var www = new WWW(path))
            {
                yield return www;
                var audioclip = www.GetAudioClip(true, true);
                if (action != null) action.Invoke(audioclip);
            }
        }
    }
}