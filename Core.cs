using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(ActualPlates.Core), "ActualPlates", "1.0.0", "Catalyss", null)]
[assembly: MelonGame("Pigeons at Play", "Mycopunk")]

namespace ActualPlates
{
    public class Core : MelonMod
    {
        private GameObject replacementPlate;
        private static Il2CppAssetBundle persistentBundle;

        public override void OnInitializeMelon()
        {

        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            LoggerInstance.Msg($"Scene loaded: {sceneName}");

            if (sceneName == "Players")
            {
                LoggerInstance.Msg("ActualPlates initialized!");
                var assembly = typeof(Core).Assembly;
                foreach (string resName in assembly.GetManifestResourceNames())
                    LoggerInstance.Msg("Found resource: " + resName);

                using (Stream stream = assembly.GetManifestResourceStream("ActualPlates.Resources.actualplatesbundle"))
                {
                    if (stream == null)
                    {
                        LoggerInstance.Error("Failed to find embedded AssetBundle!");
                        return;
                    }

                    byte[] bundleData = new byte[stream.Length];
                    stream.Read(bundleData, 0, bundleData.Length);

                    persistentBundle = Il2CppAssetBundleManager.LoadFromMemory(bundleData);


                    if (persistentBundle == null)
                    {
                        LoggerInstance.Error("Failed to load AssetBundle from memory!");
                        return;
                    }
                    foreach (var item in persistentBundle.AllAssetNames())
                    {
                        LoggerInstance.Msg(item.ToString());
                    }
                    replacementPlate = persistentBundle.LoadAsset<GameObject>("assets/mods/actualplates/replacementplate.prefab");

                    if (replacementPlate == null)
                    {
                        LoggerInstance.Error("Failed to load one or more replacement assets!");
                    }
                    else
                    {
                        var holder = new GameObject("ModelHolder");
                        GameObject.DontDestroyOnLoad(holder);

                        // Store the prefabs here so Unity keeps references
                        replacementPlate = GameObject.Instantiate(replacementPlate, holder.transform);
                        replacementPlate.SetActive(false); // hide it

                        LoggerInstance.Msg("Embedded assets loaded successfully.");

                    }
                }
            }
        }

        public override void OnUpdate()
        {
            if (SceneManager.GetActiveScene().name == "Players")
            {
                var launcher = GameObject.Find("PlateLauncherPlate");
                var Plate = GameObject.Find("Plate");

                if (launcher != null)   ReplaceAll("PlateLauncherPlate", replacementPlate);
                if (Plate != null)      ReplaceAll("Plate", replacementPlate);
            }
        }


        private void ReplaceAll(string targetName, GameObject replacement)
        {
            if (replacement == null) return;

            System.Collections.Generic.IEnumerable<UnityEngine.GameObject> originals = GameObject.FindObjectsOfType<GameObject>().Where(obj => obj.name == targetName);

            foreach (var original in originals)
            {
                if (original == null || original.name != targetName) continue;

                // Avoid replacing it more than once
                if (original.GetComponent<SpriteMask>() == null) continue;

                GameObject newObj = GameObject.Instantiate(replacement, original.transform.position, original.transform.rotation);
                newObj.transform.SetParent(original.transform.parent);
                newObj.name = original.name;

                // Optional: Copy over transform/scale/etc.
                newObj.transform.localScale = original.transform.localScale;
                newObj.transform.rotation = original.transform.rotation;
                newObj.transform.position = original.transform.position;
                original.GetComponent<MeshFilter>().mesh = newObj.transform.GetChild(0).GetComponent<MeshFilter>().mesh;
                original.AddComponent<UnityEngine.SpriteMask>();
                GameObject.Destroy(newObj);
            }
        }
    }
}