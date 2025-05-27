#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEssentials
{
    [ExecuteAlways]
    public class ProbeReferenceVolumeProvider : MonoBehaviour
    {
        public static ProbeReferenceVolume Volume { get; private set; }
        public static bool IsInitialized { get; private set; } = false;

        private static Action s_onInitialize;

        public void Update()
        {
            Volume ??= ProbeReferenceVolume.instance;

            if (Volume.isInitialized && Volume.currentBakingSet != null && !IsInitialized)
            {
                IsInitialized = true;
                s_onInitialize?.Invoke();
            }
        }

        public static void AddListener(Action action)
        {
            if (!IsInitialized)
                s_onInitialize = action;
            else action();
        }
    }
}
#endif