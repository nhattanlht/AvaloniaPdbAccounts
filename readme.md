# AvaloniaPdbAccounts

## Yêu cầu

- Đã cài đặt [.NET SDK 8.0 trở lên](https://dotnet.microsoft.com/en-us/download).
- Máy tính sử dụng Windows 64-bit.

## Cách build file `.exe`

### 1. Mở Terminal (CMD, PowerShell hoặc Terminal trong IDE)

### 2. Chạy lệnh publish sau:

```bash
dotnet publish AvaloniaPdbAccounts.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o ./publish
