using System;
using System.Collections.Generic;

namespace TurnBasedFW
{
    [Serializable]
    public class Attributes
    {
        public void Initialize()
        {
            Indexer = new Dictionary<string, float>();
            foreach (var att in List)
                Indexer.Add(att.Id, att.Value);
        }

        [Serializable]
        public class Attribute
        {
            public string Id;
            public float Value;

            public Action<float> OnAttributeChanged;
        }

        public List<Attribute> List;

        private Dictionary<string, float> Indexer;

        private Dictionary<string, Action<float>> listeners = new Dictionary<string, Action<float>>();

        public float GetAttribute(string Id)
        {
            if (!Indexer.ContainsKey(Id))
                return 0;

            return Indexer[Id];
        }

        public void SetAttribute(string Id, float Value)
        {
            if (!Indexer.ContainsKey(Id))
                return;

            Indexer[Id] = Value;

            if (listeners.ContainsKey(Id))
            {
                listeners[Id]?.Invoke(Value);
            }
        }

        public void RegisterListener(string Id, Action<float> listener)
        {
            listeners.Add(Id, listener);
        }
    }
}
