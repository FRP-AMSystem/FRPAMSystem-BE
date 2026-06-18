/*
================================================================================
  Forestry Resource Planning DB - Sample Data
  Chạy SAU khi đã execute ForestryResourcePlanningDB.sql (schema)

  Local     : USE [ForestryResourcePlanningDB]
  MonsterASP: đổi thành USE [db54885]

  Tài khoản demo (mật khẩu chung): Password123!
================================================================================
*/

USE [ForestryResourcePlanningDB];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

-- -----------------------------------------------------------------------------
-- Xóa dữ liệu cũ (con → cha)
-- -----------------------------------------------------------------------------
DELETE FROM [dbo].[Schedule];
DELETE FROM [dbo].[EquipmentShortageLog];
DELETE FROM [dbo].[AllocationLandDetail];
DELETE FROM [dbo].[AllocationHumanDetail];
DELETE FROM [dbo].[AllocationEquipmentDetail];
DELETE FROM [dbo].[AllocationPlan];
DELETE FROM [dbo].[PhaseHumanRequirement];
DELETE FROM [dbo].[PhaseEquipmentRequirement];
DELETE FROM [dbo].[ExperimentPhase];
DELETE FROM [dbo].[ExperimentHumanRequirement];
DELETE FROM [dbo].[ExperimentEquipmentRequirement];
DELETE FROM [dbo].[ExperimentLandRequirement];
DELETE FROM [dbo].[Experiment];
DELETE FROM [dbo].[EquipmentSubstitution];
DELETE FROM [dbo].[EquipmentInstance];
DELETE FROM [dbo].[EquipmentType];
DELETE FROM [dbo].[HumanResourceSkill];
DELETE FROM [dbo].[HumanResourceProfile];
DELETE FROM [dbo].[User];
DELETE FROM [dbo].[LandResource];
DELETE FROM [dbo].[Area];
DELETE FROM [dbo].[EquipmentCategory];
DELETE FROM [dbo].[Skill];
DELETE FROM [dbo].[Role];

-- Reset identity
DBCC CHECKIDENT ('[dbo].[Role]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Skill]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Area]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[EquipmentCategory]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[EquipmentType]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[EquipmentInstance]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[EquipmentSubstitution]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[User]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[HumanResourceProfile]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[HumanResourceSkill]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[LandResource]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Experiment]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[ExperimentLandRequirement]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[ExperimentEquipmentRequirement]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[ExperimentHumanRequirement]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[ExperimentPhase]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[PhaseEquipmentRequirement]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[PhaseHumanRequirement]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[AllocationPlan]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[AllocationLandDetail]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[AllocationEquipmentDetail]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[AllocationHumanDetail]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[EquipmentShortageLog]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Schedule]', RESEED, 0);

-- -----------------------------------------------------------------------------
-- 1. Role (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[Role] ([role_name]) VALUES
(N'Admin'),
(N'Manager'),
(N'Researcher'),
(N'Technician'),
(N'Student');

-- -----------------------------------------------------------------------------
-- 2. Skill (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[Skill] ([skill_name], [description]) VALUES
(N'Drone Operation', N'Vận hành drone khảo sát và chụp ảnh vùng thí nghiệm'),
(N'Soil Sampling', N'Lấy mẫu đất và phân tích cơ bản tại hiện trường'),
(N'GIS Mapping', N'Dựng bản đồ và xử lý dữ liệu không gian'),
(N'Field Measurement', N'Đo đạc diện tích, độ ẩm, nhiệt độ tại lô thí nghiệm'),
(N'Data Collection', N'Thu thập và ghi nhận số liệu thí nghiệm');

-- -----------------------------------------------------------------------------
-- 3. Area (5) - Khu vực trại thực nghiệm
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[Area] ([area_name], [description], [created_at]) VALUES
(N'Zone A', N'Khu đất cát phía Đông - phù hợp thí nghiệm độ ẩm', '2026-05-01'),
(N'Zone B', N'Khu đất thịt trung tâm - phù hợp trồng thử giống cây', '2026-05-01'),
(N'Zone C', N'Khu đất sét phía Tây - thí nghiệm thoát nước', '2026-05-01'),
(N'Zone D', N'Khu vườn ươm và thử nghiệm hạt giống', '2026-05-01'),
(N'Zone E', N'Khu bảo tồn mẫu rừng và quan trắc dài hạn', '2026-05-01');

-- -----------------------------------------------------------------------------
-- 4. EquipmentCategory (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[EquipmentCategory] ([category_name], [description]) VALUES
(N'Hand Tools', N'Công cụ thủ công: xẻng, cuốc, găng tay'),
(N'Survey Equipment', N'Thiết bị khảo sát: drone, GPS, cảm biến'),
(N'Irrigation', N'Thiết bị tưới tiêu và bình phun'),
(N'Heavy Machinery', N'Máy móc cơ giới: máy cày, máy phun'),
(N'Safety Gear', N'Trang bị an toàn lao động');

-- -----------------------------------------------------------------------------
-- 5. EquipmentType (5)
-- QuantityBased: Shovel, Gloves | Individual: Drone, Soil Sensor, GPS
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[EquipmentType]
([equipment_category_id], [name], [tracking_type], [base_maintenance_interval_hours],
 [total_quantity], [damaged_quantity], [available_quantity], [reserved_quantity], [in_use_quantity], [missing_quantity],
 [description], [created_at]) VALUES
(1, N'Shovel', N'QuantityBased', NULL, 50, 2, 35, 3, 8, 2, N'Xẻng làm việc tại hiện trường', '2026-05-01'),
(5, N'Work Gloves', N'QuantityBased', NULL, 100, 5, 72, 8, 12, 3, N'Găng tay bảo hộ', '2026-05-01'),
(2, N'Survey Drone', N'Individual', 200, 3, 0, 1, 1, 1, 0, N'Drone khảo sát DJI M300', '2026-05-01'),
(2, N'Soil Moisture Sensor', N'QuantityBased', 500, 15, 1, 10, 2, 2, 0, N'Cảm biến đo độ ẩm đất', '2026-05-01'),
(2, N'GPS Device', N'Individual', 300, 2, 0, 1, 0, 0, 0, N'Thiết bị định vị GPS RTK', '2026-05-01');

-- -----------------------------------------------------------------------------
-- 6. EquipmentInstance (5) - Thiết bị quản lý theo cá thể
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[EquipmentInstance]
([equipment_type_id], [asset_code], [serial_number], [total_usage_hour], [last_maintenance_date],
 [usage_hours_since_last_maintenance], [condition_level], [status], [effective_interval_hour], [maintenance_count], [note], [created_at]) VALUES
(3, N'DRONE-001', N'DJI-M300-2024-A91X', 120, '2026-05-10', 45, N'Good', N'InUse', 160, 3, N'Đang phục vụ thí nghiệm độ ẩm đất', '2026-05-01'),
(3, N'DRONE-002', N'DJI-M300-2025-B22Y', 30, '2026-04-15', 30, N'Good', N'Available', 200, 1, N'Ưu tiên phân bổ cho thí nghiệm mới', '2026-05-01'),
(3, N'DRONE-003', N'DJI-M300-2023-C33Z', 210, '2026-05-01', 80, N'Fair', N'Reserved', 120, 6, N'Gần đến lịch bảo trì', '2026-05-01'),
(5, N'GPS-001', N'TRIMBLE-R12-7788', 85, '2026-03-20', 25, N'Good', N'Available', 240, 2, N'Dùng cho khảo sát bản đồ', '2026-05-01'),
(5, N'GPS-002', N'TRIMBLE-R12-9901', 150, '2026-01-10', 90, N'Fair', N'Maintenance', 180, 4, N'Đang bảo trì định kỳ', '2026-05-01');

-- -----------------------------------------------------------------------------
-- 7. EquipmentSubstitution (5) - Thay thế thiết bị khi thiếu
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[EquipmentSubstitution]
([primary_equipment_type_id], [sub_equipment_type_id], [efficiency_rate], [time_multiplier], [note], [created_at]) VALUES
(3, 5, 0.75, 1.20, N'Khi không có drone, có thể dùng GPS kết hợp đo thủ công (hiệu suất thấp hơn)', '2026-05-01'),
(5, 3, 0.85, 1.10, N'Drone có thể thay một phần nhiệm vụ định vị GPS', '2026-05-01'),
(1, 2, 0.90, 1.00, N'Găng tay hỗ trợ thao tác xẻng an toàn hơn (thay thế phụ trợ)', '2026-05-01'),
(4, 1, 0.80, 1.15, N'Khi thiếu cảm biến, tăng nhân công đo thủ công bằng xẻng/lấy mẫu', '2026-05-01'),
(2, 1, 0.95, 1.00, N'Găng tay thay thế tạm khi hết size phù hợp (chất lượng gần tương đương)', '2026-05-01');

-- -----------------------------------------------------------------------------
-- 8. User (5)
-- role_id: 1 Admin, 2 Manager, 3 Researcher, 4 Technician, 5 Student
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[User]
([full_name], [username], [password_hash], [email], [role_id], [created_at]) VALUES
(N'Le Van Admin', N'admin',  N'$2a$11$eJRZPKWYC4HAgIjOrQuvv.xUkkyhG4iYPAOytFLTIouVD.uLv3qOa', N'admin@frpam.edu.vn', 1, '2026-05-01'),
(N'Tran Thi Manager', N'manager',  N'$2a$11$eJRZPKWYC4HAgIjOrQuvv.xUkkyhG4iYPAOytFLTIouVD.uLv3qOa', N'manager@frpam.edu.vn', 2, '2026-05-01'),
(N'Nguyen Van Researcher', N'researcher01',  N'$2a$11$eJRZPKWYC4HAgIjOrQuvv.xUkkyhG4iYPAOytFLTIouVD.uLv3qOa', N'researcher01@frpam.edu.vn', 3, '2026-05-01'),
(N'Pham Van Technician', N'technician01',  N'$2a$11$eJRZPKWYC4HAgIjOrQuvv.xUkkyhG4iYPAOytFLTIouVD.uLv3qOa', N'technician01@frpam.edu.vn', 4, '2026-05-01'),
(N'Hoang Thi Student', N'student01',  N'$2a$11$eJRZPKWYC4HAgIjOrQuvv.xUkkyhG4iYPAOytFLTIouVD.uLv3qOa', N'student01@frpam.edu.vn', 5, '2026-05-01');

-- -----------------------------------------------------------------------------
-- 9. HumanResourceProfile (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[HumanResourceProfile]
([user_id], [max_working_hours_per_day], [current_workload], [status], [created_at]) VALUES
(2, 8, 2, N'Available', '2026-05-01'),   -- Manager (có thể giám sát)
(3, 8, 4, N'Busy', '2026-05-01'),       -- Researcher
(4, 8, 6, N'Busy', '2026-05-01'),       -- Technician chính
(5, 6, 3, N'Available', '2026-05-01'),  -- Student
(1, 8, 0, N'Unavailable', '2026-05-01'); -- Admin không tham gia phân bổ

-- -----------------------------------------------------------------------------
-- 10. HumanResourceSkill (5)
-- human_resource_id 3 = technician, 4 = student, 2 = researcher, 5 = admin(unavail)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[HumanResourceSkill] ([human_resource_id], [skill_id], [skill_level]) VALUES
(3, 1, N'Advanced'),    -- technician - Drone
(3, 4, N'Advanced'),   -- technician - Field measurement
(4, 5, N'Intermediate'), -- student - Data collection
(4, 2, N'Beginner'),   -- student - Soil sampling
(2, 3, N'Intermediate'); -- researcher - GIS

-- -----------------------------------------------------------------------------
-- 11. LandResource (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[LandResource]
([area_id], [land_code], [area_size], [location], [soil_type], [status], [created_at]) VALUES
(1, N'PLOT-001', 200.00, N'Zone A - Lô 1', N'Sandy Soil', N'InUse', '2026-05-01'),
(1, N'PLOT-002', 180.00, N'Zone A - Lô 2', N'Sandy Soil', N'Available', '2026-05-01'),
(2, N'PLOT-003', 250.00, N'Zone B - Lô 1', N'Loam', N'Reserved', '2026-05-01'),
(3, N'PLOT-004', 150.00, N'Zone C - Lô 1', N'Clay Soil', N'Available', '2026-05-01'),
(4, N'PLOT-005', 120.00, N'Zone D - Vườn ươm', N'Peat Soil', N'Maintenance', '2026-05-01');

-- -----------------------------------------------------------------------------
-- 12. Experiment (5)
-- priority: 1=Cao nhất, 4=Thấp nhất
-- researcher_id = 3 (Nguyen Van Researcher)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[Experiment]
([experiment_name], [description], [researcher_id], [expect_start_date], [expect_end_date], [deadline], [priority], [status], [created_at]) VALUES
(N'Soil Moisture Monitoring Experiment', N'Theo dõi độ ẩm đất tại Zone A bằng drone và cảm biến', 3, '2026-06-01', '2026-06-10', '2026-06-15', 1, N'Approved', '2026-05-20'),
(N'Tree Growth Analysis', N'Phân tích sinh trưởng cây keo tại Zone B', 3, '2026-06-15', '2026-07-15', '2026-07-20', 2, N'Pending', '2026-05-22'),
(N'Irrigation Efficiency Test', N'Đánh giá hiệu quả tưới tiêu tại Zone C', 3, '2026-05-25', '2026-06-05', '2026-06-10', 2, N'InProgress', '2026-05-18'),
(N'Drone Mapping Survey', N'Khảo sát bản đồ 3D khu vực Zone E', 3, '2026-07-01', '2026-07-10', '2026-07-15', 3, N'Draft', '2026-05-25'),
(N'Seed Germination Study', N'Nghiên cứu nảy mầm hạt giống tại vườn ươm', 3, '2026-04-01', '2026-04-30', '2026-05-05', 4, N'Completed', '2026-03-28');

-- -----------------------------------------------------------------------------
-- 13. ExperimentLandRequirement (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[ExperimentLandRequirement]
([experiment_id], [required_area], [required_soil_type], [note], [created_at]) VALUES
(1, 200.00, N'Sandy Soil', N'Lô đất cố định suốt thí nghiệm', '2026-05-20'),
(2, 250.00, N'Loam', N'Cần diện tích lớn cho nhiều lô trồng', '2026-05-22'),
(3, 150.00, N'Clay Soil', N'Khu thử nghiệm tưới', '2026-05-18'),
(4, 100.00, N'Loam', N'Khu khảo sát drone', '2026-05-25'),
(5, 120.00, N'Peat Soil', N'Vườn ươm hạt giống', '2026-03-28');

-- -----------------------------------------------------------------------------
-- 14. ExperimentEquipmentRequirement (5)
-- equipment_type: 1 Shovel, 2 Gloves, 3 Drone, 4 Sensor, 5 GPS
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[ExperimentEquipmentRequirement]
([experiment_id], [equipment_type_id], [quantity], [allow_substitute], [min_acceptable_efficiency], [note], [created_at]) VALUES
(1, 3, 1, 1, 0.75, N'Cần 1 drone khảo sát', '2026-05-20'),
(1, 1, 5, 0, NULL, N'5 xẻng cho lấy mẫu thủ công', '2026-05-20'),
(1, 4, 3, 1, 0.80, N'3 cảm biến độ ẩm', '2026-05-20'),
(2, 1, 10, 0, NULL, N'10 xẻng cho thí nghiệm sinh trưởng', '2026-05-22'),
(3, 4, 5, 1, 0.85, N'5 cảm biến theo dõi tưới', '2026-05-18');

-- -----------------------------------------------------------------------------
-- 15. ExperimentHumanRequirement (5)
-- role_id: 4 Technician, 5 Student
-- skill_id: 1 Drone, 5 Data collection
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[ExperimentHumanRequirement]
([experiment_id], [role_id], [quantity], [required_skill_id], [working_hours_per_day], [note], [created_at]) VALUES
(1, 4, 1, 1, 8, N'Kỹ thuật viên vận hành drone', '2026-05-20'),
(1, 5, 2, 5, 6, N'2 sinh viên hỗ trợ thu thập dữ liệu', '2026-05-20'),
(2, 4, 2, 4, 8, N'2 kỹ thuật viên đo đạc hiện trường', '2026-05-22'),
(3, 5, 3, 2, 6, N'3 sinh viên lấy mẫu đất', '2026-05-18'),
(4, 4, 1, 3, 8, N'Kỹ thuật viên GIS/drone mapping', '2026-05-25');

-- -----------------------------------------------------------------------------
-- 16. ExperimentPhase (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[ExperimentPhase]
([experiment_id], [phase_name], [phase_description], [phase_order], [expected_start_date], [expected_end_date], [status], [created_at]) VALUES
(1, N'Site Preparation', N'Chuẩn bị lô đất và lắp cảm biến', 1, '2026-06-01', '2026-06-03', N'Completed', '2026-05-20'),
(1, N'Field Monitoring', N'Bay drone và thu thập dữ liệu độ ẩm', 2, '2026-06-04', '2026-06-10', N'InProgress', '2026-05-20'),
(3, N'Irrigation Setup', N'Lắp đặt hệ thống tưới thử nghiệm', 1, '2026-05-25', '2026-05-28', N'Completed', '2026-05-18'),
(3, N'Data Collection', N'Đo lường hiệu quả tưới', 2, '2026-05-29', '2026-06-05', N'InProgress', '2026-05-18'),
(2, N'Planting Phase', N'Gieo trồng cây keo thử nghiệm', 1, '2026-06-15', '2026-06-25', N'Planned', '2026-05-22');

-- -----------------------------------------------------------------------------
-- 17. PhaseEquipmentRequirement (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[PhaseEquipmentRequirement]
([phase_id], [equipment_type_id], [quantity], [note], [created_at]) VALUES
(1, 1, 3, N'Xẻng chuẩn bị lô thí nghiệm 1', '2026-05-20'),
(2, 3, 1, N'Drone cho giai đoạn monitoring', '2026-05-20'),
(2, 4, 3, N'Cảm biến giai đoạn monitoring', '2026-05-20'),
(3, 4, 2, N'Cảm biến lắp hệ thống tưới', '2026-05-18'),
(4, 1, 5, N'Xẻng hỗ trợ đo đạc tưới', '2026-05-18');

-- -----------------------------------------------------------------------------
-- 18. PhaseHumanRequirement (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[PhaseHumanRequirement]
([phase_id], [role_id], [quantity], [required_skill_id], [note], [created_at]) VALUES
(1, 5, 2, 2, N'Sinh viên chuẩn bị lô', '2026-05-20'),
(2, 4, 1, 1, N'Kỹ thuật viên bay drone', '2026-05-20'),
(2, 5, 1, 5, N'Sinh viên ghi dữ liệu', '2026-05-20'),
(3, 4, 1, 4, N'Kỹ thuật viên lắp cảm biến', '2026-05-18'),
(4, 5, 2, 5, N'Sinh viên thu thập số liệu tưới', '2026-05-18');

-- -----------------------------------------------------------------------------
-- 19. AllocationPlan (5)
-- created_by=2 Manager, approve_by=2 Manager
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[AllocationPlan]
([experiment_id], [fitness_score], [created_by], [approve_by], [approve_status], [approved_at], [created_at]) VALUES
(1, 92.5, 2, 2, N'Approved', '2026-05-25', '2026-05-24'),
(3, 78.0, 2, 2, N'Approved', '2026-05-20', '2026-05-19'),
(2, 65.0, 2, NULL, N'Pending', NULL, '2026-05-23'),
(4, 55.0, 2, NULL, N'Pending', NULL, '2026-05-26'),
(5, 88.0, 2, 2, N'Approved', '2026-04-01', '2026-03-30');

-- -----------------------------------------------------------------------------
-- 20. AllocationLandDetail (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[AllocationLandDetail]
([allocation_plan_id], [land_id], [exp_land_req_id], [start_date], [end_date], [status], [created_at]) VALUES
(1, 1, 1, '2026-06-01', '2026-06-10', N'Active', '2026-05-25'),      -- Exp1 -> PLOT-001
(2, 4, 3, '2026-05-25', '2026-06-05', N'Active', '2026-05-20'),      -- Exp3 -> PLOT-004
(3, 3, 2, '2026-06-15', '2026-07-15', N'Planned', '2026-05-23'),     -- Exp2 -> PLOT-003
(4, 2, 4, '2026-07-01', '2026-07-10', N'Planned', '2026-05-26'),     -- Exp4 -> PLOT-002
(5, 5, 5, '2026-04-01', '2026-04-30', N'Completed', '2026-04-01');   -- Exp5 -> PLOT-005

-- -----------------------------------------------------------------------------
-- 21. AllocationEquipmentDetail (5)
-- exp_equipment_req_id XOR phase_equipment_req_id
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[AllocationEquipmentDetail]
([allocation_plan_id], [exp_equipment_req_id], [phase_equipment_req_id], [allocated_equipment_type_id],
 [equipment_instance_id], [quantity], [is_substitute], [efficiency_rate], [start_date], [end_date], [status], [created_at]) VALUES
(1, NULL, 2, 3, 1, 1, 0, 1.0, '2026-06-04', '2026-06-10', N'Active', '2026-05-25'),       -- DRONE-001
(1, NULL, 1, 1, NULL, 3, 0, 1.0, '2026-06-01', '2026-06-03', N'Completed', '2026-05-25'), -- 3 xẻng
(1, 3, NULL, 4, NULL, 3, 0, 1.0, '2026-06-01', '2026-06-10', N'Active', '2026-05-25'),    -- 3 cảm biến
(2, 5, NULL, 4, NULL, 5, 0, 1.0, '2026-05-25', '2026-06-05', N'Active', '2026-05-20'),    -- Exp3 sensors
(3, 4, NULL, 1, NULL, 10, 0, 1.0, '2026-06-15', '2026-07-15', N'Planned', '2026-05-23');  -- Exp2 shovels

-- -----------------------------------------------------------------------------
-- 22. AllocationHumanDetail (5)
-- human_resource_id: 3=technician, 4=student
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[AllocationHumanDetail]
([allocation_plan_id], [exp_human_req_id], [phase_human_req_id], [human_resource_id],
 [working_hours], [start_date], [end_date], [status], [created_at]) VALUES
(1, 1, NULL, 3, 16, '2026-06-01', '2026-06-10', N'Active', '2026-05-25'),    -- technician drone
(1, 2, NULL, 4, 12, '2026-06-01', '2026-06-10', N'Active', '2026-05-25'),    -- student data
(1, NULL, 2, 3, 8, '2026-06-04', '2026-06-10', N'Active', '2026-05-25'),     -- phase drone op
(2, NULL, 4, 4, 12, '2026-05-29', '2026-06-05', N'Active', '2026-05-20'),    -- irrigation data
(2, NULL, 3, 3, 8, '2026-05-25', '2026-05-28', N'Completed', '2026-05-20');  -- irrigation setup

-- -----------------------------------------------------------------------------
-- 23. EquipmentShortageLog (5)
-- Ghi nhận thiếu thiết bị khi phân bổ (phục vụ demo conflict / AI)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[EquipmentShortageLog]
([allocation_plan_id], [exp_equipment_req_id], [phase_equipment_req_id], [shortage_quantity], [created_at]) VALUES
(3, 4, NULL, 2, '2026-05-23'),   -- Exp2: thiếu 2/10 xẻng
(1, 2, NULL, 2, '2026-05-24'),   -- Exp1: thiếu 2/5 xẻng
(2, 5, NULL, 1, '2026-05-19'),   -- Exp3: thiếu 1/5 cảm biến
(2, NULL, 5, 2, '2026-05-20'),   -- Exp3 phase Data Collection: thiếu 2/5 xẻng
(1, NULL, 3, 1, '2026-05-25');   -- Exp1 phase Monitoring: thiếu 1/3 cảm biến

-- -----------------------------------------------------------------------------
-- 24. Schedule (5)
-- assigned_human_resource_id: 3=technician, 4=student
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[Schedule]
([allocation_plan_id], [phase_id], [title], [description], [start_date], [end_date],
 [status], [created_by], [assigned_human_resource_id], [notes], [priority], [created_at]) VALUES
(1, 1, N'Chuẩn bị lô PLOT-001', N'Lắp cảm biến và đánh dấu vùng thí nghiệm', '2026-06-01', '2026-06-03', N'Completed', 2, 4, N'Hoàn thành đúng tiến độ', 1, '2026-05-25'),
(1, 2, N'Bay drone khảo sát', N'Kỹ thuật viên bay drone thu ảnh và dữ liệu độ ẩm', '2026-06-04', '2026-06-10', N'InProgress', 2, 3, N'DRONE-001 đang sử dụng', 1, '2026-05-25'),
(2, 3, N'Lắp hệ thống tưới', N'Lắp cảm biến và đường ống tưới thử nghiệm', '2026-05-25', '2026-05-28', N'Completed', 2, 3, NULL, 2, '2026-05-20'),
(2, 4, N'Thu thập số liệu tưới', N'Sinh viên ghi nhận lưu lượng và độ ẩm', '2026-05-29', '2026-06-05', N'InProgress', 2, 4, NULL, 2, '2026-05-20'),
(3, 5, N'Gieo trồng cây keo', N'Chuẩn bị gieo trồng thí nghiệm sinh trưởng', '2026-06-15', '2026-06-25', N'Planned', 2, 3, N'Chờ duyệt kế hoạch phân bổ', 2, '2026-05-23');

COMMIT TRANSACTION;
GO

PRINT N'Seed data đã được nạp thành công.';
PRINT N'Tài khoản demo: admin / manager / researcher01 / technician01 / student01';
PRINT N'Mật khẩu chung: Password123!';
GO
