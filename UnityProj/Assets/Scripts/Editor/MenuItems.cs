using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using UnityEngine;
using UnityEditor;

public class MenuItems
{
    [MenuItem("Hexes/Delete save")]
    private static void DeleteSave()
    {
        string saveDirName = Application.persistentDataPath + "/Save/";
        if (Directory.Exists(saveDirName))
            Directory.Delete(saveDirName, true);

        saveDirName = Application.persistentDataPath + "/SaveBackup/";
        if (Directory.Exists(saveDirName))
            Directory.Delete(saveDirName, true);
    }

    [MenuItem("Hexes/Backup save")]
    private static void BackupSave()
    {
        string saveDirName = Application.persistentDataPath + "/Save/";
        if (!Directory.Exists(saveDirName)) return;

        string bakSaveDirName = Application.persistentDataPath + "/SaveBackup/";
        if (Directory.Exists(bakSaveDirName))
            Directory.Delete(bakSaveDirName, true);

        CopyDir(saveDirName, bakSaveDirName);
    }

    [MenuItem("Hexes/Restore save")]
    private static void RestoreSave()
    {
        string bakSaveDirName = Application.persistentDataPath + "/SaveBackup/";        
        if (!Directory.Exists(bakSaveDirName)) return;

        string saveDirName = Application.persistentDataPath + "/Save/";
        if (Directory.Exists(saveDirName))
            Directory.Delete(saveDirName, true);

        CopyDir(bakSaveDirName, saveDirName);
    }

    [MenuItem("Hexes/Enable edit")]
    private static void EnableEdit()
    {
        GameObject.Find("G").GetComponent<G>().isEditMode = true;
    }

    [MenuItem("Hexes/Disable edit")]
    private static void DisableEdit()
    {
        GameObject.Find("G").GetComponent<G>().isEditMode = false;
    }

    static void CopyDir(string src, string dest)
    {
        if (!Directory.Exists(dest))
            Directory.CreateDirectory(dest);

        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(src, "*",
            SearchOption.AllDirectories))
            Directory.CreateDirectory(dirPath.Replace(src, dest));

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(src, "*.*",
            SearchOption.AllDirectories))
            File.Copy(newPath, newPath.Replace(src, dest), true);
    }
}
