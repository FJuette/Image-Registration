// include Fake lib
#r @"packages\FAKE\tools\FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile

RestorePackages()

// Directories
let buildDir  = @".\build\"
let deployDir = @".\deploy\"
let packagesDir = @".\packages"

// version info
let version = "0.1"

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; deployDir]
)

Target "CompileApp" (fun _ ->
    !! @"*.csproj"
      |> MSBuildRelease buildDir "Build"
      |> Log "AppBuild-Output: "
)

Target "Zip" (fun _ ->
    !! (buildDir + "\**\*.*")
    -- (buildDir + "\**\*.pdb")
    -- (buildDir + "\**\*nunit*.*")
    -- (buildDir + "\**\*.xml")
        |> Zip buildDir (deployDir + "Image Registration." + version + ".zip")
)

// Dependencies
"Clean"
  ==> "CompileApp"
  ==> "Zip"

// start build
RunTargetOrDefault "Zip"