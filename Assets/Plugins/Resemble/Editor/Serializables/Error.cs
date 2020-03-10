namespace Resemble.Structs
{
    [System.Serializable]
    public struct Error
    {
        public bool exist;
        public long code;
        public string message;

        public static implicit operator bool(Error error)
        {
            return error.exist;
        }

        public override string ToString()
        {
            if (code >= 0)
                return string.Format("Error : {0} : {1}", code, message);
            else
                return string.Format("Error : {0}", message);
        }

        public void Log()
        {
            UnityEngine.Debug.LogError(ToString());
        }

        public Error(long code, string message)
        {
            exist = true;
            this.code = code;

            switch (code)
            {
                case 400: this.message = "Bad Request - The query format does not match the API. Please check for plugin updates."; break;
                case 401: this.message = "Unauthorized -- Your API key is wrong."; break;
                case 403: this.message = "Forbidden -- The endpointt requested is hidden for administrators only."; break;
                case 404: this.message = "Not Found -- The specified endpoint could not be found."; break;
                case 405: this.message = "Method Not Allowed -- You tried to access endpoint with an invalid method."; break;
                case 406: this.message = "Not Acceptable -- You requested a format that isn't json."; break;
                case 410: this.message = "Gone -- The resource requested has been removed from our servers."; break;
                case 418: this.message = "I'm a teapot."; break;
                case 429: this.message = "Too Many Requests -- You're requesting too fast! Slow down!"; break;
                case 500: this.message = "Internal Server Error -- We had a problem with our server. Try again later."; break;
                case 503: this.message = "	Service Unavailable -- We're temporarily offline for maintenance. Please try again later."; break;
                default: this.message = message; break;
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