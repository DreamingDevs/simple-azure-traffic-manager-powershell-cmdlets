simple-azure-traffic-manager-powershell-cmdlets
===============================================

This project builds a simple DLL written in C# to offer Azure Traffic Manager cmdlets. As we all know Traffic manager got no support through Azure PowerShell cmdlets (at least until recent 0.7.2 version).

So we made simple REST API calls in the DLL and then use System.Management.Automation to build basic cmdlets.

Anybody who want to use these cmdlets can simply dowload the source code and build it. Get the DLL and import it to PowerShell context and enjoy the cmdlets.
