using System;
using Un4seen.Bass;

namespace Ducky.Helpers
{
    public class BassNetHelper
    {
        private static int HZ = 44100;
        private static bool InitDefaultDevice;
        public  int Stream;
        public bool isStopped = true;
        public bool dragStarted = false;
        public bool EndPlayList;
        public float[] buffer = new float[256];

        //Methods
        public void Play(string track, float vol)
        {
            if (Bass.BASS_ChannelIsActive(Stream) != BASSActive.BASS_ACTIVE_PAUSED)
            {
                Stop();
                if (InitBass(HZ))
                {
                    Stream = Bass.BASS_StreamCreateFile(track, 0, 0, BASSFlag.BASS_DEFAULT);
                    if (Stream != 0)
                    {
                        Properties.Settings.Default.UserVolume = vol;
                        Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Properties.Settings.Default.UserVolume / 100F);
                        Bass.BASS_ChannelPlay(Stream, false);
                    }
                }
            }
            else
                Bass.BASS_ChannelPlay(Stream, false);
            isStopped = false;

        }
        public void Stop()
        {
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            isStopped = true;
        }
        public void Pause()
        {
            if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING)
                Bass.BASS_ChannelPause(Stream);
            isStopped = true ;
        }
          
        //BassNetMainMethods
        private static bool InitBass(int hz)
        {
            if (!InitDefaultDevice)
                InitDefaultDevice = Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            return InitDefaultDevice;
        }
        public  void SetStreamVolume(int stream, float vol)
        {
            Properties.Settings.Default.UserVolume = vol;
            Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, Properties.Settings.Default.UserVolume / 100F);
        }
        public int GetTimeOfStream(int stream)
        {
            long TimeBytes = Bass.BASS_ChannelGetLength(stream);
            double Time = Bass.BASS_ChannelBytes2Seconds(stream, TimeBytes);
            return (int)Time;
        }
        public int GetTimeOfStream(string songpath)
        {
            if (InitBass(HZ))
            {
                int streaminit = Bass.BASS_StreamCreateFile(songpath, 0, 0, BASSFlag.BASS_DEFAULT);
                Bass.BASS_ChannelSetAttribute(streaminit, BASSAttribute.BASS_ATTRIB_VOL, Properties.Settings.Default.UserVolume / 100F);
                long TimeBytes = Bass.BASS_ChannelGetLength(streaminit);
                double Time = Bass.BASS_ChannelBytes2Seconds(streaminit, TimeBytes);
                return (int)Time;
            }return 0;
        }
        public int GetStreamPos(int steam)
        {
            long pos = Bass.BASS_ChannelGetPosition(steam);
            int posSec = (int)Bass.BASS_ChannelBytes2Seconds(steam, pos);
            return posSec;
        }
        public void SetPosOfScroll(int stream, double pos)
        {
            Bass.BASS_ChannelSetPosition(stream, (double)pos);
        }

        #region RadioButtons
        public void PlayFromURL(string url, float vol)
        {
            if (Bass.BASS_ChannelIsActive(Stream) != BASSActive.BASS_ACTIVE_PAUSED)
            {
                Stop();
                if (InitBass(HZ))
                {
                    Stream = Bass.BASS_StreamCreateURL(url, 0, BASSFlag.BASS_DEFAULT, null, IntPtr.Zero); //ERROR IF NO STATION IS SELECTED
                    if (Stream != 0)
                    {
                        Properties.Settings.Default.UserVolume = vol;
                        Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Properties.Settings.Default.UserVolume / 100F);
                        Bass.BASS_ChannelPlay(Stream, false);
                       
                    }
                }
            }
        }
        public void StopUrlStream()
        {
            Stop();
        }

        #endregion
    }
}
