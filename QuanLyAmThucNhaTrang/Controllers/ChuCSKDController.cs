using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using QuanLyAmThucNhaTrang.BLL;
using QuanLyAmThucNhaTrang.DAL;

namespace QuanLyAmThucNhaTrang.Controllers
{
    public class ChuCSKDController : Controller
    {
        private TaiKhoanBLL _taiKhoanBLL = new TaiKhoanBLL();
        private DiaDiemBLL _diaDiemBLL = new DiaDiemBLL();
        private KhuyenMaiBLL _khuyenMaiBLL = new KhuyenMaiBLL();
        private PhanHoiBLL _phanHoiBLL = new PhanHoiBLL();

        // 1. GIAO DIỆN FORM ĐĂNG KÝ (GET)
        public ActionResult ThemDiaDiem()
        {
            // Bảo mật: Kiểm tra đúng quyền Chủ cơ sở mới cho vào
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD")
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // Truyền danh sách Danh mục và Khu vực lên Form (Dropdown)
            ViewBag.MaDM = new SelectList(_diaDiemBLL.LayTatCaDanhMuc(), "MaDM", "TenDM");
            ViewBag.MaKV = new SelectList(_diaDiemBLL.LayTatCaKhuVuc(), "MaKV", "TenKV");

            return View();
        }

        // 2. XỬ LÝ LƯU DỮ LIỆU VÀ FILE ẢNH (POST)
        [HttpPost]
        [ValidateInput(false)] // Cho phép nhập mô tả có ký tự đặc biệt (HTML)
        public ActionResult ThemDiaDiem(DIADIEM dd, HttpPostedFileBase AnhMatTien)
        {
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD") return RedirectToAction("DangNhap", "TaiKhoan");

            try
            {
                // Gán mã người tạo (Chủ quán)
                dd.MaTK = (int)Session["MaTK"];

                // 1. Lưu thông tin địa điểm vào DB trước để lấy MaDD
                int maDDMoi = _diaDiemBLL.ThemDiaDiemMoi(dd);

                // 2. Xử lý Upload file ảnh mặt tiền (Nếu có chọn file)
                if (AnhMatTien != null && AnhMatTien.ContentLength > 0)
                {
                    // Tạo tên file ngẫu nhiên để không bị trùng lặp (Thêm mốc thời gian)
                    string fileName = Path.GetFileName(AnhMatTien.FileName);
                    string uniqueName = DateTime.Now.ToString("yyyyMMdd_HHmmss_") + fileName;

                    // Chỉ định đường dẫn vật lý trên Server
                    string serverPath = Path.Combine(Server.MapPath("~/images/uploads/"), uniqueName);

                    // Lưu file vật lý
                    AnhMatTien.SaveAs(serverPath);

                    // Lưu đường dẫn tương đối vào DB
                    string dbPath = "/images/uploads/" + uniqueName;
                    _diaDiemBLL.ThemHinhAnh(maDDMoi, dbPath, "MatTien");
                }

                TempData["Success"] = "Đăng ký địa điểm thành công! Vui lòng chờ Ban Quản Trị xét duyệt.";
                return RedirectToAction("ThemDiaDiem");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                // Nạp lại Viewbag nếu form bị lỗi
                ViewBag.MaDM = new SelectList(_diaDiemBLL.LayTatCaDanhMuc(), "MaDM", "TenDM");
                ViewBag.MaKV = new SelectList(_diaDiemBLL.LayTatCaKhuVuc(), "MaKV", "TenKV");
                return View(dd);
            }
        }

        // 3. TRANG DANH SÁCH GIAN HÀNG CỦA TÔI (GET)
        public ActionResult QuanLyGianHang()
        {
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD")
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            int maTK = (int)Session["MaTK"];
            var dsGianHang = _diaDiemBLL.LayDanhSachTheoChuQuan(maTK);
            return View(dsGianHang);
        }

        // 4. GIAO DIỆN SỬA ĐỊA ĐIỂM (GET)
        public ActionResult SuaDiaDiem(int id)
        {
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD") return RedirectToAction("DangNhap", "TaiKhoan");

            var diaDiem = _diaDiemBLL.LayChiTietDiaDiem(id); // Dùng lại hàm lấy chi tiết có sẵn của bạn

            // Bảo mật tối cao: Tránh trường hợp chủ quán dùng URL sửa quán của người khác
            if (diaDiem == null || diaDiem.MaTK != (int)Session["MaTK"])
            {
                return HttpNotFound();
            }

            ViewBag.MaDM = new SelectList(_diaDiemBLL.LayTatCaDanhMuc(), "MaDM", "TenDM", diaDiem.MaDM);
            ViewBag.MaKV = new SelectList(_diaDiemBLL.LayTatCaKhuVuc(), "MaKV", "TenKV", diaDiem.MaKV);

            return View(diaDiem);
        }

        // 5. XỬ LÝ LƯU THÔNG TIN SỬA ĐỔI (POST)
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SuaDiaDiem(DIADIEM dd)
        {
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD") return RedirectToAction("DangNhap", "TaiKhoan");

            if (_diaDiemBLL.CapNhatGianHang(dd))
            {
                TempData["Success"] = "Cập nhật thông tin gian hàng thành công! Quán đang chờ kiểm duyệt lại.";
                return RedirectToAction("QuanLyGianHang");
            }

            ViewBag.Error = "Cập nhật thất bại. Vui lòng kiểm tra dữ liệu.";
            ViewBag.MaDM = new SelectList(_diaDiemBLL.LayTatCaDanhMuc(), "MaDM", "TenDM", dd.MaDM);
            ViewBag.MaKV = new SelectList(_diaDiemBLL.LayTatCaKhuVuc(), "MaKV", "TenKV", dd.MaKV);
            return View(dd);
        }

        public ActionResult TrangCaNhan()
        {
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD") return RedirectToAction("DangNhap", "TaiKhoan");
            int maTK = (int)Session["MaTK"];
            var user = _taiKhoanBLL.LayThongTinAcount(maTK);
            return View(user);
        }

        [HttpPost]
        public ActionResult CapNhatHoSo(string HoTen, string Email, string SDT)
        {
            int maTK = (int)Session["MaTK"];
            var tk = _taiKhoanBLL.LayThongTinAcount(maTK);
            tk.HoTen = HoTen; tk.Email = Email; tk.SDT = SDT;

            if (_taiKhoanBLL.CapNhatHoSo(tk))
            {
                Session["HoTen"] = tk.HoTen;
                TempData["Success"] = "Cập nhật hồ sơ thành công!";
            }
            return RedirectToAction("TrangCaNhan");
        }

        [HttpPost]
        public ActionResult DoiMatKhau(string MatKhauCu, string MatKhauMoi)
        {
            int maTK = (int)Session["MaTK"];
            string ketQua = _taiKhoanBLL.DoiMatKhau(maTK, MatKhauCu, MatKhauMoi);
            if (ketQua == "Success") TempData["Success"] = "Đổi mật khẩu thành công!";
            else TempData["Error"] = ketQua;

            return RedirectToAction("TrangCaNhan");
        }

        public ActionResult KhuyenMai()
        {
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD") return RedirectToAction("DangNhap", "TaiKhoan");
            int maTK = (int)Session["MaTK"];

            // Lấy danh sách các quán ăn của chủ quán này để đổ vào Dropdown chọn quán
            ViewBag.MaDD = new SelectList(_diaDiemBLL.LayDanhSachTheoChuQuan(maTK), "MaDD", "TenDD");

            var listKM = _khuyenMaiBLL.LayDanhSachKhuyenMai(maTK);
            return View(listKM);
        }

        [HttpPost]
        public ActionResult ThemKhuyenMai(KHUYENMAI km)
        {
            string ketQua = _khuyenMaiBLL.ThemKhuyenMai(km);
            if (ketQua == "Success") TempData["Success"] = "Đã tạo chương trình khuyến mãi mới!";
            else TempData["Error"] = ketQua;

            return RedirectToAction("KhuyenMai");
        }

        [HttpPost]
        public JsonResult XoaKhuyenMai(int maKM)
        {
            bool result = _khuyenMaiBLL.XoaKhuyenMai(maKM);
            return Json(new { success = result });
        }

        //=======================================================================
        public ActionResult PhanHoiDanhGia()
        {
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD") return RedirectToAction("DangNhap", "TaiKhoan");

            int maTK = (int)Session["MaTK"];
            var listDanhGia = _phanHoiBLL.LayDanhGiaTheoChuQuan(maTK);

            return View(listDanhGia);
        }

        [HttpPost]
        public ActionResult GuiPhanHoi(int MaDG, string NoiDung)
        {
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD") return RedirectToAction("DangNhap", "TaiKhoan");

            int maTK = (int)Session["MaTK"];
            string ketQua = _phanHoiBLL.GuiPhanHoi(MaDG, NoiDung, maTK);

            if (ketQua == "Success")
            {
                TempData["Success"] = "Đã gửi phản hồi thành công tới khách hàng!";
            }
            else
            {
                TempData["Error"] = ketQua;
            }

            return RedirectToAction("PhanHoiDanhGia");
        }
    }
}