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

            int hairSlotCount = cc.HairTables.Count;

            if (randomizedCustomers.Value)
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
                }

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

                randomizer.randomizeAll(cc);
            }
        }
    }

    [HarmonyPatch(typeof(CustomerManager), "Start")]
    private class CustomerBodyPrefabSwap
    {
        [HarmonyPrefix]
        [HarmonyBefore("devopsdinosaur.tcgshop.custom_customers")]
        private static void SwapCustomerBodyPrefab(CustomerManager __instance)
        {
            if (!modEnabled.Value) return;
            if (hqClothing.Value || hqCustomers.Value || hqHair.Value)
            {
                Instance.UpdateCustomerAssets(__instance);
                List<GameObject> newCustomers = new();
                List<GameObject> oldCustomers = new();
                // Replace existing customers
                for (int i = 0; i < __instance.m_CustomerParentGrp.childCount; i++)
                {
                    GameObject oldCust = __instance.m_CustomerParentGrp.GetChild(i).gameObject;
                    oldCustomers.Add(oldCust);

                    Customer newCust = UnityEngine.Object.Instantiate<Customer>(oldCust.GetComponent<Customer>().m_IsFemale ? __instance.m_CustomerFemalePrefab : __instance.m_CustomerPrefab);
                    newCust.gameObject.name = oldCust.gameObject.name;
                    newCustomers.Add(newCust.gameObject);
                }

                foreach (GameObject oldCustGO in oldCustomers)
                {
                    DestroyImmediate(oldCustGO);
                }

                foreach (GameObject newCust in newCustomers)
                {
                    newCust.transform.parent = __instance.m_CustomerParentGrp;
                }
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

    private static CCRandomizer randomizer;



    internal static RandomizedCustomers Instance;

    private void Awake()
    {
        Instance = this;
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        modEnabled = Config.Bind<bool>("", "Enable Mod", true, "Enable or Disable. Reload after disabling to return to default customers and assets.");

        hqCustomers = Config.Bind<bool>("Customer Options", "Enable High Quality Customers", true, "Swaps customer models to higher quality models. Game reload required to disable.");
        hqCustomers.SettingChanged += UpdateCustomerAssets;
        hqHair = Config.Bind<bool>("Customer Options", "Enable High Quality Hair", true, "Swaps hairstyles to a higher quality model. Game reload required to disable.");
        hqHair.SettingChanged += UpdateCustomerAssets;
        randomizedCustomers = Config.Bind<bool>("Customer Options", "Randomize Customers", true, "Randomizes every customer that comes to the store. Randomizes face and body appearance, hair, and clothes. Reload to return to default customers.");

        hqClothing = Config.Bind<bool>("Clothing Options", "Enable High Quality Clothing", true, "Swaps clothing to higher quality models. Game reload required to disable.");
        hqClothing.SettingChanged += UpdateCustomerAssets;
        normalClothingMode = Config.Bind<bool>("Clothing Options", "Normal Clothes Only", true, "Makes sure that customers only wear relatively normal clothes. \n IF YOU TURN THIS OFF, CUSTOMERS WILL SPAWN IN THEIR UNDERWEAR. Disable at your own risk. \n This mod adds no additional clothing, only selects from what is already available in the game's files.");

        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        randomizer = new CCRandomizer(Logger);
    }

    private void UpdateCustomerAssets(object sender, EventArgs e)
    {
        UpdateCustomerAssets(GameObject.Find("CustomerManager").GetComponent<CustomerManager>());
    }

    private void UpdateCustomerAssets(CustomerManager manager)
    {
        if (hqCustomers.Value)
            UpdateBodyPrefabs(manager);
        if (hqClothing.Value || hqHair.Value)
            UpdateHairApparelPrefabObjects();
    }

    private void UpdateBodyPrefabs(CustomerManager manager)
    {
        List<GameObject> bodyPrefabs =
        [
            manager.m_CustomerPrefab.m_CharacterCustom.gameObject,
            manager.m_CustomerFemalePrefab.m_CharacterCustom.gameObject,
        ];

        foreach (GameObject bodyObject in bodyPrefabs)
        {
            LODGroup group = bodyObject.GetComponentInChildren<CharacterCustomization>().GetComponent<LODGroup>();
            if (group == null) continue;
            ReconstructLODGroup(group);
            bodyObject.GetComponentInChildren<CharacterCustomization>().m_HasInit = false;
        }
    }

    private void UpdateHairApparelPrefabObjects()
    {
        CustomerManager manager = GameObject.Find("CustomerManager").GetComponent<CustomerManager>();
        if (manager == null) return;

        
        List<GameObject> hairPrefabs = new();
        List<GameObject> apparelPrefabs = new();

        Logger.LogInfo($"Swapping from Customer List: {manager.GetComponentsInChildren<Customer>(true).Count()}");

        foreach (Customer customer in manager.GetComponentsInChildren<Customer>(true))
        {
            foreach (scrObj_Hair hair in customer.m_CharacterCustom.HairTables)
            {
                foreach (GameObject hairPrefab in hair.Hairstyles.Select(x => x.Mesh))
                {
                    if (!hairPrefabs.Contains(hairPrefab) && hairPrefab != null)
                    {
                        hairPrefabs.Add(hairPrefab);
                    }
                }
            }

            foreach (scrObj_Apparel apparel in customer.m_CharacterCustom.ApparelTables)
            {
                foreach (GameObject apparelPrefab in apparel.Items.Select(x => x.Mesh))
                {
                    if (!apparelPrefabs.Contains(apparelPrefab) && apparelPrefab != null)
                    {
                        apparelPrefabs.Add(apparelPrefab);
                    }
                }
            }
        }

        Logger.LogInfo($"Got Hair and Apparel: {hairPrefabs.Count}, {apparelPrefabs.Count}");

        if (hqHair.Value)
        {
            foreach (GameObject hair in hairPrefabs)
            {
                if (hair == null) continue;
                LODGroup group = hair.GetComponentInChildren<LODGroup>(true);
                if (group == null) continue;

                ReconstructLODGroup(group);
            }
        }

        if (hqClothing.Value)
        {
            foreach (GameObject apparel in apparelPrefabs)
            {
                if (apparel == null) continue;
                LODGroup group = apparel.GetComponentInChildren<LODGroup>(true);
                if (group == null) continue;

                ReconstructLODGroup(group);
            }
        }
    }

    private void ReconstructLODGroup(LODGroup group)
    {
        // Are all LODs active?
        bool inactiveLODs = group.GetComponentsInChildren<SkinnedMeshRenderer>().Count() != group.GetComponentsInChildren<SkinnedMeshRenderer>(true).Count();
        if (inactiveLODs)
        {
            Array.ForEach(group.GetComponentsInChildren<SkinnedMeshRenderer>(true), x =>
            {
                x.gameObject.SetActive(true);
                x.enabled = true;
            });
        }

        // Does the first listed LOD in the group refer to an "LOD0"?
        bool firstIsLOD0 = group.GetLODs()[0].renderers.Where(x => x.name.Contains("LOD0")).Count() > 0;
        if (firstIsLOD0)
        {
            return;
        }

        // Does an LOD0 renderer exist?
        bool zeroExists = group.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(x => x.gameObject.name.Contains("LOD0")).Count() > 0;

        // Does the number of LODs match the number of Renderers?
        //bool countMatch = group.GetLODs().Count() == group.GetComponentsInChildren<SkinnedMeshRenderer>(true).Count();

        if (zeroExists)
        {
            Logger.LogInfo($"Swapping LODs for Prefab: {group.gameObject.name}");
            List<SkinnedMeshRenderer> skinnedMeshRenderers = group.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(x => x.gameObject.name.Contains("LOD")).OrderBy(x => x.name).ToList();
            Logger.LogInfo($"Renderers: {string.Join(", ", skinnedMeshRenderers.Select(x => x.name))}");
            LOD[] newLODs = new LOD[skinnedMeshRenderers.Count()];
            for (int i = 0; i < skinnedMeshRenderers.Count(); i++)
            {
                float height = 0f;
                if (i < (skinnedMeshRenderers.Count() - 1))
                {
                    height = 1f / (i + 2f);
                }
                newLODs[i] = new LOD(height, [skinnedMeshRenderers[i]]);
            }
            group.SetLODs(newLODs);
        }
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
}
