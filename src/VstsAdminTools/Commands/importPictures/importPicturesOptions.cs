using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsAdminTools.Commands
{
    [Verb("importpictures", HelpText = "Iterates through a folder of pictures, named for the user, and imports then into the Visual Studio Profile.")]
    public class ImportPicturesOptions : OptionsBase
    {
        [Option('o', "outpath", Required = true, HelpText = "Path to output folder.")]
        public string OutPath { get; set; }

    }
}
