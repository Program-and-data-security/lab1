using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Эмулятор_ФС
{
    public class PermissionsClass
    {
        public UInt16 CatalogBit = 1 << 0;

        public UInt16 Read_Owner = 1 << 1;
        public UInt16 Write_Owner = 1 << 2;
        public UInt16 Execute_Owner = 1 << 3;

        public UInt16 Read_Group = 1 << 4;
        public UInt16 Write_Group = 1 << 5;
        public UInt16 Execute_Group = 1 << 6;

        public UInt16 Read_Other = 1 << 7;
        public UInt16 Write_Other = 1 << 8;
        public UInt16 Execute_Other = 1 << 9;

        public UInt16 All_Read;
        public UInt16 All_Write;
        public UInt16 All_Execute;

        public UInt16 All_Permissions_File;
        public UInt16 All_Permisisons_Catalog;
        public UInt16 Root_Permissions_File;
        public UInt16 Root_Permissions_Catalog;

        public PermissionsClass()
        {
            All_Read = (UInt16)(Read_Owner | Read_Group | Read_Other);

            All_Write = (UInt16)(Write_Owner | Write_Group | Write_Other);

            All_Execute = (UInt16)(Execute_Owner | Execute_Group | Execute_Other);

            All_Permissions_File = (UInt16)(Read_Owner | Read_Group | Read_Other | Write_Owner | Write_Group | 
                                       Write_Other | Execute_Owner | Execute_Group | Execute_Other);

            All_Permisisons_Catalog = (UInt16)(Read_Owner | Read_Group | Read_Other | Write_Owner | Write_Group |
                                       Write_Other | Execute_Owner | Execute_Group | Execute_Other | CatalogBit);

            Root_Permissions_File = (UInt16)(Read_Owner | Write_Owner | Execute_Owner);

            Root_Permissions_Catalog = (UInt16)(Read_Owner | Write_Owner | Execute_Owner | CatalogBit);
        }
    }
}
