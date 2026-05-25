using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Nomnom.UnityProjectPatcher.Editor.Steps;
using UnityEditor;
using UnityEngine;

namespace Kesomannen.RepoProjectPatcher.Editor {
    public struct ReplaceAssetsStep : IPatcherStep {
        [MenuItem("Tools/R.E.P.O. Project Patcher/Patch Assets/All")]
        static void MenuItem() {
            ReplaceAssets(null);
        }

        [MenuItem("Tools/R.E.P.O. Project Patcher/Patch Assets/Scripts")]
        static void MenuItemScripts() {
            ReplaceAssets(".cs");
        }

        [MenuItem("Tools/R.E.P.O. Project Patcher/Patch Assets/Shaders")]
        static void MenuItemShaders() {
            ReplaceAssets(".shader");
        }

        static void ReplaceAssets(string extensionFilter) {
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
                    var ext = Path.GetExtension(srcPath);
                    if (ext == ".meta") continue;
                    if (!string.IsNullOrEmpty(extensionFilter) && ext != extensionFilter) continue;

                    var relativePath = Path.GetRelativePath(customFolder, srcPath);
                    var destPath = Path.Combine(gameFolder, relativePath).Replace("EditorScripts", "Editor");
                    var destDir = Path.GetDirectoryName(destPath);

                    if (!Directory.Exists(destDir)) {
                        Directory.CreateDirectory(destDir);
                    }

                    File.Copy(srcPath, destPath, overwrite: true);
                    count++;
                }

                Debug.Log($"Copied {count} replacement assets" + (extensionFilter != null ? $" ({extensionFilter})" : ""));
            } finally {
                AssetDatabase.StopAssetEditing();
                if (count > 0) {
                    AssetDatabase.Refresh();
                }
            }
        }

        public UniTask<StepResult> Run() {
            ReplaceAssets(null);
            return UniTask.FromResult(StepResult.Success);
        }

        public void OnComplete(bool failed) { }
    }
}