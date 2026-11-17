using System.IO;
using System.IO.Compression;
using System.Net.Http;
using Cysharp.Threading.Tasks;
using Nomnom.UnityProjectPatcher.Editor.Steps;
using UnityEditor;
using UnityEngine;

namespace Kesomannen.RepoProjectPatcher.Editor {
    public readonly struct ReplaceSteamLinuxStep : IPatcherStep {
        public UniTask<StepResult> Run() {
            var pluginsPath = Path.Combine(Application.dataPath, "REPO/Plugins");
            var metaFolder = Path.Combine(RepoAssets.GetPackageRoot(), "Facepunch.Steamworks~");
            var destDir = Path.Combine(pluginsPath, "Facepunch.Steamworks");

            string[] linuxFiles = {
                "Facepunch.Steamworks.Posix.dll",
                "Facepunch.Steamworks.Posix.pdb",
                "Facepunch.Steamworks.Posix.xml",
                "redistributable_bin/linux64/libsteam_api.so",
                "redistributable_bin/steam_api.lib"
            };

            string[] winAssemblies = {
                "Facepunch.Steamworks.Win64.dll",
                "Facepunch.Steamworks.Win64.dll.meta",
                "steam_api64.dll",
                "steam_api64.dll.meta"
            };

            using var client = new HttpClient();
            var bytes = client.GetByteArrayAsync("https://github.com/Facepunch/Facepunch.Steamworks/releases/download/2.5.2/Facepunch.Steamworks.2.5.2.zip").GetAwaiter().GetResult();
            using var zip = new ZipArchive(new MemoryStream(bytes));

            if (Directory.Exists(destDir)) Directory.Delete(destDir, true);

            foreach (var file in linuxFiles) {
                var destPath = Path.Combine(destDir, file);
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                zip.GetEntry("Release/Unity/" + file).ExtractToFile(destPath);
            }

            foreach (var metaPath in Directory.GetFiles(metaFolder, "*.meta", SearchOption.AllDirectories)) {
                File.Copy(metaPath, Path.Combine(destDir, Path.GetRelativePath(metaFolder, metaPath)));
            }

            // Shouldn't be used but works so i dont really care rn
            AssetDatabase.StartAssetEditing();
            try {
                foreach (var fileName in winAssemblies) {
                    string fullPath = Path.Combine(pluginsPath, fileName);
                    if (File.Exists(fullPath)) {
                        File.Delete(fullPath);
                        Debug.Log($"Deleted: {fileName}");
                    }
                }
            } finally {
                // same for this
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
            return UniTask.FromResult(StepResult.RestartEditor);
        }

        public void OnComplete(bool failed) { }
    }
}