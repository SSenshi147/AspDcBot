# Slash commands

# CI/CD notes
[linux service config](https://swimburger.net/blog/dotnet/how-to-run-a-dotnet-core-console-app-as-a-service-using-systemd-on-linux)
## bottest.service
```
# location: /etc/systemd/system/bottest.service
[Unit]
Description=Foo Bar Service
[Service]
WorkingDirectory=/home/marci/test/linux-x64
ExecStart=/home/marci/test/linux-x64/DonDumbledore.ConsoleHost
Restart=always
RestartSec=10
SyslogIdentifier=DonDumbledore
User=marci
Environment=DOTNET_ENVIRONMENT=Development

[Install]
WantedBy=multi-user.target
```
## terminal commands used
```
# reload service configs
sudo systemctl daemon-reload

# enable service start on boot
sudo systemctl enable bottest

# view logs
systemctl status bottest
journalctl -u bottest.service -f
```
## jenkins sudo user
```
sudo visudo

# append:
jenkins ALL=(ALL) NOPASSWD: ALL
```
## jenkins job build steps
```
sudo dotnet build DonDumbledore.ConsoleHost/DonDumbledore.ConsoleHost.csproj /p:PublishProfile=DonDumbledore.ConsoleHost/Properties/PublishProfiles/LinuxX64.pubxml
sudo systemctl stop bottest
sudo dotnet publish DonDumbledore.ConsoleHost/DonDumbledore.ConsoleHost.csproj /p:PublishProfile=DonDumbledore.ConsoleHost/Properties/PublishProfiles/LinuxX64.pubxml -o /home/marci/test/linux-x64
sudo systemctl start bottest
```