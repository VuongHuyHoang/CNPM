using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace QuanLyAmThucNhaTrang.DAL
{
    public class DiaDiemDAL
    {
        // Lấy danh sách địa điểm đang hoạt động lên trang chủ
        public List<DIADIEM> LayDanhSachTrangChu()
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.DIADIEM
                         .Include(d => d.DANHMUC) // Kéo theo tên danh mục (Quán ăn, nhà hàng...)
                         .Include(d => d.HINHANH) // Kéo theo danh sách hình ảnh của quán
                         .Where(d => d.TrangThai == "DangHoatDong")
                         .OrderByDescending(d => d.DiemDanhGiaTB) // Quán nhiều sao xếp lên trước
                         .ToList();
            }
        }
        // Lấy thông tin chi tiết của một địa điểm cụ thể
        public DIADIEM LayChiTietDiaDiem(int maDD)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.DIADIEM
                         .Include(d => d.DANHMUC) // Lấy thông tin phân loại
                         .Include(d => d.HINHANH) // Lấy danh sách toàn bộ ảnh của quán
                         .Include(d => d.KHUVUC) // Lấy thông tin khu vực để hiện bản đồ
                                                 // Lấy danh sách đánh giá, đồng thời kéo theo thông tin của người đã viết đánh giá đó
                         .Include(d => d.DANHGIA.Select(dg => dg.TAIKHOAN))
                         .FirstOrDefault(d => d.MaDD == maDD);
            }
        }

        // 1. Hàm tìm kiếm và lọc kết hợp (Động)
        public List<DIADIEM> TimKiemVaLoc(string tuKhoa, int? maDM, int? maKV)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                // Khởi tạo câu truy vấn ban đầu: chỉ lấy những quán Đang hoạt động
                var query = db.DIADIEM
                              .Include(d => d.DANHMUC)
                              .Include(d => d.HINHANH)
                              .Where(d => d.TrangThai == "DangHoatDong");

                // Nếu có nhập từ khóa -> tìm theo Tên quán hoặc Địa chỉ
                if (!string.IsNullOrEmpty(tuKhoa))
                {
                    query = query.Where(d => d.TenDD.Contains(tuKhoa) || d.DiaChiChiTiet.Contains(tuKhoa));
                }

                // Nếu có chọn Danh mục -> lọc theo Danh mục
                if (maDM.HasValue)
                {
                    query = query.Where(d => d.MaDM == maDM.Value);
                }

                // Nếu có chọn Khu vực -> lọc theo Khu vực
                if (maKV.HasValue)
                {
                    query = query.Where(d => d.MaKV == maKV.Value);
                }

                // Sắp xếp quán nhiều sao lên trước và xuất ra danh sách
                return query.OrderByDescending(d => d.DiemDanhGiaTB).ToList();
            }
        }

        // 2. Hàm lấy danh sách Danh mục để làm bộ lọc dropdown/sidebar
        public List<DANHMUC> LayTatCaDanhMuc()
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.DANHMUC.Where(dm => dm.TrangThai == "HoatDong").ToList();
            }
        }

        // 3. Hàm lấy danh sách Khu vực để làm bộ lọc dropdown/sidebar
        public List<KHUVUC> LayTatCaKhuVuc()
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.KHUVUC.ToList();
            }
        }
    }
}
