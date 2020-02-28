namespace Resemble.Structs
{
    [System.Serializable]
    public class ClipStatus
    {
        public string status;
        public string id;
        public string project_id;

        public override string ToString()
        {
            return string.Format("(id: {0}, project_uuid: {1}, status:{2})", id, project_id, status);
        }
    }
}