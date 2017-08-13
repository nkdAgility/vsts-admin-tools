$toolsLoc = Get-ToolsLocation
$vstsadmintoolspath =Join-Path -Path $toolsLoc -ChildPath "\VstsAdminTools"

if(test-path $vstsadmintoolspath) {
  write-host "Cleaning out the contents of $vstsadmintoolspath"
  Remove-Item "$($vstsadmintoolspath)\*" -recurse -force
}

Install-ChocolateyZipPackage '$vstsadmintools' 'https://github.com/nkdAgility/vsts-admin-tools/releases/download/#{GITVERSION.FULLSEMVER}#/vstsadmintools-#{GITVERSION.FULLSEMVER}#.zip' $$vstsadmintoolspath -Checksum #{Chocolatey.FileHash}# -ChecksumType SHA256
write-host 'VSTS Admin Tools has been installed. Call `vstsadmin` from the command line to see options. You may need to close and reopen the command shell.'
