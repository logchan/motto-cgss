using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Utilities
{
    public static class AudioHelper
    {
        #region SoundTouch DllImport

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_ANDROID

        [DllImport("SoundTouch", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr soundtouch_createInstance();
        [DllImport("SoundTouch", CallingConvention = CallingConvention.Cdecl)]
        private static extern void soundtouch_destroyInstance(IntPtr h);
        [DllImport("SoundTouch", CallingConvention = CallingConvention.Cdecl)]
        private static extern void soundtouch_setRate(IntPtr h, float newRate);
        [DllImport("SoundTouch", CallingConvention = CallingConvention.Cdecl)]
        private static extern void soundtouch_setTempo(IntPtr h, float newTempo);
        [DllImport("SoundTouch", CallingConvention = CallingConvention.Cdecl)]
        private static extern void soundtouch_setRateChange(IntPtr h, float newRate);
        [DllImport("SoundTouch", CallingConvention = CallingConvention.Cdecl)]
        private static extern void soundtouch_setTempoChange(IntPtr h, float newTempo);
        [DllImport("SoundTouch", CallingConvention = CallingConvention.Cdecl)]
        private static extern void soundtouch_setPitch(IntPtr h, float newPitch);
        [DllImport("SoundTouch", CallingConvention = CallingConvention.Cdecl)]
        private static extern void soundtouch_setPitchOctaves(IntPtr h, float newPitch);
        [DllImport("SoundTouch", CallingConvention = CallingConvention.Cdecl)]
        private static extern void soundtouch_setPitchSemiTones(IntPtr h, float newPitch);
        [DllImport("SoundTouch", CallingConvention = CallingConvention.Cdecl)]
        private static extern void soundtouch_setChannels(IntPtr h, uint numChannels);
        [DllImport("SoundTouch", CallingConvention = CallingConvention.Cdecl)]
        private static extern void soundtouch_setSampleRate(IntPtr h, uint srate);
        [DllImport("SoundTouch", CallingConvention = CallingConvention.Cdecl)]
        private static extern void soundtouch_flush(IntPtr h);
        [DllImport("SoundTouch", CallingConvention = CallingConvention.Cdecl)]
        private static extern void soundtouch_putSamples(IntPtr h, float[] samples, uint numSamples);
        [DllImport("SoundTouch", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool soundtouch_setSetting(IntPtr h, int settingId, int value);
        [DllImport("SoundTouch", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint soundtouch_receiveSamples(IntPtr h, float[] outBuffer, uint maxSamples);

#endif

        #endregion

        private const int BufferSize = 67200;

        private static float[] CopyArray(float[] src, float[] dst, long dstIndex, int length)
        {
            if (dstIndex + length > dst.Length)
            {
                var ndst = new float[dst.Length*2];
                Array.Copy(dst, ndst, dst.Length);
                dst = ndst;
            }

            Array.Copy(src, 0, dst, dstIndex, length);
            return dst;
        }

        public static float[] ChangeAudioSpeed(uint channels, uint sampleRate, float factor, float[] data)
        {
            
            var h = soundtouch_createInstance();
            soundtouch_setSampleRate(h, sampleRate);
            soundtouch_setChannels(h, channels);
            soundtouch_setPitch(h, 1.0f);
            soundtouch_setRate(h, 1.0f);
            soundtouch_setTempo(h, factor);

            var numData = 0;
            var inbuf = new float[BufferSize];
            var outbuf = new float[BufferSize];
            var result = new float[data.Length];
            uint numRecv = 0;
            uint recv;

            while (numData < data.Length)
            {
                var loopNumData = Math.Min(BufferSize, data.Length - numData);
                Array.Copy(data, numData, inbuf, 0, loopNumData);
                numData += loopNumData;

                soundtouch_putSamples(h, inbuf, (uint)loopNumData / channels);
                do
                {
                    recv = soundtouch_receiveSamples(h, outbuf, BufferSize / channels);

                    recv *= channels;
                    CopyArray(outbuf, result, numRecv, (int)recv);
                    numRecv += recv;
                } while (recv != 0);
            }

            soundtouch_flush(h);
            do
            {
                recv = soundtouch_receiveSamples(h, outbuf, BufferSize / channels);

                recv *= channels;
                CopyArray(outbuf, result, numRecv, (int)recv);
                numRecv += recv;
            } while (recv != 0);

            soundtouch_destroyInstance(h);
            return result;
        }
    }
}
