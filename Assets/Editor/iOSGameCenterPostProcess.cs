#if UNITY_IOS

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class iOSGameCenterPostProcess
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(
        BuildTarget buildTarget,
        string pathToBuiltProject
    )
    {
        if (buildTarget != BuildTarget.iOS)
            return;

        string projectPath =
            PBXProject.GetPBXProjectPath(
                pathToBuiltProject
            );

        PBXProject pbxProject =
            new PBXProject();

        pbxProject.ReadFromFile(
            projectPath
        );

        string mainTargetGuid =
            pbxProject.GetUnityMainTargetGuid();

        string entitlementsFile =
            pbxProject.GetBuildPropertyForAnyConfig(
                mainTargetGuid,
                "CODE_SIGN_ENTITLEMENTS"
            );

        if (string.IsNullOrEmpty(entitlementsFile))
        {
            entitlementsFile =
                "XRoad.entitlements";
        }

        ProjectCapabilityManager capabilityManager =
            new ProjectCapabilityManager(
                projectPath,
                entitlementsFile,
                targetGuid: mainTargetGuid
            );

        capabilityManager.AddGameCenter();

        capabilityManager.WriteToFile();
    }
}

#endif