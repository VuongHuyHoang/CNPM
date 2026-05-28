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
        // 1. Thêm địa điểm mới và trả về ID vừa tạo
        public int ThemDiaDiemMoi(DIADIEM dd)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                db.DIADIEM.Add(dd);
                db.SaveChanges(); // EF tự động cập nhật ID vào thuộc tính MaDD
                return dd.MaDD;
            }
        }

        // 2. Thêm hình ảnh cho địa điểm
        public bool ThemHinhAnh(HINHANH ha)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    db.HINHANH.Add(ha);
                    db.SaveChanges();
                    return true;
                }
                catch { return false; }
            }
        }

        // 1. Lấy danh sách các địa điểm do một tài khoản chủ quán đăng ký
        public List<DIADIEM> LayDanhSachTheoChuQuan(int maTK)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.DIADIEM.Include("DANHMUC") // Đảm bảo dùng đúng db.DIADIEM hoặc db.DIADIEMs theo cấu hình EF của bạn
                         .Where(d => d.MaTK == maTK)
                         .OrderByDescending(d => d.MaDD)
                         .ToList();
            }
        }

        // 2. Cập nhật thông tin địa điểm (Sửa quán ăn)
        public bool CapNhatDiaDiem(DIADIEM ddThayDoi)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    var dd = db.DIADIEM.FirstOrDefault(d => d.MaDD == ddThayDoi.MaDD);
                    if (dd != null)
                    {
                        dd.TenDD = ddThayDoi.TenDD;
                        dd.MaDM = ddThayDoi.MaDM;
                        dd.MaKV = ddThayDoi.MaKV;
                        dd.DiaChiChiTiet = ddThayDoi.DiaChiChiTiet;
                        dd.SDT = ddThayDoi.SDT;
                        dd.GioMoCua = ddThayDoi.GioMoCua;
                        dd.GioDongCua = ddThayDoi.GioDongCua;
                        dd.MoTa = ddThayDoi.MoTa;
                        dd.ViDo = ddThayDoi.ViDo;
                        dd.KinhDo = ddThayDoi.KinhDo;
                        dd.TrangThai = ddThayDoi.TrangThai; // Cập nhật lại trạng thái nếu có thay đổi nghiệp vụ

                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
                catch { return false; }
            }
        }
    }
}
