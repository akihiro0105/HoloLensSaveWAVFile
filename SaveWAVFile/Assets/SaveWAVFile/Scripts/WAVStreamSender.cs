using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveWAVFile
{
    /// <summary>
    /// 音声データStream送信用
    /// </summary>
    public class WAVStreamSender
    {
        private bool audioStopFlag;

        /// <summary>
        /// 音声ストリーミング配信用
        /// actionのfloat[]をストリーミング配信
        /// </summary>
        /// <param name="action"></param>
        /// <param name="close"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public IEnumerator StartStreamRecordData(Action<byte[]> action, int frequency = 44100)
        {
            var databuffer = new float[frequency];
            var processbuffer = new float[256];
            var head = 0;
            var list = new List<byte>();

            // 録音デバイスの設定
            var audioClip = Microphone.Start(null, true, 1, frequency);
            while (!(Microphone.GetPosition(null) > 0)) { }
            audioStopFlag = true;
            while (audioStopFlag)
            {
                var tail = Microphone.GetPosition(null);
                if (tail > 0 && head != tail)
                {
                    audioClip.GetData(databuffer, 0);
                    while (true)
                    {
                        var length = dataLength(head, tail, databuffer.Length);
                        if (length > processbuffer.Length)
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
                            for (var i = 0; i < processbuffer.Length; i++)
                            {
                                var b = BitConverter.GetBytes(processbuffer[i]);
                                list.AddRange(b);
                            }

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
                    // stream用にbyte配列
                    if (action != null) action.Invoke(list.ToArray());
                }
                yield return null;
            }
            Microphone.End(null);
        }

        /// <summary>
        /// ストリーミング停止
        /// </summary>
        public void StopStreamRecordData()
        {
            audioStopFlag = false;
        }

        private int dataLength(int head, int tail, int length)
        {
            return (head < tail) ? tail - head : length - (tail - head);
        }
    }
}
