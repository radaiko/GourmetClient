namespace GourmetClient.Settings
{
    using System;
    using System.Security;

    public class UserSettings
    {
        public UserSettings()
        {
            GourmetLoginUsername = string.Empty;
            GourmetLoginPassword = new SecureString();
            VentopayUsername = string.Empty;
            VentopayPassword = new SecureString();
            CacheValidity = TimeSpan.FromHours(4);
        }

        public string GourmetLoginUsername { get; set; }

        public SecureString GourmetLoginPassword { get; set; }

        public string VentopayUsername { get; set; }

        public SecureString VentopayPassword { get; set; }

        public TimeSpan CacheValidity { get; set; }
    }
}
