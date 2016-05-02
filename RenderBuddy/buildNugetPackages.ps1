nuget pack .\RenderBuddy.nuspec -IncludeReferencedProjects -Prop Configuration=Release
nuget push *.nupkg