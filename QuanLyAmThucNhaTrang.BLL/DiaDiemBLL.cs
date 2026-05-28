using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyAmThucNhaTrang.DAL;

namespace QuanLyAmThucNhaTrang.BLL
{
    public class DiaDiemBLL
    {
        private DiaDiemDAL _diaDiemDAL = new DiaDiemDAL();
        public List<DIADIEM> TimKiemVaLoc(string tuKhoa, int? maDM, int? maKV)
        {
            return _diaDiemDAL.TimKiemVaLoc(tuKhoa, maDM, maKV);
        }

        public List<DANHMUC> LayTatCaDanhMuc()
        {
            return _diaDiemDAL.LayTatCaDanhMuc();
        }

        public List<KHUVUC> LayTatCaKhuVuc()
        {
            return _diaDiemDAL.LayTatCaKhuVuc();
        }

        public List<DIADIEM> LayDanhSachTrangChu()
        {
            // Có thể thêm logic kiểm tra dữ liệu hoặc giới hạn số lượng quán ở đây nếu muốn
            return _diaDiemDAL.LayDanhSachTrangChu();
        }
        public DIADIEM LayChiTietDiaDiem(int maDD)
        {
            // Nếu sau này bạn cần đếm số lượt xem (View count), có thể viết code cộng lượt xem ở đây
            return _diaDiemDAL.LayChiTietDiaDiem(maDD);
        }
    }
}
