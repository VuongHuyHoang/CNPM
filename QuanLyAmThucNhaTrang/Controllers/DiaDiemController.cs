using QuanLyAmThucNhaTrang.BLL;
using QuanLyAmThucNhaTrang.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            // 1. ĐÃ SỬA LẠI THỨ TỰ THAM SỐ: maKV đứng trước, maDM đứng sau cho khớp với BLL
            var dsDiaDiem = _diaDiemBLL.TimKiemVaLoc(tuKhoa, maKV, maDM);

            // 2. Bọc dữ liệu lại, kết hợp check Null cho danh mục
            var result = dsDiaDiem.Select(d => new {
                MaDD = d.MaDD,
                TenDD = d.TenDD,
                ViDo = d.ViDo,
                KinhDo = d.KinhDo,
                DiaChi = d.DiaChiChiTiet,
                Diem = d.DiemDanhGiaTB,
                // Dùng toán tử an toàn (C# 6.0+) để tránh sập web nếu DANHMUC bị null
                TenDM = d.DANHMUC != null ? d.DANHMUC.TenDM : "Chưa phân loại"
            }).ToList();

            // 3. Trả về định dạng JSON
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
        [HttpPost]
        public ActionResult XoaDanhGia(int maDG, int maDD)
        {
            if (Session["MaTK"] == null) return RedirectToAction("DangNhap", "TaiKhoan");
            int maTK = (int)Session["MaTK"];

            if (_danhGiaBLL.XoaDanhGia(maDG, maTK))
                TempData["Success"] = "Đã xóa đánh giá của bạn.";
            else
                TempData["Error"] = "Có lỗi xảy ra, không thể xóa đánh giá.";

            return RedirectToAction("ChiTiet", "DiaDiem", new { id = maDD });
        }

        [HttpPost]
        public ActionResult SuaDanhGia(int maDG, int maDD, int SoSao, string NoiDung)
        {
            if (Session["MaTK"] == null) return RedirectToAction("DangNhap", "TaiKhoan");
            int maTK = (int)Session["MaTK"];

            if (_danhGiaBLL.SuaDanhGia(maDG, SoSao, NoiDung, maTK))
                TempData["Success"] = "Đã cập nhật lại đánh giá của bạn!";
            else
                TempData["Error"] = "Có lỗi xảy ra khi sửa đánh giá.";

            return RedirectToAction("ChiTiet", "DiaDiem", new { id = maDD });
        }

        public ActionResult TimKiem(string tuKhoa, int? maDM, int? maKV, string sapXep, int page = 1)
        {
            // 1. Lấy toàn bộ danh sách địa điểm đã lọc theo điều kiện
            var danhSach = _diaDiemBLL.TimKiemVaLoc(tuKhoa, maKV, maDM);

            // 2. THỰC HIỆN LOGIC SẮP XẾP ĐỘNG
            if (string.IsNullOrEmpty(sapXep)) sapXep = "danhGia"; // Mặc định nếu chưa chọn gì

            switch (sapXep)
            {
                case "moiNhat":
                    danhSach = danhSach.OrderByDescending(d => d.NgayDangKy).ToList();
                    break;
                case "tenAZ":
                    danhSach = danhSach.OrderBy(d => d.TenDD).ToList();
                    break;
                default: // "danhGia"
                    danhSach = danhSach.OrderByDescending(d => d.DiemDanhGiaTB).ToList();
                    break;
            }

            // 3. THIẾT LẬP CÁC BIẾN PHÂN TRANG
            int pageSize = 6;
            int totalItems = danhSach.Count; // Đây mới là TỔNG SỐ THỰC TẾ (ví dụ: 40 quán)
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var duLieuTrang = danhSach.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // 4. ĐẨY DỮ LIỆU QUA VIEW
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems; // <--- Đẩy tổng số lượng thực tế ra View

            ViewBag.TuKhoaHienTai = tuKhoa;
            ViewBag.MaKVHienTai = maKV;
            ViewBag.MaDMHienTai = maDM;
            ViewBag.SapXepHienTai = sapXep;  // <--- Giữ lại trạng thái sắp xếp để hiển thị trên Dropdown

            ViewBag.DanhMucList = _diaDiemBLL.LayTatCaDanhMuc();
            ViewBag.KhuVucList = _diaDiemBLL.LayTatCaKhuVuc();

            return View(duLieuTrang);
        }

        // GET: DiaDiem/ChiTiet/5
        public ActionResult ChiTiet(int id)
        {
            // 1. Lấy thông tin địa điểm từ BLL
            var dd = _diaDiemBLL.LayChiTietDiaDiem(id);
            if (dd == null) return HttpNotFound("Không tìm thấy dữ liệu.");

            // 2. KHỞI TẠO BỘ NHẬN DIỆN QUYỀN (VIP PASS)
            // Kiểm tra xem người đang truy cập có phải là Admin không?
            bool isAdmin = Session["LoaiTK"] != null && Session["LoaiTK"].ToString() == "QuanTriVien";

            // Kiểm tra xem người đang truy cập có phải là Chủ của chính quán này không?
            bool isOwner = Session["MaTK"] != null && (int)Session["MaTK"] == dd.MaTK;
            ViewBag.KhuyenMaiList = _khuyenMaiBLL.LayKhuyenMaiTheoDiaDiem(id);
            var testKM = ViewBag.KhuyenMaiList as List<QuanLyAmThucNhaTrang.DAL.KHUYENMAI>;
            System.Diagnostics.Debug.WriteLine("====> SỐ LƯỢNG KHUYẾN MÃI LẤY ĐƯỢC LÀ: " + testKM.Count);

            // 3. LOGIC CHẶN HIỂN THỊ
            // Nếu quán đang không hoạt động bình thường, và người xem lại KHÔNG PHẢI admin, CŨNG KHÔNG PHẢI chủ quán
            // -> Lúc này mới chắc chắn là Khách vãng lai -> Chặn lại báo lỗi 404
            if (dd.TrangThai.Trim() != "DangHoatDong" && dd.TrangThai.Trim() != "TamNgung" && !isAdmin && !isOwner)
            {
                return HttpNotFound("Cơ sở này hiện không hoạt động hoặc đang chờ cấp phép.");
            }

            // Nếu qua được ải trên (tức là quán đang Hoạt động, HOẶC người đang xem là Admin/Chủ quán)
            // -> Lấy thêm dữ liệu liên quan (như bình luận, đánh giá...) và hiển thị View
            return View(dd);
        }

        // [POST] Xử lý đăng ký quán mới - Nhận nhiều file ảnh cùng lúc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKyDiaDiem(DIADIEM dd, IEnumerable<HttpPostedFileBase> hinhAnhs)
        {
            // 1. Kiểm tra điều kiện đăng nhập và quyền Chủ quán
            if (Session["MaTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD")
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            int maTK = Convert.ToInt32(Session["MaTK"]);
            dd.MaTK = maTK;

            // 2. Chuyển đổi danh sách file hợp lệ (loại bỏ các file rỗng)
            var danhSachFile = hinhAnhs != null
                ? hinhAnhs.Where(f => f != null && f.ContentLength > 0).ToList()
                : new List<HttpPostedFileBase>();

            // 3. RÀNG BUỘC QĐ5: Kiểm tra số lượng ảnh tối thiểu (ít nhất 2 ảnh mặt tiền)
            if (danhSachFile.Count < 2)
            {
                ModelState.AddModelError("", "Biểu mẫu bắt buộc phải tải lên ít nhất 2 hình ảnh mặt tiền quán để xác minh!");
                // Nạp lại các danh mục, khu vực cho Dropdown trước khi trả về View hiển thị lỗi
                ViewBag.MaDM = new SelectList(_diaDiemBLL.LayTatCaDanhMuc(), "MaDM", "TenDM");
                ViewBag.MaKV = new SelectList(_diaDiemBLL.LayTatCaKhuVuc(), "MaKV", "TenKV");
                return View(dd);
            }

            if (ModelState.IsValid)
            {
                // 4. Lưu thông tin địa điểm trước để sinh ra MaDD (Khóa chính tự tăng)
                int maDDMoi = _diaDiemBLL.ThemDiaDiemMoi(dd);

                if (maDDMoi > 0)
                {
                    // 5. Vòng lặp lưu từng file ảnh vào thư mục và ghi vào CSDL
                    int thuTu = 1;
                    foreach (var file in danhSachFile)
                    {
                        // Tạo tên file duy nhất bằng GUID để tránh trùng lặp tệp tin trên máy chủ
                        string tenFile = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string duongDanThuMuc = Server.MapPath("~/images/uploads/");

                        // Đảm bảo thư mục tồn tại
                        if (!Directory.Exists(duongDanThuMuc)) Directory.CreateDirectory(duongDanThuMuc);

                        string duongDanVatLy = Path.Combine(duongDanThuMuc, tenFile);
                        file.SaveAs(duongDanVatLy); // Lưu file vào ổ cứng server

                        // Đường dẫn tương đối lưu xuống database để hiển thị trên web
                        string duongDanDb = "/images/uploads/" + tenFile;

                        // Gọi BLL ghi vào bảng HINHANH, gán loại hình là 'MatTien' cho các ảnh xác minh ban đầu
                        _diaDiemBLL.ThemHinhAnh(maDDMoi, duongDanDb, "MatTien");
                        thuTu++;
                    }

                    TempData["Success"] = "Gửi yêu cầu đăng ký địa điểm thành công! Vui lòng chờ Ban quản trị kiểm duyệt.";
                    return RedirectToAction("QuanLyGianHang", "ChuCSKD");
                }
            }

            return View(dd);
        }

        // [POST] Xử lý hủy yêu cầu phê duyệt từ phía Chủ quán
        [HttpPost]
        public ActionResult XoaYeuCau(int id)
        {
            if (Session["MaTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD")
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            int maTK = Convert.ToInt32(Session["MaTK"]);

            if (_diaDiemBLL.XoaYeuCauDangKy(id, maTK))
            {
                TempData["Success"] = "Đã hủy bỏ và xóa hoàn toàn yêu cầu phê duyệt địa điểm thành công!";
            }
            else
            {
                TempData["Error"] = "Không thể xóa yêu cầu này (Có thể địa điểm đã được phê duyệt hoặc không thuộc quyền sở hữu của bạn).";
            }

            return RedirectToAction("QuanLyGianHang", "ChuCSKD");
        }
    }
}