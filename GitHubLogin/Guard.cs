using System;

namespace GitHubLogin
{
    public static class Guard
    {
        public static void ArgumentNotNull(object value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}
