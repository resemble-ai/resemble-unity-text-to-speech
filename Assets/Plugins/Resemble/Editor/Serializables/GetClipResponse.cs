namespace Resemble.Structs
{
    /// <summary> Response format of the api after a GetClip request. </summary>
    [System.Serializable]
    public class GetClipResponse
    {
        public long id;
        public string title;
        public string body;
        public string published;
        public string author;
        public bool finished;
        public string link;
        public string voice;
        public long project_id;

        /// <summary> Construct the class from json data. </summary>
        public GetClipResponse(string jsonFormat)
        {
            GetClipResponse r = UnityEngine.JsonUtility.FromJson<GetClipResponse>(jsonFormat);
            id = r.id;
            title = r.title;
            body = r.body;
            published = r.published;
            author = r.author;
            finished = r.finished;
            link = r.link;
            voice = r.voice;
            project_id = r.project_id;
        }
    }
}