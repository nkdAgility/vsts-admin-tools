﻿using CommandLine;
using Microsoft.ApplicationInsights.DataContracts;
using NuGet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using VstsAdminTools.Commands;

namespace VstsAdminTools.ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            Telemetry.Current.TrackEvent("ApplicationStart");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DateTime startTime = DateTime.Now;
            Stopwatch mainTimer = new Stopwatch();
            mainTimer.Start();
            /////////////////////////////////////////////////
            string logsPath = CreateLogsPath();
            //////////////////////////////////////////////////
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.Listeners.Add(new TextWriterTraceListener(Path.Combine(logsPath, "VstsAdminTools.log"), "myListener"));
            //////////////////////////////////////////////////
            Trace.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, "[Info]");
            Version thisVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Trace.WriteLine(string.Format("Running version detected as {0}", thisVersion), "[Info]");
            if (IsOnline())
            {
                Version latestVersion = GetLatestVersion();
                Trace.WriteLine(string.Format("Latest version detected as {0}", latestVersion), "[Info]");
                if (latestVersion > thisVersion)
                {
                    Trace.WriteLine(
                        string.Format("You are currenlty running version {0} and a newer version ({1}) is available. You should upgrade now using Chocolatey command 'choco update vsts-admin-tools' from the command line.",
                        thisVersion, latestVersion
                        ),
                        "[Warning]");
#if !DEBUG

                    Console.WriteLine("Do you want to continue? (y/n)");
                    if (Console.ReadKey().Key != ConsoleKey.Y)
                    {
                        Trace.WriteLine("User aborted to update version", "[Warning]");
                        return 2;
                    }
#endif
                }
            }

            Trace.WriteLine(string.Format("Telemitery Enabled: {0}", Telemetry.Current.IsEnabled().ToString()), "[Info]");
            Trace.WriteLine(string.Format("SessionID: {0}", Telemetry.Current.Context.Session.Id), "[Info]");
            Trace.WriteLine(string.Format("User: {0}", Telemetry.Current.Context.User.Id), "[Info]");
            Trace.WriteLine(string.Format("Start Time: {0}", startTime.ToUniversalTime()), "[Info]");
            Trace.WriteLine("------------------------------START-----------------------------", "[Info]");
            //////////////////////////////////////////////////
            int result = 0;

            result = (int)Parser.Default.ParseArguments<exportUserProfilePicturesOptions>(args).MapResult(
                (exportUserProfilePicturesOptions opts) => new ExportUserProfilePicturesCommand().Run(opts, logsPath),
                errs => 1);
            //////////////////////////////////////////////////
            Trace.WriteLine("-------------------------------END------------------------------", "[Info]");
            mainTimer.Stop();
            Telemetry.Current.TrackEvent("ApplicationEnd", null,
                new Dictionary<string, double> {
                        { "ApplicationDuration", mainTimer.ElapsedMilliseconds }
                });
            if (Telemetry.Current != null)
            {
                Telemetry.Current.Flush();
                // Allow time for flushing:
                System.Threading.Thread.Sleep(1000);
            }
            Trace.WriteLine(string.Format("Duration: {0}", mainTimer.Elapsed.ToString("c")), "[Info]");
            Trace.WriteLine(string.Format("End Time: {0}", startTime.ToUniversalTime()), "[Info]");
#if DEBUG
            Console.ReadKey();
#endif
            //return result;
        }



        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ExceptionTelemetry excTelemetry = new ExceptionTelemetry((Exception)e.ExceptionObject);
            excTelemetry.SeverityLevel = SeverityLevel.Critical;
            excTelemetry.HandledAt = ExceptionHandledAt.Unhandled;
            Telemetry.Current.TrackException(excTelemetry);
            Telemetry.Current.Flush();
            System.Threading.Thread.Sleep(1000);
        }

        private static Version GetLatestVersion()
        {
            string packageID = "vsts-admin-tools";

            //Connect to the official package repository
            IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("https://chocolatey.org/api/v2/");
            var version = repo.FindPackagesById(packageID).Max(p => p.Version);
            return new Version(version.ToString());
        }

        private static bool IsOnline()
        {
            try
            {
                Ping myPing = new Ping();
                String host = "8.8.4.4";
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                // Likley no network is even available
                return false;
            }

        }

        private static string CreateLogsPath()
        {
            string exportPath;
            string assPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            exportPath = Path.Combine(Path.GetDirectoryName(assPath), "logs", DateTime.Now.ToString("yyyyMMddHHmmss"));
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }

            return exportPath;
        }



    }
}
