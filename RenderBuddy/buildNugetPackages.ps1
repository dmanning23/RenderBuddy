rm *.nupkg
nuget pack .\RenderBuddy.nuspec -IncludeReferencedProjects -Prop Configuration=Release 
cp *.nupkg C:\Projects\Nugets\
nuget push *.nupkg -Source https://www.nuget.org/api/v2/package