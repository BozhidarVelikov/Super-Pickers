namespace SuperPickers {
    [System.Serializable]
    public class Item {
        public string name;
        public int width;
        public int height;
        public int depth;
        public int rotation;
        public int[] position;
        public Rotation[] eulerRotations {
            get {
                switch (rotation) {
                    case 0:
                        return new Rotation[] {
                            new Rotation(0f, 0f, 0f),
                            new Rotation(0f, 0f, 0f)
                        };
                    case 1:
                        return new Rotation[] {
                            new Rotation(0f, 0f, 90f),
                            new Rotation(0f, 0f, 90f),
                        };
                    case 2:
                        return new Rotation[] {
                            new Rotation(0f, 90f, 0f),
                            new Rotation(0f, 90f, 0f),
                        };
                    case 3:
                        return new Rotation[] {
                            new Rotation(90f, 0f, 0f),
                            new Rotation(90f, 90f, 0f),
                        };
                    case 4:
                        return new Rotation[] {
                            new Rotation(90f, 0f, 0f),
                            new Rotation(90f, 0f, 0f)
                        };
                    case 5:
                        return new Rotation[] {
                            new Rotation(90f, 0f, 0f),
                            new Rotation(90f, 0f, 90f)
                        };
                    default:
                        return null;
                }
            }
        }
    }

    [System.Serializable]
    public class Rotation {
        public float x, y, z;

        public Rotation(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}