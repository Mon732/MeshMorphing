using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class CreateFolderStructure : MonoBehaviour
{
    [MenuItem("Assets/Create Folder Structure")]
    public static void CreateFolders()
    {
        Sort[] sorts = new Sort[]
        {
            new Sort("Scripts", new string[]{"*.cs", "*.js"}),
            new Sort("Materials", new string[]{"*.mat"})
        };

        Debug.Log("Creating Structure...");

        string dir = Directory.GetCurrentDirectory();
        dir = dir + @"\Assets\";

        foreach (Sort sort in sorts)
        {
            string folder = sort.folder;

            if (!Directory.Exists(dir + folder))
            {
                AssetDatabase.CreateFolder("Assets", folder);
            }

            foreach (string fileType in sort.filetypes)
            {
                string[] assets = Directory.GetFiles(dir, fileType, SearchOption.AllDirectories);

                foreach (string asset in assets)
                {
                    Debug.Log(asset);

                    string localAsset = asset.Replace(dir, "");

                    AssetDatabase.MoveAsset(localAsset, @"Assets\" + folder);
                }
            }
        }

        AssetDatabase.Refresh();

        Debug.Log("Created");
    }

    public class Sort
    {
        public Sort(string a, string[] b)
        {
            folder = a;
            filetypes = b;
        }

        public string folder;
        public string[] filetypes;
    }
}
