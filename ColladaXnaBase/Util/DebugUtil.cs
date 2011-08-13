using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ColladaXna.Base.Util
{
    public static class DebugUtil
    {
        public static string DefaultDebugOutputFile = @"debug.log";

        static Dictionary<string, TraceListener> listeners = 
            new Dictionary<string, TraceListener>();

        public static bool HasListenerFor(string filename)
        {
            return listeners.ContainsKey(filename);
        }

        public static void FileWrite(string filename, string message)
        {
            if (HasListenerFor(filename) == false) BeginFile(filename);
            listeners[filename].WriteLine(message);
        }

        public static void FileWrite(string message)
        {
            FileWrite(DefaultDebugOutputFile, message);
        }

        public static void BeginFile(string filename)
        {
            TraceListener listener = new TextWriterTraceListener(filename);
            listeners.Add(filename, listener);            
        }

        public static void BeginFile()
        {
            BeginFile(DefaultDebugOutputFile);
        }

        public static void EndFile(string filename)
        {            
            TraceListener listener = listeners[filename];
            listeners.Remove(filename);
            listener.Flush();
            listener.Close();
            Debug.Listeners.Remove(listener);
        }

        public static void EndFile()
        {
            EndFile(DefaultDebugOutputFile);
        }

        public static void LaunchDebugger()
        {
            System.Diagnostics.Debugger.Launch();
        }
    }

    public static class ContentAssert
    {
        public static void AreEqual(Object a, Object b, string message, bool launchDebugger)
        {
            if (a.Equals(b) == false)
            {
                if (launchDebugger)
                    DebugUtil.LaunchDebugger();
                throw new Exception(message + ", '" + a.ToString() + "' != '" + b.ToString() + "'");
            }
        }

        public static void AreEqual(Object a, Object b, string message)
        {
            AreEqual(a, b, message, false);
        }

        public static void IsTrue(bool expression, string message, bool launchDebugger)
        {
            if(!expression)
            {
                if (launchDebugger)
                    DebugUtil.LaunchDebugger();
                throw new ApplicationException(message);
            }
        }

        public static void IsTrue(bool expression, string message)
        {
            IsTrue(expression, message, false);
        }
    }
}
