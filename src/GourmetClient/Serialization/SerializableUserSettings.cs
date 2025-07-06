namespace GourmetClient.Serialization
{
	using System;
	using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using Settings;
	using Utils;

    internal class SerializableUserSettings
	{
		public SerializableUserSettings()
		{
			// Used for deserialization
			CacheValidityMinutes = 0;
			GourmetLoginUsername = string.Empty;
			GourmetLoginPassword = string.Empty;
            VentopayUsername = string.Empty;
			VentopayPassword = string.Empty;
        }

		public SerializableUserSettings(UserSettings userSettings)
		{
			CacheValidityMinutes = (int)userSettings.CacheValidity.TotalMinutes;
			GourmetLoginUsername = userSettings.GourmetLoginUsername;
			GourmetLoginPassword = Encrypt(userSettings.GourmetLoginPassword);
			VentopayUsername = userSettings.VentopayUsername;
			VentopayPassword = Encrypt(userSettings.VentopayPassword);
		}

		public int CacheValidityMinutes { get; set; }

		public string GourmetLoginUsername { get; set; }

		public string GourmetLoginPassword { get; set; }

		public string VentopayUsername { get; set; }

		public string VentopayPassword { get; set; }

		public UserSettings ToUserSettings()
		{
			var userSettings = new UserSettings
			{
				GourmetLoginUsername = GourmetLoginUsername,
				GourmetLoginPassword = Decrypt(GourmetLoginPassword),
				VentopayUsername = VentopayUsername,
				VentopayPassword = Decrypt(VentopayPassword)
			};

			if (CacheValidityMinutes > 0)
			{
				userSettings.CacheValidity = TimeSpan.FromMinutes(CacheValidityMinutes);
			}

			return userSettings;
		}

		private static string Encrypt(string value)
		{
			return EncryptionHelper.Encrypt(value, GetEncryptionKey());
		}

		private static string Decrypt(string encryptedText)
		{
			try
			{
				return EncryptionHelper.Decrypt(encryptedText, GetEncryptionKey());
			}
			catch (Exception)
            {
                return string.Empty;
            }
		}

        private static string GetEncryptionKey()
        {
            var machineName = Environment.MachineName;
            var userName = Environment.UserName;

            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{userName}@{machineName}"));

            return Encoding.UTF8.GetString(hash);
        }
    }
}
