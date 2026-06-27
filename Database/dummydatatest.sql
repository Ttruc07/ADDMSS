USE UAV_Management;
GO

-- =========================================
-- USERS
-- =========================================
INSERT INTO Users
(FullName, Username, Password, Email, Phone, Role, Status)
VALUES
(N'Nguyễn Văn Admin', 'admin', '123456', '[admin@uav.com](mailto:admin@uav.com)', '0901111111', 0, 1),
(N'Trần Minh Operator', 'operator1', '123456', '[operator1@uav.com](mailto:operator1@uav.com)', '0902222222', 1, 1),
(N'Lê Hoàng Nam', 'operator2', '123456', '[operator2@uav.com](mailto:operator2@uav.com)', '0903333333', 1, 1),
(N'Phạm Thu Hà', 'operator03', '123456', '[ha@uav.com](mailto:ha@uav.com)', '0901000004', 1, 1),
(N'Nguyễn Quốc Bảo', 'operator04', '123456', '[bao@uav.com](mailto:bao@uav.com)', '0901000005', 1, 1);
GO

-- =========================================
-- LOCATIONS
-- =========================================
INSERT INTO Locations
(LocationName, Latitude, Longitude, Address, Type)
VALUES
(N'Trạm cứu hộ TP.HCM', 10.77688900, 106.70080600, N'Quận 1, TP.HCM', 1),
(N'Bệnh viện Chợ Rẫy', 10.75641000, 106.66017000, N'201B Nguyễn Chí Thanh, Q5', 2),
(N'Bệnh viện Nhi Đồng 1', 10.76265000, 106.67350000, N'341 Sư Vạn Hạnh, Q10', 2),
(N'Khu vực cứu trợ Bình Chánh', 10.68050000, 106.59020000, N'Bình Chánh, TP.HCM', 2),
(N'Khu vực cứu trợ Củ Chi', 11.00050000, 106.51030000, N'Củ Chi, TP.HCM', 2);
GO

-- =========================================
-- SUPPLIES
-- =========================================
INSERT INTO Supplies
(SupplyName, Description, Weight, Quantity, Category, Status)
VALUES
(N'Thuốc kháng sinh', N'Thuốc điều trị nhiễm khuẩn', 0.50, 200, N'Y tế', 1),
(N'Túi máu O+', N'Máu phục vụ cấp cứu', 0.45, 100, N'Y tế', 1),
(N'Bộ sơ cứu', N'Dụng cụ sơ cứu cơ bản', 1.20, 80, N'Y tế', 1),
(N'Nước uống đóng chai', N'Nước tinh khiết 500ml', 0.50, 500, N'Nhu yếu phẩm', 1),
(N'Lương khô', N'Lương thực khẩn cấp', 0.80, 300, N'Nhu yếu phẩm', 1);
GO

-- =========================================
-- UAVS
-- =========================================
INSERT INTO UAVs
(UAV_Name, Model, MaxWeight, BatteryLevel, Status, CurrentLocationID)
VALUES
(N'UAV Rescue 01', 'DJI FlyCart 30', 30, 95, N'Available', 1),
(N'UAV Rescue 02', 'DJI FlyCart 30', 40, 80, N'Available', 1),
(N'UAV Medical 01', 'DJI M350 RTK', 15, 75, N'In Mission', 1);
GO

-- =========================================
-- MISSIONS
-- =========================================
INSERT INTO Missions
(UAV_ID, CreatedBy, StartLocationID, EndLocationID,
MissionName, StartTime, EndTime, Status, TotalWeight)
VALUES
(1, 2, 1, 2, N'Vận chuyển thuốc đến Chợ Rẫy',
'2026-06-01 08:00:00', '2026-06-01 09:00:00', 3, 5.00),

(2, 2, 1, 3, N'Giao máu cấp cứu',
'2026-06-03 10:00:00', '2026-06-03 11:00:00', 3, 3.00),

(3, 3, 1, 4, N'Cứu trợ Bình Chánh',
'2026-06-05 14:00:00', NULL, 2, 10.00),

(1, 4, 1, 5, N'Giao thuốc đến Củ Chi',
'2026-06-06 08:00:00', '2026-06-06 09:30:00', 3, 4.50),

(2, 5, 1, 4, N'Cứu trợ Bình Chánh đợt 2',
'2026-06-06 13:00:00', '2026-06-06 15:00:00', 3, 8.00),

(1, 2, 1, 2, N'Vận chuyển thuốc khẩn cấp',
'2026-06-07 07:30:00', NULL, 2, 2.50),

(3, 3, 1, 3, N'Giao máu cấp cứu lần 2',
NULL, NULL, 1, 1.80),

(2, 4, 1, 5, N'Hỗ trợ cứu trợ Củ Chi',
NULL, NULL, 1, 6.20);
GO

-- =========================================
-- MISSION DETAILS
-- =========================================
INSERT INTO MissionDetails
(MissionID, SupplyID, Quantity, Weight, Note)
VALUES
(1,1,10,0.50,N'Thuốc khẩn cấp'),
(2,2,5,0.45,N'Giao máu cho bệnh viện'),
(2,3,1,1.20,N'Kèm bộ sơ cứu'),
(3,4,10,0.50,N'Nước uống'),
(3,5,5,0.80,N'Lương thực'),

(4,1,5,0.50,N'Thuốc kháng sinh'),
(4,4,4,0.50,N'Nước uống'),

(5,4,10,0.50,N'Nước uống'),
(5,5,3,0.80,N'Lương khô'),

(6,1,3,0.50,N'Thuốc khẩn cấp'),
(6,3,1,1.20,N'Bộ sơ cứu'),

(7,2,4,0.45,N'Máu cấp cứu'),

(8,4,8,0.50,N'Nước uống'),
(8,5,2,0.80,N'Lương khô');
GO
