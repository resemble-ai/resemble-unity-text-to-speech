namespace Resemble
{
    [System.Serializable]
    public struct Label
    {
        public int hash;
        public string text;
        public int value;

        public Label(string text)
        {
            hash = text.GetHashCode();
            this.text = text;
            value = 0;
        }

        public Label(string text, int value)
        {
            hash = text.GetHashCode();
            this.text = text;
            this.value = value;
        }

        public static bool operator ==(Label a, Label b) { return a.hash == b.hash && a.value == b.value; }
        public static bool operator !=(Label a, Label b) { return a.hash != b.hash && a.value == b.value; }
        public static bool operator ==(Label a, string b) { return a.hash == b.GetHashCode(); }
        public static bool operator !=(Label a, string b) { return a.hash != b.GetHashCode(); }
        public static bool operator ==(Label a, int b) { return a.value == b; }
        public static bool operator !=(Label a, int b) { return a.value != b; }

        public override bool Equals(object obj)
        {
            if (!(obj is Label))
                return false;
            return hash == ((Label)obj).hash;
        }

        public override string ToString()
        {
            return string.Format("Label ({0} : {1})", text, value);
        }

        public override int GetHashCode()
        {
            return hash + value.GetHashCode();
        }
    }
}