dotnet pack -c Release

dotnet tool uninstall Cruddy.Cli

dotnet tool install Cruddy.Cli \
  --add-source /Users/tahirtahirli/Desktop/Projects/Cruddy/Cruddy/Cruddy.Cli/bin/Release \
  --version 0.1.1