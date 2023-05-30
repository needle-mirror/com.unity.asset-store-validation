using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.WSA;
using Debug = UnityEngine.Debug;

namespace UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite
{
    /// <summary>
    /// Standardize npm launch in order to make sure registry is always specified and npm path is escaped
    /// </summary>
    class NodeLauncher
    {
        static string _nodePath;
        static string _npmScriptPath;

        static string NodePath
        {
            get
            {
                if (!string.IsNullOrEmpty(_nodePath))
                    return _nodePath;
                _nodePath = Path.Combine(EditorApplication.applicationContentsPath, "Tools");
                _nodePath = Path.Combine(_nodePath, "nodejs");
#if (UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX)
                _nodePath = Path.Combine(_nodePath, "bin");
                _nodePath = Path.Combine(_nodePath, "node");
#elif UNITY_EDITOR_WIN
                _nodePath = Path.Combine(_nodePath, "node.exe");
#endif
                return _nodePath;
            }
        }

        static string NpmScriptPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_npmScriptPath))
                    return _npmScriptPath;
                _npmScriptPath = Path.Combine(EditorApplication.applicationContentsPath, "Tools");
                _npmScriptPath = Path.Combine(_npmScriptPath, "nodejs");
#if (UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX)
                _npmScriptPath = Path.Combine(_npmScriptPath, "lib");
#endif
                _npmScriptPath = Path.Combine(_npmScriptPath, "node_modules");
                _npmScriptPath = Path.Combine(_npmScriptPath, "npm");
                _npmScriptPath = Path.Combine(_npmScriptPath, "bin");
                _npmScriptPath = Path.Combine(_npmScriptPath, "npm-cli.js");
                return _npmScriptPath;
            }
        }

        public const string ProductionRepositoryUrl = "https://packages.unity.com/";

        public string NpmRegistry { get; set; }
        string NpmLogLevel { get; set; }
        string NpmPrefix { get; set; }
        bool NpmOnlineOperation { get; set; }

        string Script { get; set; }
        string Args { get; set; }

        int WaitTime { get; set; }

        public StringBuilder OutputLog = new StringBuilder();
        StringBuilder ErrorLog = new StringBuilder();

        public string WorkingDirectory
        {
            set => Process.StartInfo.WorkingDirectory = value;
        }

        Process Process { get; set; }

        void NodeSetup()
        {
            WaitTime = 1000 * 60 * 10;        // 10 Minutes
            NpmOnlineOperation = false;

            Process = new Process();
            Process.StartInfo.FileName = NodePath;
            Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.CreateNoWindow = true;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.RedirectStandardError = true;
        }

        public NodeLauncher()
        {
            NodeSetup();
        }

        public NodeLauncher(string workingPath = "", string npmLogLevel = "error", string npmRegistry = ProductionRepositoryUrl, string npmPrefix = "")
        {
            NodeSetup();

            if (npmLogLevel != "")
                NpmLogLevel = npmLogLevel;

            if (npmRegistry != "")
                NpmRegistry = npmRegistry;

            if (workingPath != "")
                WorkingDirectory = workingPath;

            if (npmPrefix != "")
                NpmPrefix = npmPrefix;
        }

        void Launch()
        {
            if (string.IsNullOrEmpty(Script))
                throw new Exception("No node script set to run;");

            Process.StartInfo.Arguments = "\"" + Script + "\" " + Args;

            using (var outputWaitHandle = new AutoResetEvent(false))
            using (var errorWaitHandle = new AutoResetEvent(false))
            {
                Process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                        outputWaitHandle.Set();
                    else
                        OutputLog.AppendLine(e.Data);
                };
                Process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                        errorWaitHandle.Set();
                    else
                        ErrorLog.AppendLine(e.Data);
                };

                Process.Start();

                Process.BeginOutputReadLine();
                Process.BeginErrorReadLine();


                var waitTimePerIteration = 500; // 0.5 second each iteration
                var currentWaitTime = 0;
                var exited = false;
                var offline = false;
                while (true)
                {
                    System.Threading.Thread.Sleep(waitTimePerIteration);
                    currentWaitTime += waitTimePerIteration;
                    exited = Process.HasExited && outputWaitHandle.WaitOne(0) && errorWaitHandle.WaitOne(0);
                    offline = NpmOnlineOperation && Utilities.NetworkNotReachable;
                    if (exited || offline || currentWaitTime > WaitTime)
                        break;
                }

                if (exited)
                {
                    if (Process.ExitCode != 0)
                    {
                        var message = "Launching script {0} has failed with args: {1}\nOutput: {2}\nError: {3}";
                        throw new ApplicationException(string.Format(message, Script, Args, OutputLog, ErrorLog));
                    }
                }
                else
                {
                    Process.Kill();
                    if (offline)
                    {
                        var message = "Launching npm has failed with args: {0}\nError: Network not reachable";
                        throw new ApplicationException(string.Format(message, Args));
                    }
                    else
                    {
                        var message = "Launching script {0} has failed with timeout for args: {1}\nOutput: {2}\nError: {3}";
                        throw new TimeoutException(string.Format(message, Script, Args, OutputLog, ErrorLog));
                    }
                }
            }
        }

        protected void NpmLaunch(string command, string packageId)
        {
            NpmOnlineOperation = !Directory.Exists(packageId) && !File.Exists(packageId);
            Script = NpmScriptPath;
            Args = command + " \"" + packageId + "\"";
            if (!string.IsNullOrEmpty(NpmRegistry))
                Args += " --registry \"" + NpmRegistry + "\"";
            if (!string.IsNullOrEmpty(NpmLogLevel))
                Args += " --loglevel=" + NpmLogLevel;
            if (!string.IsNullOrEmpty(NpmPrefix))
                Args += " --prefix " + NpmPrefix;

            Launch();
        }

        public void NpmInstall(string packageId)
        {
            NpmLaunch("install", packageId);
        }

        public void NpmPack(string packageId)
        {
            NpmLaunch("pack", packageId);
        }

        public void NpmView(string packageId)
        {
            NpmLaunch("view", packageId);
        }
    }
}
