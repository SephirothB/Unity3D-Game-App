﻿using UnityEngine;

namespace PrefabUtil
{
    [ExecuteInEditMode]
    public class PrefabsSerializer : MonoBehaviour
    {
        public GameObject[] prefabs;
        public static string PrefabsJson;

        static bool serializedOnce;

        void Awake()
        {
            Init();
        }

        public void SerializeData()
        {
            serializedOnce = true;

            var json = JsonHelper.arrayToJson(prefabs);

            Utility.Console.Log(json);

            //System.IO.File.WriteAllText("prefabs.json", json);
            PrefabsJson = json;
        }

        public void Init()
        {
            if (serializedOnce == false)
            {
                SerializeData();
            }
        }
    }
}
