namespace Resemble
{
    public struct Error
    {
        public bool exist;
        public long code;
        public string message;

        public static implicit operator bool(Error error)
        {
            return error.exist;
        }

        public Error(long code, string message)
        {
            exist = true;
            this.code = code;

            switch (code)
            {
                case 400:
                    this.message = "Bad Request - The query format does not match the API. Please check for plugin updates.";
                    break;
                default:
                    this.message = message;
                    break;
            }
        }

        public static Error None
        { get { return new Error(); } }

        public static Error NetworkError
        { get { return new Error(-1, "Unable to connect to Internet. Please, check your connection."); } }

        public static Error EmptyResponse
        { get { return new Error(-1, "Empty response - Please, check your connection or retry later."); } }

        public static Error Timeout
        { get { return new Error(-1, "Timout - Please, check your connection or retry later."); } }

        #region Json
        /// <summary> Convert a json formated error in a Error struct. </summary>
        /// <param name="code">Error code</param>
        /// <param name="jsonError">Json structure</param>
        public static Error FromJson(long code, string jsonError)
        {
            if (string.IsNullOrEmpty(jsonError))
                return new Error(code, "");
            var se = UnityEngine.JsonUtility.FromJson<SerializedErrors>(jsonError);
            return new Error(code, se.allText());
        }

        [System.Serializable]
        public struct SerializedErrors
        {
            public string[] errors;
            public string allText()
            {
                if (errors == null || errors.Length == 0)
                    return "";
                string result = errors[0];
                for (int i = 1; i < errors.Length; i++)
                    result = result + "\n" + errors[i];
                return result;
            }
        }
        #endregion
    }
}