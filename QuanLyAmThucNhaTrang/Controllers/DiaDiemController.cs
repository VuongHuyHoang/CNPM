using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyAmThucNhaTrang.BLL;
using QuanLyAmThucNhaTrang.DAL;

namespace QuanLyAmThucNhaTrang.Controllers
{
    public class DiaDiemController : Controller
    {
        private YeuThichBLL _yeuThichBLL = new YeuThichBLL();
        private DiaDiemBLL _diaDiemBLL = new DiaDiemBLL();
        private DanhGiaBLL _danhGiaBLL = new DanhGiaBLL();
        private KhuyenMaiBLL _khuyenMaiBLL = new KhuyenMaiBLL();

        // 1. GIAO DIỆN BẢN ĐỒ (GET)
        public ActionResult BanDo()
        {
            // Truyền dữ liệu danh mục và khu vực sang View để làm bộ lọc
            ViewBag.DanhMucList = _diaDiemBLL.LayTatCaDanhMuc();
            ViewBag.KhuVucList = _diaDiemBLL.LayTatCaKhuVuc();
            return View();
        }

        // 2. API TRẢ VỀ DỮ LIỆU JSON CHO BẢN ĐỒ (Dùng AJAX gọi ngầm)
        [HttpGet]
        public JsonResult LayDuLieuBanDo(string tuKhoa, int? maDM, int? maKV)
        {
            // Tái sử dụng hàm tìm kiếm đã viết
            var dsDiaDiem = _diaDiemBLL.TimKiemVaLoc(tuKhoa, maDM, maKV);

            // Bọc dữ liệu lại, CHỈ LẤY những cột cần thiết để đưa lên bản đồ
            // (Tránh lỗi Circular Reference - Vòng lặp vô tận của Entity Framework khi parse JSON)
            var result = dsDiaDiem.Select(d => new {
                MaDD = d.MaDD,
                TenDD = d.TenDD,
                ViDo = d.ViDo,
                KinhDo = d.KinhDo,
                DiaChi = d.DiaChiChiTiet,
                Diem = d.DiemDanhGiaTB,
                TenDM = d.DANHMUC.TenDM
            }).ToList();

            // Trả về định dạng JSON
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ThemDanhGia(int MaDD, int SoSao, string NoiDung)
        {
            // 1. Kiểm tra đăng nhập (Chặn những ai cố tình gọi URL khi chưa đăng nhập)
            if (Session["MaTK"] == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để viết đánh giá!";
                return RedirectToAction("ChiTiet", new { id = MaDD });
            }

            int maTK = (int)Session["MaTK"];

            // 2. Gọi BLL xử lý
            string ketQua = _danhGiaBLL.GuiDanhGia(maTK, MaDD, SoSao, NoiDung);

            if (ketQua == "Success")
            {
                TempData["Success"] = "Cảm ơn bạn đã chia sẻ trải nghiệm tuyệt vời này!";
            }
            else
            {
                TempData["Error"] = ketQua; // Báo lỗi nếu đã đánh giá rồi
            }

            // 3. Xong xuôi thì load lại trang chi tiết đó
            return RedirectToAction("ChiTiet", new { id = MaDD });
        }

        // GET: DiaDiem/TimKiem
        public ActionResult TimKiem(string tuKhoa, int? maDM, int? maKV)
        {
            // 1. Lấy dữ liệu nạp vào các bộ lọc trên giao diện
            ViewBag.DanhMucList = _diaDiemBLL.LayTatCaDanhMuc();
            ViewBag.KhuVucList = _diaDiemBLL.LayTatCaKhuVuc();

            // 2. Giữ lại trạng thái người dùng đã chọn để hiển thị lại trên Form sau khi tải lại trang
            ViewBag.TuKhoaHienTai = tuKhoa;
            ViewBag.MaDMHienTai = maDM;
            ViewBag.MaKVHienTai = maKV;

            // 3. Thực hiện tìm kiếm dữ liệu
            var kếtQuả = _diaDiemBLL.TimKiemVaLoc(tuKhoa, maDM, maKV);

            return View(kếtQuả);
        }

        // GET: DiaDiem/ChiTiet/5
        public ActionResult ChiTiet(int id)
        {
            var diaDiem = _diaDiemBLL.LayChiTietDiaDiem(id);
            if (diaDiem == null) return HttpNotFound();

            // Nếu người dùng nhập mã bậy bạ trên URL không tồn tại
            if (diaDiem == null)
            {
                return HttpNotFound("Không tìm thấy địa điểm ẩm thực này.");
            }

            bool daLuu = false;
            if (Session["MaTK"] != null)
            {
                int maTK = (int)Session["MaTK"];
                daLuu = _yeuThichBLL.KiemTraTrangThaiLuu(maTK, id);
            }
            ViewBag.DaLuu = daLuu; // Gửi trạng thái sang View
            ViewBag.KhuyenMaiList = _khuyenMaiBLL.LayKhuyenMaiHieuLuc(id);

            return View(diaDiem);
        }
    }
}