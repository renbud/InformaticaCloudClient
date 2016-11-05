using System;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;

namespace InformaticaCloudClient
{
    /// <summary>
    ///  If credentials were provided on the command line, save them encrypted in app.exe.config
    ///  If not, load from app.exe.config and decrypt
    ///
    ///  http://stackoverflow.com/questions/12657792/how-to-securely-save-username-password-local
    ///  http://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string
    /// </summary>
    public class Credentials
    {
        public static string UserName
        {
            get
            {
                return ConfigurationManager.AppSettings["UserName"];
            }
            set
            {
                UpsertSetting("UserName", value);
            }
        }

        public static string Password
        {
            get
            {
                // Encrypted string is stored as base 64
                string base64EncodedData = ConfigurationManager.AppSettings["Password"];
                if (String.IsNullOrEmpty(base64EncodedData))
                {
                    return null; 
                }
                if (base64EncodedData.Length < 100)
                {
                    // Encrypted base64 will always be over 100 length.
                    // This is unencrypted data - someone has manually saved it - assume its plaintext password
                    // Save it as encrypted and return the cleartext
                    string plainpwdstring = base64EncodedData;
                    Password = plainpwdstring; // save as encrypted
                    return plainpwdstring;
                }
                // decrypt
                byte [] ciphertext = System.Convert.FromBase64String(base64EncodedData); // convert to byte[]
                byte[] plaintext = ProtectedData.Unprotect(ciphertext, entropy, DataProtectionScope.CurrentUser); // decrypt
                return System.Text.Encoding.UTF8.GetString(plaintext); // convert from byte[] to string
            }
            set
            {
                // encrypt
                byte[] plaintext = Encoding.ASCII.GetBytes(value);
                byte[] ciphertext = ProtectedData.Protect(plaintext, entropy, DataProtectionScope.CurrentUser);
                string base64EncodedData = System.Convert.ToBase64String(ciphertext);
                UpsertSetting("Password", base64EncodedData);
            }
        }

        private static byte[] entropy
        {
            // Generate additional entropy (will be used as the Initialization vector)
            get
            {
                string base64EncodedData = ConfigurationManager.AppSettings["Entropy"];
                if (!String.IsNullOrEmpty(base64EncodedData))
                {
                    return System.Convert.FromBase64String(base64EncodedData);
                }

                // generate a new one
                byte[] entropy = new byte[20];
                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(entropy);
                }
                base64EncodedData = System.Convert.ToBase64String(entropy);
                UpsertSetting("Entropy", base64EncodedData);
                return entropy;
            }
        }

        private static void UpsertSetting(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[key] == null)
            {
                config.AppSettings.Settings.Add(key, value);
            }
            else
            {
                config.AppSettings.Settings[key].Value = value;
            }
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
