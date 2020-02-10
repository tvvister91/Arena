using System;
using System.Text;

using Android.Content;

using MvvmCross.Plugin;

using PE.Plugins.Validation;

namespace Arena.Droid.Bootstrap
{
    public class ValidationBootstrap
    {
        public static IMvxPluginConfiguration Configure()
        {
            return new ValidationConfig
            {
                CreateHash = delegate (string value)
                {
                    if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("Cannot hash empty string");
                    var provider = System.Security.Cryptography.MD5.Create();
                    var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(value));

                    //  returned hashed data as string
                    return Convert.ToBase64String(hash);
                }
            };
        }
    }
}
