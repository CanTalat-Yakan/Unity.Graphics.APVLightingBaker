#if UNITY_EDITOR
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace UnityEssentials
{
    [ExecuteAlways]
    [RequireComponent(typeof(ProbeReferenceVolumeProvider))]
    public class APVBaker : MonoBehaviour
    {
        public static bool IsLightmappingInProgress => Lightmapping.isRunning;

        [Button]
        public bool BakeLightingScenario(string scenarioName, bool async = false)
        {
            if (IsLightmappingInProgress)
                return false;

            if (string.IsNullOrEmpty(scenarioName))
                UnityEngine.Debug.LogWarning("Scenario name cannot be null or empty.");

            if (!AddAndApplyLightingScenario(scenarioName))
            {
                UnityEngine.Debug.LogWarning($"Failed to add or apply lighting scenario '{scenarioName}'. " +
                    $"Ensure the Probe Reference Volume is initialized and a baking set is created.");
                return false;
            }

            var stopwatch = Stopwatch.StartNew();
            var result = async ? Lightmapping.BakeAsync() : Lightmapping.Bake();
            stopwatch.Stop();

            if (result)
                UnityEngine.Debug.Log($"Successfully baked scenario '{scenarioName}' in {stopwatch.Elapsed.TotalSeconds:0.00} seconds.");

            return result;
        }

        public bool AddAndApplyLightingScenario(string scenarioName)
        {
            var bakingSet = ProbeReferenceVolumeProvider.Volume?.currentBakingSet;
            if (bakingSet == null)
            {
                UnityEngine.Debug.LogWarning("No baking set found. Ensure the Probe Reference Volume is initialized and a baking set is created.");
                return false;
            }

            bakingSet.TryAddScenario(scenarioName);
            ProbeReferenceVolumeProvider.Volume.lightingScenario = scenarioName;

            return true;
        }

        [Button]
        public void ConvertAllMeshesToProbeVolumesGI()
        {
            int convertedCount = 0;
            // Iterate through all loaded scenes
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                    continue;

                var rootObjects = scene.GetRootGameObjects();
                foreach (var root in rootObjects)
                {
                    var meshRenderers = root.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (var meshRenderer in meshRenderers)
                    {
                        meshRenderer.receiveGI = ReceiveGI.LightProbes;
                        meshRenderer.lightProbeUsage = LightProbeUsage.UseProxyVolume;
                        EditorUtility.SetDirty(meshRenderer);
                        convertedCount++;
                    }
                }
            }
            UnityEngine.Debug.Log($"Converted {convertedCount} MeshRenderers in loaded scenes to receive GI from Probe Volumes.");
        }
    }
}
#endif