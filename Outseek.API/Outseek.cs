using System;
using System.IO;

namespace Outseek.API;

public static class Outseek
{
    /// The root directory where the application stores stuff.
    /// This is a fixed directory in the user's home directory.
    public static string StorageDirectory
    {
        get
        {
            string profileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string storageDir = Path.Join(profileFolder, ".outseek");
            Directory.CreateDirectory(storageDir);
            return storageDir;
        }
    }
}
