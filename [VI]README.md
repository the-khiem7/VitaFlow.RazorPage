# 🩸 VitaFlow – Phần mềm hỗ trợ hiến máu cho cơ sở y tế

**VitaFlow** là một ứng dụng web được xây dựng với ASP.NET Core Razor Pages, nhằm hỗ trợ quản lý và kết nối giữa người hiến máu và người cần máu, giúp tối ưu quá trình vận hành và lưu trữ máu tại cơ sở y tế.

---

## 📌 Mục tiêu dự án

Cung cấp một giải pháp đơn giản, hiệu quả và kịp thời để:

* Kết nối người hiến máu với các ca cần máu.
* Tra cứu nhanh chóng thông tin tương thích nhóm máu.
* Quản lý kho máu và lịch sử hiến máu.
* Xử lý các tình huống khẩn cấp về truyền máu.

---

## 🚀 Chức năng chính

* Trang chủ giới thiệu cơ sở y tế, tài liệu y khoa, và blog chia sẻ kinh nghiệm.
* Đăng ký người hiến máu: nhóm máu, thời điểm sẵn sàng hiến.
* Tra cứu nhóm máu phù hợp theo truyền máu toàn phần và từng thành phần máu (hồng cầu, huyết tương, tiểu cầu).
* Tìm kiếm người hiến máu hoặc người cần máu theo vị trí địa lý.
* Xử lý và đăng ký các ca cần máu khẩn cấp.
* Theo dõi và quản lý toàn bộ quy trình từ yêu cầu máu đến khi hoàn tất hiến máu.
* Quản lý kho máu tại cơ sở y tế.
* Nhắc nhở người dùng về thời gian phục hồi giữa các lần hiến máu.
* Quản lý hồ sơ cá nhân, lịch sử hiến máu.
* Dashboard và báo cáo thống kê.

---

## 🛠️ Công nghệ sử dụng

* **ASP.NET Core 8** với **Razor Pages**
* **Entity Framework Core**
* **SQL Server / PostgreSQL**
* (Tùy chọn) Google Maps API, Chart.js

---

## 📈 Định hướng phát triển

* Tích hợp xác thực bằng OTP hoặc Email
* Tích hợp bản đồ trực quan khi tìm người hiến gần nhất
* Gửi thông báo khẩn cấp tự động
* Xây dựng API mở cho ứng dụng mobile trong tương lai

---

## 📂 Cấu trúc dự án dự định

```
VitaFlow.sln
├── VitaFlow.Web             # Razor Pages project
├── VitaFlow.Core            # Domain models, interfaces
├── VitaFlow.Infrastructure  # EF Core, database access
└── VitaFlow.Services        # Business logic
```