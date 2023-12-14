apt-get install wget
wget https://dot.net/v1/dotnet-install.sh -O $HOME/dotnet-install.sh
export DOTNET_ROOT=$HOME/.dotnet && export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
chmod 777 $HOME/dotnet-install.sh
$HOME/dotnet-install.sh --channel 6.0
dotnet build /var/www/html/app/custom-libs/driversignpdf/DriverSignPDF.csproj
chmod 777 -R /var/www/html/app/custom-libs/driversignpdf/bin/Debug/net6.0