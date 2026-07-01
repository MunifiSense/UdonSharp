using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace UdonSharpEditor
{
    /// <summary>
    /// Deletes the old UdonSharp files at the first opportunity, this is to prevent conflicts with the new UdonSharp package
    /// </summary>
    [InitializeOnLoad]
    internal static class AutoDelete
    {
        private const string VrcWorldsPackageName = "com.vrchat.worlds";
        private const string IntegrationFolderAssetPath = "Packages/com.vrchat.worlds/Integrations/UdonSharp";
        private const string IntegrationMetaAssetPath = "Packages/com.vrchat.worlds/Integrations/UdonSharp.meta";

        private static bool deleteScheduled;

        static AutoDelete()
        {
            Events.registeredPackages += OnPackagesRegistered;
            EditorApplication.delayCall += TryDeleteIntegration;
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            ScheduleDelete();
        }

        private static void OnPackagesRegistered(PackageRegistrationEventArgs args)
        {
            if (!HasVrcWorldsPackageChange(args))
                return;

            ScheduleDelete();
        }

        private static bool HasVrcWorldsPackageChange(PackageRegistrationEventArgs args)
        {
            for (int i = 0; i < args.added.Count; i++)
            {
                if (args.added[i].name == VrcWorldsPackageName)
                    return true;
            }

            for (int i = 0; i < args.changedTo.Count; i++)
            {
                if (args.changedTo[i].name == VrcWorldsPackageName)
                    return true;
            }

            return false;
        }

        private static void ScheduleDelete()
        {
            if (deleteScheduled)
                return;

            deleteScheduled = true;
            EditorApplication.delayCall += TryDeleteIntegration;
        }

        private static void TryDeleteIntegration()
        {
            deleteScheduled = false;

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string folderFullPath = Path.Combine(projectRoot, IntegrationFolderAssetPath);
            string metaFullPath = Path.Combine(projectRoot, IntegrationMetaAssetPath);
            bool deletedAnything = false;

            if (Directory.Exists(folderFullPath))
            {
                Directory.Delete(folderFullPath, true);
                deletedAnything = true;
            }

            if (File.Exists(metaFullPath))
            {
                File.Delete(metaFullPath);
                deletedAnything = true;
            }

            if (!deletedAnything)
                return;

            Debug.Log("[<color=#0c824c>UdonSharp</color>] Found and deleted SDK copy of UdonSharp to prevent conflicts with the new UdonSharp package");
            AssetDatabase.Refresh();
        }
    }
}
