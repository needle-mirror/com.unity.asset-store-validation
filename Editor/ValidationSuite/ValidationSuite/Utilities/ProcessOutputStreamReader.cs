using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite
{
    //Copied from UnityEditor
    class ProcessOutputStreamReader
    {
        readonly Func<bool> hostProcessExited;
        readonly StreamReader stream;
        internal List<string> lines;
        Thread thread;

        internal ProcessOutputStreamReader(Process p, StreamReader stream) : this(() => p.HasExited, stream)
        {
        }

        internal ProcessOutputStreamReader(Func<bool> hostProcessExited, StreamReader stream)
        {
            this.hostProcessExited = hostProcessExited;
            this.stream = stream;
            lines = new List<string>();

            thread = new Thread(ThreadFunc);
            thread.Start();
        }

        void ThreadFunc()
        {
            try
            {
                while (true)
                {
                    if (stream.BaseStream == null) return;
                    var line = stream.ReadLine();
                    if (line == null)
                        return;
                    lock (lines)
                    {
                        lines.Add(line);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // We have had this throw in a run on Katana in what appears to be a case of a very short running
                // process exiting between the check to hostProcessExited() and the call to stream.ReadLine();
                // So catch this case to avoid this from happening again.
                lock (lines)
                {
                    lines.Add("Could not read output because an ObjectDisposedException was thrown.");
                }
            }
        }

        internal string[] GetOutput()
        {
            if (hostProcessExited())
                thread.Join();
            lock (lines)
            {
                return lines.ToArray();
            }
        }
    }
}
