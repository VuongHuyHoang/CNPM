using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyAmThucNhaTrang.BLL;

namespace QuanLyAmThucNhaTrang.Controllers
{
    public class HomeController : Controller
    {
        private DiaDiemBLL _diaDiemBLL = new DiaDiemBLL();

        public ActionResult Index()
        {
            // Lấy danh sách địa điểm đang hoạt động từ BLL
            var dsDiaDiem = _diaDiemBLL.LayDanhSachTrangChu();

            // Truyền danh sách này vào trong View
            return View(dsDiaDiem);
        }
    }
}