using UnityEngine;
using System;

namespace TurnBasedFW
{
    [Serializable]
    public class Playable
    {
        public T Clone<T>()
        {
            return JsonUtility.FromJson<T>(JsonUtility.ToJson(this));
        }

        /*
         * Sample Json
         * 
         * {
         *     "AssetId": "asset99",
         *     "Name": "tankofbattle",
         *     "Attributes": {
         *          "List": [
         *              {
         *                  "Id": "health",
         *                  "Value": 100
         *              },
         *              {
         *                  "Id": "damage",
         *                  "Value": 20
         *              }
         *          ]
         *     }
         * }
         * 
         * */

        [NonSerialized]
        public string ID;
        [NonSerialized]
        public int PosX, PosY;

        public void SetPosition(int x, int y)
        {
            PosX = x;
            PosY = y;
        }

        public Vector3 GetPosition()
        {
            return new Vector3(PosX, 0, PosY);
        }

        public string AssetId;
        public string Name;
        public Attributes Attributes;
    }
}

