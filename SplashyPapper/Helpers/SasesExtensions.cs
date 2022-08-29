using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SplashyPapper.Helpers;

public class SasesExtensions
{
    public static bool IsContainsFile(IEnumerable<StorageFile> files, StorageFile file)
    {
        foreach (var fileFromFiles in files)
        {
            if (fileFromFiles.FolderRelativeId == file.FolderRelativeId)
                return true;
        }
        return false;
    }
}