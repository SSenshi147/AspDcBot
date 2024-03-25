# CI/CD notes
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
## jenkins job build steps
```
sudo systemctl stop bottest
sudo dotnet clean
sudo dotnet build DonDumbledore.ConsoleHost/DonDumbledore.ConsoleHost.csproj /p:PublishProfile=DonDumbledore.ConsoleHost/Properties/PublishProfiles/LinuxX64.pubxml -o /home/marci/test/linux-x64
sudo systemctl start bottest
```