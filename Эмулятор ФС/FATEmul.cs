using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Эмулятор_ФС
{
    public partial class FATEmul : Form
    {
        static private readonly string path = "File.fat"; // путь к файлу с данными

        private BPB BPB;
        private FSinfo FSinfo;

        private List<UserList> UserList;
        private List<GroupList> GroupList;

        private Byte BPBSize = 8; // Размер BPB в байтах
        private Byte FSinfoSize = 8; // Размер FSinfo в байтах
        private Byte EntrySize = 64; // Размер Entry в байтах
        private Byte NextSize = 4; // Размер Next в байтах
        private UInt16 ClasterSize; // Размер Claster в байтах
        private UInt16 ClasterSimbolSize; // Количество символов в кластере в байтах
        private UInt32 FATCountSize; // Размер всех FAT в байтах
        private UInt32 FATSize; // Размер одной FAT в байтах

        private UInt32 CurrentFAT = 0; // Текущий FAT индекс
        private Stack<UInt32> CatalogStack = new Stack<UInt32>();
        private List<string> NameStack = new List<string>();
        private Stack<bool> PerrmissionsWriteCheck = new Stack<bool>();

        private UInt16 OwnerID = 0;
        private UInt16 GroupID = 0;
        private char[] OwnerName;
        private char[] GroupName;

        private FATIndex FATIndex = new FATIndex(); // Стандартные размеры и значения эмулятора
        private PermissionsClass PermissionsClass = new PermissionsClass(); // Стандартные значения аттрибутов для битовых операций

        public string tempString;

        private MyStream stream;

        public Entry Entry = null;
        public long cutIndex = 0;
        public bool Copy = false;

        private UInt16 PerrmisionsFileTemp;

        public FATEmul(BPB bpb, FSinfo fsinfo, UserList user, GroupList group, List<UserList> userList, List<GroupList> groupList)
        {
            InitializeComponent();

            catalogTable.Rows.Clear();
            catalogTable.ShowCellToolTips = false;

            BPB = bpb;
            FSinfo = fsinfo;

            OwnerID = user.UserID;
            GroupID = user.GroupID;

            OwnerName = user.Name;
            GroupName = group.Name;

            UserList = userList;
            GroupList = groupList;

            label3.Text = String.Join("", user.Name);

            ClasterSize = (UInt16)(BPB.SectorsByteCount * BPB.SectorsPerClaster);
            ClasterSimbolSize = (UInt16)(ClasterSize / 2);
            FATCountSize = BPB.FATSize * BPB.FATCount * NextSize;
            FATSize = BPB.FATSize;

            stream = new MyStream(path, FileMode.Open);

            ReadAllFileToTable();
        }

        private void createFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateFileMethod();
        }

        private void createCatalogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateCatalogMethod();
        }

        private void CreateFileMethod()
        {
            try
            {
                if (PerrmissionsWriteCheck.Count != 0 && !PerrmissionsWriteCheck.Peek()) throw new Exception("У вас нет прав на редактирование этой папки");

                tempString = String.Empty;

                string[] names = new string[catalogTable.Rows.Count];

                for (int i = 0; i < names.Length; i++) if (catalogTable.Rows[i].Cells[0].Value.ToString() != null)
                        names[i] = catalogTable.Rows[i].Cells[0].Value.ToString().Trim('\0');

                using (NameFileReader nameReader = new NameFileReader(names, this)) nameReader.ShowDialog();

                if (!string.IsNullOrEmpty(tempString))
                {
                    char[] Name = new char[21];
                    char[] temp = tempString.ToCharArray();

                    for (int i = 0; i < temp.Length && i < Name.Length; i++)
                        Name[i] = temp[i];

                    CreateFile(temp);

                    ReadAllFileToTable();
                }
                else return;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButtons.OK);
                ReadAllFileToTable();
            }
        }

        private void CreateCatalogMethod()
        {
            try
            {
                if (PerrmissionsWriteCheck.Count != 0 && !PerrmissionsWriteCheck.Peek()) throw new Exception("У вас нет прав на редактирование этой папки");

                tempString = String.Empty;

                string[] names = new string[catalogTable.Rows.Count];

                for (int i = 0; i < names.Length; i++) if (catalogTable.Rows[i].Cells[0].Value.ToString() != null)
                        names[i] = catalogTable.Rows[i].Cells[0].Value.ToString().Trim('\0');

                using (NameFileReader nameReader = new NameFileReader(names, this)) nameReader.ShowDialog();

                if (!string.IsNullOrEmpty(tempString))
                {
                    char[] Name = new char[21];
                    char[] temp = tempString.ToCharArray();

                    CreateCatalog(temp);

                    ReadAllFileToTable();
                }
                else return;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButtons.OK);
                ReadAllFileToTable();
            }
        }

        private void свойстваToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PropertiesMethod();
        }

        private void PropertiesMethod()
        {
            try
            {
                tempString = "";

                if (catalogTable.SelectedRows[0].Index == -1) return;

                string name = catalogTable.SelectedRows[0].Cells[0].Value.ToString();

                int number = -1;

                if (catalogTable.SelectedRows[0].Cells[4].Value.ToString() == String.Join("", OwnerName)) number = 0;
                else if (catalogTable.SelectedRows[0].Cells[5].Value.ToString() == String.Join("", GroupName)) number = 1;
                else number = 2;

                Entry entry = ReturnEntry(name.ToCharArray(), number);

                if (entry != null)
                {
                    string[] columnName = (from row in catalogTable.Rows.OfType<DataGridViewRow>()
                                  where !row.Cells[0].Value.ToString().Equals(name)
                                  select (row.Cells[0].Value.ToString().Replace("\0", "")
                                  )).ToArray();

                    PropertiesEntry propertiesEntry = new PropertiesEntry(entry, UserList, GroupList, this, columnName, PermissionsMethod(number, entry.Permissions, PermissionsClass.Write_Owner, PermissionsClass.Write_Group, PermissionsClass.Write_Other));
                    propertiesEntry.ShowDialog();

                    if (entry != Entry)
                    {
                        OverwriteEntry(Entry, name.ToCharArray());
                    }
                }
                else throw new Exception("Такой файл или папка не найдены");

                Entry = null;

                ReadAllFileToTable();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButtons.OK);
                ReadAllFileToTable();
            }
        }

        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CutMethod();
        }

        private void CutMethod()
        {
            try
            {
                tempString = "";

                if (catalogTable.SelectedRows[0].Index == -1) return;

                string name = catalogTable.SelectedRows[0].Cells[0].Value.ToString();

                if (name[0] == '%') throw new Exception("Данный файл нельзя перемещать");

                int number = -1;

                if (catalogTable.SelectedRows[0].Cells[4].Value.ToString() == String.Join("", OwnerName)) number = 0;
                else if (catalogTable.SelectedRows[0].Cells[5].Value.ToString() == String.Join("", GroupName)) number = 1;
                else number = 2;

                Entry = ReturnEntry(name.ToCharArray(), number, true);

                Copy = false;

                ReadAllFileToTable();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButtons.OK);
                ReadAllFileToTable();
            }
        }

        private void копироватьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CopyMethod();
        }

        private void CopyMethod()
        {
            try
            {
                if (catalogTable.SelectedRows[0].Index == -1) return;

                string name = catalogTable.SelectedRows[0].Cells[0].Value.ToString();

                int number = -1;

                if (catalogTable.SelectedRows[0].Cells[4].Value.ToString() == String.Join("", OwnerName)) number = 0;
                else if (catalogTable.SelectedRows[0].Cells[5].Value.ToString() == String.Join("", GroupName)) number = 1;
                else number = 2;

                Entry = CopyReturnEntry(name.ToCharArray(), number, true);

                Copy = true;

                ReadAllFileToTable();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButtons.OK);
                ReadAllFileToTable();
            }
        }

        private Entry CopyReturnEntry(char[] Name, int number, bool check = false)
        {
            try
            {
                UInt32 CurrFatTemp = CurrentFAT;

                byte[] nameBytes = Encoding.Unicode.GetBytes(Name);

                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array = new byte[42];

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                    for (int i = 0; i < maxEntry; i++)
                    {
                        stream.Read(array, 0, array.Length);

                        // Если место пустое или не занято
                        if (CompareArrays(nameBytes, array))
                        {
                            cutIndex = stream.Position - 42;

                            array = new byte[2];

                            stream.Read(array, 0, array.Length);

                            UInt16 permissions = BitConverter.ToUInt16(array, 0);

                            array = new byte[4];

                            stream.Read(array, 0, array.Length);

                            UInt32 firstClaster = BitConverter.ToUInt32(array, 0);

                            stream.Read(array, 0, array.Length);

                            UInt32 fileSize = BitConverter.ToUInt32(array, 0);

                            array = new byte[2];

                            stream.Read(array, 0, array.Length);

                            UInt16 owner = BitConverter.ToUInt16(array, 0);

                            stream.Read(array, 0, array.Length);

                            UInt16 group = BitConverter.ToUInt16(array, 0);

                            UInt32 date = (UInt32)new DateTimeOffset(DateTime.UtcNow.ToLocalTime()).ToUnixTimeSeconds();

                            Entry entry;

                            if ((permissions & PermissionsClass.CatalogBit) == 0)
                                entry = new FileEntry(Name, PermissionsClass.All_Permissions_File, firstClaster, fileSize, owner, group, date, date);
                            else entry = new CatalogEntry(Name, PermissionsClass.All_Permisisons_Catalog, firstClaster, owner, group, date, date);

                            return entry;
                        }

                        stream.Seek(stream.Position + 22, SeekOrigin.Begin);
                    }

                    stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                    array = new byte[4];

                    stream.Read(array, 0, 4);

                    UInt32 Index = BitConverter.ToUInt32(array, 0);

                    if (Index != FATIndex.EOC) CurrFatTemp = Index;
                    else return null;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void CopyEnterEntry(char[] Name)
        {
            try
            {
                UInt32 CurrFatTemp = CurrentFAT;

                byte[] nameBytes = Encoding.Unicode.GetBytes(Name);

                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array = new byte[42];

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                    for (int i = 0; i < maxEntry; i++)
                    {
                        stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize) + (EntrySize * i), SeekOrigin.Begin);

                        stream.Read(array, 0, array.Length);

                        if (CompareArrays(nameBytes, array))
                        {
                            string strName = String.Join("", Name).Trim('\0');

                            if (strName.Length < 21)
                            {
                                for (int j = 0; j < FATIndex.Alphabet.Length; j++)
                                {
                                    if (FindName(strName + FATIndex.Alphabet[j]))
                                    {
                                        strName += FATIndex.Alphabet[j];
                                        break;
                                    }

                                    if (j == FATIndex.Alphabet.Length - 1) throw new Exception("Недопустимое имя файла");
                                }
                            }
                            else
                            {
                                for (int j = 0; j < FATIndex.Alphabet.Length; j++)
                                {
                                    if (FindName(strName.TrimEnd(strName.Last()) + FATIndex.Alphabet[j]))
                                    {
                                        strName = strName.TrimEnd(strName.Last()) + FATIndex.Alphabet[j];
                                        break;
                                    }

                                    if (j == FATIndex.Alphabet.Length - 1) throw new Exception("Недопустимое имя файла");
                                }
                            }

                            var result = MessageBox.Show($"В этой папке уже существует элемент с таким названием\n" +
                                $"Заменить {String.Join("", Name).Trim('\0')} на {strName}", "", MessageBoxButtons.YesNo);

                            if (result == DialogResult.Yes) Entry.Name = strName.ToCharArray();
                            else return;
                        }

                        stream.Seek(stream.Position + 22, SeekOrigin.Begin);
                    }

                    stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                    array = new byte[4];

                    stream.Read(array, 0, 4);

                    UInt32 Index = BitConverter.ToUInt32(array, 0);

                    if (Index != FATIndex.EOC) CurrFatTemp = Index;
                    else
                    {
                        CurrFatTemp = CurrentFAT;

                        while (true)
                        {
                            array = new byte[42];

                            stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                            for (int i = 0; i < maxEntry; i++)
                            {
                                stream.Read(array, 0, array.Length);

                                if (CompareArrays(FATIndex.NullName, array) || array[0] == FATIndex.EmptyEntry)
                                {
                                    stream.Seek(stream.Position - 42, SeekOrigin.Begin);

                                    if ((Entry.Permissions & PermissionsClass.CatalogBit) == 0)
                                    {
                                        array = Encoding.Unicode.GetBytes(Entry.Name);
                                        stream.Write(array, 0, array.Length);

                                        array = BitConverter.GetBytes(Entry.Permissions);
                                        stream.Write(array, 0, array.Length);

                                        FSinfo.DeleteOneClaster();

                                        array = BitConverter.GetBytes(FATIndex.EOC);
                                        stream.Write(array, 0, array.Length);

                                        array = BitConverter.GetBytes(Entry.FileOfSize);
                                        stream.Write(array, 0, array.Length);

                                        array = BitConverter.GetBytes(Entry.OwnerID);
                                        stream.Write(array, 0, array.Length);

                                        array = BitConverter.GetBytes(Entry.GroupID);
                                        stream.Write(array, 0, array.Length);

                                        array = BitConverter.GetBytes(Entry.EditDate);
                                        stream.Write(array, 0, array.Length);

                                        array = BitConverter.GetBytes(Entry.CreateDate);
                                        stream.Write(array, 0, array.Length);

                                        WriteFileToClaster(Entry.Name, CopyOpenFile(Entry.FirstClusterIndex, Entry.FileOfSize * 2), CurrentFAT);
                                    }
                                    else
                                    {
                                        array = Encoding.Unicode.GetBytes(Entry.Name);
                                        stream.Write(array, 0, array.Length);

                                        UInt32 start = (UInt32)stream.Position - 42;

                                        stream.Seek(start, SeekOrigin.Begin);

                                        stream.Write(FATIndex.EmptyEntryArray, 0, 1);

                                        stream.Seek(start + 42, SeekOrigin.Begin);

                                        array = BitConverter.GetBytes(Entry.Permissions);
                                        stream.Write(array, 0, array.Length);

                                        UInt32 Next = FSinfo.NextFreeClaster;

                                        array = BitConverter.GetBytes(Next);
                                        stream.Write(array, 0, array.Length);

                                        array = BitConverter.GetBytes(Entry.FileOfSize);
                                        stream.Write(array, 0, array.Length);

                                        array = BitConverter.GetBytes(Entry.OwnerID);
                                        stream.Write(array, 0, array.Length);

                                        array = BitConverter.GetBytes(Entry.GroupID);
                                        stream.Write(array, 0, array.Length);

                                        array = BitConverter.GetBytes(Entry.EditDate);
                                        stream.Write(array, 0, array.Length);

                                        array = BitConverter.GetBytes(Entry.CreateDate);
                                        stream.Write(array, 0, array.Length);

                                        stream.Seek(BPBSize + FSinfoSize + (Next * NextSize), SeekOrigin.Begin);

                                        array = BitConverter.GetBytes(FATIndex.EOC);
                                        stream.Write(array, 0, array.Length);

                                        FSinfo.DeleteOneClaster();

                                        SeachNextFreeClaster();

                                        CopyOpenCatalog(Entry.FirstClusterIndex, Next);

                                        stream.Seek(start, SeekOrigin.Begin);

                                        array = Encoding.Unicode.GetBytes(Entry.Name);
                                        stream.Write(array, 0, 1);
                                    }

                                    return;
                                }

                                stream.Seek(stream.Position + 22, SeekOrigin.Begin);
                            }

                            stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                            array = new byte[4];

                            stream.Read(array, 0, 4);

                            Index = BitConverter.ToUInt32(array, 0);

                            if (Index != FATIndex.EOC) CurrFatTemp = Index;
                            else
                            {
                                stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                                CurrFatTemp = FSinfo.NextFreeClaster;

                                array = BitConverter.GetBytes(CurrFatTemp);
                                stream.Write(array, 0, array.Length);

                                stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                                array = BitConverter.GetBytes(FATIndex.EOC);

                                stream.Write(array, 0, array.Length);

                                FSinfo.DeleteOneClaster();

                                SeachNextFreeClaster();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string CopyOpenFile(UInt32 firstClaster, UInt32 fileSize)
        {
            try
            {
                string text = "";

                while (true)
                {
                    if (firstClaster != FATIndex.EOC)
                    {
                        byte[] array = new byte[ClasterSize];
                        byte[] next = new byte[NextSize];

                        while (fileSize >= ClasterSize)
                        {
                            // Текущий считываемый кластер
                            stream.Seek(BPBSize + FSinfoSize + FATCountSize + (firstClaster * ClasterSize), SeekOrigin.Begin);

                            stream.Read(array, 0, array.Length);

                            text += Encoding.Unicode.GetString(array, 0, array.Length);

                            // Текущий считываемый FAT
                            stream.Seek(BPBSize + FSinfoSize + (firstClaster * NextSize), SeekOrigin.Begin);

                            stream.Read(next, 0, next.Length);

                            UInt32 FATNextIndex = BitConverter.ToUInt32(next, 0);

                            if (FATNextIndex != FATIndex.EOC) // Если это  не последний кластер в стеке
                            {
                                firstClaster = FATNextIndex;
                            }

                            fileSize -= ClasterSize;
                        }

                        // Текущий считываемый кластер
                        stream.Seek(BPBSize + FSinfoSize + FATCountSize + (firstClaster * ClasterSize), SeekOrigin.Begin);

                        if (fileSize != 0)
                        {
                            array = new byte[fileSize];

                            stream.Read(array, 0, array.Length);

                            text += Encoding.Unicode.GetString(array, 0, array.Length);
                        }
                    }

                    return text;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void CopyOpenCatalog(UInt32 firstClaster, UInt32 newFirstClaster)
        {
            try
            {
                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array;

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (firstClaster * ClasterSize), SeekOrigin.Begin);

                    for (int i = 0; i < maxEntry; i++)
                    {
                        UInt32 next = FATIndex.EOC;

                        stream.Seek(BPBSize + FSinfoSize + FATCountSize + (firstClaster * ClasterSize) + (i * EntrySize), SeekOrigin.Begin);

                        array = new byte[42];

                        stream.Read(array, 0, array.Length);

                        if (array[0] == FATIndex.EmptyEntry) continue;

                        if (CompareArrays(array, FATIndex.NullName)) return;

                        char[] name = Encoding.Unicode.GetChars(array);

                        array = new byte[2];

                        stream.Read(array, 0, array.Length);

                        UInt16 perr = BitConverter.ToUInt16(array, 0);

                        array = new byte[4];

                        stream.Read(array, 0, array.Length);

                        UInt32 first = BitConverter.ToUInt32(array, 0);

                        stream.Read(array, 0, array.Length);

                        UInt32 fileSize = BitConverter.ToUInt32(array, 0);

                        array = new byte[2];

                        stream.Read(array, 0, array.Length);

                        UInt16 owner = BitConverter.ToUInt16(array, 0);

                        stream.Read(array, 0, array.Length);

                        UInt16 group = BitConverter.ToUInt16(array, 0);

                        stream.Seek(stream.Position + 8, SeekOrigin.Begin);

                        UInt32 date = (UInt32)new DateTimeOffset(DateTime.UtcNow.ToLocalTime()).ToUnixTimeSeconds();

                        Entry entry;

                        if ((perr & PermissionsClass.CatalogBit) == 0)
                        {
                            entry = new FileEntry(name, PermissionsClass.All_Permissions_File, next, fileSize, owner, group, date, date);

                            CreateFile(entry, newFirstClaster);

                            WriteFileToClaster(name, CopyOpenFile(first, fileSize * 2), newFirstClaster);
                        }
                        else
                        {
                            entry = new CatalogEntry(name, PermissionsClass.All_Permisisons_Catalog, next, owner, group, date, date);

                            UInt32 newClaster = CreateCatalog(entry, newFirstClaster);

                            CopyOpenCatalog(first, newClaster);
                        }
                    }

                    stream.Seek(BPBSize + FSinfoSize + (firstClaster * NextSize), SeekOrigin.Begin);

                    array = new byte[4];

                    stream.Read(array, 0, 4);

                    UInt32 Index = BitConverter.ToUInt32(array, 0);

                    if (Index != FATIndex.EOC) firstClaster = Index;
                    else return;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void CreateFile(Entry entry, UInt32 first)
        {
            try
            {
                UInt32 CurrFatTemp = first;

                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array = new byte[EntrySize];

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                    for (int i = 0; i < maxEntry; i++)
                    {
                        stream.Read(array, 0, EntrySize);

                        // Если место пустое или не занято
                        if (CompareArrays(array, FATIndex.NullEntryByte) || array[0] == FATIndex.EmptyEntry)
                        {
                            stream.Seek(stream.Position - 64, SeekOrigin.Begin);

                            // Запись файловой записи
                            char[] namech = entry.Name;
                            array = Encoding.Unicode.GetBytes(namech); // Запись Name
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.Permissions); // Запись Permissions
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.FirstClusterIndex); // Запись FirstClusterIndex
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.FileOfSize); // Запись FileOfSize
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.OwnerID); // Запись OwnerID
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.GroupID); // Запись GroupID 
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.EditDate); // Запись EditDate
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.CreateDate); // Запись CreateDate
                            stream.Write(array, 0, array.Length);

                            return;
                        }
                    }

                    stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                    array = new byte[4];

                    stream.Read(array, 0, 4);

                    UInt32 Index = BitConverter.ToUInt32(array, 0);

                    if (Index != FATIndex.EOC) CurrFatTemp = Index;
                    else
                    {
                        stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                        CurrFatTemp = FSinfo.NextFreeClaster;

                        array = BitConverter.GetBytes(CurrFatTemp);
                        stream.Write(array, 0, array.Length);

                        stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                        array = BitConverter.GetBytes(FATIndex.EOC);
                        stream.Write(array, 0, array.Length);

                        FSinfo.DeleteOneClaster();

                        SeachNextFreeClaster();
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private UInt32 CreateCatalog(Entry entry, UInt32 first)
        {
            try
            {
                UInt32 tempFirstClaster;

                UInt32 CurrFatTemp = first;

                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array = new byte[EntrySize];

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                    for (int i = 0; i < maxEntry; i++)
                    {
                        stream.Read(array, 0, EntrySize);

                        // Если место пустое или не занято
                        if (CompareArrays(array, FATIndex.NullEntryByte) || array[0] == FATIndex.EmptyEntry)
                        {
                            stream.Seek(stream.Position - 64, SeekOrigin.Begin);

                            // Запись Каталоговой записи
                            char[] namech = entry.Name;
                            byte[] temp = Encoding.Unicode.GetBytes(namech); // Запись Name
                            stream.Write(temp, 0, temp.Length);

                            array = BitConverter.GetBytes(entry.Permissions); // Запись Permissions
                            stream.Write(array, 0, array.Length);

                            tempFirstClaster = FSinfo.NextFreeClaster;

                            array = BitConverter.GetBytes(tempFirstClaster); // Запись FirstClusterIndex
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.FileOfSize); // Запись FileOfSize -- всегда 0
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.OwnerID); // Запись OwnerID
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.GroupID); // Запись GroupID 
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.EditDate); // Запись EditDate
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.CreateDate); // Запись CreateDate
                            stream.Write(array, 0, array.Length);

                            stream.Seek(BPBSize + FSinfoSize + (tempFirstClaster * NextSize), SeekOrigin.Begin);

                            array = BitConverter.GetBytes(FATIndex.EOC);
                            stream.Write(array, 0, array.Length);

                            FSinfo.DeleteOneClaster();

                            SeachNextFreeClaster();

                            return tempFirstClaster;
                        }
                    }

                    stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                    array = new byte[4];

                    stream.Read(array, 0, 4);

                    UInt32 Index = BitConverter.ToUInt32(array, 0);

                    if (Index != FATIndex.EOC) CurrFatTemp = Index;
                    else
                    {
                        stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                        CurrFatTemp = FSinfo.NextFreeClaster;

                        array = BitConverter.GetBytes(CurrFatTemp);
                        stream.Write(array, 0, array.Length);

                        stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                        array = BitConverter.GetBytes(FATIndex.EOC);

                        stream.Write(array, 0, array.Length);

                        FSinfo.DeleteOneClaster();

                        SeachNextFreeClaster();
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EnterMethod();
        }

        private void EnterMethod()
        {
            try
            {
                int number = -1;

                if (PerrmissionsWriteCheck.Count != 0 && !PerrmissionsWriteCheck.Peek()) throw new Exception("У вас нет прав на редактирование этой папки");

                char[] name;
                if (Entry != null)
                {
                    if (!Copy)
                    {
                        name = Entry.Name;
                        EnterEntry(name);
                    }
                    else
                    {
                        name = Entry.Name;
                        CopyEnterEntry(name);
                    }
                }

                ReadAllFileToTable();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButtons.OK);
                ReadAllFileToTable();
            }
        }

        private void EnterEntry(char[] Name)
        {
            try
            {
                ChechCatalogFirstClaster(Entry);

                UInt32 CurrFatTemp = CurrentFAT;

                byte[] nameBytes = Encoding.Unicode.GetBytes(Name);

                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array = new byte[42];

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                    for (int i = 0; i < maxEntry; i++)
                    {
                        stream.Read(array, 0, array.Length);

                        if (CompareArrays(nameBytes, array))
                        {
                            if (stream.Position - 42 == cutIndex)
                            {
                                MessageBox.Show("Нельзя переместить элемент в эту папку\nЭтот файл уже содержится в этой папке", "Перемещение", MessageBoxButtons.OK);
                                return;
                            }
                            else
                            {
                                string strName = String.Join("", Name).Trim('\0');

                                if (strName.Length < 21)
                                {
                                    for (int j = 0; j < FATIndex.Alphabet.Length; j++)
                                    {
                                        if (FindName(strName + FATIndex.Alphabet[j]))
                                        {
                                            strName += FATIndex.Alphabet[j];
                                            break;
                                        }

                                        if (j == FATIndex.Alphabet.Length - 1) throw new Exception("Недопустимое имя файла");
                                    }
                                }
                                else
                                {
                                    for (int j = 0; j < FATIndex.Alphabet.Length; j++)
                                    {
                                        if (FindName(strName.TrimEnd(strName.Last()) + FATIndex.Alphabet[j]))
                                        {
                                            strName = strName.TrimEnd(strName.Last()) + FATIndex.Alphabet[j];
                                            break;
                                        }

                                        if (j == FATIndex.Alphabet.Length - 1) throw new Exception("Недопустимое имя файла");
                                    }
                                }

                                var result = MessageBox.Show($"В этой папке уже существует элемент с таким названием\n" +
                                    $"Заменить {String.Join("", Name).Trim('\0')} на {strName}", "", MessageBoxButtons.YesNo);

                                if (result == DialogResult.Yes)Entry.Name = strName.ToCharArray();
                                else return;
                            }
                        }

                        stream.Seek(stream.Position + 22, SeekOrigin.Begin);
                    }

                    stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                    array = new byte[4];

                    stream.Read(array, 0, 4);

                    UInt32 Index = BitConverter.ToUInt32(array, 0);

                    if (Index != FATIndex.EOC) CurrFatTemp = Index;
                    else
                    {
                        CurrFatTemp = CurrentFAT;

                        while (true)
                        {
                            array = new byte[42];

                            stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                            for (int i = 0; i < maxEntry; i++)
                            {
                                stream.Read(array, 0, array.Length);

                                if (CompareArrays(FATIndex.NullName, array) || array[0] == FATIndex.EmptyEntry)
                                {
                                    stream.Seek(stream.Position - 42, SeekOrigin.Begin);

                                    // Запись файловой записи
                                    char[] namech = Entry.Name;
                                    array = Encoding.Unicode.GetBytes(namech); // Запись Name
                                    stream.Write(array, 0, array.Length);

                                    array = BitConverter.GetBytes(Entry.Permissions); // Запись Permissions
                                    stream.Write(array, 0, array.Length);

                                    array = BitConverter.GetBytes(Entry.FirstClusterIndex); // Запись FirstClusterIndex
                                    stream.Write(array, 0, array.Length);

                                    array = BitConverter.GetBytes(Entry.FileOfSize); // Запись FileOfSize
                                    stream.Write(array, 0, array.Length);

                                    array = BitConverter.GetBytes(Entry.OwnerID); // Запись OwnerID
                                    stream.Write(array, 0, array.Length);

                                    array = BitConverter.GetBytes(Entry.GroupID); // Запись GroupID 
                                    stream.Write(array, 0, array.Length);

                                    array = BitConverter.GetBytes(Entry.EditDate); // Запись EditDate
                                    stream.Write(array, 0, array.Length);

                                    array = BitConverter.GetBytes(Entry.CreateDate); // Запись CreateDate
                                    stream.Write(array, 0, array.Length);

                                    stream.Seek(cutIndex, SeekOrigin.Begin);

                                    stream.Write(FATIndex.EmptyEntryArray, 0, 1);

                                    Entry = null;

                                    cutIndex = 0;

                                    return;
                                }

                                stream.Seek(stream.Position + 22, SeekOrigin.Begin);
                            }

                            stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                            array = new byte[4];

                            stream.Read(array, 0, 4);

                            Index = BitConverter.ToUInt32(array, 0);

                            if (Index != FATIndex.EOC) CurrFatTemp = Index;
                            else
                            {
                                stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                                CurrFatTemp = FSinfo.NextFreeClaster;

                                array = BitConverter.GetBytes(CurrFatTemp);
                                stream.Write(array, 0, array.Length);

                                stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                                array = BitConverter.GetBytes(FATIndex.EOC);

                                stream.Write(array, 0, array.Length);

                                FSinfo.DeleteOneClaster();

                                SeachNextFreeClaster();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private Entry ReturnEntry(char[] Name, int number, bool check = false)
        {
            try
            {
                UInt32 CurrFatTemp = CurrentFAT;

                byte[] nameBytes = Encoding.Unicode.GetBytes(Name);

                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array = new byte[42];

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                    for (int i = 0; i < maxEntry; i++)
                    {
                        stream.Read(array, 0, array.Length);

                        // Если место пустое или не занято
                        if (CompareArrays(nameBytes, array))
                        {
                            cutIndex = stream.Position - 42;

                            array = new byte[2];

                            stream.Read(array, 0, array.Length);

                            UInt16 permissions = BitConverter.ToUInt16(array, 0);

                            array = new byte[4];

                            stream.Read(array, 0, array.Length);

                            if (check && !PermissionsMethod(number, permissions, PermissionsClass.Write_Owner, PermissionsClass.Write_Group,
                                PermissionsClass.Write_Other)) throw new Exception("Вы не можете переместить этот файл");

                            UInt32 firstClaster = BitConverter.ToUInt32(array, 0);

                            stream.Read(array, 0, array.Length);

                            UInt32 fileSize = BitConverter.ToUInt32(array, 0);

                            array = new byte[2];

                            stream.Read(array, 0, array.Length);

                            UInt16 owner = BitConverter.ToUInt16(array, 0);

                            stream.Read(array, 0, array.Length);

                            UInt16 group = BitConverter.ToUInt16(array, 0);

                            array = new byte[4];

                            stream.Read(array, 0, array.Length);

                            UInt32 createDate = BitConverter.ToUInt32(array, 0);

                            stream.Read(array, 0, array.Length);

                            UInt32 editDate = BitConverter.ToUInt32(array, 0);

                            Entry entry;

                            if ((permissions & PermissionsClass.CatalogBit) == 0)
                                entry = new FileEntry(Name, permissions, firstClaster, fileSize, owner, group, editDate, createDate);
                            else entry = new CatalogEntry(Name, permissions, firstClaster, owner, group, editDate, createDate);

                            return entry;
                        }

                        stream.Seek(stream.Position + 22, SeekOrigin.Begin);
                    }

                    stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                    array = new byte[4];

                    stream.Read(array, 0, 4);

                    UInt32 Index = BitConverter.ToUInt32(array, 0);

                    if (Index != FATIndex.EOC) CurrFatTemp = Index;
                    else return null;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void OverwriteEntry(Entry entry, char[] Name)
        {
            try
            {
                UInt32 CurrFatTemp = CurrentFAT;

                byte[] nameBytes = Encoding.Unicode.GetBytes(Name);

                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array = new byte[42];

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                    for (int i = 0; i < maxEntry; i++)
                    {
                        stream.Read(array, 0, array.Length);

                        // Если место пустое или не занято
                        if (CompareArrays(nameBytes, array))
                        {
                            stream.Seek(stream.Position - 42, SeekOrigin.Begin);

                            // Запись файловой записи
                            char[] namech = entry.Name;
                            array = Encoding.Unicode.GetBytes(namech); // Запись Name
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.Permissions); // Запись Permissions
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.FirstClusterIndex); // Запись FirstClusterIndex
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.FileOfSize); // Запись FileOfSize
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.OwnerID); // Запись OwnerID
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.GroupID); // Запись GroupID 
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.EditDate); // Запись EditDate
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(entry.CreateDate); // Запись CreateDate
                            stream.Write(array, 0, array.Length);

                            return;
                        }

                        stream.Seek(stream.Position + 22, SeekOrigin.Begin);
                    }

                    stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                    array = new byte[4];

                    stream.Read(array, 0, 4);

                    UInt32 Index = BitConverter.ToUInt32(array, 0);

                    if (Index != FATIndex.EOC) CurrFatTemp = Index;
                    else return;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void открытьFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMethod();
        }

        private void OpenMethod()
        {
            try
            {
                tempString = "";

                if (catalogTable.SelectedRows[0].Index == -1) return;

                string name = catalogTable.SelectedRows[0].Cells[0].Value.ToString();

                if (catalogTable.SelectedRows[0].Cells[1].Value.ToString() == "Файл")
                {
                    string text = "";
                    int number = -1;
                    if (catalogTable.SelectedRows[0].Cells[4].Value.ToString() == String.Join("", OwnerName)) number = 0;
                    else if (catalogTable.SelectedRows[0].Cells[5].Value.ToString() == String.Join("", GroupName)) number = 1;
                    else number = 2;

                    text = OpenFile(name, number);

                    text = text.Replace("\0", "");

                    TextEditor textEditor = new TextEditor(name, text, this, PermissionsMethod(number, PerrmisionsFileTemp, PermissionsClass.Write_Owner, PermissionsClass.Write_Group, PermissionsClass.Write_Other));
                    textEditor.ShowDialog();

                    if (text != tempString)
                        WriteFileToClaster(name.ToCharArray(), tempString);
                }
                else if (catalogTable.SelectedRows[0].Cells[1].Value.ToString() == "Папка")
                {
                    if (catalogTable.SelectedRows[0].Cells[4].Value.ToString() == String.Join("", OwnerName)) OpenCatalog(name, 0);
                    else if (catalogTable.SelectedRows[0].Cells[5].Value.ToString() == String.Join("", GroupName)) OpenCatalog(name, 1);
                    else OpenCatalog(name, 2);

                    label1.Text = "$/";

                    for (int i = 0; i < NameStack.Count; i++)
                    {
                        label1.Text += NameStack.First().ToString().Trim('\0') + "/";
                        NameStack.Add(NameStack.First());
                        NameStack.RemoveAt(0);
                    }
                }

                ReadAllFileToTable();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButtons.OK);
                ReadAllFileToTable();
            }
        }

        private void удалитьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DeleteMethod();
        }

        private void DeleteMethod()
        {
            try
            {
                int number = -1;

                if (PerrmissionsWriteCheck.Count != 0 && !PerrmissionsWriteCheck.Peek()) throw new Exception("У вас нет прав на редактирование этой папки");

                if (catalogTable.SelectedRows[0].Index == -1) return;

                string name = catalogTable.SelectedRows[0].Cells[0].Value.ToString();

                if (name[0] == '%') throw new Exception("Этот файл нельзя удалить");

                if (catalogTable.SelectedRows[0].Cells[1].Value.ToString() == "Файл")
                {
                    if (catalogTable.SelectedRows[0].Cells[4].Value.ToString() == String.Join("", OwnerName)) DeleteFile(name.ToCharArray(), 0);
                    else if (catalogTable.SelectedRows[0].Cells[5].Value.ToString() == String.Join("", GroupName)) DeleteFile(name.ToCharArray(), 1);
                    else DeleteFile(name.ToCharArray(), 2);
                }
                else if (catalogTable.SelectedRows[0].Cells[1].Value.ToString() == "Папка")
                {
                    if (catalogTable.SelectedRows[0].Cells[4].Value.ToString() == String.Join("", OwnerName)) DeleteCatalog(name.ToCharArray(), 0);
                    else if (catalogTable.SelectedRows[0].Cells[5].Value.ToString() == String.Join("", GroupName)) DeleteCatalog(name.ToCharArray(), 1);
                    else DeleteCatalog(name.ToCharArray(), 2);
                }

                ReadAllFileToTable();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButtons.OK);
                ReadAllFileToTable();
            }
        }

        private void CreateFile(char[] Name)
        {
            try
            {
                UInt32 CurrFatTemp = CurrentFAT;

                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array = new byte[EntrySize];

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                    for (int i = 0; i < maxEntry; i++)
                    {
                        stream.Read(array, 0, EntrySize);

                        // Если место пустое или не занято
                        if (CompareArrays(array, FATIndex.NullEntryByte) || array[0] == FATIndex.EmptyEntry)
                        {
                            stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize) + (i * EntrySize), SeekOrigin.Begin);

                            UInt32 date = (UInt32)new DateTimeOffset(DateTime.UtcNow.ToLocalTime()).ToUnixTimeSeconds();

                            FileEntry fileEntry = new FileEntry(Name, FATIndex.EOC, OwnerID, GroupID, date);

                            // Запись файловой записи
                            char[] namech = fileEntry.Name;
                            array = Encoding.Unicode.GetBytes(namech); // Запись Name
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(fileEntry.Permissions); // Запись Permissions
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(fileEntry.FirstClusterIndex); // Запись FirstClusterIndex
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(fileEntry.FileOfSize); // Запись FileOfSize
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(fileEntry.OwnerID); // Запись OwnerID
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(fileEntry.GroupID); // Запись GroupID 
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(fileEntry.EditDate); // Запись EditDate
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(fileEntry.CreateDate); // Запись CreateDate
                            stream.Write(array, 0, array.Length);

                            return;
                        }
                    }

                    stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                    array = new byte[4];

                    stream.Read(array, 0, 4);

                    UInt32 Index = BitConverter.ToUInt32(array, 0);

                    if (Index != FATIndex.EOC) CurrFatTemp = Index;
                    else
                    {
                        stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                        CurrFatTemp = FSinfo.NextFreeClaster;

                        array = BitConverter.GetBytes(CurrFatTemp);
                        stream.Write(array, 0, array.Length);

                        stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                        array = BitConverter.GetBytes(FATIndex.EOC);

                        stream.Write(array, 0, array.Length);

                        FSinfo.DeleteOneClaster();

                        SeachNextFreeClaster();
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void CreateCatalog(char[] Name)
        {
            try
            {
                UInt32 CurrFatTemp = CurrentFAT;

                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array = new byte[EntrySize];

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                    for (int i = 0; i < maxEntry; i++)
                    {
                        stream.Read(array, 0, EntrySize);

                        // Если место пустое или не занято
                        if (CompareArrays(array, FATIndex.NullEntryByte) || array[0] == FATIndex.EmptyEntry)
                        {
                            stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize) + (i * EntrySize), SeekOrigin.Begin);
                            
                            UInt32 date = (UInt32)new DateTimeOffset(DateTime.UtcNow.ToLocalTime()).ToUnixTimeSeconds();

                            // ---------------------------------------------- //

                            CatalogEntry catalogEntry = new CatalogEntry(Name, FSinfo.NextFreeClaster, OwnerID, GroupID, date);

                            // Запись Каталоговой записи
                            char[] namech = catalogEntry.Name;
                            byte[] temp = Encoding.Unicode.GetBytes(namech); // Запись Name
                            stream.Write(temp, 0, temp.Length);

                            array = BitConverter.GetBytes(catalogEntry.Permissions); // Запись Permissions
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(catalogEntry.FirstClusterIndex); // Запись FirstClusterIndex
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(catalogEntry.FileOfSize); // Запись FileOfSize -- всегда 0
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(catalogEntry.OwnerID); // Запись OwnerID
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(catalogEntry.GroupID); // Запись GroupID 
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(catalogEntry.EditDate); // Запись EditDate
                            stream.Write(array, 0, array.Length);

                            array = BitConverter.GetBytes(catalogEntry.CreateDate); // Запись CreateDate
                            stream.Write(array, 0, array.Length);

                            stream.Seek(BPBSize + FSinfoSize + (catalogEntry.FirstClusterIndex * NextSize), SeekOrigin.Begin);

                            array = BitConverter.GetBytes(FATIndex.EOC);
                            stream.Write(array, 0, array.Length);

                            FSinfo.DeleteOneClaster();

                            SeachNextFreeClaster();

                            return;
                        }
                    }

                    stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                    array = new byte[4];

                    stream.Read(array, 0, 4);

                    UInt32 Index = BitConverter.ToUInt32(array, 0);

                    if (Index != FATIndex.EOC) CurrFatTemp = Index;
                    else
                    {
                        stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                        CurrFatTemp = FSinfo.NextFreeClaster;

                        array = BitConverter.GetBytes(CurrFatTemp);
                        stream.Write(array, 0, array.Length);

                        stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                        array = BitConverter.GetBytes(FATIndex.EOC);

                        stream.Write(array, 0, array.Length);

                        FSinfo.DeleteOneClaster();

                        SeachNextFreeClaster();
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void DeleteFile(char[] Name, int number, UInt32 FATTemp = 0x0FFFFFFF)
        {
            UInt32 Previous = FATIndex.EOC;

            try
            {
                byte[] nameBytes = Encoding.Unicode.GetBytes(Name);

                UInt32 CurrFatTemp;

                if (FATTemp == 0x0FFFFFFF) CurrFatTemp = CurrentFAT;
                else CurrFatTemp = FATTemp;

                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array = new byte[nameBytes.Length];
                    int length = nameBytes.Length;

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                    for (int i = 0; i < maxEntry; i++)
                    {
                        stream.Read(array, 0, length);

                        if (CompareArrays(array, nameBytes))
                        {
                            if (stream.Position - 42 == cutIndex)
                            {
                                Entry = null;
                                cutIndex = 0;
                                Copy = false;
                            }

                            array = new byte[2];

                            stream.Read(array, 0, array.Length);

                            UInt16 perr = BitConverter.ToUInt16(array, 0);

                            if (!PermissionsMethod(number, perr, PermissionsClass.Write_Owner, PermissionsClass.Write_Group, PermissionsClass.Write_Other)) throw new Exception("У вас нет прав чтобы удалить этот файл");

                            stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize) + (i * EntrySize), SeekOrigin.Begin);

                            stream.Write(FATIndex.EmptyEntryArray, 0, 1);

                            stream.Seek(stream.Position + 43, SeekOrigin.Begin);

                            array = new byte[NextSize];

                            stream.Read(array, 0, array.Length);

                            UInt32 startFATIndex = BitConverter.ToUInt32(array, 0);

                            while (startFATIndex != FATIndex.EOC)
                            {
                                stream.Seek(BPBSize + FSinfoSize + FATCountSize + (startFATIndex * ClasterSize), SeekOrigin.Begin);

                                array = FATIndex.NullClasterByte;

                                stream.Write(array, 0, array.Length);

                                stream.Seek(BPBSize + FSinfoSize + (startFATIndex * NextSize), SeekOrigin.Begin);

                                array = new byte[NextSize];

                                stream.Read(array, 0, array.Length);

                                startFATIndex = BitConverter.ToUInt32(array, 0);

                                stream.Seek(stream.Position - 4, SeekOrigin.Begin);

                                array = BitConverter.GetBytes(FATIndex.NOT_USING);

                                stream.Write(array, 0, array.Length);

                                FSinfo.AddOneCLaster();
                            }

                            stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                            array = new byte[EntrySize];

                            for (int j = 0; j < maxEntry; j++)
                            {
                                stream.Read(array, 0, EntrySize);

                                if (!CompareArrays(array, FATIndex.NullEntryByte) && (array[0] != FATIndex.EmptyEntry)) return;
                            }

                            stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                            byte[] next = new byte[4];

                            stream.Read(next, 0, NextSize);

                            if (BitConverter.ToUInt32(next, 0) != FATIndex.EOC && Previous != FATIndex.EOC)
                            {
                                stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                                next = BitConverter.GetBytes(FATIndex.NOT_USING);

                                stream.Write(next, 0, next.Length);

                                stream.Seek(BPBSize + FSinfoSize + (Previous * NextSize), SeekOrigin.Begin);

                                stream.Write(next, 0, next.Length);

                                FSinfo.AddOneCLaster();
                            }
                            else if (Previous != FATIndex.EOC)
                            {
                                stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                                next = BitConverter.GetBytes(FATIndex.NOT_USING);

                                stream.Write(next, 0, next.Length);

                                stream.Seek(BPBSize + FSinfoSize + (Previous * NextSize), SeekOrigin.Begin);

                                next = BitConverter.GetBytes(FATIndex.EOC);

                                stream.Write(next, 0, next.Length);

                                FSinfo.AddOneCLaster();
                            }

                            SeachNextFreeClaster();

                            break;
                        }

                        stream.Seek(stream.Position + 22, SeekOrigin.Begin);
                    }

                    stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                    array = new byte[4];

                    stream.Read(array, 0, 4);

                    UInt32 Index = BitConverter.ToUInt32(array, 0);

                    if (Index != FATIndex.EOC)
                    {
                        Previous = CurrFatTemp;
                        CurrFatTemp = Index;
                    }
                    else return;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private bool DeleteCatalog(char[] Name, int number, UInt32 currFATTemp = 0x0FFFFFFF)
        {
            UInt32 Previous = FATIndex.EOC;
            UInt32 Index;

            try
            {
                byte[] nameBytes = Encoding.Unicode.GetBytes(Name);

                UInt32 CurrFatTemp;

                if (currFATTemp == 0x0FFFFFFF) CurrFatTemp = CurrentFAT;
                else CurrFatTemp = currFATTemp;

                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array = new byte[nameBytes.Length];
                    int length = nameBytes.Length;

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                    for (int i = 0; i < maxEntry; i++) // Поиск папки в стеке кластеров
                    {
                        stream.Read(array, 0, length);

                        if (CompareArrays(array, nameBytes)) // Папка найдена
                        {
                            if (stream.Position - 42 == cutIndex)
                            {
                                Entry = null;
                                cutIndex = 0;
                                Copy = false;
                            }

                            byte[] name = array; // Имя удаляемой папки

                            array = new byte[2];

                            stream.Read(array, 0, array.Length);

                            UInt16 perr = BitConverter.ToUInt16(array, 0); // атрибуты папки

                            if (!PermissionsMethod(number, perr, PermissionsClass.Write_Owner, PermissionsClass.Write_Group, PermissionsClass.Write_Other)) throw new Exception("У вас нет прав чтобы удалить эту папку");

                            array = new byte[4];

                            stream.Read(array, 0, array.Length);

                            UInt32 firstClaster = BitConverter.ToUInt32(array, 0); // Первый индекс

                            for (int j = 0; j < maxEntry; j++) // Поиск папки в стеке кластеров
                            {
                                stream.Seek(BPBSize + FSinfoSize + FATCountSize + (firstClaster * ClasterSize) + (j * EntrySize), SeekOrigin.Begin);

                                array = new byte[42];

                                stream.Read(array, 0, array.Length);

                                if (!CompareArrays(array, FATIndex.NullName))
                                {
                                    if (array[0] == FATIndex.EmptyEntry) continue;

                                    name = array;

                                    array = new byte[2];

                                    stream.Read(array, 0, array.Length);

                                    perr = BitConverter.ToUInt16(array, 0); // атрибуты

                                    stream.Seek(stream.Position + 4, SeekOrigin.Begin);

                                    stream.Read(array, 0, array.Length);

                                    UInt16 ownerid = BitConverter.ToUInt16(array, 0);

                                    stream.Read(array, 0, array.Length);

                                    UInt16 groupid = BitConverter.ToUInt16(array, 0);

                                    try
                                    {
                                        int num = -1;
                                        if (ownerid == OwnerID) num = 0;
                                        else if (groupid == GroupID) num = 1;
                                        else num = 2;

                                        if ((perr & PermissionsClass.CatalogBit) == 0)
                                        {
                                            DeleteFile(Encoding.Unicode.GetChars(name), num, firstClaster);
                                        }
                                        else
                                        {
                                            if (!DeleteCatalog(Encoding.Unicode.GetChars(name), num, firstClaster)) return false;
                                        }

                                        continue;
                                    }
                                    catch
                                    {
                                        var res = MessageBox.Show("Не возможно удалить файл или папку\n" +
                                            $"Имя: {Encoding.Unicode.GetString(name)}\n" +
                                            "Пропустить и продолжить удаление?", "Удаление", MessageBoxButtons.YesNo);

                                        if (res == DialogResult.No) return false;

                                        continue;
                                    }
                                }

                                stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize) + (j * EntrySize), SeekOrigin.Begin);

                                stream.Write(FATIndex.EmptyEntryArray, 0, 1);

                                stream.Seek(BPBSize + FSinfoSize + firstClaster * NextSize, SeekOrigin.Begin);

                                array = BitConverter.GetBytes(FATIndex.NOT_USING);

                                stream.Write(array, 0, array.Length);

                                FSinfo.AddOneCLaster();

                                SeachNextFreeClaster();

                                return true;
                            }

                            stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                            array = new byte[4];

                            stream.Read(array, 0, 4);

                            Index = BitConverter.ToUInt32(array, 0);

                            if (Index != FATIndex.EOC)
                            {
                                Previous = CurrFatTemp;
                                CurrFatTemp = Index;
                            }
                            else return true;
                        }

                        stream.Seek(stream.Position + 22, SeekOrigin.Begin);
                    }

                    stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                    array = new byte[4];

                    stream.Read(array, 0, 4);

                    Index = BitConverter.ToUInt32(array, 0);

                    if (Index != FATIndex.EOC)
                    {
                        Previous = CurrFatTemp;
                        CurrFatTemp = Index;
                    }
                    else return true;
                }

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string OpenFile(string name, int number)
        {
            try
            {
                string text = "";
                byte[] nameBytes = Encoding.Unicode.GetBytes(name);

                UInt32 CurrFatTemp = CurrentFAT;

                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array = new byte[nameBytes.Length];
                    int length = nameBytes.Length;

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                    for (int i = 0; i < maxEntry; i++)
                    {
                        stream.Read(array, 0, length);

                        if (CompareArrays(array, nameBytes))
                        {
                            array = new byte[2];

                            stream.Read(array, 0, array.Length);

                            UInt16 perr = BitConverter.ToUInt16(array, 0);

                            if (!PermissionsMethod(number, perr, PermissionsClass.Read_Owner, PermissionsClass.Read_Group, PermissionsClass.Read_Other)) throw new Exception("У вас нет прав чтобы открыть этот файл");

                            PerrmisionsFileTemp = perr;

                            array = new byte[4];

                            stream.Read(array, 0, array.Length);

                            UInt32 firstClasterIndex = BitConverter.ToUInt32(array, 0);

                            if (firstClasterIndex != FATIndex.EOC)
                            {
                                stream.Read(array, 0, array.Length);

                                UInt32 fileSize = BitConverter.ToUInt32(array, 0) * 2;

                                array = new byte[ClasterSize];
                                byte[] next = new byte[NextSize];

                                while (fileSize >= ClasterSize)
                                {
                                    // Текущий считываемый кластер
                                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (firstClasterIndex * ClasterSize), SeekOrigin.Begin);

                                    stream.Read(array, 0, array.Length);

                                    text += Encoding.Unicode.GetString(array, 0, array.Length);

                                    // Текущий считываемый FAT
                                    stream.Seek(BPBSize + FSinfoSize + (firstClasterIndex * NextSize), SeekOrigin.Begin);

                                    stream.Read(next, 0, next.Length);

                                    UInt32 FATNextIndex = BitConverter.ToUInt32(next, 0);

                                    if (FATNextIndex != FATIndex.EOC) // Если это  не последний кластер в стеке
                                    {
                                        firstClasterIndex = FATNextIndex;
                                    }

                                    fileSize -= ClasterSize;

                                    FSinfo.AddOneCLaster();
                                }

                                // Текущий считываемый кластер
                                stream.Seek(BPBSize + FSinfoSize + FATCountSize + (firstClasterIndex * ClasterSize), SeekOrigin.Begin);

                                if (fileSize != 0)
                                {
                                    array = new byte[fileSize];

                                    stream.Read(array, 0, array.Length);

                                    text += Encoding.Unicode.GetString(array, 0, array.Length);
                                }
                            }

                            return text;
                        }

                        stream.Seek(stream.Position + 22, SeekOrigin.Begin);
                    }

                    stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                    array = new byte[4];

                    stream.Read(array, 0, 4);

                    UInt32 Index = BitConverter.ToUInt32(array, 0);

                    if (Index != FATIndex.EOC) CurrFatTemp = Index;
                    else throw new Exception("Такого файла не существует");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void OpenCatalog(string name, int number)
        {
            try
            {
                string text = "";
                byte[] nameBytes = Encoding.Unicode.GetBytes(name);


                UInt32 CurrFatTemp = CurrentFAT;

                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array = new byte[nameBytes.Length];
                    int length = nameBytes.Length;

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                    for (int i = 0; i < maxEntry; i++)
                    {
                        stream.Read(array, 0, length);

                        if (CompareArrays(array, nameBytes))
                        {
                            array = new byte[2];

                            stream.Read(array, 0, array.Length);

                            UInt16 perr = BitConverter.ToUInt16(array, 0);

                            if (!PermissionsMethod(number, perr, PermissionsClass.Read_Owner, PermissionsClass.Read_Group, PermissionsClass.Read_Other)) throw new Exception("У вас нет прав чтобы открыть эту папку");

                            array = new byte[4];

                            stream.Read(array, 0, array.Length);

                            CatalogStack.Push(CurrentFAT);
                            NameStack.Add(name);
                            PerrmissionsWriteCheck.Push(PermissionsMethod(number, perr, PermissionsClass.Write_Owner, PermissionsClass.Write_Group, PermissionsClass.Write_Other));

                            CurrentFAT = BitConverter.ToUInt32(array, 0);

                            return;
                        }

                        stream.Seek(stream.Position + 22, SeekOrigin.Begin);
                    }

                    stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                    array = new byte[4];

                    stream.Read(array, 0, 4);

                    UInt32 Index = BitConverter.ToUInt32(array, 0);

                    if (Index != FATIndex.EOC) CurrFatTemp = Index;
                    else throw new Exception("Такого файла не существует");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void WriteFileToClaster(char[] name, string text, UInt32 currTempFat = 0x0FFFFFFF)
        {
            try
            {
                byte[] nameBytes = Encoding.Unicode.GetBytes(name);

                UInt32 CurrFatTemp;

                if (currTempFat == FATIndex.EOC) CurrFatTemp = CurrentFAT;
                else CurrFatTemp = currTempFat;

                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array = new byte[nameBytes.Length];
                    int length = nameBytes.Length;

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                    for (int i = 0; i < maxEntry; i++)
                    {
                        stream.Read(array, 0, length);

                        if (CompareArrays(array, nameBytes))
                        {
                            array = new byte[4];

                            long start = stream.Position + 2;

                            stream.Seek(start, SeekOrigin.Begin);

                            stream.Read(array, 0, array.Length);

                            if (text.Length == 0) // Если введенный текст пуст
                            {
                                UInt32 next = BitConverter.ToUInt32(array, 0);

                                if (next != FATIndex.EOC)
                                {
                                    while (true)
                                    {
                                        stream.Seek(BPBSize + FSinfoSize + (next * NextSize), SeekOrigin.Begin);

                                        stream.Read(array, 0, array.Length);

                                        next = BitConverter.ToUInt32(array, 0);

                                        if (next == FATIndex.EOC)
                                            break;

                                        array = BitConverter.GetBytes(FATIndex.EOC);

                                        stream.Seek(stream.Position - 4, SeekOrigin.Begin);

                                        stream.Write(array, 0, array.Length);
                                    }
                                }

                                stream.Seek(start, SeekOrigin.Begin);

                                array = BitConverter.GetBytes(FATIndex.EOC);

                                stream.Write(array, 0, array.Length);

                                array = BitConverter.GetBytes(0);

                                stream.Write(array, 0, array.Length);

                                return;
                            }

                            UInt32 tempNext = BitConverter.ToUInt32(array, 0);

                            if (tempNext == FATIndex.EOC)
                            {
                                tempNext = FSinfo.NextFreeClaster;

                                stream.Seek(stream.Position - 4, SeekOrigin.Begin);

                                // Записываем первый кластер в стеке
                                array = BitConverter.GetBytes(tempNext);
                                stream.Write(array, 0, array.Length);

                                // Записываем размер файла
                                array = BitConverter.GetBytes(text.Length);
                                stream.Write(array, 0, array.Length);

                                // Перемещаем указатель на новый FAT
                                stream.Seek(BPBSize + FSinfoSize + (tempNext * NextSize), SeekOrigin.Begin);

                                array = BitConverter.GetBytes(FATIndex.EOC);
                                stream.Write(array, 0, array.Length);

                                // Ищем новый свободный кластер
                                FSinfo.DeleteOneClaster();

                                SeachNextFreeClaster();
                            }
                            else
                            {
                                // Записываем размер файла
                                array = BitConverter.GetBytes(text.Length);
                                stream.Write(array, 0, array.Length);
                            }

                            // Перемещаем указатель на новый FAT
                            stream.Seek(BPBSize + FSinfoSize + (tempNext * NextSize), SeekOrigin.Begin);

                            // Считываем старый FAT
                            stream.Read(array, 0, array.Length);

                            UInt32 previousNext = BitConverter.ToUInt32(array, 0);

                            // Возвращаем указатель на новый FAT
                            stream.Seek(stream.Position - 4, SeekOrigin.Begin);

                            // Запись EOC в новый FAT
                            array = BitConverter.GetBytes(FATIndex.EOC);

                            stream.Write(array, 0, array.Length);

                            int startIndex = 0;
                            char[] mass = new char[0];

                            while (startIndex < text.Length)
                            {
                                // Переместили указатель на текущий кластер
                                stream.Seek(BPBSize + FSinfoSize + FATCountSize + (tempNext * ClasterSize), SeekOrigin.Begin);

                                // Проверили оставшуюся длину строки
                                if (startIndex <= (text.Length - ClasterSimbolSize)) // Если в строке осталось больше/равно элементов чем в кластере
                                {
                                    mass = new char[ClasterSimbolSize];
                                    for (int j = 0; j < mass.Length; j++) mass[j] = text[j + startIndex]; // Записывает информацию в массив char
                                }
                                else // Если элементов в строке осталось меньше чем в кластере
                                {
                                    mass = new char[text.Length - startIndex];
                                    for (int j = 0; j < mass.Length; j++) mass[j] = text[j + startIndex]; // Записывает информацию в массив char
                                }

                                startIndex += ClasterSimbolSize;

                                byte[] textArray = Encoding.Unicode.GetBytes(mass); // Encoding в массив byte

                                // Записывает информацию в кластер
                                stream.Write(textArray, 0, textArray.Length);

                                // Проверяем закончилась ли информация в строке и нужно ли искать следующий FAT
                                if (startIndex < text.Length && previousNext != FATIndex.NOT_USING)
                                {
                                    if (previousNext != FATIndex.EOC)
                                    {
                                        // Перемещаем указатель на текущий FAT
                                        stream.Seek(BPBSize + FSinfoSize + (tempNext * NextSize), SeekOrigin.Begin);

                                        tempNext = previousNext;

                                        // Записываем его в текущий FAT
                                        array = BitConverter.GetBytes(tempNext);

                                        stream.Write(array, 0, array.Length);

                                        // Переместили указатель на следующий FAT
                                        stream.Seek(BPBSize + FSinfoSize + (previousNext * NextSize), SeekOrigin.Begin);

                                        stream.Read(array, 0, array.Length);

                                        previousNext = BitConverter.ToUInt32(array, 0);

                                        // Перемещаем указатель на следующий FAT
                                        stream.Seek(BPBSize + FSinfoSize + (tempNext * NextSize), SeekOrigin.Begin);

                                        // Записываем в него EOC
                                        array = BitConverter.GetBytes(FATIndex.EOC);
                                        stream.Write(array, 0, array.Length);

                                        FSinfo.DeleteOneClaster();

                                        SeachNextFreeClaster();

                                        continue;
                                    }
                                    else previousNext = FATIndex.EOC;

                                    // Перемещаем указатель на текущий FAT
                                    stream.Seek(BPBSize + FSinfoSize + (tempNext * NextSize), SeekOrigin.Begin);

                                    // Определяем следующий кластер
                                    tempNext = FSinfo.NextFreeClaster;

                                    // Записываем его в текущий FAT
                                    array = BitConverter.GetBytes(tempNext);
                                    stream.Write(array, 0, array.Length);

                                    // Перемещаем указатель на следующий FAT
                                    stream.Seek(BPBSize + FSinfoSize + (tempNext * NextSize), SeekOrigin.Begin);

                                    // Записываем в него EOC
                                    array = BitConverter.GetBytes(FATIndex.EOC);
                                    stream.Write(array, 0, array.Length);

                                    // Ищем новый свободный кластер
                                    FSinfo.DeleteOneClaster();

                                    SeachNextFreeClaster();
                                }
                            }

                            if (previousNext != FATIndex.EOC && previousNext != FATIndex.NOT_USING)
                            {
                                while (true)
                                {
                                    stream.Seek(BPBSize + FSinfoSize + (previousNext * NextSize), SeekOrigin.Begin);
                                    stream.Read(array, 0, array.Length);
                                    previousNext = BitConverter.ToUInt32(array, 0);

                                    if (previousNext == FATIndex.EOC)
                                        break;

                                    array = BitConverter.GetBytes(FATIndex.EOC);

                                    stream.Seek(stream.Position - 4, SeekOrigin.Begin);

                                    stream.Write(array, 0, array.Length);
                                }
                            }

                            return;
                        }

                        stream.Seek(stream.Position + 22, SeekOrigin.Begin);
                    }

                    stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                    array = new byte[4];

                    stream.Read(array, 0, 4);

                    UInt32 Index = BitConverter.ToUInt32(array, 0);

                    if (Index != FATIndex.EOC) return;
                    else
                    {
                        stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                        CurrFatTemp = FSinfo.NextFreeClaster;

                        array = BitConverter.GetBytes(CurrFatTemp);
                        stream.Write(array, 0, array.Length);

                        stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                        array = BitConverter.GetBytes(FATIndex.EOC);

                        stream.Write(array, 0, array.Length);

                        FSinfo.DeleteOneClaster();

                        SeachNextFreeClaster();
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void ReadAllFileToTable()
        {
            try
            {
                catalogTable.Rows.Clear();

                UInt32 CurrFatTemp = CurrentFAT;

                string name;
                string type;
                string permiss;
                int size;
                string owner;
                string group;
                string edit;
                string create;

                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array = new byte[EntrySize];

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                    for (int i = 0; i < maxEntry; i++)
                    {
                        stream.Read(array, 0, EntrySize);

                        owner = "Unknown";
                        group = "Unknown";

                        if (CompareArrays(array, FATIndex.NullEntryByte)) return;

                        if (array[0] != FATIndex.EmptyEntry)
                        {
                            byte[] temp = new byte[42];
                            Array.Copy(array, temp, 42);

                            name = Encoding.Unicode.GetString(temp);

                            if (GroupID != 0 && name[0] == '%') continue;

                            temp = new byte[] { array[42], array[43] };
                            UInt16 permissions = BitConverter.ToUInt16(temp, 0);

                            temp = new byte[] { array[44], array[45], array[46], array[47] };
                            UInt32 firstclasterindex = BitConverter.ToUInt32(temp, 0);

                            temp = new byte[] { array[48], array[49], array[50], array[51] };
                            UInt32 fileofsize = BitConverter.ToUInt32(temp, 0);

                            temp = new byte[] { array[52], array[53] };
                            UInt16 ownerid = BitConverter.ToUInt16(temp, 0);

                            temp = new byte[] { array[54], array[55] };
                            UInt16 groupid = BitConverter.ToUInt16(temp, 0);

                            temp = new byte[] { array[56], array[57], array[58], array[59] };
                            UInt32 editdate = BitConverter.ToUInt32(temp, 0);

                            temp = new byte[] { array[60], array[61], array[62], array[63] };
                            UInt32 createdate = BitConverter.ToUInt32(temp, 0);



                            if ((permissions & PermissionsClass.CatalogBit) == 0) type = "Файл";
                            else type = "Папка";

                            if ((permissions & PermissionsClass.Read_Owner) == PermissionsClass.Read_Owner) permiss = "r";
                            else permiss = "-";
                            if ((permissions & PermissionsClass.Write_Owner) == PermissionsClass.Write_Owner) permiss += "w";
                            else permiss += "-";
                            if ((permissions & PermissionsClass.Execute_Owner) == PermissionsClass.Execute_Owner) permiss += "x";
                            else permiss += "-";

                            permiss += " ";

                            if ((permissions & PermissionsClass.Read_Group) == PermissionsClass.Read_Group) permiss += "r";
                            else permiss += "-";
                            if ((permissions & PermissionsClass.Write_Group) == PermissionsClass.Write_Group) permiss += "w";
                            else permiss += "-";
                            if ((permissions & PermissionsClass.Execute_Group) == PermissionsClass.Execute_Group) permiss += "x";
                            else permiss += "-";

                            permiss += " ";

                            if ((permissions & PermissionsClass.Read_Other) == PermissionsClass.Read_Other) permiss += "r";
                            else permiss += "-";
                            if ((permissions & PermissionsClass.Write_Other) == PermissionsClass.Write_Other) permiss += "w";
                            else permiss += "-";
                            if ((permissions & PermissionsClass.Execute_Other) == PermissionsClass.Execute_Other) permiss += "x";
                            else permiss += "-";

                            size = (int)fileofsize;
                            if (type == "Папка") size = 0;

                            for (int j = 0; j < UserList.Count; j++)
                            {
                                if (UserList[j].UserID == ownerid)
                                    if (UserList[j].Name[0] != '#')
                                    owner = String.Join("", UserList[j].Name);
                            }

                            for (int j = 0; j < GroupList.Count; j++)
                            {
                                if (GroupList[j].GroupID == groupid)
                                    if (GroupList[j].Name[0] != '#')
                                        group = String.Join("", GroupList[j].Name);
                            }

                            var c = DateTimeOffset.FromUnixTimeSeconds(editdate);

                            edit = c.ToLocalTime().ToString();

                            c = DateTimeOffset.FromUnixTimeSeconds(createdate);

                            create = c.ToLocalTime().ToString();

                            catalogTable.Rows.Add(name, type, permiss, size, owner, group, edit, create);
                        }
                    }

                    stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                    array = new byte[4];

                    stream.Read(array, 0, 4);

                    UInt32 Index = BitConverter.ToUInt32(array, 0);

                    if (Index != FATIndex.EOC) CurrFatTemp = Index;
                    else return;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private bool CompareArrays(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length) return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i]) return false;
            }

            return true;
        }

        private void SeachNextFreeClaster()
        {
            if (FSinfo.FreeClastersCount == 0) throw new Exception("Нет свободного места на диске!");

            byte[] array = new byte[4];

            stream.Seek(8, SeekOrigin.Begin);

            array = BitConverter.GetBytes(FSinfo.FreeClastersCount);

            stream.Write(array, 0, array.Length);

            for (UInt32 j = 0; j < FATSize; j++)
            {
                stream.Seek(BPBSize + FSinfoSize + (j * NextSize), SeekOrigin.Begin);

                stream.Read(array, 0, 4);

                if (BitConverter.ToInt32(array, 0) == FATIndex.NOT_USING)
                {
                    FSinfo.NextFreeClaster = j;

                    stream.Seek(12, SeekOrigin.Begin);

                    array = BitConverter.GetBytes(FSinfo.NextFreeClaster);

                    stream.Write(array, 0, array.Length);

                    return;
                }
            }

            throw new Exception("Не удалось найти свободное место на диске...");
        }

        private void catalogTable_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                RMBClick.Visible = false;
                CreateClick.Visible = false;
                return;
            }

            var temp = catalogTable.HitTest(e.X, e.Y);

            if (temp.Type == DataGridViewHitTestType.None || temp.Type == DataGridViewHitTestType.ColumnHeader) CreateClick.Show(new Point(Cursor.Position.X, Cursor.Position.Y));
            else if (temp.Type == DataGridViewHitTestType.Cell)
            {
                catalogTable[temp.ColumnIndex, temp.RowIndex].Selected = true;

                RMBClick.Show(new Point(Cursor.Position.X, Cursor.Position.Y));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (NameStack.Count != 0)
            {
                NameStack.RemoveAt(NameStack.Count - 1);
                CurrentFAT = CatalogStack.Pop();
                PerrmissionsWriteCheck.Pop();

                label1.Text = "$/";

                for (int i = 0; i < NameStack.Count; i++)
                {
                    label1.Text += NameStack.First().ToString().Trim('\0') + "/";
                    NameStack.Add(NameStack.First());
                    NameStack.RemoveAt(0);
                }

                ReadAllFileToTable();
            }
        }

        private bool PermissionsMethod(int number, UInt16 perr, UInt16 check1, UInt16 check2, UInt16 check3) // Проверка доступности пользователя к файлу
        {
            try
            {
                if (GroupID == 0) return true;

                if (number == 0 & (perr & check1) == 0)
                {
                    return false;
                }
                else if (number == 1 & (perr & check2) == 0)
                {
                    return false;
                }
                else if (number == 2 & (perr & check3) == 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButtons.OK);
                return false;
            }
        }

        private bool ChechCatalogFirstClaster(Entry entry)
        {
            if (CurrentFAT == entry.FirstClusterIndex) throw new Exception("Вы не можете вложить папку в саму себя");

            foreach (UInt32 first in CatalogStack)
            {
                if (first == entry.FirstClusterIndex)
                {
                    throw new Exception("Вы не можете переместить папку сюда\n" +
                        "так как она является дочерней для перемещаемой");
                }
            }
            return true;
        }

        private bool FindName(string Name)
        {
            UInt32 CurrFatTemp = CurrentFAT;

            char[] name = new char[21];
            for (int i = 0; i < Name.Length && i < name.Length; i++)
            {
                name[i] = Name[i];
            }

            byte[] nameBytes = Encoding.Unicode.GetBytes(name);

            while (true)
            {
                UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                byte[] array = new byte[42];

                stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize), SeekOrigin.Begin);

                for (int i = 0; i < maxEntry; i++)
                {
                    stream.Read(array, 0, array.Length);

                    if (CompareArrays(nameBytes, array))
                    {
                        return false;
                    }

                    stream.Seek(stream.Position + 22, SeekOrigin.Begin);
                }

                stream.Seek(BPBSize + FSinfoSize + (CurrFatTemp * NextSize), SeekOrigin.Begin);

                array = new byte[4];

                stream.Read(array, 0, 4);

                UInt32 Index = BitConverter.ToUInt32(array, 0);

                if (Index != FATIndex.EOC) CurrFatTemp = Index;
                else return true;
            }
        }

        private void FATEmul_FormClosing(object sender, FormClosingEventArgs e)
        {
            stream.Close();
        }

        private void оСистемеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutSystem about = new AboutSystem(BPB.FATSize, FSinfo.FreeClastersCount)) about.ShowDialog();
        }

        private void группыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GroupMethod();
        }

        private void GroupMethod()
        {
            try
            {
                Groups groups = new Groups(ref GroupList, GroupID);
                groups.ShowDialog();

                string text = "";

                for (int i = 0; i < GroupList.Count; i++)
                {
                    text += $"{String.Join("", GroupList[i].Name)}:{GroupList[i].GroupID}/";
                }

                string temp = "%Groups%";

                char[] name = new char[21];
                for (int i = 0; i < temp.Length && i < name.Length; i++)
                {
                    name[i] = temp[i];
                }

                WriteFileToClaster(name, text);

                ReadAllFileToTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK);
                ReadAllFileToTable();
            }
        }

        private void пользователиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UserMethod();
        }

        private void UserMethod()
        {
            try
            {
                Users users = new Users(ref UserList, ref GroupList, GroupID, OwnerID);
                users.ShowDialog();

                string text = "";

                for (int i = 0; i < UserList.Count; i++)
                {
                    text += $"{String.Join("", UserList[i].Name)}:{String.Join("", UserList[i].Password)}:{UserList[i].UserID}:{UserList[i].GroupID}/";
                }

                string temp = "%Users%";

                char[] name = new char[21];
                for (int i = 0; i < temp.Length && i < name.Length; i++)
                {
                    name[i] = temp[i];
                }

                WriteFileToClaster(name, text);

                text = "";

                for (int i = 0; i < GroupList.Count; i++)
                {
                    text += $"{String.Join("", GroupList[i].Name)}:{GroupList[i].GroupID}/";
                }

                temp = "%Groups%";

                name = new char[21];
                for (int i = 0; i < temp.Length && i < name.Length; i++)
                {
                    name[i] = temp[i];
                }

                WriteFileToClaster(name, text);

                ReadAllFileToTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK);
                ReadAllFileToTable();
            }
        }

        private void catalogTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex == -1) return;

                OpenMethod();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK);
                ReadAllFileToTable();
            }
        }
    }
}
