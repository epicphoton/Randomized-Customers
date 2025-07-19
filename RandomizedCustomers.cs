using BepInEx;
using BepInEx.Logging;

using System.Linq;

using HarmonyLib;
using BepInEx.Configuration;
using CC;
using System.Collections.Generic;
using System;
using UnityEngine;
using ExtensionMethods;

namespace RandomizedCustomers;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Card Shop Simulator.exe")]
public class RandomizedCustomers : BaseUnityPlugin
{

    [HarmonyPatch(typeof(Customer), "ActivateCustomer")]
    private class Customer_Randomizer
    {
        [HarmonyPostfix]
        private static void ChangeHairAndApparel(ref Customer __instance)
        {
            if (!modEnabled.Value) return;
            CharacterCustomization cc = __instance.m_CharacterCustom;
            LODGroup[] enableGroups = cc.GetComponentsInChildren<LODGroup>(true);

            if (!__instance.name.Contains("hq_") && hqCustomers.Value)
            {
                Mesh replaceBodyMesh;
                if (__instance.m_IsFemale)
                {
                    replaceBodyMesh = Instance.hqFemale;
                }
                else
                {
                    replaceBodyMesh = Instance.hqMale;
                }
                if (replaceBodyMesh != null)
                {
                    LOD[] lods = cc.GetComponent<LODGroup>().GetLODs();
                    // Pick up mesh above slot
                    for (int i = lods.Length - 1; i > 0; i--)
                    {
                        ((SkinnedMeshRenderer)lods[i].renderers[0]).sharedMesh = ((SkinnedMeshRenderer)lods[i - 1].renderers[0]).sharedMesh;
                        ((SkinnedMeshRenderer)lods[i].renderers[0]).materials = ((SkinnedMeshRenderer)lods[i - 1].renderers[0]).materials;
                        ((SkinnedMeshRenderer)lods[i].renderers[0]).material = ((SkinnedMeshRenderer)lods[i - 1].renderers[0]).material;
                    }
                    ((SkinnedMeshRenderer)lods[0].renderers[0]).sharedMesh = replaceBodyMesh;

                    // Swaps materials to order required for LOD0
                    Material[] shuffleMats = new Material[6];
                    SkinnedMeshRenderer rend = (SkinnedMeshRenderer)lods[0].renderers[0];
                    shuffleMats[0] = rend.materials[2];
                    shuffleMats[1] = rend.materials[5];
                    shuffleMats[2] = rend.materials[4];
                    shuffleMats[3] = rend.materials[0];
                    shuffleMats[4] = rend.materials[1];
                    shuffleMats[5] = rend.materials[3];
                    rend.materials = shuffleMats;
                    __instance.name = "hq_" + __instance.name;
                }
            }

            int hairSlotCount = cc.HairTables.Count;

            if (randomizedCustomers.Value || hqHair.Value)
            {
                // setHair
                for (int i = 0; i < hairSlotCount; i++)
                {
                    string hairKey = "";
                    if (randomizedCustomers.Value)
                    {
                        int styleMin = 0;
                        int styleMax = cc.HairTables[i].Hairstyles.Count;
                        if (i == 0 && __instance.m_IsFemale)
                        {
                            if (UnityEngine.Random.Range(0f, 1f) > 0.1)
                            {
                                styleMin = 1;
                            }
                        }
                        if (i == 1)
                        {
                            if (UnityEngine.Random.Range(0f, 1f) > 0.05)
                            {
                                styleMin = 1;
                            }
                        }

                        int styleSelection = UnityEngine.Random.RandomRangeInt(styleMin, styleMax);
                        hairKey = cc.HairTables[i].Hairstyles[styleSelection].Name;
                        Logger.LogInfo($"Setting Hair: {i}, {styleSelection}, {cc.gameObject.name}, {hairKey}");
                        cc.setHair(styleSelection, i);
                    }

                    if (hqHair.Value)
                    {
                        // Replace hair mesh
                        GameObject hairObject = cc.GetHairObjects()[i];
                        if (hairObject != null)
                        {
                            LODGroup lodGroup = hairObject.GetComponentInChildren<LODGroup>();

                            string meshName = ((SkinnedMeshRenderer)lodGroup.GetLODs()[0].renderers[0]).sharedMesh.name;
                            string translatedKey = "";

                            if (hairKey == "" && !meshName.Contains("LOD0") && meshName.Contains("LOD"))
                            {
                                hairKey = meshName.Remove(meshName.Length - 1, 1) + "0";
                                translatedKey = hairKey;
                            }
                            if (!meshName.Contains("LOD0") && (hqHairTranslate.ContainsKey(hairKey) || Instance.hqHairDict.ContainsKey(hairKey)))
                            {
                                string[] meshSplit = meshName.Split("_");
                                if (hqHairTranslate.ContainsKey(hairKey) && translatedKey == "")
                                {
                                    if (hqHairTranslate[hairKey].Count() > 1)
                                    {
                                        if (__instance.m_IsFemale)
                                        {
                                            translatedKey = hqHairTranslate[hairKey][1];
                                        }
                                        else
                                        {
                                            translatedKey = hqHairTranslate[hairKey][0];
                                        }
                                    }
                                    else
                                    {
                                        translatedKey = hqHairTranslate[hairKey][0];
                                    }
                                }

                                Logger.LogInfo($"Replacing hair {meshName} with {translatedKey}");
                                if (Instance.hqHairDict.ContainsKey(translatedKey))
                                {

                                    ((SkinnedMeshRenderer)lodGroup.GetLODs()[0].renderers[0]).sharedMesh = Instance.hqHairDict[translatedKey];

                                }
                            }
                        }
                    }
                }
            }



            if (randomizedCustomers.Value)
            {
                int colorIndex = UnityEngine.Random.RandomRangeInt(0, hairColors.Length);

                for (int i = 0; i < hairSlotCount; i++)
                {
                    CC_Property hairColor = cc.StoredCharacterData.HairColor[i];
                    Color color;
                    if (ColorUtility.TryParseHtmlString("#" + hairColors[colorIndex], out color))
                    {
                        cc.setHairColor(hairColor, color, i);
                    }
                }

                // setApparel
                for (int i = 0; i < cc.ApparelTables.Count; i++)
                {
                    int appMin = 0;
                    if (normalClothingMode.Value && cc.ApparelTables[i].Items[0].Name == "None") appMin++;
                    if (normalClothingMode.Value && (i == 1 || (i == 0 && __instance.m_IsFemale))) appMin++;
                    int appMax = cc.ApparelTables[i].Items.Count;
                    int appSelection = UnityEngine.Random.RandomRangeInt(appMin, appMax);
                    int mat = UnityEngine.Random.RandomRangeInt(0, cc.ApparelTables[i].Items[appSelection].Materials.Count);
                    cc.setApparel(appSelection, i, mat);

                    List<GameObject> apparelObjects = cc.GetApparelObjects();
                }
            }

            if (hqClothing.Value)
            {
                // Attempt to upgrade apparel LODs
                foreach (GameObject apparel in cc.GetApparelObjects())
                {
                    Logger.LogInfo($"Setting LOD of {apparel.name}");
                    LODGroup group = apparel.GetComponent<LODGroup>();
                    if (group == null) continue;
                    if (group.GetLODs()[0].renderers[0].name.Contains("LOD0"))
                    {
                        group.GetLODs()[0].renderers[0].gameObject.SetActive(true);
                        group.GetLODs()[0].renderers[0].enabled = true;
                        continue;
                    }
                    // Find LOD0
                    SkinnedMeshRenderer zeroRenderer = null;
                    foreach (SkinnedMeshRenderer renderer in apparel.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    {
                        if (renderer.name.Contains("LOD0"))
                        {
                            zeroRenderer = renderer;
                            break;
                        }
                    }

                    if (zeroRenderer == null) break;
                    LOD[] lods = group.GetLODs();
                    if (zeroRenderer.bones.Count() != ((SkinnedMeshRenderer)lods[0].renderers[0]).bones.Count()) break;

                    // Pick up mesh above slot
                    for (int i = lods.Length - 1; i > 0; i--)
                    {
                        ((SkinnedMeshRenderer)lods[i].renderers[0]).sharedMesh = ((SkinnedMeshRenderer)lods[i - 1].renderers[0]).sharedMesh;
                    }

                    ((SkinnedMeshRenderer)lods[0].renderers[0]).sharedMesh = zeroRenderer.sharedMesh;
                }
            }

            if (randomizedCustomers.Value)
            {
                randomizer.randomizeAll(cc);
            }
        }
    }

    internal static new ManualLogSource Logger;

    private Harmony harmony;
    private static ConfigEntry<bool> modEnabled;
    private static ConfigEntry<bool> normalClothingMode;
    private static ConfigEntry<bool> hqClothing;
    private static ConfigEntry<bool> hqHair;

    private static ConfigEntry<bool> hqCustomers;
    private static ConfigEntry<bool> randomizedCustomers;

    private Mesh hqFemale;
    private Mesh hqMale;

    private static CCRandomizer randomizer;

    public Dictionary<string, Mesh> hqHairDict = new();


    internal static RandomizedCustomers Instance;

    private void Awake()
    {
        Instance = this;
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        modEnabled = Config.Bind<bool>("", "Enable Mod", true, "Enable or Disable. Reload to return to default customers.");

        hqCustomers = Config.Bind<bool>("Customer Options", "Enable High Quality Customers", true, "Swaps customer models to higher quality models. Game reload required to disable.");
        hqHair = Config.Bind<bool>("Customer Options", "Enable High Quality Hair", true, "Swaps many hairstyles to a higher quality model. Many are not working at the moment and remain lower quality.");
        randomizedCustomers = Config.Bind<bool>("Customer Options", "Randomize Customers", true, "Randomizes every customer that comes to the store. Randomizes face and body appearance, hair, and clothes. Reload to return to default customers.");

        hqClothing = Config.Bind<bool>("Clothing Options", "Enable High Quality Clothing", true, "Swaps most clothing to higher quality models.");
        normalClothingMode = Config.Bind<bool>("Clothing Options", "Normal Clothes Only", true, "Makes sure that customers only wear relatively normal clothes. \n IF YOU TURN THIS OFF, CUSTOMERS WILL SPAWN IN THEIR UNDERWEAR. Disable at your own risk. \n This mod adds no additional clothing, only selects from what is already available in the game's files.");

        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        randomizer = new CCRandomizer(Logger);
    }

    private void Start()
    {
        CEventManager.AddListener<CEventPlayer_GameDataFinishLoaded>(OnGameDataFinishLoaded);

    }

    private void OnGameDataFinishLoaded(CEventPlayer_GameDataFinishLoaded evt)
    {
        hqFemale = Resources.FindObjectsOfTypeAll<Mesh>().Where(x => x.name == "Female_Combined_LOD0").ToArray()[0];
        hqMale = Resources.FindObjectsOfTypeAll<Mesh>().Where(x => x.name == "Male_Combined_LOD0").ToArray()[0];

        // Hair Archive
        foreach (Mesh hairMesh in Resources.FindObjectsOfTypeAll<Mesh>().Where(x => hqHairNames.Contains(x.name)))
        {
            hqHairDict[hairMesh.name] = hairMesh;
        }

        Logger.LogInfo($"Hair dict built: {string.Join(", ", hqHairDict.Keys)}");

        Logger.LogWarning($"Quality Settings Test: {QualitySettings.antiAliasing}, {QualitySettings.maximumLODLevel}");
        // QualitySettings.antiAliasing = 8;
    }

    private static string[] hairColors
    {
        get
        {
            return
                [
                    "a2826d",
                    "d19f7e",
                    "733a2f",
                    "6f4a2f",
                    "785c4e",
                    "734835",
                    "906145",
                    "502922",
                    "7f674d",
                    "a4846b",
                    "a18b66",
                    "97714a",
                    "b89669",
                    "b5a180",
                    "be9f6d",
                    "bca689",
                    "b39573",
                    "7c6b54",
                    "654835",
                    "947a68",
                    "e0d0b9",
                    "2d221c",
                    "542217",
                    "785630",
                    "8a704b",
                    "a48e67",
                    "3a1413",
                    "413026",
                    "c9aa95",
                    "5f3d22",
                    "db0646",
                    "ea19d2",
                    "fe6603",
                    "dfb900",
                    "90bd0b",
                    "069898",
                    "037ced",
                    "4e0047",
                    "830404",
                    "782525"
                ];
        }
    }

    private static Dictionary<string, string[]> hqHairTranslate => new Dictionary<string, string[]>()
            {
                {"Bob_01", ["Hair_Bob_LOD0"]},
                {"Side_Part_02", ["Side_Part_02_LOD0"]},
                {"Long_01", ["Long_01_Male_LOD0", "Hair_Long_01_F_LOD0"]},
                {"Afro_Curl_01", ["Hair_Afro_Curl_01_Male_LOD0", "Hair_Afro_Curl_01_Female_LOD0"]},
                {"Long_Fringe_02", ["Long_Fringe_02_LOD0"]},
                {"Ponytail_01", ["Ponytail_01_LOD0"]},
                {"Ponytail_02", ["Ponytail_02_LOD0"]},
                {"Short_Middle_Part_01", ["Short_Middle_Part_01_Male_LOD0", "Short_Middle_Part_01_Female_LOD0"]},
                {"Wavey_01", ["Wavey_01_LOD0"]},
                {"Receded_01", ["Receded_01_LOD0"]}
            };

    private static string[] hqHairNames => [
        "Hair_Bob_LOD0",
        "Side_Part_02_LOD0",
        "Long_01_Male_LOD0",
        "Hair_Long_01_F_LOD0",
        "Long_Fringe_02_LOD0",
        "Wavey_01_LOD0",
        "Ponytail_01_LOD0",
        "Ponytail_02_LOD0",
        //"Hair_Afro_Curl_01_Male_LOD0", // These hairstyles don't work with a simple replacement
        "Hair_Afro_Curl_01_Female_LOD0",
        //"Short_Middle_Part_01_Female_LOD0", // These hairstyles don't work with a simple replacement
        "Short_Middle_Part_01_Male_LOD0",
        "Receded_01_LOD0",
    ];
}
