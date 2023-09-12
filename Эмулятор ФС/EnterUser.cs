using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Эмулятор_ФС
{
    public partial class EnterUser : Form
    {
        static private readonly string path = "File.fat"; // путь к файлу с данными
        static private char[] UserNameFile;
        static private char[] GroupNameFile;

        private Byte BPBSize = 8; // Размер BPB в байтах
        private Byte FSinfoSize = 8; // Размер FSinfo в байтах
        private Byte EntrySize = 64; // Размер Entry в байтах
        private Byte NextSize = 4; // Размер Next в байтах
        private UInt16 ClasterSize; // Размер Claster в байтах
        private UInt16 ClasterSimbolSize; // Количество символов в кластере в байтах
        private UInt32 FATCountSize; // Размер всех FAT в байтах
        private UInt32 FATSize; // Размер одной FAT в байтах

        private BPB BPB;
        private FSinfo FSinfo;

        private List<UserList> Users = new List<UserList>();
        public List<GroupList> Groups = new List<GroupList>();

        private int counter = 0;

        FATIndex FATIndex = new FATIndex();

        bool isStart = false;

        int index = 0;
        int group = 0;

        public EnterUser()
        {
            InitializeComponent();
        }

        private void EnterUser_Load(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(path))
                {
                    throw new Exception("Не найден файл диска");
                }

                ReadSuperBlock();

                ClasterSize = (UInt16)(BPB.SectorsByteCount * BPB.SectorsPerClaster);
                ClasterSimbolSize = (UInt16)(ClasterSize / 2);
                FATCountSize = BPB.FATSize * BPB.FATCount * NextSize;
                FATSize = BPB.FATSize;

                UserNameFile = new char[21];

                string str = "%Users%";

                for (int i = 0; i < str.Length; i++)
                {
                    UserNameFile[i] = str[i];
                }

                GroupNameFile = new char[21];

                str = "%Groups%";

                for (int i = 0; i < str.Length; i++)
                {
                    GroupNameFile[i] = str[i];
                }

                string[] user = OpenFile(String.Join("", UserNameFile)).Split('/');

                foreach (var arr in user)
                {
                    string[] temp = arr.Split(':');

                    if (temp.Length == 4)
                    {
                        char[] name = temp[0].ToCharArray();

                        char[] password = temp[1].ToCharArray();

                        UInt16 userid = UInt16.Parse(temp[2]);

                        UInt16 groupid = UInt16.Parse(temp[3]);

                        Users.Add(new UserList(name, password, userid, groupid));
                    }
                }

                string[] group = OpenFile(String.Join("", GroupNameFile)).Split('/');

                foreach (var arr in group)
                {
                    string[] temp = arr.Split(':');

                    if (temp.Length == 2)
                    {
                        char[] name = temp[0].ToCharArray();

                        UInt16 groupid = UInt16.Parse(temp[1]);

                        Groups.Add(new GroupList(name, groupid));
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButtons.OK);
                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox1.Text) && !String.IsNullOrEmpty(textBox2.Text))
            {
                char[] name = new char[7];

                char[] temp = textBox1.Text.ToCharArray();

                for (int i = 0; i < temp.Length && i < name.Length; i++)
                {
                    name[i] = temp[i];
                }

                int index = 0;

                for (int i = 0; i < Users.Count; i++)
                {
                    if (CompareArrays(Users[i].Name, name))
                    {
                        index = i;
                        this.index = i;
                        break;
                    }

                    if (i == Users.Count - 1)
                    {
                        MessageBox.Show("Такого пользователя не существует", "Ошибка", MessageBoxButtons.OK);
                        return;
                    }
                }

                char[] password = new char[7];

                temp = textBox2.Text.ToCharArray();

                for (int i = 0; i < temp.Length && i < name.Length; i++)
                {
                    password[i] = temp[i];
                }

                if (CompareArrays(Users[index].Password, password))
                {
                    int groupIndex = 0;

                    for (int i = 0; i < Groups.Count; i++)
                    {
                        if (Groups[i].GroupID == Users[index].GroupID)
                        {
                            groupIndex = i;
                            this.group = i;
                            break;
                        }
                    }

                    isStart = true;

                    this.Close();
                }
            }
            else MessageBox.Show("Какое-то из полей не заполнено", "Ошибка", MessageBoxButtons.OK);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ReadSuperBlock()
        {
            try
            {
                using (FileStream stream = File.Open(path, FileMode.Open))
                {
                    UInt16 sectorsByteCount;
                    Byte sectorsPerClaster;
                    Byte fATCount;
                    UInt32 fATSize;

                    byte[] array = new byte[2];

                    stream.Read(array, 0, 2);
                    sectorsByteCount = BitConverter.ToUInt16(array, 0);

                    array = new byte[1];

                    stream.Read(array, 0, 1);
                    sectorsPerClaster = array[0];

                    stream.Read(array, 0, 1);
                    fATCount = array[0];

                    array = new byte[4];

                    stream.Read(array, 0, 4);
                    fATSize = BitConverter.ToUInt32(array, 0);



                    UInt32 freeClastersCount;
                    UInt32 nextFreeClaster;

                    stream.Read(array, 0, 4);
                    freeClastersCount = BitConverter.ToUInt32(array, 0);

                    stream.Read(array, 0, 4);
                    nextFreeClaster = BitConverter.ToUInt32(array, 0);

                    BPB = new BPB(sectorsByteCount, sectorsPerClaster, fATCount, fATSize);
                    FSinfo = new FSinfo(freeClastersCount, nextFreeClaster);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string OpenFile(string name)
        {
            try
            {
                string text = "";
                byte[] nameBytes = Encoding.Unicode.GetBytes(name);

                UInt32 CurrFatTemp = 0;

                using (FileStream stream = File.Open(path, FileMode.Open))
                {
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
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (CreateNewUsers createNewUsers = new CreateNewUsers(ref Users, ref Groups))
                createNewUsers.ShowDialog();

            
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

        private bool CompareArrays(char[] array1, char[] array2)
        {
            if (array1.Length != array2.Length) return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i]) return false;
            }

            return true;
        }

        private void SeachNextFreeClaster(FileStream stream)
        {
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

                    break;
                }
            }
        }

        private void WriteFileToClaster(char[] name, string text)
        {
            try
            {
                byte[] nameBytes = Encoding.Unicode.GetBytes(name);

                UInt32 CurrFatTemp;

                CurrFatTemp = 0;

                using (FileStream stream = File.Open(path, FileMode.Open))
                {
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

                                    SeachNextFreeClaster(stream);
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

                                            SeachNextFreeClaster(stream);

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

                                        SeachNextFreeClaster(stream);
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

                            SeachNextFreeClaster(stream);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void EnterUser_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (File.Exists(path))
            {
                // Запись пользователей в файл
                string text = "";

                for (int i = 0; i < Users.Count; i++)
                {
                    text += $"{String.Join("", Users[i].Name)}:{String.Join("", Users[i].Password)}:{Users[i].UserID}:{Users[i].GroupID}/";
                }

                WriteFileToClaster(UserNameFile, text);

                // Запись групп в файл
                text = "";

                for (int i = 0; i < Groups.Count; i++)
                {
                    text += $"{String.Join("", Groups[i].Name)}:{Groups[i].GroupID}/";
                }

                WriteFileToClaster(GroupNameFile, text);

                if (isStart) // Запуск ФС
                    using (FATEmul fATEmul = new FATEmul(BPB, FSinfo, Users[index], Groups[group], Users, Groups))
                    {
                        this.Hide();
                        fATEmul.ShowDialog();
                    }
            }
        }

        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (FATIndex.BannedSymbols.Contains(e.KeyChar)) e.Handled = true;
            if ((sender as TextBox).Text.Length == 7 && e.KeyChar != 8) e.Handled = true;
            if ((sender as TextBox).Text.Length != 7 && e.KeyChar == ' ') e.Handled = true;
            return;
        }
    }
}
