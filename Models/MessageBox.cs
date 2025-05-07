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


        public static class MessageBox
        {
            public enum MessageBoxButtons { Ok, YesNo }
            public enum MessageBoxResult { Ok, Yes, No }

            public static async Task<MessageBoxResult> Show(Window owner, string text, string caption, MessageBoxButtons buttons)
            {
                var dlg = new Window
                {
                    Title = caption,
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                var textBlock = new TextBlock { Text = text, Margin = new Avalonia.Thickness(10) };
                var okButton = new Button { Content = "OK", Width = 60 };
                var yesButton = new Button { Content = "Yes", Width = 60 };
                var noButton = new Button { Content = "No", Width = 60 };

                var buttonPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Spacing = 10 };

                if (buttons == MessageBoxButtons.Ok)
                    buttonPanel.Children.Add(okButton);
                else
                {
                    buttonPanel.Children.Add(yesButton);
                    buttonPanel.Children.Add(noButton);
                }

                var stack = new StackPanel();
                stack.Children.Add(textBlock);
                stack.Children.Add(buttonPanel);

                dlg.Content = stack;

                MessageBoxResult result = MessageBoxResult.Ok;

                okButton.Click += (_, __) => { result = MessageBoxResult.Ok; dlg.Close(); };
                yesButton.Click += (_, __) => { result = MessageBoxResult.Yes; dlg.Close(); };
                noButton.Click += (_, __) => { result = MessageBoxResult.No; dlg.Close(); };

                await dlg.ShowDialog(owner);
                return result;
            }

            public static async Task<string?> InputBox(Window owner, string text, string caption)
            {
                var dlg = new Window
                {
                    Title = caption,
                    Width = 300,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                var textBlock = new TextBlock { Text = text, Margin = new Avalonia.Thickness(10) };
                var textBox = new TextBox { Margin = new Avalonia.Thickness(10) };
                var okButton = new Button { Content = "OK", Width = 60, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };

                var stack = new StackPanel();
                stack.Children.Add(textBlock);
                stack.Children.Add(textBox);
                stack.Children.Add(okButton);

                dlg.Content = stack;

                string? result = null;

                okButton.Click += (_, __) => { result = textBox.Text; dlg.Close(); };

                await dlg.ShowDialog(owner);
                return result;
            }
        }