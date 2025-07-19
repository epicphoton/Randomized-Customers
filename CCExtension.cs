using UnityEngine;
using System.Collections.Generic;
using CC;
using System.Reflection;

namespace ExtensionMethods
{
    public static class CCExtension
    {
        public static List<GameObject> GetHairObjects(this CharacterCustomization cc)
        {
            FieldInfo fi = typeof(CharacterCustomization).GetField("HairObjects", BindingFlags.NonPublic | BindingFlags.Instance);
            return (List<GameObject>)fi?.GetValue(cc);
        }

        public static List<GameObject> GetApparelObjects(this CharacterCustomization cc)
        {
            FieldInfo fi = typeof(CharacterCustomization).GetField("ApparelObjects", BindingFlags.NonPublic | BindingFlags.Instance);
            return (List<GameObject>)fi?.GetValue(cc);
        }
    }
}