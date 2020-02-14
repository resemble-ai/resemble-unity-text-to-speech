namespace Resemble.Structs
{
    /// <summary> Response format of the api after a Create clip request. </summary>
    [System.Serializable]
    public class CreateClipResponse
    {
        public string status;
        public long id;
        public string uuid;

        /// <summary> Construct the class from json data. </summary>
        public CreateClipResponse(string jsonFormat)
        {
            CreateClipResponse r = UnityEngine.JsonUtility.FromJson<CreateClipResponse>(jsonFormat);
            status = r.status;
            id = r.id;
            uuid = r.uuid;
        }
    }
}