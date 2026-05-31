using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyAmThucNhaTrang.DAL;

namespace QuanLyAmThucNhaTrang.BLL
{
    public class KhuyenMaiBLL
    {
        private KhuyenMaiDAL _kmDAL = new KhuyenMaiDAL();

        public List<KHUYENMAI> LayDanhSachKhuyenMai(int maTK)
        {
            return _kmDAL.LayDanhSachKhuyenMai(maTK);
        }
        public List<KHUYENMAI> LayKhuyenMaiTheoDiaDiem(int maDD)
        {
            return _kmDAL.LayKhuyenMaiTheoDiaDiem(maDD);
        }

        // Mở file KhuyenMaiBLL.cs và sửa lại chữ 'string' thành 'bool'
        public bool ThemKhuyenMai(KHUYENMAI km)
        {
            return _kmDAL.ThemKhuyenMai(km);
        }

        public bool XoaKhuyenMai(int maKM)
        {
            return _kmDAL.XoaKhuyenMai(maKM);
        }

        public List<KHUYENMAI> LayKhuyenMaiHieuLuc(int maDD)
        {
            return _kmDAL.LayKhuyenMaiHieuLuc(maDD);
        }
    }
}
