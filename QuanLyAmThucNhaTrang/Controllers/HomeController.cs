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
        private DanhMucBLL _danhMucBLL = new DanhMucBLL();

        public ActionResult Index()
        {
            // Lấy danh sách danh mục từ BLL
            var danhSachDM = _danhMucBLL.GetAll();

            // Truyền danh sách này ra View
            return View(danhSachDM);
        }
    }
}