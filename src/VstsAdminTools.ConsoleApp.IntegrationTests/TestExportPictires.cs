using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommandLine;
using VstsAdminTools.Commands;
using System.IO;

namespace VstsAdminTools.ConsoleApp.IntegrationTests
{
    [TestClass]
    public class TestExportPictires
    {
        [TestMethod]
        public void TestExportPicturesRun()
        {

            int result = 0;
            string[] args = new string[] { "" };

            result = (int)Parser.Default.ParseArguments<ExportPicturesOptions>(args).MapResult(
                (ExportPicturesOptions opts) => new ExportPicturesCommand().Run(opts, CreateLogsPath()),
                errs => 1);
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
