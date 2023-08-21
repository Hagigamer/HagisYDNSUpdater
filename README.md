# HagisYDNSUpdater

This tool requires .NET Framework 4.8 installed.

Features: Updates one or multiple records on ydns.io in a configurable interval with your external IP address.

After installation, edit the file %programfiles(x86)%\HagisYDNSUpdater\HagisYDNSUpdater.exe.config file.

The following settings are available:

> timerInterval: Update interval in seconds.

> Logging: Either true or false - if true, a Log File is created in %programfiles(x86)%\HagisYDNSUpdater\Logs\

> API User and API Password: The credentials used to update your records on ydns.io. Can be found here: https://ydns.io/user/api

In the host region below, add one (or more) hosts you want to update with your external IP.

Example Hosts section: 

    <Hosts>
      <add key="myhost.ydns.io"/>
      <add key="anotherhost.ydns.io"/>
    </Hosts>

That's it, you can start the windows service now: open the windows service list (services.msc), look for "YDNS Updater" and start it.
The service is configured to automatically start with your computer.
