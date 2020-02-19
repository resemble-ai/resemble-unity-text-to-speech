namespace Resemble.Structs
{
    /// <summary> Response format of the api after a Create clip request. </summary>
    [System.Serializable]
    public class CreateClipRequest
    {
        public CreateClipData data;
        public string quality;
        public bool raw;

        public CreateClipRequest(CreateClipData data)
        {
            this.data = data;
            quality = "high";
            raw = false;
        }

        public CreateClipRequest(CreateClipData data, string quality, bool raw)
        {
            this.data = data;
            this.quality = quality;
            this.raw = raw;
        }

        public string Json()
        {
            return UnityEngine.JsonUtility.ToJson(this);
        }
    }

    [System.Serializable]
    public class CreateClipData
    {
        public string title;
        public string body;
        public string voice;

        public CreateClipData(string title, string body, string voice)
        {
            this.title = title;
            this.body = body;
            this.voice = voice;
        }
    }
}