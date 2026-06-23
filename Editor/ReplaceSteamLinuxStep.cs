using System.IO;
using System.IO.Compression;
using Cysharp.Threading.Tasks;
using Nomnom.UnityProjectPatcher.Editor.Steps;
using UnityEditor;
using UnityEngine;

namespace Kesomannen.RepoProjectPatcher.Editor {
    public readonly struct ReplaceSteamLinuxStep : IPatcherStep {
        public UniTask<StepResult> Run() {
            var pluginsPath = Path.Combine(Application.dataPath, "REPO/Plugins");
            var zipPath = Path.GetFullPath($"Packages/{Constants.PackageName}/Replacements/Facepunch.Steamworks.zip");
            var destDir = Path.Combine(pluginsPath, "Facepunch.Steamworks");

            string[] winAssemblies = {
                "Facepunch.Steamworks.Win64.dll",
                "Facepunch.Steamworks.Win64.dll.meta",
                "steam_api64.dll",
                "steam_api64.dll.meta"
            };

            bool changesMade = false;

            if (File.Exists(zipPath)) {
                if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
                Directory.CreateDirectory(destDir);
                ZipFile.ExtractToDirectory(zipPath, destDir);
                changesMade = true;
            }

            // Shouldn't be used but works so i dont really care rn
            AssetDatabase.StartAssetEditing();
            try {
                foreach (var fileName in winAssemblies) {
                    string fullPath = Path.Combine(pluginsPath, fileName);
                    if (File.Exists(fullPath)) {
                        File.Delete(fullPath);
                        changesMade = true;
                        Debug.Log($"Deleted: {fileName}");
                    }
                }
            } finally {
                // same for this
                AssetDatabase.StopAssetEditing();
            }

            if (changesMade) {
                AssetDatabase.Refresh();
                return UniTask.FromResult(StepResult.RestartEditor);
            }
            return UniTask.FromResult(StepResult.Success);
        }

        public void OnComplete(bool failed) { }
    }
}