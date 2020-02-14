namespace Resemble.Structs
{
    /// <summary> Response format of the api after a Create clip request. </summary>
    [System.Serializable]
    public class CreateClipRequest
    {
        public PostPod data;
        public string quality;
        public bool raw;

        public CreateClipRequest(PostPod data)
        {
            this.data = data;
            quality = "high";
            raw = false;
        }

        public CreateClipRequest(PostPod data, string quality, bool raw)
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
}