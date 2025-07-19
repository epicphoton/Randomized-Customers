using System.Collections.Generic;
using CC;
using UnityEngine;

using BepInEx.Logging;
using System.Linq;

namespace RandomizedCustomers;

public class CCRandomizer
{
    public static ManualLogSource Logger;

    public CCRandomizer(ManualLogSource logger)
    {
        Logger = logger;
    }

    public void randomizeAll(CharacterCustomization script)
    {
        Ethnicity ethnicity = (Ethnicity)Random.Range(0, 3);
        Color randomEyeColor = getRandomEyeColor(ethnicity);
        script.setColorProperty(new CC_Property()
        {
            propertyName = "_Eye_Color",
            stringValue = "",
            materialIndex = -1
        }, randomEyeColor, false);
        List<string> stringList1 = new List<string>()
            {
                "mod_brow_height",
                "mod_brow_depth",
                "mod_jaw_height",
                "mod_jaw_width",
                "mod_cheeks_size",
                "mod_cheekbone_size",
                "mod_nose_height",
                "mod_nose_width",
                "mod_nose_out",
                "mod_nose_size",
                "mod_mouth_size",
                "mod_mouth_depth",
                "mod_mouth_height",
                "mod_eyes_depth",
                "mod_eyes_height",
                "mod_eyes_narrow",
                "mod_chin_size"
            };
        Logger.LogInfo($"Randomizing Blendshapes...");
        for (int index = 0; index < stringList1.Count; ++index)
        {
            float normalRandom = GenerateNormalRandom(0.2f);
            script.setBlendshapeByName(stringList1[index], normalRandom);
        }
        float num1 = Mathf.Abs(GenerateNormalRandom(0.5f));
        script.setFloatProperty(new CC_Property()
        {
            propertyName = "_Freckles_Strength",
            floatValue = num1
        }, true);
        script.setColorProperty(new CC_Property()
        {
            propertyName = "_Skin_Tint",
            stringValue = ""
        }, new Color(Random.Range(0.0f, 1f), Random.Range(0.0f, 1f), Random.Range(0.0f, 1f))
        {
            a = Mathf.Abs(GenerateNormalRandom(0.1f))
        }, true);
        script.setColorProperty(new CC_Property()
        {
            propertyName = "_Lips_Color",
            stringValue = ""
        },
        new Color(0.8f, 0.2f, 0.2f)
        {
            a = Mathf.Abs(GenerateNormalRandom(0.1f))
        },
        true);
        List<string> stringList2 = new List<string>()
            {
                "",
                "shp_head_01",
                "shp_head_02",
                "shp_head_03",
                "shp_head_04",
                "shp_head_05",
                "shp_head_06",
                "shp_head_07",
                "shp_head_08"
            };
        foreach (string name in stringList2)
            script.setBlendshapeByName(name, 0.0f);
        string name1 = stringList2[Random.Range(0, stringList2.Count)];
        stringList2.Remove(name1);
        switch (ethnicity)
        {
            case Ethnicity.Caucasian:
                stringList2.Remove("shp_head_01");
                stringList2.Remove("shp_head_04");
                stringList2.Remove("shp_head_06");
                stringList2.Remove("shp_head_07");
                break;
            case Ethnicity.African:
                stringList2.RemoveAt(0);
                stringList2.Remove("shp_head_02");
                stringList2.Remove("shp_head_03");
                stringList2.Remove("shp_head_04");
                stringList2.Remove("shp_head_05");
                stringList2.Remove("shp_head_06");
                stringList2.Remove("shp_head_08");
                break;
            case Ethnicity.Asian:
                stringList2.RemoveAt(0);
                stringList2.Remove("shp_head_01");
                stringList2.Remove("shp_head_02");
                stringList2.Remove("shp_head_03");
                stringList2.Remove("shp_head_05");
                stringList2.Remove("shp_head_07");
                stringList2.Remove("shp_head_08");
                break;
        }
        string name2 = stringList2[Random.Range(0, stringList2.Count)];
        float num2 = Mathf.Abs(GenerateNormalRandom(0.33f));
        script.setBlendshapeByName(name1, num2);
        script.setBlendshapeByName(name2, 1f - num2);

        List<string> stringList3 = new List<string>()
            {
                "T_Skin_Head_01",
                "T_Skin_Head_02",
                "T_Skin_Head_03"
            };
        List<string> stringList4 = new List<string>()
            {
                "T_Skin_Body_01",
                "T_Skin_Body_02",
                "T_Skin_Body_03"
            };
        int index1 = 0;
        float num3 = Random.Range(0.0f, 1f);
        switch (ethnicity)
        {
            case Ethnicity.African:
                index1 = (double)num3 > 0.5 ? 1 : 2;
                break;
            case Ethnicity.Asian:
                index1 = (double)num3 > 0.25 ? 0 : 2;
                break;
            case Ethnicity.Other:
                index1 = (double)num3 > 0.5 ? 0 : 2;
                break;
        }
        script.setTextureProperty(new CC_Property()
        {
            propertyName = "_Color_Map",
            stringValue = stringList3[index1],
            meshTag = "Head",
            materialIndex = 0
        }, true);
        script.setTextureProperty(new CC_Property()
        {
            propertyName = "_Color_Map",
            stringValue = stringList4[index1],
            meshTag = "Body",
            materialIndex = 0
        }, true);
        float hV = GenerateNormalRandom(0.25f, scale: 0.125f, bias: 1.05f);
        script.setFloatProperty(new CC_Property()
        {
            propertyName = "Height",
            floatValue = hV
        });
        script.setFloatProperty(new CC_Property()
        {
            propertyName = "Weight",
            floatValue = GenerateNormalRandom(0.33f) - 10f
        });
    }

    private Color getRandomEyeColor(Ethnicity ethnicity)
    {
        List<EyeColor> eyeColorList1 = new List<EyeColor>();
        switch (ethnicity)
        {
            case Ethnicity.Caucasian:
                List<EyeColor> eyeColorList2 = new List<EyeColor>()
          {
            EyeColor.LightBrown,
            EyeColor.MediumBrown,
            EyeColor.Amber,
            EyeColor.Hazel,
            EyeColor.Green,
            EyeColor.LightBlue,
            EyeColor.DarkBlue
          };
                return getEyeColor(eyeColorList2[Random.Range(0, eyeColorList2.Count)]);
            case Ethnicity.African:
            case Ethnicity.Other:
                List<EyeColor> eyeColorList3 = new List<EyeColor>()
          {
            EyeColor.DarkBrown,
            EyeColor.MediumBrown,
            EyeColor.Amber,
            EyeColor.Hazel
          };
                return getEyeColor(eyeColorList3[Random.Range(0, eyeColorList3.Count)]);
            case Ethnicity.Asian:
                List<EyeColor> eyeColorList4 = new List<EyeColor>()
          {
            EyeColor.DarkBrown,
            EyeColor.MediumBrown
          };
                return getEyeColor(eyeColorList4[Random.Range(0, eyeColorList4.Count)]);
            default:
                return getEyeColor(EyeColor.MediumBrown);
        }
    }

    private static Color getEyeColor(EyeColor eyeColor)
    {
        Color color;
        switch (eyeColor)
        {
            case EyeColor.LightBrown:
                ColorUtility.TryParseHtmlString("#875E40", out color);
                break;
            case EyeColor.MediumBrown:
                ColorUtility.TryParseHtmlString("#604531", out color);
                break;
            case EyeColor.DarkBrown:
                ColorUtility.TryParseHtmlString("#3A2B1F", out color);
                break;
            case EyeColor.Amber:
                ColorUtility.TryParseHtmlString("#87763C", out color);
                break;
            case EyeColor.Hazel:
                ColorUtility.TryParseHtmlString("#9C9662", out color);
                break;
            case EyeColor.Green:
                ColorUtility.TryParseHtmlString("#677851", out color);
                break;
            case EyeColor.LightBlue:
                ColorUtility.TryParseHtmlString("#698AA3", out color);
                break;
            case EyeColor.DarkBlue:
                ColorUtility.TryParseHtmlString("#4E6373", out color);
                break;
            default:
                return Color.black;
        }
        return color;
    }

    private enum Ethnicity
    {
        Caucasian,
        African,
        Asian,
        Other,
    }

    private enum EyeColor
    {
        LightBrown,
        MediumBrown,
        DarkBrown,
        Amber,
        Hazel,
        Green,
        LightBlue,
        DarkBlue,
    }

    public static float GenerateNormalRandom(float stdDev, float scale = 1f, float bias = 0.0f)
    {
        float f = 1f - UnityEngine.Random.Range(0.0f, 1f);
        float num1 = 1f - UnityEngine.Random.Range(0.0f, 1f);
        float num2 = Mathf.Sqrt(-2f * Mathf.Log(f)) * Mathf.Sin(6.283185f * num1);
        return Mathf.Clamp(stdDev * num2, -1f, 1f) * scale + bias;
    }
}