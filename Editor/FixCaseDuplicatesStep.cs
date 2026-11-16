using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Nomnom.UnityProjectPatcher.Editor;
using Nomnom.UnityProjectPatcher.Editor.Steps;
using UnityEngine;

namespace Kesomannen.RepoProjectPatcher.Editor {
    public readonly struct FixCaseDuplicatesStep : IPatcherStep {
        public UniTask<StepResult> Run() {
            var exportPath = this.GetAssetRipperSettings().OutputExportAssetsFolderPath;
            var count = 0;

            foreach (var folder in Directory.GetDirectories(exportPath)) {
                var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var duplicates = new List<string>();

                foreach (var file in Directory.GetFiles(folder)) {
                    if (file.EndsWith(".meta")) continue;

                    var fileName = Path.GetFileName(file);
                    if (usedNames.Contains(fileName)) {
                        duplicates.Add(file);
                    } else {
                        usedNames.Add(fileName);
                    }
                }

                foreach (var file in duplicates) {
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
            return UniTask.FromResult(StepResult.Success);
        }

        public void OnComplete(bool failed) { }
    }
}