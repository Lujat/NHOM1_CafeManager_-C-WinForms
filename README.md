# Khoa Công nghệ Thông tin - HCMUE 
# 2611COMP101904 - Lập trình trên Windows
GIảng viên hướng dẫn: ThS. Lê Thanh Thoại 
Nhóm 1 - Đề tài 10: Quản lý quán cà phê

Thanh viên nhóm: 
Nguyễn Trung Luật -- 49.01.104.086 
Nguyễn Trương Thanh Đông – 51.01.104.130

# CafeManager — Ứng dụng quản lý quán cà phê
Ứng dụng desktop quản lý hoạt động cơ bản của quán cà phê: bàn, thực đơn, gọi món, thanh toán, màn hình bếp và doanh thu.

# Mô tả: Quản lý bàn, thực đơn, gọi món, thanh toán và doanh thu của quán cà phê.
# Vai trò sử dụng: Nhân viên phục vụ; thu ngân; quản lý.
Dữ liệu chính: Bàn, Nhóm món, Món, Hóa đơn, Chi tiết hóa đơn, Tài khoản.

## Chức năng
- Hiển thị bàn theo trạng thái trống hoặc đang có khách.
- Gọi món và thay đổi số lượng món.
- Chuyển bàn hoặc gộp hóa đơn.
- Thanh toán, áp dụng giảm giá và in hóa đơn.
- Quản lý nhóm món, món và ảnh món.
- Theo dõi món đang chờ xử lý trên màn hình bếp.
- Tra cứu doanh thu theo khoảng ngày và xem biểu đồ.
## Công nghệ
- C# WinForms
- .NET 8
- SQL Server
- GDI+ để vẽ biểu đồ doanh thu
## Yêu cầu

- Windows
- Visual Studio 2022 phiên bản 17.8 trở lên, kèm workload **.NET desktop development**
- SQL Server 2016 trở lên
- SQL Server Management Studio (SSMS)
## Tài khoản demo
mật khẩu demo là `123456`.

| Tài khoản | Vai trò |
|---|---|
| `phucvu` | Phục vụ |
| `thungan` | Thu ngân |
| `admin` | Quản lý |

Các tài khoản mẫu chỉ phục vụ việc chạy thử. Không dùng mật khẩu demo cho môi trường thực tế.

## Cấu trúc mã nguồn

```text
CafeManager/
├── Database/       Script khởi tạo database và xử lý dữ liệu trùng
├── Forms/          Các màn hình WinForms
├── Helpers/        Kết nối dữ liệu, in hóa đơn, xử lý mật khẩu và ảnh
├── Models/         Các lớp dữ liệu và vai trò
├── Presenters/     Điều phối nghiệp vụ theo mô hình MVP
├── Repositories/   Đọc và ghi dữ liệu SQL Server
├── Views/
│   └── Contracts/  Giao diện hợp đồng giữa View và Presenter
├── App.config      Cấu hình kết nối database
├── Program.cs      Điểm bắt đầu ứng dụng
└── CafeManager.sln
```

## Database và dữ liệu

- `Database/CafeDB.sql` tạo cấu trúc database 
- `Database/RepairDuplicates.sql` dùng để gộp dữ liệu trùng.
- `Database/MigrateFoodImagePaths.sql` dùng để chuyển đường dẫn ảnh
- Hóa đơn và chi tiết hóa đơn được lưu trong SQL Server, ở các bảng `Bill` và `BillInfo`.
- Khi chạy từ mã nguồn, ảnh món được lưu trong `Resources/FoodImages/`.

## 

# -------------------------------------------------HET-----------------------------------------------------
