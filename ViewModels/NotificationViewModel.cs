using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using AvaloniaPdbAccounts.Models;
using Oracle.ManagedDataAccess.Client;
using ReactiveUI;

namespace AvaloniaPdbAccounts.ViewModels
{
    public class NotificationViewModel : ViewModelBase
    {
        // private string _connectionString = "User Id={0};Password=123;Data Source=localhost:1521/PDB";

        private string _connectionString = DatabaseSettings.GetConnectionString();        
        private ObservableCollection<Notification> _notifications;
        public ObservableCollection<Notification> Notifications
        {
            get => _notifications;
            set => SetProperty(ref _notifications, value);
        }
        
        private Notification _selectedNotification;
        public Notification SelectedNotification
        {
            get => _selectedNotification;
            set => this.SetProperty(ref _selectedNotification, value);
        }
        
        public NotificationViewModel()
        {
            Notifications = new ObservableCollection<Notification>();
            Console.WriteLine(Notifications);            
            LoadNotifications();
        }
        
        private void LoadNotifications()
        {
            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();
                    var cmd = new OracleCommand("SELECT MATB, TIEUDE, NOIDUNG, NGAYTB FROM OLS_ADMIN.THONGBAO ORDER BY NGAYTB DESC", conn);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Notifications.Add(new Notification
                            {
                                Id = reader["MATB"].ToString(),
                                Title = reader["TIEUDE"].ToString(),
                                Content = reader["NOIDUNG"].ToString(),
                                Date = Convert.ToDateTime(reader["NGAYTB"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error (show in UI)
                Console.WriteLine($"Error loading notifications: {ex.Message}");
            }
        }
    }
    

}