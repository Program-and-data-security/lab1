using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Эмулятор_ФС
{
    public partial class FIleFormattingForm : Form
    {
        static private readonly string path = "File.fat"; // путь к файлу с данными

        private BPB BPB;
        private FSinfo FSinfo;

        private Byte BPBSize = 8; // Размер BPB в байтах
        private Byte FSinfoSize = 8; // Размер FSinfo в байтах
        private Byte EntrySize = 64; // Размер Entry в байтах
        private Byte NextSize = 4; // Размер Next в байтах
        private UInt16 ClasterSize; // Размер Claster в байтах
        private UInt16 ClasterSimbolSize; // Количество символов в кластере в байтах
        private UInt32 FATCountSize; // Размер всех FAT в байтах
        private UInt32 FATSize; // Размер одной FAT в байтах

        FATIndex FATIndex = new FATIndex();
        PermissionsClass PermissionsClass = new PermissionsClass();

        FileStream stream;

        public FIleFormattingForm(bool isDemo)
        {
            InitializeComponent();

            if (isDemo)
                SetFileSizes(1);
            else
                SetFileSizes();
        }

        private void SetFileSizes(int maxSize = 6)
        {
            for (int i = 1; i <= maxSize; i++)
            {
                fileSize.Items.Add((i * 32).ToString() + " мб.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(fileSize.Text) || String.IsNullOrEmpty(fatFat.Text)) return;

            UInt32 size = UInt32.Parse(fileSize.Text.Split(' ')[0]);

            CreateNewFileFS(size * 512);

            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void CreateNewFileFS(UInt32 fATSize)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);

                BPB = new BPB(fATSize);
                FSinfo = new FSinfo(fATSize - 1, 1);

                ClasterSize = (UInt16)(BPB.SectorsByteCount * BPB.SectorsPerClaster);
                ClasterSimbolSize = (UInt16)(ClasterSize / 2);
                FATCountSize = BPB.FATSize * BPB.FATCount * NextSize;
                FATSize = BPB.FATSize;

                WriteSuperBlock();
                WriteFAT();
                WriteUserGroupFile();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButtons.OK);

                if (File.Exists(path)) File.Delete(path);

                this.Close();
            }
        }

        private void WriteSuperBlock()
        {
            try
            {
                stream = File.Open(path, FileMode.OpenOrCreate);

                stream.SetLength(BPBSize + FSinfoSize + FATCountSize + (FATSize * ClasterSize));

                byte[] array = new byte[2];

                array = BitConverter.GetBytes(BPB.SectorsByteCount);
                stream.Write(array, 0, 2);

                array = new byte[1];

                array[0] = BPB.SectorsPerClaster;
                stream.Write(array, 0, 1);

                array[0] = BPB.FATCount;
                stream.Write(array, 0, 1);

                array = BitConverter.GetBytes(BPB.FATSize);
                stream.Write(array, 0, 4);



                array = BitConverter.GetBytes(FSinfo.FreeClastersCount);
                stream.Write(array, 0, 4);

                array = BitConverter.GetBytes(FSinfo.NextFreeClaster);
                stream.Write(array, 0, 4);

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void WriteFAT()
        {
            try
            {
                stream.Seek(BPBSize + FSinfoSize, SeekOrigin.Begin);

                byte[] array = BitConverter.GetBytes(FATIndex.EOC);
                stream.Write(array, 0, 4);

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void WriteUserGroupFile()
        {
            CreateFile("%Users%".ToCharArray(), 0);
            CreateFile("%Groups%".ToCharArray(), 1);

            WriteUserGroupToClaster("root\0\0\0:root\0\0\0:0:0/", 1, 0);
            WriteUserGroupToClaster("root\0\0\0:0/", 2, 1);

            FSinfo.NextFreeClaster = 3;
        }

        private void CreateFile(char[] Name, UInt16 entryCount)
        {
            try
            {
                UInt32 CurrFatTemp = 0;

                while (true)
                {
                    UInt16 maxEntry = (UInt16)(ClasterSize / EntrySize);

                    byte[] array = new byte[EntrySize];

                    stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize) + (entryCount * 64), SeekOrigin.Begin);

                    UInt32 date = (UInt32)new DateTimeOffset(DateTime.UtcNow.ToLocalTime()).ToUnixTimeSeconds();

                    FileEntry fileEntry = new FileEntry(Name, PermissionsClass.Read_Owner, FATIndex.EOC, 0, 0, date);

                    // Запись файловой записи
                    char[] namech = fileEntry.Name;
                    byte[] temp = Encoding.Unicode.GetBytes(namech); // Запись Name
                    stream.Write(temp, 0, temp.Length);

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
            catch (Exception e)
            {
                throw e;
            }
        }

        private void WriteUserGroupToClaster(string text, UInt32 NextFree, UInt16 fileCount)
        {
            try
            {
                UInt32 CurrFatTemp = 0;

                stream.Seek(BPBSize + FSinfoSize + FATCountSize + (CurrFatTemp * ClasterSize) + (fileCount * 64) + 44, SeekOrigin.Begin);

                UInt32 tempNext = NextFree;

                // Записываем первый кластер в стеке
                byte[] array = BitConverter.GetBytes(tempNext);

                stream.Write(array, 0, array.Length);

                // Записываем размер файла
                array = BitConverter.GetBytes(text.Length);

                stream.Write(array, 0, array.Length);

                // Записываем в FAT свободного кластера EOC
                stream.Seek(BPBSize + FSinfoSize + (tempNext * NextSize), SeekOrigin.Begin);

                array = BitConverter.GetBytes(FATIndex.EOC);

                stream.Write(array, 0, array.Length);

                // Ищем новый свободный кластер
                FSinfo.DeleteOneClaster();

                SeachNextFreeClaster();

                // Переместили указатель на текущий кластер
                stream.Seek(BPBSize + FSinfoSize + FATCountSize + (tempNext * ClasterSize), SeekOrigin.Begin);



                char[] mass = new char[text.Length];
                for (int j = 0; j < mass.Length; j++) mass[j] = text[j]; // Записывает информацию в массив char

                byte[] textArray = Encoding.Unicode.GetBytes(mass); // Encoding в массив byte

                // Записывает информацию в кластер
                stream.Write(textArray, 0, textArray.Length);

                return;

            }
            catch (Exception e)
            {
                throw e;
            }
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

        private void FIleFormattingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (stream != null) stream.Close();
        }
    }
}
