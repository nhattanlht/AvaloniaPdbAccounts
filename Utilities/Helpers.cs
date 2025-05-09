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
using AvaloniaPdbAccounts.Services; // Import service

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




    }
