using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Nomnom.UnityProjectPatcher.Editor;
using Nomnom.UnityProjectPatcher.Editor.Steps;
using UnityEditor;
using UnityEngine;

namespace Kesomannen.RepoProjectPatcher.Editor {
    public readonly struct FixAssetCasingStep : IPatcherStep {
        [MenuItem("Tools/R.E.P.O. Project Patcher/Patch Assets/Fix Asset Casing")]
        static void MenuItem() {
            FixAssetCasing(Directory.GetDirectories("Assets/REPO/Game", "*", SearchOption.AllDirectories));
            AssetDatabase.Refresh();
        }

        public UniTask<StepResult> Run() {
            var exportPath = this.GetAssetRipperSettings().OutputExportAssetsFolderPath;
            FixAssetCasing(Directory.GetDirectories(exportPath));
            return UniTask.FromResult(StepResult.Success);
        }

        public void OnComplete(bool failed) { }

        static void FixAssetCasing(string[] folders) {
            var count = 0;

            foreach (var folder in folders) {
                var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var conflicts = new List<string>();

                foreach (var file in Directory.GetFiles(folder)) {
                    if (file.EndsWith(".meta")) continue;

                    var fileName = Path.GetFileName(file);
                    if (usedNames.Contains(fileName)) {
                        conflicts.Add(file);
                    } else {
                        usedNames.Add(fileName);
                    }
                }

                foreach (var file in conflicts) {
                    var name = Path.GetFileNameWithoutExtension(file);
                    var extension = Path.GetExtension(file);

                    var number = 0;
                    while (usedNames.Contains($"{name}_{number}{extension}")) number++;

                    var newName = $"{name}_{number}{extension}";
                    usedNames.Add(newName);

                    var newPath = Path.Combine(folder, newName);
                    File.Move(file, newPath);
                    File.Move(file + ".meta", newPath + ".meta");
                    count++;
                }
            }

            Debug.Log($"Renamed {count} files for linux case-sensitive filesystem");
        }
    }
}