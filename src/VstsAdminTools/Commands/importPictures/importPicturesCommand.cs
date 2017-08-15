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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace VstsAdminTools.Commands
{
    public class ImportPicturesCommand : CommandBase<ImportPicturesOptions>
    {

       
        public override int RunInternal(ImportPicturesOptions opts)
        {
            if (!Directory.Exists(opts.OutPath))
            {
                Directory.CreateDirectory(opts.OutPath);
            }

            TfsTeamProjectCollection collection = new TfsTeamProjectCollection(opts.CollectionURL);
            collection.EnsureAuthenticated();
            IIdentityManagementService2 ims2 = (IIdentityManagementService2)collection.GetService(typeof(IIdentityManagementService2));


            var files = Directory.GetFiles(opts.OutPath);
            var regex = new Regex(Regex.Escape("-"));
            foreach (string file in files)
            {
                string ident = regex.Replace(Path.GetFileNameWithoutExtension(file), @"\", 1);
                string mess;
                if (SetProfileImage(ims2, ident, file, out mess))
                {
                    Trace.WriteLine(string.Format(" [UPDATE] New Profile for : {0} ", ident));
                    File.Delete(file);
                }
                else
                {
                    Trace.WriteLine(string.Format(" [FAIL] Unable to set: {0} ", ident));
                }
            }

            return 0;
        }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need to kill all errors")]
        public bool SetProfileImage(IIdentityManagementService2 ims2, string identity, string imagePath, out string message)
        {
            bool ret = true;
            message = string.Empty;
            byte[] image = new byte[0];

            TeamFoundationIdentity i = ims2.ReadIdentity(IdentitySearchFactor.AccountName, identity, MembershipQuery.Direct, ReadIdentityOptions.None);

            if (i == null)
            {
                message = "User/Group [" + identity + "] not found";
                ret = false;
            }

            if (!File.Exists(imagePath))
            {
                message = "File [" + imagePath + "] not found";
                ret = false;
            }

            if (ret)
            {
                try
                {
                    byte[] rawImage = File.ReadAllBytes(imagePath);
                    image = ConvertAndResizeImage(rawImage);
                }
                catch (Exception ex)
                {
                    message = "Could not read the profile image: " + ex.Message;
                    ret = false;
                }
            }

            if (ret)
            {
                i.SetProperty("Microsoft.TeamFoundation.Identity.Image.Data", image);
                i.SetProperty("Microsoft.TeamFoundation.Identity.Image.Type", "image/png");
                i.SetProperty("Microsoft.TeamFoundation.Identity.Image.Id", Guid.NewGuid().ToByteArray());
                i.SetProperty("Microsoft.TeamFoundation.Identity.CandidateImage.Data", null);
                i.SetProperty("Microsoft.TeamFoundation.Identity.CandidateImage.UploadDate", null);

                try
                {
                    ims2.UpdateExtendedProperties(i);
                }
                catch (PropertyServiceException)
                {
                    // swallow; this exception happens each and every time, but the changes are applied :S.
                }


                message = "Profile image set";
            }

            return ret;
        }

        public bool ClearProfileImage(IIdentityManagementService2 ims2, string identity, out string message)
        {
            bool ret = true;
            message = string.Empty;

            TeamFoundationIdentity i = ims2.ReadIdentity(IdentitySearchFactor.AccountName, identity, MembershipQuery.Direct, ReadIdentityOptions.None);

            if (i == null)
            {
                message = "User/Group [" + identity + "] not found";
                ret = false;
            }

            if (ret)
            {
                i.SetProperty("Microsoft.TeamFoundation.Identity.Image.Data", null);
                i.SetProperty("Microsoft.TeamFoundation.Identity.Image.Type", null);
                i.SetProperty("Microsoft.TeamFoundation.Identity.Image.Id", null);
                i.SetProperty("Microsoft.TeamFoundation.Identity.CandidateImage.Data", null);
                i.SetProperty("Microsoft.TeamFoundation.Identity.CandidateImage.UploadDate", null);

                try
                {
                    ims2.UpdateExtendedProperties(i);
                }
                catch (PropertyServiceException)
                {
                    // swallow; this exception happens each and every time, but the changes are applied :S.
                }

                message = "Profile image cleared";
            }

            return ret;
        }

        private static byte[] ConvertAndResizeImage(byte[] bytes)
        {
            if ((bytes == null) || (bytes.Length < 1))
            {
                throw new ArgumentException("The file could not be found.");
            }

            if (bytes.Length > 0x400000)
            {
                throw new ArgumentException("The file is too large to be used as profile image.");
            }

            using (var imageStream = new MemoryStream(bytes))
            using (Image image = Image.FromStream(imageStream))
            {
                int width = 0x90;
                int height = 0x90;
                if (image.Height > image.Width)
                {
                    width = (0x90 * image.Width) / image.Height;
                }
                else
                {
                    height = (0x90 * image.Height) / image.Width;
                }

                int x = (0x90 - width) / 2;
                int y = (0x90 - height) / 2;
                using (Bitmap bitmap = new Bitmap(0x90, 0x90))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.DrawImage(image, x, y, width, height);
                    }

                    using (MemoryStream stream = new MemoryStream())
                    {
                        bitmap.Save(stream, ImageFormat.Png);
                        return stream.ToArray();
                    }
                }
            }
        }

    }
}
