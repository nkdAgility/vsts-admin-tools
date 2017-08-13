$toolsLoc = Get-ToolsLocation
$$vstsadmintoolspath =Join-Path -Path $toolsLoc -ChildPath "\VstsAdminTools"

Uninstall-ChocolateyZipPackage '$vstsadmintools' 'vstsadmintools-#{GITVERSION.FULLSEMVER}#.zip'

write-host 'VSTS Sync Migration has been uninstalled.'
