using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace QuanLyAmThucNhaTrang.DAL
{
    public class KhuyenMaiDAL
    {
        // Lấy danh sách Khuyến mãi thuộc các quán của 1 Chủ sở hữu
        public List<KHUYENMAI> LayDanhSachKhuyenMai(int maTK)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                // Lọc KM dựa trên MaTK của bảng DIADIEM
                return db.KHUYENMAI.Include(k => k.DIADIEM)
                         .Where(k => k.DIADIEM.MaTK == maTK)
                         .OrderByDescending(k => k.NgayBatDau)
                         .ToList();
            }
        }
        public List<KHUYENMAI> LayKhuyenMaiTheoDiaDiem(int maDD)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                // Chỉ lấy những khuyến mãi thuộc quán này và CÒN HẠN SỬ DỤNG
                return db.KHUYENMAI
                         .Where(k => k.MaDD == maDD && k.NgayKetThuc >= DateTime.Now)
                         .OrderByDescending(k => k.NgayBatDau) // Ưu tiên hiển thị KM mới nhất
                         .ToList();
            }
        }

        public bool ThemKhuyenMai(KHUYENMAI km)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    // 1. TÌM ĐỊA ĐIỂM MÀ CHỦ QUÁN ĐANG THAO TÁC
                    var diaDiem = db.DIADIEM.FirstOrDefault(d => d.MaDD == km.MaDD);

                    if (diaDiem != null)
                    {
                        // 2. CHỐT CHẶN CHUYỂN HƯỚNG DỮ LIỆU
                        // Kiểm tra xem địa điểm này có phải là bản nháp không (có MaDD_Goc)
                        if (diaDiem.MaDD_Goc.HasValue)
                        {
                            // Nếu là nháp -> Gắn khuyến mãi này cho bản GỐC đang chiếu ngoài trang chủ
                            km.MaDD = diaDiem.MaDD_Goc.Value;
                        }
                        else
                        {
                            // Nếu không có MaDD_Goc -> Đây chính là bản gốc (hoặc quán mới tinh chưa duyệt)
                            // -> Giữ nguyên km.MaDD
                        }

                        // 3. Thêm vào CSDL và lưu lại
                        db.KHUYENMAI.Add(km);
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi ra nếu cần
                    System.Diagnostics.Debug.WriteLine("Lỗi thêm Khuyến mãi: " + ex.Message);
                    return false;
                }
            }
        }

        public bool XoaKhuyenMai(int maKM)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    var km = db.KHUYENMAI.Find(maKM);
                    if (km != null) { db.KHUYENMAI.Remove(km); db.SaveChanges(); }
                    return true;
                }
                catch { return false; }
            }
        }
        public List<KHUYENMAI> LayKhuyenMaiHieuLuc(int maDD)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                var homNay = System.DateTime.Now.Date;

                return db.KHUYENMAI
                         .Where(k => k.MaDD == maDD
                                  && k.TrangThai == "ConHieuLuc"
                                  && k.NgayBatDau <= homNay
                                  && k.NgayKetThuc >= homNay)
                         .OrderBy(k => k.NgayKetThuc) // Ưu đãi nào sắp hết hạn thì xếp lên đầu
                         .ToList();
            }
        }
    }
}
