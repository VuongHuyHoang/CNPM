using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity; // Bắt buộc phải có để dùng Include

namespace QuanLyAmThucNhaTrang.DAL
{
    public class PhanHoiDAL
    {
        // 1. Lấy tất cả đánh giá thuộc các quán ăn của Chủ sở hữu này
        public List<DANHGIA> LayDanhGiaTheoChuQuan(int maTKChuQuan)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                // Truy vấn: Lấy Đánh giá -> Kéo theo Quán ăn -> Kéo theo Tên Khách Hàng -> Lọc theo Mã Chủ quán
                return db.DANHGIA // (Sửa thành db.DANHGIAs nếu EF của bạn là số nhiều)
                         .Include(dg => dg.DIADIEM)
                         .Include(dg => dg.TAIKHOAN)
                         .Include("PHANHOI") // Kéo theo phản hồi (nếu có)
                         .Where(dg => dg.DIADIEM.MaTK == maTKChuQuan)
                         .OrderByDescending(dg => dg.NgayDanhGia)
                         .ToList();
            }
        }

        // 2. Thêm phản hồi mới vào CSDL
        public bool ThemPhanHoi(PHANHOI ph)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    db.PHANHOI.Add(ph); // (Sửa thành db.PHANHOIs nếu EF là số nhiều)
                    db.SaveChanges();
                    return true;
                }
                catch { return false; }
            }
        }

        // Xóa phản hồi (Kiểm tra thêm MaTK để chống hack)
        public bool XoaPhanHoi(int maPH, int maTK)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    var ph = db.PHANHOI.FirstOrDefault(p => p.MaPH == maPH && p.MaTK == maTK);
                    if (ph != null)
                    {
                        db.PHANHOI.Remove(ph);
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
                catch { return false; }
            }
        }

        // Sửa nội dung phản hồi
        public bool SuaPhanHoi(int maPH, string noiDungMoi, int maTK)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    var ph = db.PHANHOI.FirstOrDefault(p => p.MaPH == maPH && p.MaTK == maTK);
                    if (ph != null)
                    {
                        ph.NoiDung = noiDungMoi;
                        ph.NgayPhanHoi = DateTime.Now; // Cập nhật lại thời gian sửa mới nhất
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
