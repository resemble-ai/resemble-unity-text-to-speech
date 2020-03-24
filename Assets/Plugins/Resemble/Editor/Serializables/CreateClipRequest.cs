namespace Resemble.Structs
{
    /// <summary> Response format of the api after a Create clip request. </summary>
    [System.Serializable]
    public class CreateClipRequest
    {
        public ClipPatch.Data data;
        public string quality;
        public bool raw;
        public bool phoneme_timestamps;

        public CreateClipRequest(ClipPatch.Data data, bool phonemes)
        {
            this.data = data;
            quality = "high";
            raw = false;
            phoneme_timestamps = phonemes;
        }

        public CreateClipRequest(ClipPatch.Data data, string quality, bool raw, bool phonemes)
        {
            this.data = data;
            this.quality = quality;
            this.raw = raw;
            this.phoneme_timestamps = phonemes;
        }

        public string Json()
        {
            return UnityEngine.JsonUtility.ToJson(this);
        }
    }
}