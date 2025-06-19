using System;

namespace AvaloniaPdbAccounts.Models;
public class Notification
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime Date { get; set; }
    public string OLS_LABEL { get; set; } // Thêm thuộc tính này nếu cần thiết
    public string DateDisplay => Date.ToString("dd/MM/yyyy HH:mm");
}