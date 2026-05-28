using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyAmThucNhaTrang.BLL;
using QuanLyAmThucNhaTrang.DAL;

namespace QuanLyAmThucNhaTrang.Controllers
{
    public class TaiKhoanController : Controller
    {
        private TaiKhoanBLL _taiKhoanBLL = new TaiKhoanBLL();

        // 1. GIAO DIỆN ĐĂNG NHẬP (GET)
        public ActionResult DangNhap()
        {
            return View();
        }

        // 2. XỬ LÝ ĐĂNG NHẬP (POST)
        [HttpPost]
        public ActionResult DangNhap(string TenDangNhap, string MatKhau)
        {
            var user = _taiKhoanBLL.DangNhap(TenDangNhap, MatKhau);
            if (user != null)
            {
                // Gán Session chuẩn để _LoginPartial.cshtml nhận diện được
                Session["MaTK"] = user.MaTK;
                Session["HoTen"] = user.HoTen;
                Session["LoaiTK"] = user.LoaiTK;

                TempData["Success"] = "Chào mừng " + user.HoTen + " đã quay trở lại!";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác!";
            return View();
        }

        // 3. GIAO DIỆN ĐĂNG KÝ (GET)
        public ActionResult DangKy()
        {
            return View();
        }

        // 4. XỬ LÝ ĐĂNG KÝ (POST)
        [HttpPost]
        public ActionResult DangKy(TAIKHOAN tk)
        {
            // Mặc định đăng ký mới là Khách hàng (theo báo cáo của bạn)
            tk.LoaiTK = "KhachHang";

            string result = _taiKhoanBLL.DangKy(tk);
            if (result == "Success")
            {
                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("DangNhap");
            }

            ViewBag.Error = result;
            return View(tk);
        }

        // 5. ĐĂNG XUẤT
        public ActionResult DangXuat()
        {
            Session.Clear(); // Xóa sạch thông tin đăng nhập
            return RedirectToAction("Index", "Home");
        }
    }
}