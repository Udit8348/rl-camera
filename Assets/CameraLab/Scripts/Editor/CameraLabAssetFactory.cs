using System.Collections.Generic;
using CameraLab.Content;
using CameraLab.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace CameraLab.Editor
{
    public static class CameraLabAssetFactory
    {
        [MenuItem("CameraLab/Create Default Content Assets")]
        public static void CreateDefaultContentAssets()
        {
            EnsureFolder("Assets/CameraLab/ScriptableObjects");
            EnsureFolder("Assets/CameraLab/ScriptableObjects/Scenes");
            EnsureFolder("Assets/CameraLab/ScriptableObjects/Challenges");
            EnsureFolder("Assets/CameraLab/ScriptableObjects/Presets");

            var content = BuiltinCameraLabContent.Create();
            var createdChallenges = new Dictionary<string, ChallengeConfig>();

            foreach (var challenge in content.challenges)
            {
                var asset = Object.Instantiate(challenge);
                asset.hideFlags = HideFlags.None;
                var path = AssetDatabase.GenerateUniqueAssetPath($"Assets/CameraLab/ScriptableObjects/Challenges/{asset.challengeId}.asset");
                AssetDatabase.CreateAsset(asset, path);
                createdChallenges[asset.challengeId] = asset;
            }

            foreach (var scene in content.scenes)
            {
                var asset = Object.Instantiate(scene);
                asset.hideFlags = HideFlags.None;
                if (asset.challengeConfig != null && createdChallenges.TryGetValue(asset.challengeConfig.challengeId, out var challengeAsset))
                {
                    asset.challengeConfig = challengeAsset;
                }

                var path = AssetDatabase.GenerateUniqueAssetPath($"Assets/CameraLab/ScriptableObjects/Scenes/{asset.sceneId}.asset");
                AssetDatabase.CreateAsset(asset, path);
            }

            var presetAsset = Object.Instantiate(content.cameraPreset);
            presetAsset.hideFlags = HideFlags.None;
            AssetDatabase.CreateAsset(
                presetAsset,
                AssetDatabase.GenerateUniqueAssetPath($"Assets/CameraLab/ScriptableObjects/Presets/{presetAsset.name}.asset"));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = presetAsset;
            Debug.Log("CameraLab default content assets created under Assets/CameraLab/ScriptableObjects.");
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            var parent = assetPath.Substring(0, assetPath.LastIndexOf('/'));
            var folder = assetPath.Substring(assetPath.LastIndexOf('/') + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
