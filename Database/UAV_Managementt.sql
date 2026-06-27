-- =========================================
-- CREATE DATABASE
-- =========================================
CREATE DATABASE UAV_Management;
GO

USE UAV_Management;
GO

-- =========================================
-- TABLE: Users
-- =========================================
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),

    FullName NVARCHAR(100) NOT NULL,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    Email VARCHAR(100) UNIQUE,
    Phone VARCHAR(15),

    -- Dùng TINYINT cho Enum trong C#: 0 = Admin, 1 = Operator
    Role TINYINT NOT NULL DEFAULT 1, 
    
    -- Dùng TINYINT cho Enum: 0 = Khóa, 1 = Hoạt động
    Status TINYINT DEFAULT 1,

    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- =========================================
-- TABLE: Locations
-- =========================================
CREATE TABLE Locations (
    LocationID INT PRIMARY KEY IDENTITY(1,1),

    LocationName NVARCHAR(100) NOT NULL,
    
    -- STREAMING_CHUNK:Cập nhật độ chính xác tọa độ GPS lên mức milimet...
    -- DECIMAL(11,8) cho độ chính xác GPS đến ~1.1mm
    Latitude DECIMAL(11,8),
    Longitude DECIMAL(11,8),
    Address NVARCHAR(255),

    -- Enum: 1 = Trạm cứu hộ gốc (Base), 2 = Điểm tập kết nhận hàng
    Type TINYINT NOT NULL DEFAULT 2 
);
GO

-- =========================================
-- TABLE: Supplies
-- =========================================
CREATE TABLE Supplies (
    SupplyID INT PRIMARY KEY IDENTITY(1,1),

    SupplyName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),

    -- DECIMAL(10,2) cho tính toán khối lượng chính xác (đơn vị: kg)
    Weight DECIMAL(10,2) CHECK (Weight >= 0),
    Quantity INT CHECK (Quantity >= 0),

    Category NVARCHAR(50),
    
    -- Enum: 0 = Hết hàng, 1 = Còn hàng
    Status TINYINT DEFAULT 1
);
GO

-- =========================================
-- TABLE: UAVs
-- =========================================
CREATE TABLE UAVs (
    UAV_ID INT PRIMARY KEY IDENTITY(1,1),

    UAV_Name NVARCHAR(100) NOT NULL,
    Model NVARCHAR(100),

    MaxWeight FLOAT CHECK (MaxWeight > 0),
    BatteryLevel FLOAT CHECK (BatteryLevel BETWEEN 0 AND 100),

    Status NVARCHAR(50),

    CurrentLocationID INT,

    CreatedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_UAV_Location
        FOREIGN KEY (CurrentLocationID)
        REFERENCES Locations(LocationID)
);
GO

-- =========================================
-- TABLE: Missions
-- =========================================
CREATE TABLE Missions (
    MissionID INT PRIMARY KEY IDENTITY(1,1),

    UAV_ID INT NOT NULL,
    CreatedBy INT NOT NULL,

    StartLocationID INT NOT NULL,
    EndLocationID INT NOT NULL,

    MissionName NVARCHAR(100) NOT NULL,

    StartTime DATETIME,
    EndTime DATETIME,

    -- Enum: 0 = Đã hủy, 1 = Chờ cất cánh, 2 = Đang bay, 3 = Hoàn thành
    Status TINYINT DEFAULT 1,

    TotalWeight DECIMAL(10,2) CHECK (TotalWeight >= 0),

    -- FOREIGN KEYS
    CONSTRAINT FK_Mission_UAV
        FOREIGN KEY (UAV_ID)
        REFERENCES UAVs(UAV_ID),

    CONSTRAINT FK_Mission_User
        FOREIGN KEY (CreatedBy)
        REFERENCES Users(UserID),

    CONSTRAINT FK_Mission_StartLocation
        FOREIGN KEY (StartLocationID)
        REFERENCES Locations(LocationID),

    CONSTRAINT FK_Mission_EndLocation
        FOREIGN KEY (EndLocationID)
        REFERENCES Locations(LocationID)
);
GO

-- =========================================
-- TABLE: MissionDetails
-- =========================================
CREATE TABLE MissionDetails (
    MissionDetailID INT PRIMARY KEY IDENTITY(1,1),

    MissionID INT NOT NULL,
    SupplyID INT NOT NULL,

    Quantity INT CHECK (Quantity > 0),
    
    -- Lưu lại Weight của vật tư tại thời điểm đó (đề phòng bảng Supplies đổi giá trị)
    Weight DECIMAL(10,2) CHECK (Weight >= 0),

    Note NVARCHAR(255),

    -- FOREIGN KEYS
    CONSTRAINT FK_MissionDetails_Mission
        FOREIGN KEY (MissionID)
        REFERENCES Missions(MissionID),

    CONSTRAINT FK_MissionDetails_Supply
        FOREIGN KEY (SupplyID)
        REFERENCES Supplies(SupplyID)
);
GO

