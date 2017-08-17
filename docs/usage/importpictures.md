# Usage of Import Pictures (TFS ONLY)

In TFS you can update the profile picture of all of your users so that they all at least have a picture. You pass this command a folder where you have a set of pictures in the format "domain-username.jpg". This command line tool then picks up each of these pictures and uploads them to the user account represented in the filename. 

**This ONLY work in TFS as the API for VSTS only allows for users to upload thier own pictures.

## Usage

VstsAdminTools importpictures --outpath c:\temp\out --collection https://tfs.nkdagility.com:8080/tfs
