# Hướng dẫn cài đặt 
  
b1. mở thư mục QuanLyAmThucNhaTrang (không có đuôi để mở dự án), mở file có đuôi .sln  
b2. query_CSDL là cơ sở dữ liệu excute trên sqlServer để tạo csdl  
b3. Đổi Connection String: Mở file Web.config trong project chính (QuanLyAmThucNhaTrang),
tìm đến thẻ <connectionStrings>. sửa lại data source=TÊN_MÁY(ví dụ: .\SQLEXPRESS hoặc localhost) và thông tin đăng nhập SQL Server cho khớp với máy cá nhân.  
b4. cài nuget, khi mở sẽ hiện thông báo cài nuget.  chuột phải vào Solution trong khung Solution Explorer $\rightarrow$ Chọn Restore NuGet Packages.
