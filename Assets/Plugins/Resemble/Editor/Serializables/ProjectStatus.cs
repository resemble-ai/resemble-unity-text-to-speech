namespace Resemble.Structs
{
    [System.Serializable]
    public class ProjectStatus
    {
        public string status;
        public int id;
        public string uuid;

        public override string ToString()
        {
            return string.Format("(id: {0}, uuid: {1}, status:{2})", id, uuid, status);
        }
    }
}