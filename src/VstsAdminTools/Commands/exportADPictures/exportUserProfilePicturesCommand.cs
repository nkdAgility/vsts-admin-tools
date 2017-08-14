using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VstsAdminTools.Commands
{
    public class ExportUserProfilePicturesCommand : CommandBase<exportUserProfilePicturesOptions>
    {

        //private readonly TfsTeamService teamService;
        //private readonly ProjectInfo projectInfo;
        public override int RunInternal(exportUserProfilePicturesOptions opts)
        {
            if (!Directory.Exists(opts.OutPath))
            {
                Directory.CreateDirectory(opts.OutPath);
            }

            TfsTeamProjectCollection collection = new TfsTeamProjectCollection(opts.CollectionURL);
            collection.EnsureAuthenticated();
            IIdentityManagementService2 ims2 = (IIdentityManagementService2)collection.GetService(typeof(IIdentityManagementService2));
            TeamFoundationIdentity SIDS = ims2.ReadIdentity(IdentitySearchFactor.AccountName, "Project Collection Valid Users", MembershipQuery.Expanded, ReadIdentityOptions.None);

            Trace.WriteLine(string.Format("Found {0}", SIDS.Members.Count()));
            var itypes = (from IdentityDescriptor id in SIDS.Members select id.IdentityType).Distinct();

            foreach (string item in itypes)
            {
                var infolks = (from IdentityDescriptor id in SIDS.Members where id.IdentityType == item select id);
                Trace.WriteLine(string.Format("Found {0} of {1}", infolks.Count(), item));
            }
            var folks = (from IdentityDescriptor id in SIDS.Members where id.IdentityType == "System.Security.Principal.WindowsIdentity" || id.IdentityType == "Microsoft.IdentityModel.Claims.ClaimsIdentity" select id);

            DirectoryContext objContext = new DirectoryContext(DirectoryContextType.Domain, opts.Domain, opts.Username, opts.Password);
            Domain objDomain = Domain.GetDomain(objContext);
            string ldapName = string.Format("LDAP://{0}", objDomain.Name);

            int current = folks.Count();
            foreach (IdentityDescriptor id in folks)
            {
                try
                {
                    TeamFoundationIdentity i = ims2.ReadIdentity(IdentitySearchFactor.Identifier, id.Identifier, MembershipQuery.Direct, ReadIdentityOptions.None);
                    Trace.WriteLine(i.DisplayName);
                    if (!(i == null) && (i.IsContainer == false))
                    {
                        if ((!i.DisplayName.StartsWith("Microsoft.") && (!i.DisplayName.StartsWith("OssManagement"))))
                            { 
                        DirectoryEntry d = new DirectoryEntry(ldapName, opts.Username, opts.Password);
                        DirectorySearcher dssearch = new DirectorySearcher(d);
                        if (i.UniqueName.Contains("@"))
                            {
                                dssearch.Filter = string.Format("(sAMAccountName={0})", i.UniqueName.Split(char.Parse(@"@"))[0]);
                            }
                        else
                            {
                                dssearch.Filter = string.Format("(sAMAccountName={0})", i.UniqueName.Split(char.Parse(@"\"))[1]);
                            }
                        
                        SearchResult sresult = dssearch.FindOne();
                        WebClient webClient = new WebClient();
                        webClient.Credentials = CredentialCache.DefaultNetworkCredentials;
                        if (sresult != null)
                        {
                            string newImage = Path.Combine(opts.OutPath, string.Format("{0}.jpg", i.UniqueName.Replace(@"\", "-")));
                            if (!File.Exists(newImage))
                            {
                                DirectoryEntry deUser = new DirectoryEntry(sresult.Path, opts.Username, opts.Password);
                                Trace.WriteLine(string.Format("{0} [PROCESS] {1}: {2}", current, deUser.Name, newImage));
                                string empPic = string.Format(opts.CorporatePictureMask, deUser.Properties["employeeNumber"].Value);
                                try
                                {

                                    webClient.DownloadFile(empPic, newImage);
                                }
                                catch (Exception ex)
                                {
                                    Trace.WriteLine(string.Format("      [ERROR] {0}", ex.ToString()));

                                }
                            }
                            else
                            {
                                Trace.WriteLine(string.Format("{0} [SKIP] Exists {1}", current, newImage));
                            }
                        }
                    }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("      [ERROR] {0}", ex.ToString()));
                }

                current--;
            }
            return 0;
        }    
    }
}
