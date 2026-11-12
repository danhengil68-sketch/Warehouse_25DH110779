-- Kiem tra giua ky Lap trinh Web - Ca 3
-- Sinh vien: Nguyen Hong Gia Huy
-- MSSV: 25DH110779
-- Tao database, bang Products va 6 san pham mau tren SQL Server.
-- Neu bang Products da ton tai, giu nguyen du lieu hien co.

USE [master];
GO

IF DB_ID(N'Warehouse_25DH110779') IS NULL
BEGIN
    CREATE DATABASE [Warehouse_25DH110779];
END;
GO

USE [Warehouse_25DH110779];
GO

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        ProductID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Products PRIMARY KEY,
        ProductName NVARCHAR(100) NOT NULL,
        Category NVARCHAR(100) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        Quantity INT NOT NULL
    );

    INSERT INTO dbo.Products
        (ProductName, Category, Price, Quantity)
    VALUES
    (N'Bút bi - Nguyễn Hồng Gia Huy',
     N'Văn phòng phẩm - Nguyễn Hồng Gia Huy', 5000, 0),
    (N'Vở - Nguyễn Hồng Gia Huy',
     N'Văn phòng phẩm - Nguyễn Hồng Gia Huy', 12000, 3),
    (N'Bút chì - Nguyễn Hồng Gia Huy',
     N'Văn phòng phẩm - Nguyễn Hồng Gia Huy', 6000, 5),
    (N'USB 32GB - Nguyễn Hồng Gia Huy',
     N'Thiết bị học tập - Nguyễn Hồng Gia Huy', 90000, 10),
    (N'USB 64GB - Nguyễn Hồng Gia Huy',
     N'Thiết bị học tập - Nguyễn Hồng Gia Huy', 150000, 4),
    (N'Chuột máy tính - Nguyễn Hồng Gia Huy',
     N'Thiết bị học tập - Nguyễn Hồng Gia Huy', 75000, 2);
END;
GO

SELECT ProductID, ProductName, Category, Price, Quantity
FROM dbo.Products
ORDER BY ProductID;
GO

