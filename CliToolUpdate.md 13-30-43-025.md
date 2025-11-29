Updating & Installing a Local Version of Cruddy.Cli

1. Build the CLI project

dotnet pack -c Release

The .nupkg file will be created in:

Cruddy/Cruddy.Cli/bin/Release/

⸻

2. Verify proper .nupkg file naming

The file must be named:

Cruddy.Cli..nupkg

Example:

Cruddy.Cli.0.1.1.nupkg

If needed, rename it:

mv cruddy.1.1.1.nupkg Cruddy.Cli.0.1.1.nupkg

⸻

3. Uninstall the old Cruddy CLI tool

Global uninstall:

dotnet tool uninstall Cruddy.Cli –global

Local uninstall:

dotnet tool uninstall Cruddy.Cli

Check:
dotnet tool list –global

⸻

4. Install the new CLI version from local source

dotnet tool install Cruddy.Cli 
–global 
–add-source /Users/tahirtahirli/Desktop/Projects/Cruddy/Cruddy/Cruddy.Cli/bin/Release 
–version 0.1.1

Local install:

dotnet tool install Cruddy.Cli 
–add-source /Users/tahirtahirli/Desktop/Projects/Cruddy/Cruddy/Cruddy.Cli/bin/Release 
–version 0.1.1

⸻

5. Verify tool version

dotnet cruddy –version

Expected:
0.1.1

⸻

6. If installation fails

Check that the file exists:

ls /Users/tahirtahirli/Desktop/Projects/Cruddy/Cruddy/Cruddy.Cli/bin/Release

It must contain:

Cruddy.Cli.0.1.1.nupkg

⸻

Done

You now have the updated local Cruddy.Cli tool installed.
