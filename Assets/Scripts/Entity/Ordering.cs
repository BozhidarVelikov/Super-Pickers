using System.Collections.Generic;

namespace SuperPickers {
    [System.Serializable]
    public class Ordering {
        public Bin bin;
        public List<Item> items;
        public float efficiency;
        public float weight;
    }
}