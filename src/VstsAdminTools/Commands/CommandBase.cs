using CommandLine;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsAdminTools.Commands
{
    public abstract class CommandBase<TOptions> where TOptions : OptionsBase
    {
        internal string LogPathRoot;

        public int Run(TOptions opts, string logPath)
        {
            LogPathRoot = logPath;
            var verbAttribute = typeof(TOptions).GetCustomAttributes(typeof(VerbAttribute), true).FirstOrDefault() as VerbAttribute;
            Telemetry.Current.TrackEvent(string.Format("run-{0}-start", verbAttribute.Name));
            string exportPath = CreateExportPath(logPath, verbAttribute.Name);
            Trace.Listeners.Add(new TextWriterTraceListener(Path.Combine(exportPath, string.Format("{0}.log", verbAttribute.Name)), string.Format("{0}Command", verbAttribute.Name)));
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //////////////////////////////////////////////////
            int output = 0;
            try
            {
                output = RunInternal(opts);
            }
            catch (Exception ex)
            {
                Telemetry.Current.TrackException(ex);
                Trace.TraceError(ex.ToString());
                output = 1;
            }
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Dictionary<string, double> metrics = new Dictionary<string, double>();
            metrics.Add("CommmandRunTime", stopwatch.Elapsed.TotalSeconds);
            Telemetry.Current.TrackEvent(string.Format("run-{0}-complete", verbAttribute.Name), null, metrics);
            Trace.WriteLine(string.Format(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed));
            Trace.Listeners.Remove(string.Format("{0}Command", verbAttribute.Name));
            return output;
        }

        public abstract int RunInternal(TOptions opts  );

        internal static string CreateExportPath(string logPath, string CommandName)
        {
            string exportPath = Path.Combine(logPath, CommandName);
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }

            return exportPath;
        }
    }
}
