# Quản lý quán cà phê (WinForms C#)

## Yêu cầu
- Visual Studio 2022 (workload ".NET desktop development"), .NET 8
- SQL Server 2016 trở lên (Developer/Express) + SSMS

## Cách chạy
1. Mở SSMS, kết nối tới đúng instance SQL Server, mở `Database/CafeDB.sql` rồi nhấn Execute (F5).
   Script chỉ tạo database nếu chưa có, tạo các bảng còn thiếu và thêm dữ liệu mẫu chưa tồn tại. Chạy lại script không xóa database.
2. Nếu database đã có dữ liệu trùng, sao lưu `CafeDB` trước rồi chạy `Database/RepairDuplicates.sql`. Script gộp bàn, danh mục và món trùng, cập nhật liên kết hóa đơn trong transaction và dừng an toàn nếu cùng tên món đang có giá khác nhau.
3. Mở `App.config`, sửa `Server=` cho đúng máy bạn (xem ví dụ trong file):
   `localhost`, `.\SQLEXPRESS`, hoặc `TEN-MAY\TEN-INSTANCE`.
4. Bấm đúp `CafeManager.sln` để mở bằng Visual Studio, đợi restore NuGet, nhấn F5.

## Tài khoản mẫu (mật khẩu 123456)
| Tài khoản | Vai trò  | Quyền |
|-----------|----------|-------|
| phucvu    | Phục vụ  | Xem bàn, gọi món, đổi số lượng, chuyển/gộp bàn, màn hình bếp |
| thungan   | Thu ngân | Như phục vụ + giảm giá, thanh toán, in hóa đơn |
| admin     | Quản lý  | Tất cả + tra cứu doanh thu, biểu đồ |

## Lỗi thường gặp
- "A network-related or instance-specific error": sai `Server=` trong App.config hoặc dịch vụ của instance đang dùng chưa chạy.
  Kiểm tra đúng tên instance trong SQL Server Configuration Manager.
- "Login failed": dùng Windows Authentication thì tài khoản Windows phải có quyền; hoặc đổi sang `User Id=sa;Password=...`.
- "Cannot open database CafeDB": chưa chạy script `CafeDB.sql`.

## Cấu trúc mã nguồn (MVP)
- `Models/`      mô hình dữ liệu và phân quyền: `Account.cs`, `Food.cs`, `Category.cs`, `TableFood.cs`, `BillItem.cs`, `Role.cs`
- `Views/Contracts/` giao diện hợp đồng để Presenter điều khiển mà không phụ thuộc WinForms
- `Forms/`       phần hiển thị WinForms: `LoginView`, `OrderView`, `KitchenView`, `RevenueView`, `MenuManagementView`, `FoodEditorView`; `ChartPanel` vẽ biểu đồ GDI+
- `Presenters/`  xử lý luồng đăng nhập, gọi món, bếp, doanh thu và quản lý thực đơn
- `Repositories/` đọc/ghi SQL Server: `AccountRepository`, `FoodRepository`, `TableRepository`, `BillRepository`, `KitchenRepository`, `RevenueRepository`
- `Helpers/`     kết nối dữ liệu, mật khẩu, lưu ảnh món và in hóa đơn
- `Database/`    script tạo CSDL, dữ liệu mẫu và sửa dữ liệu trùng

Ví dụ luồng MVP: `OrderView` phát sự kiện khi người dùng thao tác; `OrderPresenter` kiểm tra quyền và điều phối; `BillRepository` thực hiện truy vấn/transaction với SQL Server.


