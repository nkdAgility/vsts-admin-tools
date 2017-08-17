using CsvHelper;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsAdminTools.Commands
{
    public class ExportAzureADCommand : CommandBase<ExportAzureADOptions>
    {
        public override int RunInternal(ExportAzureADOptions opts)
        {
            opts.OutPath = opts.OutPath ?? this.LogPathRoot;

            StreamWriter sw = File.CreateText(Path.Combine(opts.OutPath, "IdentityList.csv"));
            sw.AutoFlush = true;
            using (var csv = new CsvWriter(sw))
            {
                csv.WriteHeader<AzureAdGroupItem>();

                TfsTeamProjectCollection sourceCollection = new TfsTeamProjectCollection(opts.CollectionURL);
                sourceCollection.EnsureAuthenticated();
                IIdentityManagementService2 sourceIMS2 = (IIdentityManagementService2)sourceCollection.GetService(typeof(IIdentityManagementService2));
                List<CatalogNode> sourceTeamProjects = sourceCollection.CatalogNode.QueryChildren(new[] { CatalogResourceTypes.TeamProject }, false, CatalogQueryOptions.None).ToList();
                if (opts.TeamProject != null)
                {
                    sourceTeamProjects = sourceTeamProjects.Where(x => x.Resource.DisplayName == opts.TeamProject).ToList();
                }
                int current = sourceTeamProjects.Count();
                foreach (CatalogNode sourceTeamProject in sourceTeamProjects)
                {
                    Trace.WriteLine(string.Format("---------------{0}\\{1}", current, sourceTeamProjects.Count()));
                    Trace.WriteLine(string.Format("{0}, {1}", sourceTeamProject.Resource.DisplayName, sourceTeamProject.Resource.Identifier));
                    string projectUri = sourceTeamProject.Resource.Properties["ProjectUri"];
                    TeamFoundationIdentity[] appGroups = sourceIMS2.ListApplicationGroups(projectUri, ReadIdentityOptions.None);
                    foreach (TeamFoundationIdentity appGroup in appGroups.Where(x => !x.DisplayName.EndsWith("\\Project Valid Users")))
                    {
                        Trace.WriteLine(string.Format("    {0}", appGroup.DisplayName));
                        TeamFoundationIdentity sourceAppGroup = sourceIMS2.ReadIdentity(appGroup.Descriptor, MembershipQuery.Expanded, ReadIdentityOptions.None);
                        foreach (IdentityDescriptor child in sourceAppGroup.Members.Where(x => x.IdentityType == "Microsoft.TeamFoundation.Identity" || x.IdentityType == "Microsoft.IdentityModel.Claims.ClaimsIdentity"))
                        {

                            TeamFoundationIdentity sourceChildIdentity = sourceIMS2.ReadIdentity(IdentitySearchFactor.Identifier, child.Identifier, MembershipQuery.None, ReadIdentityOptions.ExtendedProperties);
                            var SpecialType = (string)sourceChildIdentity.GetProperty("SpecialType");
                            var Account = (string)sourceChildIdentity.GetProperty("Account");
                            object DirectoryAlias;
                            object Mail;
                            sourceChildIdentity.TryGetProperty("DirectoryAlias", out DirectoryAlias);
                            sourceChildIdentity.TryGetProperty("Mail", out Mail);
                            switch (SpecialType)
                            {
                                case "AzureActiveDirectoryApplicationGroup":
                                    Trace.WriteLine(string.Format("     Found AD Group {0}", sourceChildIdentity.DisplayName));
                                    csv.WriteRecord<AzureAdGroupItem>(new AzureAdGroupItem
                                    {
                                        TeamProject = sourceTeamProject.Resource.DisplayName,
                                        ApplciationGroup = appGroup.DisplayName,
                                        Account = Account,
                                        Mail = (string)Mail,
                                        DirectoryAlias = (string)DirectoryAlias
                                    });
                                    break;
                                case "Generic":
                                    if (sourceChildIdentity.IsContainer)
                                    {
                                        Trace.WriteLine(string.Format("Skipping {0} | {1} - TF GROUP", SpecialType, Account));

                                    } else
                                    {
                                        Trace.WriteLine(string.Format("     Found AD User {0}", sourceChildIdentity.DisplayName));
                                        csv.WriteRecord<AzureAdGroupItem>(new AzureAdGroupItem
                                        {
                                            TeamProject = sourceTeamProject.Resource.DisplayName,
                                            ApplciationGroup = appGroup.DisplayName,
                                            Account = Account,
                                            Mail = (string)Mail,
                                            DirectoryAlias = (string)DirectoryAlias
                                        });
                                    }                                   
                                    break;
                                default:
                                    Trace.WriteLine(string.Format("Skipping {0} | {1} - UNKNOWN", SpecialType, Account));
                                    break;
                            }
                        }
                    }
                    current--;
                    sw.Flush();
                }
            }
            sw.Close();
            return 0;
        }

    }
}
