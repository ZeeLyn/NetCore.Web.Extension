///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var output=Argument<string>("output", "Output");
var projectFile="src/NetCore.Web.Extension/NetCore.Web.Extension.csproj";
var version=Argument<string>("version", XmlPeek(projectFile,"/Project/PropertyGroup/Version"));
var target = Argument<string>("target", "Default");
var release = Argument<bool>("release", true);
var nugetApiKey = Argument<string>("nugetApiKey", null);
var currentBranch = Argument<string>("currentBranch", "");
if(string.IsNullOrEmpty(currentBranch))
{
   IEnumerable<string> branchOutput;
   var exitCode = StartProcess("git", new ProcessSettings {
      Arguments = "branch --show-current",
      RedirectStandardOutput = true,
      Silent = true
   }, out branchOutput);
   if(exitCode != 0)
      throw new Exception("Failed to read the current Git branch.");
   currentBranch = branchOutput.FirstOrDefault() ?? "";
}
var configuration=release?"Release":"Debug";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   // Executed BEFORE the first task.
   Information("Running tasks...");
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("UpdateVersion").DoesForEach(GetFiles(projectFile),(file)=>{
   Information("Update Version:"+file);
   if(XmlPeek(file,"/Project/PropertyGroup/Version") != version)
      XmlPoke(file,"/Project/PropertyGroup/Version",version);
});

Task("Restore").Does(()=>{
   DotNetCoreRestore();
});

Task("Build").Does(()=>{
   DotNetCoreBuild("NetCore.Web.Extension.sln",new DotNetCoreBuildSettings{
      Configuration=configuration
   });
});

Task("CleanPackage").Does(()=>{
   if(DirectoryExists(output))
   {
      //DeleteDirectory(output,true);
   }
});

Task("Pack")
.IsDependentOn("CleanPackage")
.IsDependentOn("UpdateVersion")
.DoesForEach(GetFiles(projectFile),(file)=>{
   DotNetCorePack(file.ToString(),new DotNetCorePackSettings{
      OutputDirectory=output,
      Configuration=configuration,
      MSBuildSettings=new DotNetCoreMSBuildSettings().WithProperty("GeneratePackageOnBuild","false")
   });
});

Task("Push")
.IsDependentOn("Pack")
.Does(()=>{
   var nuGetPushSettings= new NuGetPushSettings {
      Source="https://www.nuget.org/api/v2/package",
      ApiKey=nugetApiKey
   };
   if(currentBranch=="master")
   {
      foreach (var package in GetFiles(output + "/DotNetCore.Web.Extension." + version + ".nupkg"))
      {
         NuGetPush(package,nuGetPushSettings);
      }
   }
   else
   {
      Information("Non-master build. Not publishing to NuGet. Current branch: " + currentBranch);
   }
});

Task("Default")
.IsDependentOn("Restore")
.IsDependentOn("Build")
.Does(() => {

});



RunTarget(target);
