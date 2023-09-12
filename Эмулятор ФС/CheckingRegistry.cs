using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Эмулятор_ФС
{
    public abstract class CheckRegistry
    {
        static string regDir = @"SOFTWARE";
        static string regDirName = "FatEmul";
        static string regPath = @"SOFTWARE\FatEmul";

        private static void SetRegistryValue(string keyName, object value)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(regPath))
            {
                if (key != null)
                {
                    key.SetValue(keyName, value);
                }
            }
        }

        private static object GetRegistryValue(string keyName, object defaultValue = null)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(regPath))
            {
                if (key != null)
                {
                    return key.GetValue(keyName, defaultValue);
                }
                return defaultValue;
            }
        }

        public static bool CheckIsDemo()
        {
            object isDemo = GetRegistryValue("IsDemo");

            if (isDemo == null)
            {
                SetRegistryValue("IsDemo", true);
                return true;
            }
            else
            {
               return ParseIntToBool(isDemo.ToString());
            }
        }

        public static void CheckTrialPeriod()
        {
            DateTime startDate = DateTime.Now;
            object registryValue = GetRegistryValue("StartDate");

            if (registryValue == null)
            {
                // При первом запуске устанавливаем начальную дату
                SetRegistryValue("StartDate", startDate);

                if (MessageBox.Show("У Вас активен демо-режим.\n" +
                        "Вы не можете создавать диск более 32 мб., а также редактировать пользователей и группы системы." +
                        "\nОсталось 10 дней демо-режима\n" +
                        "Хотите ввести код активации?", "Проверка подлиности", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    using (TrialRemove form = new TrialRemove())
                    {
                        form.ShowDialog();
                    }

                    Environment.Exit(0);
                }
            }
            else
            {
                startDate = DateTime.Parse(registryValue.ToString());
                TimeSpan timeSinceStart = DateTime.Now - startDate;

                if (timeSinceStart.Days > 10)
                {
                    if (MessageBox.Show("Демо-версия истекла! Хотите ввести код активации?", "Проверка подлиности", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        using (TrialRemove form = new TrialRemove())
                        {
                            form.ShowDialog();
                        }
                    }

                    Environment.Exit(0);
                }
                else
                {
                    if (MessageBox.Show("У Вас активен демо-режим.\n" +
                        "Вы не можете создавать диск более 32 мб., а также редактировать пользователей и группы системы." +
                        "\nОсталось " + timeSinceStart.Days + " дней демо-режима\n" +
                        "Хотите ввести код активации?", "Проверка подлиности", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        using (TrialRemove form = new TrialRemove())
                        {
                            form.ShowDialog();
                        }

                        Environment.Exit(0);
                    }
                    return;
                }
            }
        }

        public static void DeleteApplicationRegistryFolder()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(regDir, true))
                {
                    if (key != null)
                    {
                        key.DeleteSubKeyTree(regDirName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении ключа: " + ex.Message);
            }
        }

        public static void ResetDemoTrial()
        {
            SetRegistryValue("StartDate", DateTime.Now);
            SetRegistryValue("IsDemo", 1);
        }

        public static void RemoveDemoTrial()
        {
            SetRegistryValue("StartDate", DateTime.Now);
            SetRegistryValue("IsDemo", 0);
        }

        private static bool ParseIntToBool(string returnValue)
        {
            return (returnValue != "0");
        }
    }
}
