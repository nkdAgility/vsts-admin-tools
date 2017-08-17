# Usage of Export Pictures

This command downloads all of the users images from a location based on a mapping of the users emplyeeID from Active Directory (AD). It connects to a collection and iterates through each user in the "Valid Users" group.

## Usage Example

VstsAdminTools exportpictures --outpath c:\temp\out --domain nkdagility.com --username mhinshelwood --password Nrx9Va4wE6By --picturemask http://directory.nkdagility.com/misc/pictures/{0}.jpg --collection https://tfs.nkdagility.com:8080/tfs --adPropertyName employeeNumber