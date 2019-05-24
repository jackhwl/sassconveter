using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;


    internal static class Logger
    {
        private static IVsOutputWindowPane _pane;
        private static IVsOutputWindow _output = (IVsOutputWindow)ServiceProvider.GlobalProvider.GetService(typeof(SVsOutputWindow));

        public static void Log(object message)
        {
            try
            {
                if (EnsurePane())
                {
                    _pane.OutputString(DateTime.Now.ToString() + ": " + message + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }
        }

        public static void EventLog(string message)
        {
            using (EventLog eventLog = new EventLog("Application")) 
            {
                eventLog.Source = "Application"; 
                eventLog.WriteEntry("sassconverter Log message: " + message, EventLogEntryType.Error, 911, 1); 
            }

        }
        private static bool EnsurePane()
        {
            if (_pane == null)
            {
                var guid = Guid.NewGuid();
                _output.CreatePane(ref guid, "LessCompiler.Vsix.Name", 1, 1);
                _output.GetPane(ref guid, out _pane);
            }

            return _pane != null;
        }
    }

