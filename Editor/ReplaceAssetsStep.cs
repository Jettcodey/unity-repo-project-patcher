using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Nomnom.UnityProjectPatcher.Editor.Steps;
using UnityEditor;
using UnityEngine;

namespace Kesomannen.RepoProjectPatcher.Editor {
    public struct ReplaceAssetsStep : IPatcherStep {
        [MenuItem("Tools/R.E.P.O. Project Patcher/Replace Additional Assets")]
        static void MenuItem() {
            ReplaceAssets();
        }

        static void ReplaceAssets() {
            var gameFolder = "Assets/REPO";
            var customFolder = Path.GetFullPath($"Packages/{Constants.PackageName}/Replacements");

            if (!Directory.Exists(gameFolder)) {
                Debug.LogWarning($"Game folder not found at: {gameFolder}");
                return;
            }

            if (!Directory.Exists(customFolder)) {
                Debug.LogWarning($"Replacements folder not found at: {customFolder}");
                return;
            }

            AssetDatabase.StartAssetEditing();
            var count = 0;

            try {
                var assets = Directory.GetFiles(customFolder, "*", SearchOption.AllDirectories);
                foreach (var srcPath in assets) {
                    if (Path.GetExtension(srcPath) == ".meta") continue;

                    var relativePath = Path.GetRelativePath(customFolder, srcPath);
                    var destPath = Path.Combine(gameFolder, relativePath).Replace("EditorScripts", "Editor");
                    var destDir = Path.GetDirectoryName(destPath);

                    if (!Directory.Exists(destDir)) {
                        Directory.CreateDirectory(destDir);
                    }

                    File.Copy(srcPath, destPath, overwrite: true);
                    count++;
                }

                Debug.Log($"Copied {count} replacement assets");
            } finally {
                AssetDatabase.StopAssetEditing();
                if (count > 0) {
                    AssetDatabase.Refresh();
                }
            }
        }

        public UniTask<StepResult> Run() {
            ReplaceAssets();
            return UniTask.FromResult(StepResult.Success);
        }

        public void OnComplete(bool failed) { }
    }
}