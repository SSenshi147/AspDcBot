# Slash commands

# CI/CD notes
[linux service config](https://swimburger.net/blog/dotnet/how-to-run-a-dotnet-core-console-app-as-a-service-using-systemd-on-linux)
## ubuntu install
```
sudo apt install default-jre
sudo apt install dotnet-sdk-8.0
sudo apt install git
```
## bottest.service
```
# location: /etc/systemd/system/bottest.service
[Unit]
Description=DonDumbledore - PROD
[Service]
WorkingDirectory=/home/marci/prod/linux-x64
ExecStart=/home/marci/prod/linux-x64/DonDumbledore.ConsoleHost $SCRIPT_ARGS
Restart=always
RestartSec=10
SyslogIdentifier=DonDumbledore
User=marci
Environment=DOTNET_ENVIRONMENT=Production
Environment="SCRIPT_ARGS=%I"

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

https://superuser.com/questions/728951/systemd-giving-my-service-multiple-arguments
https://askubuntu.com/questions/1077778/how-do-i-pass-flags-when-starting-a-systemd-service
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
sudo systemctl stop dondumbledore-prod@"*".service
sudo dotnet publish DonDumbledore.ConsoleHost/DonDumbledore.ConsoleHost.csproj /p:PublishProfile=DonDumbledore.ConsoleHost/Properties/PublishProfiles/LinuxX64.pubxml -o /home/marci/prod/linux-x64
sudo systemctl start dondumbledore-prod@"${regNew}".service
```
## jenkins
[select branches](https://www.baeldung.com/ops/jenkins-git-branch-selection)
active choices plugin
active choices reactive job parameter
```
def gitUrl = "https://github.com/SSenshi147/AspDcBot.git"
def gitBranches = "git ls-remote --heads ${gitUrl}".execute().text.readLines().collect { it.split()[1].replaceAll("refs/heads/", "") }.sort().reverse()
```
name it branch, add itt as `${branch}` to job setting "build branches"