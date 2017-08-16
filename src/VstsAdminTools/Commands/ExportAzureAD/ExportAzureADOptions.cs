using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsAdminTools.Commands
{
    [Verb("exportAzureAD", HelpText = "Iterates through a collection and lists all AD groups assigned but that have not been synced to AzureAD.")]
    public class ExportAzureADOptions : OptionsBase
    {
        [Option('o', "outpath", Required = true, HelpText = "Path to output folder.")]
        public string OutPath { get; set; }

        [Option('p', "teamproject", Required = false, HelpText = "Optional team project.")]
        public string TeamProject { get; set; }
    }
}
