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
            // NGHIỆP VỤ MỚI BỔ SUNG (QĐ11): 
            // Dùng LINQ để lọc, chỉ trả về các Danh mục đang ở trạng thái "HoatDong".
            // Tránh trường hợp Chủ quán chọn nhầm vào Danh mục đã bị Admin "NgungSuDung".
            return _diaDiemDAL.LayTatCaDanhMuc()
                              .Where(dm => dm.TrangThai == "HoatDong")
                              .ToList();
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

        public bool XoaYeuCauDangKy(int maDD, int maTK)
        {
            return _diaDiemDAL.XoaYeuCauDangKy(maDD, maTK);
        }

        // Thêm địa điểm với các giá trị mặc định ban đầu
        public int ThemDiaDiemMoi(DIADIEM dd)
        {
            // Thiết lập giá trị mặc định cho quán mới đăng ký
            dd.TrangThai = "ChoDuyet"; // Bắt buộc chờ Admin duyệt
            dd.DiemDanhGiaTB = 0;
            dd.SoLuotDanhGia = 0;
            dd.NgayDangKy = DateTime.Now;

            return _diaDiemDAL.ThemDiaDiemMoi(dd);
        }

        // Thêm hình ảnh
        public bool ThemHinhAnh(int maDD, string duongDan, string loaiHinhAnh)
        {
            HINHANH ha = new HINHANH
            {
                MaDD = maDD,
                DuongDan = duongDan,
                LoaiHinhAnh = loaiHinhAnh
            };
            return _diaDiemDAL.ThemHinhAnh(ha);
        }
        public bool XoaHinhAnh(int maHA)
        {
            return _diaDiemDAL.XoaHinhAnh(maHA);
        }

        public List<DIADIEM> LayDanhSachTheoChuQuan(int maTK)
        {
            return _diaDiemDAL.LayDanhSachTheoChuQuan(maTK);
        }

        public bool CapNhatGianHang(DIADIEM dd)
        {
            // Nghiệp vụ bảo mật: Mỗi khi sửa thông tin, hệ thống tự động ép trạng thái về "ChoDuyet"
            // để Admin kiểm tra lại tính chính xác trước khi cho hiển thị công khai.
            dd.TrangThai = "ChoDuyetSua";

            return _diaDiemDAL.CapNhatDiaDiem(dd);
        }
        public bool HuyYeuCauCapNhat(int maDD, int maTK)
        {
            return _diaDiemDAL.HuyYeuCauCapNhat(maDD, maTK);
        }
        public bool CapNhatTrangThaiNhanh(int maDD, string trangThaiMoi)
        {
            return _diaDiemDAL.CapNhatTrangThaiNhanh(maDD, trangThaiMoi);
        }
    }
}