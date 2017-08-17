# Usage of Export Azure AD

Exports a csv of [Team Project], [Application Group], [Account], [Mail], [Alias] from yoru TFS/VSTS server.

**WARNING: Not fully tested

## Usage

VstsAdminTools exportAzureAD --outpath c:\temp\out\extract.csv --collection https://tfs.nkdagility.com:8080/tfs

VstsAdminTools exportAzureAD --outpath c:\temp\out\extract.csv --collection https://tfs.nkdagility.com:8080/tfs --teamproject "nkdProjects"
