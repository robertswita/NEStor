using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using TGL;

namespace GitHub.secile.Audio
{
    class AudioInput
    {
        // [How to use]
        // string[] devices = AudioInput.FindDevices();
        // if (devices.Length == 0) return; // no device.

        // create AudioInput with default device.
        // var device = new AudioInput(44100, 16, 2);

        // start sampling
        // device.Start(data =>
        // {
        //     // called when each buffer becomes full
        //     Console.WriteLine(data.Length);
        // });

        // this.FormClosing += (s, ev) =>
        // {
        //     device.Stop();
        //     device.Close();
        // };

        private IntPtr Device;

        private const uint WAVE_MAPPER = 0xFFFFFFFF;

        /// <summary>data bytes required per second.</summary>
        public readonly int BytesPerSec;

        /// <summary>create AudioInput with default device.</summary>
        public AudioInput(int samplesPerSec, int bitsPerSamples, int channels) : this(samplesPerSec, bitsPerSamples, channels, WAVE_MAPPER) { }

        /// <summary>create AudioInput with specified device.</summary>
        /// <param name="deviceId">index of FindDevices result.</param>
        public AudioInput(int samplesPerSec, int bitsPerSamples, int channels, uint deviceId)
        {
            var format = new Win32.WaveFormatEx(samplesPerSec, bitsPerSamples, channels);
            BytesPerSec = samplesPerSec * channels * (bitsPerSamples / 8);

            WaveInProc = WaveInProcAndThreadStart();

            int rc = Win32.waveInOpen(out Device, deviceId, ref format, WaveInProc, IntPtr.Zero, Win32.CALLBACK_FUNCTION);
            if (rc != Win32.MMSYSERR_NOERROR)
            {
                var sb = new StringBuilder(256);
                Win32.waveInGetErrorText(rc, sb, sb.Capacity);
                throw new InvalidOperationException(sb.ToString());
            }
        }

        private Win32.WaveInProc WaveInProc;
        private Win32.WaveInProc WaveInProcAndThreadStart()
        {
            var proc_to_thread_event = new System.Threading.AutoResetEvent(false);
            var proc_to_thread_param = new Queue<IntPtr>();

            // callback
            var proc = new Win32.WaveInProc((hwo, msg, user, param1, param2) =>
            {
                // [waveInProc callback function remarks]
                // Applications should not call any system-defined functions from inside a callback function, except for
                // EnterCriticalSection, LeaveCriticalSection, midiOutLongMsg, midiOutShortMsg, OutputDebugString,
                // PostMessage, PostThreadMessage, SetEvent, timeGetSystemTime, timeGetTime, timeKillEvent, and timeSetEvent.
                // Calling other wave functions will cause deadlock.

                // Data:the device driver is finished with a data block
                if (msg == Win32.WaveInMessage.Data)
                {
                    lock (proc_to_thread_param)
                    {
                        proc_to_thread_param.Enqueue(param1);
                    }

                    proc_to_thread_event.Set();
                }
            });

            // thread
            var thread = new System.Threading.Thread(() =>
            {
                IntPtr header_ptr;

                while (true)
                {
                    proc_to_thread_event.WaitOne(); // wait until event is rased.
                    {
                        lock (proc_to_thread_param)
                        {
                            header_ptr = proc_to_thread_param.Dequeue();
                        }

                        var header = Win32.WaveHeader.FromIntPtr(header_ptr);
                        int bytesRecorded = (int)header.dwBytesRecorded;

                        // bytesRecored can be 0 if reset invoked.
                        if (bytesRecorded > 0)
                        {
                            var buffer = new byte[bytesRecorded];
                            Marshal.Copy(header.lpData, buffer, 0, bytesRecorded);
                            OnRecieved(buffer);
                        }

                        if (Active)
                        {
                            // keep sampling. recycle buffer.
                            Win32.waveInAddBuffer(Device, header_ptr, Win32.WaveHeader.Size);
                        }
                        else
                        {
                            // stop sampling. release buffer.
                            Win32.waveInUnprepareHeader(Device, header_ptr, Win32.WaveHeader.Size);
                            var data_handle = GCHandle.FromIntPtr(header.dwUser);
                            data_handle.Free();
                            Marshal.FreeHGlobal(header_ptr);
                        }
                    }
                }
            });
            thread.Name = "WaveInThread";
            thread.IsBackground = true;
            thread.Start();

            return proc;
        }

        private bool Active = false;

        private Action<byte[]> OnRecieved;

        /// <summary>start sampling.</summary>
        /// <param name="onRecieved">
        /// called when each buffer becomes full, by default every second.
        /// if bitsPerSamples == 8, 1 byte represent sigle byte value from 0 to 255, base line = 128.
        /// if bitsPerSamples == 16, 2 bytes represent single short value(little endian) from -32767 to 32767, base line = 0.
        /// if stereo (Channels = 2), data order is LRLR...
        /// </param>
        public void Start(Action<byte[]> onRecieved)
        {
            Start(onRecieved, BytesPerSec);
        }

        /// <summary>start sampling.</summary>
        /// <param name="onRecieved">
        /// called when each buffer becomes full.
        /// if bitsPerSamples == 8, 1 byte represent sigle byte value from 0 to 255, base line = 128.
        /// if bitsPerSamples == 16, 2 bytes represent single short value(little endian) from -32767 to 32767, base line = 0.
        /// if stereo (Channels = 2), data order is LRLR...
        /// </param>
        /// <param name="bufferSize">buffer size. use BytesPerSec if called every second.</param>
        public void Start(Action<byte[]> onRecieved, int bufferSize)
        {
            this.OnRecieved = onRecieved;
            
            // double buffering.
            for (int i = 0; i < 2; i++)
            {
                var data = new byte[bufferSize];
                var data_handle = GCHandle.Alloc(data, GCHandleType.Pinned);

                var header = new Win32.WaveHeader();
                header.lpData = data_handle.AddrOfPinnedObject();
                header.dwBufferLength = (uint)bufferSize;
                header.dwUser = GCHandle.ToIntPtr(data_handle);

                var header_ptr = Marshal.AllocHGlobal(Win32.WaveHeader.Size);
                Marshal.StructureToPtr(header, header_ptr, true);

                Win32.waveInPrepareHeader(Device, header_ptr, Win32.WaveHeader.Size);
                Win32.waveInAddBuffer(Device, header_ptr, Win32.WaveHeader.Size);
            }

            int rc = Win32.waveInStart(Device);
            if (rc != Win32.MMSYSERR_NOERROR)
            {
                var sb = new StringBuilder(256);
                Win32.waveInGetErrorText(rc, sb, sb.Capacity);
                throw new InvalidOperationException(sb.ToString());
            }

            Active = true;
        }

        /// <summary>stop sampling after buffer becomes full.</summary>
        public void Stop()
        {
            Active = false;
        }

        /// <summary>stop sampling immediately.</summary>
        public void Reset()
        {
            Stop();

            // stops input and resets the current position to zero.
            // All pending buffers are marked as done and returned to the application.
            Win32.waveInReset(Device);
        }

        public void Close()
        {
            Win32.waveInClose(Device);
            Device = IntPtr.Zero;
        }

        public static string[] FindDevices()
        {
            uint devs = Win32.waveInGetNumDevs();
            string[] devNames = new string[devs];
            for (uint i = 0; i < devs; i++)
            {
                var caps = new Win32.WaveInCaps();
                Win32.waveInGetDevCaps(i, out caps, Win32.WaveInCaps.Size);
                devNames[i] = caps.szPname;
            }
            return devNames;
        }
    }

    class AudioOutput
    {
        // [How to use]
        // string[] devices = AudioOutput.FindDevices();
        // if (devices.Length == 0) return; // no device.

        // create AudioOutput with default device.
        // var device = new AudioOutput(44100, 16, 1);

        // start writing.
        // device.WriteStart(() =>
        // {
        //     // called when each buffer becomes empty and request more data.
        //     return sign_wave;
        // });

        // this.FormClosing += (s, ev) =>
        // {
        //     device.WriteStop();
        //     device.Close();
        // };

        private IntPtr Device;

        /// <summary>data bytes required per second.</summary>
        public readonly int BytesPerSec;

        private const uint WAVE_MAPPER = 0xFFFFFFFF;

        /// <summary>create AudioOutput with default device.</summary>
        public AudioOutput(int samplesPerSec, int bitsPerSamples, int channels) : this(samplesPerSec, bitsPerSamples, channels, WAVE_MAPPER) { }

        /// <summary>create AudioOutput with specified device.</summary>
        /// <param name="deviceId">index of FindDevices result.</param>
        public AudioOutput(int samplesPerSec, int bitsPerSamples, int channels, uint deviceId)
        {
            var format = new Win32.WaveFormatEx(samplesPerSec, bitsPerSamples, channels);
            BytesPerSec = samplesPerSec * channels * (bitsPerSamples / 8);

            WaveOutProc = WaveOutProcAndThreadStart();

            int rc = Win32.waveOutOpen(out Device, deviceId, ref format, WaveOutProc, IntPtr.Zero, Win32.CALLBACK_FUNCTION);
            if (rc != Win32.MMSYSERR_NOERROR)
            {
                var sb = new StringBuilder(256);
                Win32.waveOutGetErrorText(rc, sb, sb.Capacity);
                throw new InvalidOperationException(sb.ToString());
            }            
        }

        private Win32.WaveOutProc WaveOutProc;
        private Win32.WaveOutProc WaveOutProcAndThreadStart()
        {
            var proc_to_thread_event = new System.Threading.AutoResetEvent(false);
            var proc_to_thread_param = new Queue<IntPtr>();

            // callback
            var proc = new Win32.WaveOutProc((hwo, msg, user, param1, param2) =>
            {
                // [waveOutProc callback function remarks]
                // Applications should not call any system-defined functions from inside a callback function, except for
                // EnterCriticalSection, LeaveCriticalSection, midiOutLongMsg, midiOutShortMsg, OutputDebugString,
                // PostMessage, PostThreadMessage, SetEvent, timeGetSystemTime, timeGetTime, timeKillEvent, and timeSetEvent.
                // Calling other wave functions will cause deadlock.

                // Done:the device driver is finished with a data block
                if (msg == Win32.WaveOutMessage.Done)
                {
                    lock (proc_to_thread_param)
                    {
                        proc_to_thread_param.Enqueue(param1);
                    }

                    proc_to_thread_event.Set();
                }
            });

            // thread
            var thread = new System.Threading.Thread(() =>
            {
                IntPtr header_ptr;

                while (true)
                {                    
                    proc_to_thread_event.WaitOne(); // wait until event is rased.
                    {
                        lock (proc_to_thread_param)
                        {
                            header_ptr = proc_to_thread_param.Dequeue();
                        }
                        
                        Win32.waveOutUnprepareHeader(Device, header_ptr, Win32.WaveHeader.Size);

                        var header = Win32.WaveHeader.FromIntPtr(header_ptr);
                        var data_handle = GCHandle.FromIntPtr(header.dwUser);
                        data_handle.Free();

                        Marshal.FreeHGlobal(header_ptr);
                    }

                    var func = DataSupplier;
                    if (func != null)
                    {
                        var data = func();
                        if (data != null) Write(data);
                    }
                }
            });
            thread.Name = "WaveOutThread";
            thread.IsBackground = true;
            thread.Start();

            return proc;
        }

        private void WriteBuffer(IntPtr hwo, byte[] buffer)
        {
            var size = buffer.Length;

            var data = new byte[size];
            buffer.CopyTo(data, 0);
            var data_handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            var header = new Win32.WaveHeader();
            header.lpData = data_handle.AddrOfPinnedObject();
            header.dwBufferLength = (uint)size;
            header.dwUser = GCHandle.ToIntPtr(data_handle);

            var header_ptr = Marshal.AllocHGlobal(Win32.WaveHeader.Size);
            Marshal.StructureToPtr(header, header_ptr, true);

            Win32.waveOutPrepareHeader(hwo, header_ptr, Win32.WaveHeader.Size);
            Win32.waveOutWrite(hwo, header_ptr, Win32.WaveHeader.Size);
        }

        /// <summary>write sound buffer to output device.</summary>
        public void Write(byte[] bytes)
        {
            WriteBuffer(Device, bytes);
        }

        private Func<byte[]> DataSupplier;

        /// <summary>start writing sound buffer with data supplier.</summary>
        /// <param name="dataSupplier">
        /// called when each buffer becomes empty and request more data.
        /// You have to supply extra data by dataSupplier result value.
        /// </param>
        public void WriteStart(Func<byte[]> dataSupplier)
        {
            WriteStart(dataSupplier, 2); // 2 = double buffering.
        }


        /// <summary>start writing sound buffer with data supplier.</summary>
        /// <param name="dataSupplier">
        /// called when each buffer becomes empty and request more data.
        /// You have to supply extra data by dataSupplier result value.
        /// </param>
        /// <param name="bufferDepth">
        /// internal buffer queue depth. 2 means double buffering.
        /// increase when supplied data length is too short and sound breaks.
        /// </param>
        public void WriteStart(Func<byte[]> dataSupplier, int bufferDepth)
        {
            DataSupplier = dataSupplier;

            for (int i = 0; i < bufferDepth; i++)
            {
                var buffer = dataSupplier();
                Write(buffer);
            }
        }

        /// <summary>stop calling data supplier after WriteStart.</summary>
        public void WriteStop()
        {
            DataSupplier = null;
        }

        public void Close()
        {
            Win32.waveOutClose(Device);
            Device = IntPtr.Zero;
        }

        public static string[] FindDevices()
        {
            var nums = Win32.waveOutGetNumDevs();
            var result = new string[nums];
            for (uint i = 0; i < nums; i++)
            {
                var caps = new Win32.WaveOutCaps();
                Win32.waveOutGetDevCaps(i, out caps, Win32.WaveOutCaps.Size);
                result[i] = caps.szPname;
            }
            return result;
        }
    }

}
