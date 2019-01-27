using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveWAVFile
{
    /// <summary>
    /// 音声データStream受信用
    /// </summary>
    public class WAVStreamListener
    {
        private Queue<float> list = new Queue<float>();

        /// <summary>
        /// 音声再生用設定
        /// </summary>
        /// <param name="audioSource"></param>
        /// <param name="frequency"></param>
        public WAVStreamListener(AudioSource audioSource, int frequency = 44100)
        {
            audioSource.clip = AudioClip.Create("", frequency, 1, frequency, true, readCallback, positionCallback);
            audioSource.loop = true;
            audioSource.Play();
        }

        /// <summary>
        /// 受信Streamデータセット
        /// </summary>
        /// <param name="data"></param>
        public void SetDataList(byte[] data)
        {
            for (var i = 0; i < data.Length / 4; i++)
            {
                list.Enqueue(BitConverter.ToSingle(data, i * 4));
            }
        }

        private void readCallback(float[] data)
        {
            // 受信データのQueueをAudioClipに設定
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = (list.Count > 0) ? list.Dequeue() : 0;
            }
        }

        private void positionCallback(int newPosition) { }
    }
}
