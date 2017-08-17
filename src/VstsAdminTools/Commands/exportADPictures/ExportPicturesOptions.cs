using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsAdminTools.Commands
{
    [Verb("exportpictures", HelpText = "Iterates through all of your Active Directory users and exports thier Profile picture from AD.")]
    public class ExportPicturesOptions :OptionsBase
    {
        [Option('o', "outpath", Required = true, HelpText = "Path to output folder.")]
        public string OutPath { get; set; }

   [Option('d', "domain", Required = true, HelpText = "AD Domain to connect to.")]
        public string Domain { get; set; }
   [Option('u', "username", Required = true, HelpText = "Username of user with peremsision to domain")]
        public string Username { get; set; }
   [Option('p', "password", Required = true, HelpText = "password")]
        public string Password { get; set; }
[Option('m', "picturemask", Required = true, HelpText = "Mask in the format http://comp.portal.com/{0}.jpg")]
        public string CorporatePictureMask { get; set; }
[Option('n', "adPropertyName", Required = true, HelpText = "Property to match to picturemask from Active Directory")]
        public string ADPropertyName { get; set; }
    }
}
