using NeXt.Vdf;
using System;
using System.Collections.Generic;
using System.IO;

namespace steamBackup.AppServices.src.Vdf
{
    internal class SteamVdfFolderValue
    {
        private VdfTable vdfTableValue;

        public SteamVdfFolderValue(VdfTable vdfTableValue)
        {
            this.vdfTableValue = vdfTableValue;
        }

        public string GetFolderPath()
        {
            var folderPathValue = vdfTableValue["path"];
            if (folderPathValue == null)
            {
                return null;
            }

            if (folderPathValue.Type != VdfValueType.String)
            {
                return null;
            }

            return (folderPathValue as VdfString).Content;
        }

        public static IEnumerable<SteamVdfFolderValue> InitializeFromFile(string steamDir)
        {
            var vdfDeserializer = VdfDeserializer.FromFile(Path.Combine(steamDir, SteamDirectory.Config, "libraryfolders.vdf"));
            var vdfFile = vdfDeserializer.Deserialize();

            if (vdfFile.Type != VdfValueType.Table)
            {
                throw new Exception($"libraryfolder.vdf isn't in correct format. Expected table, got {vdfFile.Type}");
            }

            var steamLibraryFolderValues = new List<SteamVdfFolderValue>();
            foreach (var vdfValue in (vdfFile as VdfTable))
            {
                if (vdfValue.Type == VdfValueType.Table)
                {
                    steamLibraryFolderValues.Add(new SteamVdfFolderValue(vdfValue as VdfTable));
                }
                else
                {
                    Console.WriteLine("Unable to get table from vdf file");
                }
            }

            return steamLibraryFolderValues;
        }
    }
}
