using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SuperPickers {
    public static class JsonUtilityWrapper {
        private static int port = 8544;

        [Serializable]
        private class OrderingsList {
            public List<Ordering> orderings;
        }

        private static string content;

        public static List<Ordering> FromJsonList() {
            // string applicationDirectory = System.IO.Path.GetDirectoryName(Application.dataPath);
            // string dataFilePath = System.IO.Path.Combine(".", "sample_data.json");
            // string contents = System.IO.File.ReadAllText(dataFilePath);

            string wrapped = "{\"orderings\":" + content + "}";
            return JsonUtility.FromJson<OrderingsList>(wrapped).orderings;
        }

        public static IEnumerator LoadContent() {
            // UnityWebRequest webRequest = UnityWebRequest.Get("http://127.0.0.1:" + port + "/sample_data.json");
            UnityWebRequest webRequest = UnityWebRequest.Get("https://packingi535930.dev004.jpaas.sapbydesign.com/api/orders/visualize");
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success) {
                Debug.LogError(webRequest.error);
            }

            content = webRequest.downloadHandler.text;

            yield return null;
        }
    }
}