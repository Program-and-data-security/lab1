using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Эмулятор_ФС
{
    internal class FileEntry : Entry
    {
        public FileEntry(FileEntry fileEntry)
        {
            Name = fileEntry.Name;
            Permissions = fileEntry.Permissions;
            FirstClusterIndex = fileEntry.FirstClusterIndex;
            FileOfSize = fileEntry.FileOfSize;
            OwnerID = fileEntry.OwnerID;
            GroupID = fileEntry.GroupID;
            EditDate = fileEntry.EditDate;
            CreateDate = fileEntry.CreateDate;
        }

        public FileEntry(char[] Name, UInt32 FirstClusterIndex, UInt16 OwnerID, UInt16 GroupID, UInt32 CreateDate)
        {
            this.Name = new char[21];

            for (int i = 0; i < Name.Length && i < this.Name.Length; i++)
            {
                this.Name[i] = Name[i];
            }

            Permissions = PermissionsClassIndex.All_Permissions_File;
            if (GroupID == 0) Permissions = PermissionsClassIndex.Root_Permissions_File;
            this.FirstClusterIndex = FirstClusterIndex;
            FileOfSize = 0;
            this.OwnerID = OwnerID;
            this.GroupID = GroupID;
            this.CreateDate = CreateDate;
            EditDate = CreateDate;
        }

        public FileEntry(char[] Name, UInt16 Permissions, UInt32 FirstClusterIndex, UInt16 OwnerID, UInt16 GroupID, UInt32 CreateDate)
        {
            this.Name = new char[21];

            for (int i = 0; i < Name.Length && i < this.Name.Length; i++)
            {
                this.Name[i] = Name[i];
            }

            this.Permissions = Permissions;
            this.FirstClusterIndex = FirstClusterIndex;
            FileOfSize = 0;
            this.OwnerID = OwnerID;
            this.GroupID = GroupID;
            this.CreateDate = CreateDate;
            EditDate = CreateDate;
        }

        public FileEntry(char[] Name, UInt16 Permissions, UInt32 FirstClusterIndex, UInt32 FileOfSize, UInt16 OwnerID, UInt16 GroupID, UInt32 EditDate, UInt32 CreateDate)
        {
            this.Name = new char[21];

            for (int i = 0; i < Name.Length && i < this.Name.Length; i++)
            {
                this.Name[i] = Name[i];
            }

            this.Permissions = Permissions;
            this.FirstClusterIndex = FirstClusterIndex;
            this.FileOfSize = FileOfSize;
            this.OwnerID = OwnerID;
            this.GroupID = GroupID;
            this.EditDate = EditDate;
            this.CreateDate = CreateDate;
        }
    }
}
