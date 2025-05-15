using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Data;
using System.Collections.ObjectModel;
using Oracle.ManagedDataAccess.Client;
using System.Threading.Tasks;
using System.Linq;
using Avalonia.Interactivity;
using System.Diagnostics;
using AvaloniaPdbAccounts.Models; // Import model
using AvaloniaPdbAccounts.Services;
using System.Text.Json;
using System.IO;
using System.Security.Cryptography;
using System.Text; // Import service

namespace AvaloniaPdbAccounts.Utilities;

    public static class Helpers
    {

        public static async Task<bool> ShowEditPrivilegesDialog(Window owner,string role, ObservableCollection<PrivilegeItem> privileges)
        {
            var dlg = new Window
            {
                Title = $"Chỉnh sửa quyền cho role '{role}'",
                Width = 300,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Star)); // List chiếm phần lớn
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Button tự co nhỏ

            var listBox = new ListBox
            {
                SelectionMode = SelectionMode.Multiple,
                [ScrollViewer.VerticalScrollBarVisibilityProperty] = ScrollBarVisibility.Auto
            };

            foreach (var privilege in privileges)
            {
                var checkBox = new CheckBox
                {
                    Content = privilege.Name,
                    IsChecked = privilege.IsGranted
                };

                listBox.Items.Add(checkBox);
            }

            var okButton = new Button
            {
                Content = "OK",
                Width = 60,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            okButton.Click += (_, __) =>
            {
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    if (listBox.Items[i] is CheckBox checkBox && privileges.Count > i)
                    {
                        privileges[i].IsGranted = checkBox.IsChecked == true;
                    }
                }
                dlg.Close();
            };

            // Add ListBox vào dòng 0
            Grid.SetRow(listBox, 0);
            grid.Children.Add(listBox);

            // Add OK Button vào dòng 1
            Grid.SetRow(okButton, 1);
            grid.Children.Add(okButton);

            dlg.Content = grid;

            await dlg.ShowDialog(owner);
            return true;
        }
        public static int GetPrivilegeTypeIndex(string type)
        {
                return type switch
                {
                    "ROLE" => 0,
                    "SYSTEM" => 1,
                    "TABLE" => 2,
                    "COL" => 3,
                    _ => -1
                };
        }
        public static List<Dictionary<string, object>> ConvertDataTableToList(DataTable table)
        {
            var list = new List<Dictionary<string, object>>(table.Rows.Count);
            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>(table.Columns.Count);
                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }
            return list;
        }

        public static Dictionary<string, string> ParseSelectedRow(string row)
        {
            return row.Split('|')
                    .Select(part => part.Trim())
                    .Where(part => part.Contains(':'))
                    .Select(part => part.Split(':', 2))
                    .ToDictionary(split => split[0].Trim(), split => split[1].Trim());
        }

                // Save account to JSON file
        public static void SaveAccountsToJson(List<UserAccount> accounts)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(accounts, options);

            string fileName = "accounts.json";
            File.WriteAllText(fileName, jsonString);
        }


        public class SymmetricEncryptionService
        {
            // Khóa bí mật và IV (Initialization Vector)
            private readonly byte[] _key; // 256-bit key
            private readonly byte[] _iv;  // 128-bit IV

            public SymmetricEncryptionService(string keyString = "YourSecretKey", string ivString = "YourInitVector")
            {
                // Trong thực tế, bạn nên lưu trữ khóa này an toàn
                // và không hardcode trong ứng dụng
                    using (var sha256 = SHA256.Create())
                    {
                        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyString));
                    }

                    // Tạo IV 16 bytes bằng MD5 hash từ chuỗi ivString
                    using (var md5 = MD5.Create())
                    {
                        _iv = md5.ComputeHash(Encoding.UTF8.GetBytes(ivString));
                    }            }

            public string Encrypt(string plainText)
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = _key;
                    aesAlg.IV = _iv;

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }

            public string Decrypt(string cipherText)
            {
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = _key;
                    aesAlg.IV = _iv;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(buffer))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
    }
