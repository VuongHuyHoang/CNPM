CREATE DATABASE QuanLyAmThucNhaTrang;
GO
 
USE QuanLyAmThucNhaTrang;
GO
 
-- ============================================================
-- BƯỚC 2: TẠO CÁC BẢNG DỮ LIỆU (10 bảng)
-- ============================================================
 
-- -----------------------------------------------
-- 1. Bảng TAIKHOAN
-- Lưu thông tin tài khoản người dùng
-- 3 loại: KhachHang, ChuCSKD, QuanTriVien
-- -----------------------------------------------
CREATE TABLE TAIKHOAN (
    MaTK            INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap     VARCHAR(30)     NOT NULL,
    MatKhau         VARCHAR(255)    NOT NULL,
    HoTen           NVARCHAR(100)   NOT NULL,
    Email           VARCHAR(100)    NULL,
    SDT             VARCHAR(15)     NULL,
    LoaiTK          VARCHAR(20)     NOT NULL,
    AvatarUrl       VARCHAR(500)    NULL,
    NgayTao         DATE            NOT NULL DEFAULT GETDATE(),
    TrangThai       VARCHAR(20)     NOT NULL DEFAULT N'HoatDong',
 
    -- Ràng buộc
    CONSTRAINT UQ_TaiKhoan_TenDangNhap  UNIQUE (TenDangNhap),
    CONSTRAINT UQ_TaiKhoan_Email        UNIQUE (Email),
    CONSTRAINT UQ_TaiKhoan_SDT          UNIQUE (SDT),
    CONSTRAINT CK_TaiKhoan_LoaiTK       CHECK (LoaiTK IN ('KhachHang', 'ChuCSKD', 'QuanTriVien')),
    CONSTRAINT CK_TaiKhoan_TrangThai    CHECK (TrangThai IN ('HoatDong', 'BiKhoa'))
);
GO
 
-- -----------------------------------------------
-- 2. Bảng DANHMUC
-- Phân loại ẩm thực (Quán ăn, Nhà hàng, Cà phê...)
-- -----------------------------------------------
CREATE TABLE DANHMUC (
    MaDM            INT IDENTITY(1,1) PRIMARY KEY,
    TenDM           NVARCHAR(100)   NOT NULL,
    MoTa            NVARCHAR(300)   NULL,
    TrangThai       VARCHAR(20)     NOT NULL DEFAULT N'HoatDong',
 
    CONSTRAINT UQ_DanhMuc_TenDM         UNIQUE (TenDM),
    CONSTRAINT CK_DanhMuc_TrangThai     CHECK (TrangThai IN ('HoatDong', 'NgungSuDung'))
);
GO
 
-- -----------------------------------------------
-- 3. Bảng KHUVUC
-- Danh sách khu vực trên địa bàn TP Nha Trang
-- ToaDoTrungTam: dùng để di chuyển camera Google Maps
-- -----------------------------------------------
CREATE TABLE KHUVUC (
    MaKV            INT IDENTITY(1,1) PRIMARY KEY,
    TenKV           NVARCHAR(100)   NOT NULL,
    MoTa            NVARCHAR(300)   NULL,
    ToaDoTrungTam   VARCHAR(30)     NULL,
 
    CONSTRAINT UQ_KhuVuc_TenKV          UNIQUE (TenKV)
);
GO
 
-- -----------------------------------------------
-- 4. Bảng DIADIEM
-- Lưu thông tin địa điểm ẩm thực
-- ViDo, KinhDo: tọa độ cho Google Maps
-- DiemDanhGiaTB, SoLuotDanhGia: thuộc tính tính toán
-- -----------------------------------------------
CREATE TABLE DIADIEM (
    MaDD            INT IDENTITY(1,1) PRIMARY KEY,
    TenDD           NVARCHAR(200)   NOT NULL,
    DiaChiChiTiet   NVARCHAR(300)   NOT NULL,
    ViDo            FLOAT           NOT NULL,       -- Latitude (Google Maps)
    KinhDo          FLOAT           NOT NULL,       -- Longitude (Google Maps)
    SDT             VARCHAR(15)     NULL,            -- SĐT liên hệ quán
    GioMoCua        TIME            NULL,
    GioDongCua      TIME            NULL,
    MoTa            NTEXT           NULL,
    TrangThai       VARCHAR(20)     NOT NULL DEFAULT N'ChoDuyet',
    DiemDanhGiaTB   FLOAT           NOT NULL DEFAULT 0,
    SoLuotDanhGia   INT             NOT NULL DEFAULT 0,
    NgayDangKy      DATE            NOT NULL DEFAULT GETDATE(),
	LyDoTuChoi NVARCHAR(500) NULL,
	MaDD_Goc INT NULL,
    MaTK            INT             NOT NULL,        -- FK → TAIKHOAN (chủ quán)
    MaDM            INT             NOT NULL,        -- FK → DANHMUC
    MaKV            INT             NOT NULL,        -- FK → KHUVUC
 
    CONSTRAINT CK_DiaDiem_TrangThai     CHECK (TrangThai IN ('ChoDuyet', 'ChoDuyetSua', 'DangHoatDong', 'TamNgung', 'TuChoi', 'TuChoiSua')),
    CONSTRAINT CK_DiaDiem_DiemTB        CHECK (DiemDanhGiaTB >= 0 AND DiemDanhGiaTB <= 5),
    CONSTRAINT CK_DiaDiem_SoLuot        CHECK (SoLuotDanhGia >= 0),
    CONSTRAINT FK_DiaDiem_TaiKhoan      FOREIGN KEY (MaTK) REFERENCES TAIKHOAN(MaTK),
    CONSTRAINT FK_DiaDiem_DanhMuc       FOREIGN KEY (MaDM) REFERENCES DANHMUC(MaDM),
    CONSTRAINT FK_DiaDiem_KhuVuc        FOREIGN KEY (MaKV) REFERENCES KHUVUC(MaKV),
	CONSTRAINT FK_DiaDiem_DiaDiemGoc    FOREIGN KEY (MaDD_Goc) REFERENCES DIADIEM(MaDD)
);
GO

 
-- -----------------------------------------------
-- 5. Bảng DANHGIA
-- Lưu đánh giá của khách hàng về địa điểm
-- Ràng buộc: mỗi TK chỉ đánh giá 1 lần / ĐĐ (QĐ3)
-- -----------------------------------------------
CREATE TABLE DANHGIA (
    MaDG            INT IDENTITY(1,1) PRIMARY KEY,
    SoSao           INT             NOT NULL,
    NoiDung         NTEXT           NULL,
    NgayDanhGia     DATETIME        NOT NULL DEFAULT GETDATE(),
    TrangThai       VARCHAR(20)     NOT NULL DEFAULT N'HienThi',
    MaTK            INT             NOT NULL,        -- FK → TAIKHOAN (khách hàng)
    MaDD            INT             NOT NULL,        -- FK → DIADIEM
 
    CONSTRAINT CK_DanhGia_SoSao        CHECK (SoSao >= 1 AND SoSao <= 5),
    CONSTRAINT CK_DanhGia_TrangThai    CHECK (TrangThai IN ('HienThi', 'DaAn', 'DaXoa')),
    CONSTRAINT UQ_DanhGia_TK_DD        UNIQUE (MaTK, MaDD),    -- QĐ3: mỗi TK chỉ ĐG 1 lần / ĐĐ
    CONSTRAINT FK_DanhGia_TaiKhoan     FOREIGN KEY (MaTK) REFERENCES TAIKHOAN(MaTK),
    CONSTRAINT FK_DanhGia_DiaDiem      FOREIGN KEY (MaDD) REFERENCES DIADIEM(MaDD)
);
GO
 
-- -----------------------------------------------
-- 6. Bảng PHANHOI
-- Phản hồi của chủ quán đối với đánh giá
-- Quan hệ 1-1 với DANHGIA (MaDG là Unique)
-- -----------------------------------------------
CREATE TABLE PHANHOI (
    MaPH            INT IDENTITY(1,1) PRIMARY KEY,
    NoiDung         NTEXT           NOT NULL,
    NgayPhanHoi     DATETIME        NOT NULL DEFAULT GETDATE(),
    MaDG            INT             NOT NULL,        -- FK → DANHGIA (1-1)
    MaTK            INT             NOT NULL,        -- FK → TAIKHOAN (chủ quán)
 
    CONSTRAINT UQ_PhanHoi_MaDG         UNIQUE (MaDG), -- 1 ĐG chỉ có 1 PH
    CONSTRAINT FK_PhanHoi_DanhGia      FOREIGN KEY (MaDG) REFERENCES DANHGIA(MaDG),
    CONSTRAINT FK_PhanHoi_TaiKhoan     FOREIGN KEY (MaTK) REFERENCES TAIKHOAN(MaTK)
);
GO
 
-- -----------------------------------------------
-- 7. Bảng YEUTHICH
-- Bảng trung gian: Tài khoản ↔ Địa điểm (m-n)
-- Ràng buộc: mỗi TK chỉ lưu 1 lần / ĐĐ (QĐ4)
-- -----------------------------------------------
CREATE TABLE YEUTHICH (
    MaYT            INT IDENTITY(1,1) PRIMARY KEY,
    NgayLuu         DATETIME        NOT NULL DEFAULT GETDATE(),
    MaTK            INT             NOT NULL,        -- FK → TAIKHOAN
    MaDD            INT             NOT NULL,        -- FK → DIADIEM
 
    CONSTRAINT UQ_YeuThich_TK_DD       UNIQUE (MaTK, MaDD),    -- QĐ4
    CONSTRAINT FK_YeuThich_TaiKhoan    FOREIGN KEY (MaTK) REFERENCES TAIKHOAN(MaTK),
    CONSTRAINT FK_YeuThich_DiaDiem     FOREIGN KEY (MaDD) REFERENCES DIADIEM(MaDD)
);
GO
 
-- -----------------------------------------------
-- 8. Bảng KHUYENMAI
-- Chương trình khuyến mãi của địa điểm
-- Ràng buộc: NgayBatDau <= NgayKetThuc (QĐ7)
-- -----------------------------------------------
CREATE TABLE KHUYENMAI (
    MaKM            INT IDENTITY(1,1) PRIMARY KEY,
    TenKM           NVARCHAR(200)   NOT NULL,
    NgayBatDau      DATE            NOT NULL,
    NgayKetThuc     DATE            NOT NULL,
    NoiDungUuDai    NTEXT           NULL,
    TrangThai       VARCHAR(20)     NOT NULL DEFAULT N'ConHieuLuc',
    MaDD            INT             NOT NULL,        -- FK → DIADIEM
 
    CONSTRAINT CK_KhuyenMai_Ngay       CHECK (NgayBatDau <= NgayKetThuc),   -- QĐ7
    CONSTRAINT CK_KhuyenMai_TrangThai  CHECK (TrangThai IN ('ConHieuLuc', 'HetHan')),
    CONSTRAINT FK_KhuyenMai_DiaDiem    FOREIGN KEY (MaDD) REFERENCES DIADIEM(MaDD)
);
GO
 
-- -----------------------------------------------
-- 9. Bảng HINHANH
-- Hình ảnh đính kèm của địa điểm
-- -----------------------------------------------
CREATE TABLE HINHANH (
    MaHA            INT IDENTITY(1,1) PRIMARY KEY,
    DuongDan        VARCHAR(500)    NOT NULL,
    LoaiHinhAnh     VARCHAR(30)     NULL,
    ThuTu           INT             NOT NULL DEFAULT 0,
    MaDD            INT             NOT NULL,        -- FK → DIADIEM
 
    CONSTRAINT CK_HinhAnh_Loai         CHECK (LoaiHinhAnh IN ('MatTien', 'ThucDon', 'KhongGian', 'DanhGia') OR LoaiHinhAnh IS NULL),
    CONSTRAINT FK_HinhAnh_DiaDiem      FOREIGN KEY (MaDD) REFERENCES DIADIEM(MaDD)
);
GO
 
-- ============================================================
-- BƯỚC 3: TẠO INDEX (tối ưu truy vấn)
-- ============================================================
 
-- Index tìm kiếm địa điểm theo trạng thái
CREATE INDEX IX_DiaDiem_TrangThai ON DIADIEM(TrangThai);
 
-- Index tìm kiếm theo danh mục, khu vực
CREATE INDEX IX_DiaDiem_MaDM ON DIADIEM(MaDM);
CREATE INDEX IX_DiaDiem_MaKV ON DIADIEM(MaKV);
 
-- Index sắp xếp theo điểm đánh giá TB
CREATE INDEX IX_DiaDiem_DiemTB ON DIADIEM(DiemDanhGiaTB DESC);
 
-- Index đánh giá theo địa điểm
CREATE INDEX IX_DanhGia_MaDD ON DANHGIA(MaDD);
 
-- Index yêu thích theo tài khoản
CREATE INDEX IX_YeuThich_MaTK ON YEUTHICH(MaTK);
 
-- Index khuyến mãi theo địa điểm
CREATE INDEX IX_KhuyenMai_MaDD ON KHUYENMAI(MaDD);
 
-- Index hình ảnh theo địa điểm
CREATE INDEX IX_HinhAnh_MaDD ON HINHANH(MaDD);
GO
 
-- ============================================================
-- BƯỚC 4: CHÈN DỮ LIỆU MẪU (SEED DATA)
-- ============================================================
 
INSERT INTO DANHMUC (TenDM, MoTa, TrangThai)
VALUES 
    (N'Bún cá', N'Các món bún cá sứa, bún chả cá đặc sản Nha Trang', 'HoatDong'),
    (N'Bánh canh', N'Bánh canh chả cá, bánh canh ghẹ, bánh canh tôm', 'HoatDong'),
    (N'Nem nướng', N'Đặc sản nem nướng Ninh Hòa chuẩn vị', 'HoatDong'),
    (N'Bánh căn', N'Bánh căn hải sản, bánh căn thịt bò, trứng tôm mực', 'HoatDong'),
    (N'Bánh xèo', N'Bánh xèo chảo tôm, bánh xèo mực đặc trưng miền Trung', 'HoatDong'),
    (N'Mì quảng', N'Mì quảng mang hương vị đặc trưng của người dân xứ biển', 'HoatDong'),
    (N'Cơm gà', N'Cơm gà xé bóp, cơm gà rô ti, cơm gà xối mỡ', 'HoatDong'),
    (N'Cơm tấm', N'Cơm tấm sườn, bì, chả, trứng ốp la truyền thống', 'HoatDong');
GO

-- =========================================================================================
-- 1. BẢNG TÀI KHOẢN (8 Tài khoản: 1 Admin, 2 Chủ quán, 5 Khách hàng)
-- =========================================================================================
INSERT INTO TAIKHOAN (TenDangNhap, MatKhau, HoTen, Email, SDT, LoaiTK, TrangThai) VALUES
('admin', '123456', N'Quản trị viên Hệ thống', 'admin@nt.com', '0901000000', 'QuanTriVien', 'HoatDong'),
('chuquan1', '123456', N'Chủ Quán - Ông Nguyễn Văn A', 'chua@nt.com', '0902000001', 'ChuCSKD', 'HoatDong'), -- Quản lý Danh mục 1,2,3,4
('chuquan2', '123456', N'Chủ Quán - Bà Trần Thị B', 'chub@nt.com', '0902000002', 'ChuCSKD', 'HoatDong'), -- Quản lý Danh mục 5,6,7,8
('khach1', '123456', N'Khách Hàng Mai', 'kh1@nt.com', '0903000001', 'KhachHang', 'HoatDong'),
('khach2', '123456', N'Khách Hàng Tuấn', 'kh2@nt.com', '0903000002', 'KhachHang', 'HoatDong'),
('khach3', '123456', N'Khách Hàng Lan', 'kh3@nt.com', '0903000003', 'KhachHang', 'HoatDong'),
('khach4', '123456', N'Khách Hàng Hùng', 'kh4@nt.com', '0903000004', 'KhachHang', 'HoatDong'),
('khach5', '123456', N'Khách Hàng Trang', 'kh5@nt.com', '0903000005', 'KhachHang', 'HoatDong');
GO

UPDATE TAIKHOAN
SET MatKhau = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', '123456'), 2));
GO

-- =========================================================================================
-- 2. BẢNG KHU VỰC (5 Khu vực thuộc TP Nha Trang)
-- =========================================================================================
INSERT INTO KHUVUC (TenKV, MoTa, ToaDoTrungTam) VALUES
(N'Lộc Thọ', N'Khu vực trung tâm du lịch sầm uất ven biển Trần Phú', '12.2387,109.1966'),
(N'Phước Hải', N'Khu dân cư đông đúc, nhiều quán ăn địa phương', '12.2458,109.1812'),
(N'Vĩnh Hải', N'Gần chợ Vĩnh Hải, tập trung ẩm thực phía Bắc thành phố', '12.2745,109.2001'),
(N'Phước Long', N'Khu vực tập trung nhiều sinh viên và khu dân cư mới', '12.2212,109.1887'),
(N'Ngọc Hiệp', N'Khu vực ngoại ô yên bình, gần Tháp Bà Ponagar', '12.2615,109.1834');
GO

-- =========================================================================================
-- 3. BẢNG ĐỊA ĐIỂM (40 Địa điểm, mỗi danh mục 5 quán, gán sẵn DiemDanhGiaTB = 5, SoLuotDanhGia = 1)
-- =========================================================================================
-- CHỦ QUÁN 1 (MaTK = 2) | Danh mục 1, 2, 3, 4
INSERT INTO DIADIEM (TenDD, DiaChiChiTiet, ViDo, KinhDo, SDT, GioMoCua, GioDongCua, TrangThai, MaTK, MaDM, MaKV, DiemDanhGiaTB, SoLuotDanhGia) VALUES
-- DM1: Bún cá
(N'Bún cá sứa Năm Beo', N'Lô B2 Chung cư Phan Bội Châu', 12.2492, 109.1911, '0981111111', '06:00', '21:00', 'DangHoatDong', 2, 1, 1, 5, 1),
(N'Bún cá Nguyên Loan', N'123 Ngô Gia Tự', 12.2411, 109.1922, '0981111112', '06:00', '22:00', 'DangHoatDong', 2, 1, 2, 4, 1),
(N'Bún cá Hạnh Nhiên', N'32C Lê Đại Hành', 12.2388, 109.1933, '0981111113', '06:00', '21:00', 'DangHoatDong', 2, 1, 3, 5, 1),
(N'Bún chả cá Mịn', N'170 Bạch Đằng', 12.2366, 109.1944, '0981111114', '07:00', '21:00', 'DangHoatDong', 2, 1, 4, 4, 1),
(N'Bún cá Ninh Hòa', N'55 Thái Nguyên', 12.2466, 109.1844, '0981111115', '06:00', '21:00', 'DangHoatDong', 2, 1, 5, 5, 1),
-- DM2: Bánh canh
(N'Bánh canh Bà Thừa', N'55 Yersin', 12.2477, 109.1922, '0982222221', '06:00', '21:00', 'DangHoatDong', 2, 2, 1, 5, 1),
(N'Bánh canh Phúc', N'53 Vân Đồn', 12.2322, 109.1833, '0982222222', '06:00', '21:00', 'DangHoatDong', 2, 2, 2, 4, 1),
(N'Bánh canh cô Hà', N'14 Phan Chu Trinh', 12.2511, 109.1911, '0982222223', '06:00', '21:00', 'DangHoatDong', 2, 2, 3, 5, 1),
(N'Bánh canh chả cá xích lô', N'Lê Thánh Tôn', 12.2400, 109.1955, '0982222224', '15:00', '23:00', 'DangHoatDong', 2, 2, 4, 4, 1),
(N'Bánh canh khô Cô Tuyết', N'Số 4 Ngô Quyền', 12.2433, 109.1966, '0982222225', '06:00', '21:00', 'DangHoatDong', 2, 2, 5, 5, 1),
-- DM3: Nem nướng
(N'Nem nướng Đặng Văn Quyên', N'16A Lãn Ông', 12.2498, 109.1915, '0983333331', '08:00', '22:00', 'DangHoatDong', 2, 3, 1, 5, 1),
(N'Nem nướng Vũ Thành An', N'15 Lê Lợi', 12.2515, 109.1925, '0983333332', '08:00', '22:00', 'DangHoatDong', 2, 3, 2, 4, 1),
(N'Nem nướng Nhã Trang', N'39 Nguyễn Thị Minh Khai', 12.2355, 109.1965, '0983333333', '08:00', '22:00', 'DangHoatDong', 2, 3, 3, 5, 1),
(N'Nem nướng Cô Nô', N'24 Hai Bà Trưng', 12.2488, 109.1928, '0983333334', '08:00', '22:00', 'DangHoatDong', 2, 3, 4, 4, 1),
(N'Nem nướng Bà Sáu', N'Phố Tô Vĩnh Diện', 12.2444, 109.1899, '0983333335', '14:00', '22:00', 'DangHoatDong', 2, 3, 5, 5, 1),
-- DM4: Bánh căn
(N'Bánh căn Tháp Bà', N'151 Tháp Bà', 12.2644, 109.1901, '0984444441', '15:00', '22:00', 'DangHoatDong', 2, 4, 1, 5, 1),
(N'Bánh căn 51 Tô Hiến Thành', N'51 Tô Hiến Thành', 12.2405, 109.1930, '0984444442', '15:00', '22:00', 'DangHoatDong', 2, 4, 2, 4, 1),
(N'Bánh căn Út Năm', N'127 Nguyễn Bỉnh Khiêm', 12.2490, 109.1930, '0984444443', '15:00', '22:00', 'DangHoatDong', 2, 4, 3, 5, 1),
(N'Bánh căn Cô Lan', N'107 2/4', 12.2600, 109.1888, '0984444444', '15:00', '22:00', 'DangHoatDong', 2, 4, 4, 4, 1),
(N'Bánh căn Cô Trang', N'Chợ Xóm Mới', 12.2422, 109.1888, '0984444445', '15:00', '22:00', 'DangHoatDong', 2, 4, 5, 5, 1);

-- CHỦ QUÁN 2 (MaTK = 3) | Danh mục 5, 6, 7, 8
INSERT INTO DIADIEM (TenDD, DiaChiChiTiet, ViDo, KinhDo, SDT, GioMoCua, GioDongCua, TrangThai, MaTK, MaDM, MaKV, DiemDanhGiaTB, SoLuotDanhGia) VALUES
-- DM5: Bánh xèo
(N'Bánh xèo chảo Hương', N'87 Hoàng Văn Thụ', 12.2477, 109.1888, '0985555551', '14:00', '22:00', 'DangHoatDong', 3, 5, 1, 5, 1),
(N'Bánh xèo Cô Tám', N'6 Tháp Bà', 12.2655, 109.1899, '0985555552', '14:00', '22:00', 'DangHoatDong', 3, 5, 2, 4, 1),
(N'Bánh xèo mực chợ Đầm', N'Chợ Đầm', 12.2533, 109.1900, '0985555553', '07:00', '18:00', 'DangHoatDong', 3, 5, 3, 5, 1),
(N'Bánh xèo 31', N'31 Ngô Đức Kế', 12.2411, 109.1877, '0985555554', '14:00', '22:00', 'DangHoatDong', 3, 5, 4, 4, 1),
(N'Bánh xèo Tôn Đản', N'11 Tôn Đản', 12.2355, 109.1911, '0985555555', '14:00', '22:00', 'DangHoatDong', 3, 5, 5, 5, 1),
-- DM6: Mì quảng
(N'Mì quảng Đá Chẹt', N'34 Hai Tháng Tư', 12.2599, 109.1888, '0986666661', '06:00', '21:00', 'DangHoatDong', 3, 6, 1, 5, 1),
(N'Mì quảng Vân', N'81 Đinh Tiên Hoàng', 12.2488, 109.1877, '0986666662', '06:00', '21:00', 'DangHoatDong', 3, 6, 2, 4, 1),
(N'Mì quảng Nam Phước', N'04 Nhị Hà', 12.2411, 109.1855, '0986666663', '06:00', '21:00', 'DangHoatDong', 3, 6, 3, 5, 1),
(N'Mì quảng Cô Khen', N'22 Điện Biên Phủ', 12.2688, 109.1911, '0986666664', '06:00', '21:00', 'DangHoatDong', 3, 6, 4, 4, 1),
(N'Mì quảng Chu Choa', N'11 Phan Đình Phùng', 12.2511, 109.1899, '0986666665', '06:00', '21:00', 'DangHoatDong', 3, 6, 5, 5, 1),
-- DM7: Cơm gà
(N'Cơm gà Trâm Anh', N'10 Bà Triệu', 12.2466, 109.1855, '0987777771', '09:00', '22:00', 'DangHoatDong', 3, 7, 1, 5, 1),
(N'Cơm gà Hà', N'75 Ngô Gia Tự', 12.2399, 109.1922, '0987777772', '09:00', '22:00', 'DangHoatDong', 3, 7, 2, 4, 1),
(N'Cơm gà Đăng Dũng', N'30 Phan Bội Châu', 12.2511, 109.1922, '0987777773', '09:00', '22:00', 'DangHoatDong', 3, 7, 3, 5, 1),
(N'Cơm gà Núi Một', N'58 Núi Một', 12.2433, 109.1888, '0987777774', '09:00', '22:00', 'DangHoatDong', 3, 7, 4, 4, 1),
(N'Cơm gà Hữu Kỳ', N'100 Mạc Đĩnh Chi', 12.2400, 109.1888, '0987777775', '09:00', '22:00', 'DangHoatDong', 3, 7, 5, 5, 1),
-- DM8: Cơm tấm
(N'Cơm tấm sườn que', N'21 Lê Quý Đôn', 12.2422, 109.1899, '0988888881', '06:00', '21:00', 'DangHoatDong', 3, 8, 1, 5, 1),
(N'Cơm tấm Minh', N'40 Lý Thánh Tôn', 12.2477, 109.1933, '0988888882', '06:00', '21:00', 'DangHoatDong', 3, 8, 2, 4, 1),
(N'Cơm tấm Trường Lái', N'185 Thái Nguyên', 12.2455, 109.1822, '0988888883', '06:00', '21:00', 'DangHoatDong', 3, 8, 3, 5, 1),
(N'Cơm tấm Chị Thủy', N'15 Nguyễn Trãi', 12.2444, 109.1877, '0988888884', '06:00', '21:00', 'DangHoatDong', 3, 8, 4, 4, 1),
(N'Cơm tấm Bình Dân', N'Ngõ 2, Trần Nhật Duật', 12.2355, 109.1888, '0988888885', '06:00', '21:00', 'DangHoatDong', 3, 8, 5, 5, 1);
GO

-- =========================================================================================
-- 4. BẢNG HÌNH ẢNH (Mỗi địa điểm 1 ảnh mặt tiền để giao diện không bị trống)
-- Sử dụng ảnh mẫu từ Unsplash thông qua URL
-- =========================================================================================
DECLARE @i INT = 1;
WHILE @i <= 40
BEGIN
    INSERT INTO HINHANH (DuongDan, LoaiHinhAnh, ThuTu, MaDD) 
    VALUES ('https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&q=80&w=600', 'MatTien', 1, @i);
    SET @i = @i + 1;
END;
GO

-- =========================================================================================
-- 5. BẢNG ĐÁNH GIÁ (Mỗi địa điểm có 1 đánh giá, lấy ngẫu nhiên 5 Khách hàng ID từ 4->8)
-- =========================================================================================
DECLARE @j INT = 1;
DECLARE @MaKhach INT;
DECLARE @SoSao INT;
WHILE @j <= 40
BEGIN
    -- Công thức luân phiên mã khách từ 4 đến 8
    SET @MaKhach = ((@j % 5) + 4); 
    -- Công thức luân phiên sao: 5 sao, 4 sao
    SET @SoSao = CASE WHEN @j % 2 = 0 THEN 4 ELSE 5 END;

    INSERT INTO DANHGIA (SoSao, NoiDung, TrangThai, MaTK, MaDD) 
    VALUES (@SoSao, N'Quán phục vụ rất tốt, đồ ăn ngon, không gian sạch sẽ! Sẽ quay lại khi đến Nha Trang.', 'HienThi', @MaKhach, @j);
    SET @j = @j + 1;
END;
GO

-- =========================================================================================
-- 6. BẢNG PHẢN HỒI (Chủ quán phản hồi lại một vài đánh giá để test giao diện Thread)
-- Lưu ý: Chủ 1 (ID 2) phản hồi quán 1->20. Chủ 2 (ID 3) phản hồi quán 21->40
-- =========================================================================================
-- Phản hồi của Chủ 1 cho 3 Đánh giá đầu tiên
INSERT INTO PHANHOI (NoiDung, MaDG, MaTK) VALUES
(N'Cảm ơn bạn đã ghé thăm quán! Mong sớm được phục vụ bạn lần nữa.', 1, 2),
(N'Dạ quán ghi nhận góp ý của bạn để cải thiện dịch vụ ạ. Cảm ơn bạn!', 2, 2),
(N'Thật vui vì bạn có trải nghiệm tốt tại cơ sở của chúng tôi.', 3, 2);

-- Phản hồi của Chủ 2 cho 3 Đánh giá của các quán thuộc Chủ 2 (MaDG 21, 22, 23)
INSERT INTO PHANHOI (NoiDung, MaDG, MaTK) VALUES
(N'Cảm ơn đánh giá nhiệt tình của bạn. Quán luôn cố gắng đem lại bữa ăn chất lượng nhất!', 21, 3),
(N'Dạ quán rất vui khi được phục vụ bạn. Chúc bạn một ngày vui vẻ.', 22, 3),
(N'Cảm ơn bạn đã tin tưởng chọn quán làm điểm dùng bữa.', 23, 3);
GO

-- =========================================================================================
-- 7. BẢNG KHUYẾN MÃI (Thêm một số khuyến mãi còn hạn sử dụng đến năm 2027)
-- =========================================================================================
INSERT INTO KHUYENMAI (TenKM, NgayBatDau, NgayKetThuc, NoiDungUuDai, TrangThai, MaDD) VALUES
(N'Giảm 10% cho khách du lịch', '2026-06-01', '2027-12-31', N'Vui lòng đưa thẻ sinh viên hoặc check-in để được giảm 10% tổng hóa đơn.', 'ConHieuLuc', 1),
(N'Tặng nước uống', '2026-06-01', '2027-01-01', N'Tặng ngay 1 ly trà đá hoặc sâm dứa cho mỗi phần ăn.', 'ConHieuLuc', 2),
(N'Combo 99k', '2026-06-01', '2026-12-30', N'Combo 2 phần ăn và 2 ly nước chỉ 99.000 VNĐ.', 'ConHieuLuc', 21),
(N'Đi 4 tính tiền 3', '2026-06-01', '2027-06-01', N'Khuyến mãi đặc biệt đi nhóm 4 người chỉ tính tiền 3 người.', 'ConHieuLuc', 22);
GO

-- =========================================================================================
-- 8. BẢNG YÊU THÍCH (Mô phỏng Khách hàng lưu quán ăn vào danh sách quan tâm)
-- =========================================================================================
INSERT INTO YEUTHICH (MaTK, MaDD) VALUES
(4, 1), (4, 21), (4, 30),
(5, 2), (5, 5), (5, 15),
(6, 11), (6, 31);
GO