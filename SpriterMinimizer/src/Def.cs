using System.Collections.Generic;

namespace SpriterMinimizer {
    class Def {
        // parent def
        public Def parent = null;

        public string name;
        public string newName;

        public Dictionary<string, Def> childElements = new Dictionary<string, Def>();
        public Dictionary<string, string> attributes = new Dictionary<string, string>();
    }
}
