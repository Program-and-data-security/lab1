using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Эмулятор_ФС
{
    internal class CatalogEntry : Entry
    {
        public CatalogEntry(CatalogEntry catalogEntry)
        {
            Name = catalogEntry.Name;
            Permissions = catalogEntry.Permissions;
            FirstClusterIndex = catalogEntry.FirstClusterIndex;
            FileOfSize = 0;
            OwnerID = catalogEntry.OwnerID;
            GroupID = catalogEntry.GroupID;
            EditDate = catalogEntry.EditDate;
            CreateDate = catalogEntry.CreateDate;
        }

        public CatalogEntry(char[] Name, UInt32 FirstClusterIndex, UInt16 OwnerID, UInt16 GroupID, UInt32 CreateDate)
        {
            this.Name = new char[21];

            for (int i = 0; i < Name.Length && i < this.Name.Length; i++)
            {
                this.Name[i] = Name[i];
            }

            Permissions = PermissionsClassIndex.All_Permisisons_Catalog;
            if (GroupID == 0) Permissions = PermissionsClassIndex.Root_Permissions_Catalog;
            this.FirstClusterIndex = FirstClusterIndex;
            FileOfSize = 0;
            this.OwnerID = OwnerID;
            this.GroupID = GroupID;
            this.CreateDate = CreateDate;
            EditDate = CreateDate;
        }

        public CatalogEntry(char[] Name, UInt16 Permissions, UInt32 FirstClusterIndex, UInt16 OwnerID, UInt16 GroupID, UInt32 EditDate, UInt32 CreateDate)
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
            this.EditDate = EditDate;
            this.CreateDate = CreateDate;
        }
    }
}
