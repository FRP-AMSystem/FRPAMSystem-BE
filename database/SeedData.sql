/*
================================================================================
  Forestry Resource Planning DB - Sample Data
  Run AFTER ForestryResourcePlanningDB.sql (schema)

  Local     : USE [ForestryResourcePlanningDB]
  MonsterASP: change to USE [db54885]

  Demo accounts (shared password): Password123!
================================================================================
*/
--Ctrl+A r?i run to�n b? file lu�n--
USE [ForestryResourcePlanningDB];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- -----------------------------------------------------------------------------
-- Phase 1: Clear existing data (child -> parent)
-- -----------------------------------------------------------------------------
DELETE FROM [dbo].[Notification];
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
GO

-- -----------------------------------------------------------------------------
-- Phase 2: Reset identity (run outside transaction for reliable reseed)
-- -----------------------------------------------------------------------------
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
DBCC CHECKIDENT ('[dbo].[Notification]', RESEED, 0);
GO

-- -----------------------------------------------------------------------------
-- Phase 3: Insert seed data (explicit IDs for all FK-referenced parent rows)
-- -----------------------------------------------------------------------------
SET XACT_ABORT ON;

BEGIN TRY
BEGIN TRANSACTION;

-- BCrypt hash of "Password123!"
-- -----------------------------------------------------------------------------
-- 1. Role (5) - IDs 1-5
-- -----------------------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Role] ON;

INSERT INTO [dbo].[Role] ([role_id], [role_name]) VALUES
(1, N'Admin'),
(2, N'Manager'),
(3, N'Researcher'),
(4, N'Technician'),
(5, N'Student');

SET IDENTITY_INSERT [dbo].[Role] OFF;

-- -----------------------------------------------------------------------------
-- 2. Skill (5) - IDs 1-5
-- -----------------------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Skill] ON;

INSERT INTO [dbo].[Skill] ([skill_id], [skill_name], [description]) VALUES
(1, N'Drone Operation', N'Operate survey drones and capture aerial imagery over experiment plots'),
(2, N'Soil Sampling', N'Collect soil samples and perform basic field analysis'),
(3, N'GIS Mapping', N'Build maps and process spatial data'),
(4, N'Field Measurement', N'Measure area, moisture, and temperature in the field'),
(5, N'Data Collection', N'Collect and record experiment measurements');

SET IDENTITY_INSERT [dbo].[Skill] OFF;

-- -----------------------------------------------------------------------------
-- 3. Area (5) - IDs 1-5
-- -----------------------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Area] ON;

INSERT INTO [dbo].[Area] ([area_id], [area_name], [description], [created_at]) VALUES
(1, N'Zone A', N'Eastern sandy soil zone suitable for moisture experiments', '2026-05-01'),
(2, N'Zone B', N'Central loam zone suitable for planting trials', '2026-05-01'),
(3, N'Zone C', N'Western clay zone for drainage experiments', '2026-05-01'),
(4, N'Zone D', N'Nursery and seed germination trial zone', '2026-05-01'),
(5, N'Zone E', N'Forest sample conservation and long-term monitoring zone', '2026-05-01');

SET IDENTITY_INSERT [dbo].[Area] OFF;

-- -----------------------------------------------------------------------------
-- 4. EquipmentCategory (5) - explicit IDs for FK references in EquipmentType
-- -----------------------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[EquipmentCategory] ON;

INSERT INTO [dbo].[EquipmentCategory] ([equipment_category_id], [category_name], [description], [created_at]) VALUES
(1, N'Hand Tools', N'Manual tools: shovels, hoes, gloves', '2026-05-01'),
(2, N'Survey Equipment', N'Survey devices: drones, GPS, sensors', '2026-05-01'),
(3, N'Irrigation', N'Irrigation and spraying equipment', '2026-05-01'),
(4, N'Heavy Machinery', N'Powered machinery: tractors, sprayers', '2026-05-01'),
(5, N'Safety Gear', N'Work safety equipment', '2026-05-01');

SET IDENTITY_INSERT [dbo].[EquipmentCategory] OFF;

-- -----------------------------------------------------------------------------
-- 5. EquipmentType (5) - IDs 1-5
-- 1 Shovel | 2 Gloves | 3 Drone | 4 Sensor | 5 GPS
-- -----------------------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[EquipmentType] ON;

INSERT INTO [dbo].[EquipmentType]
([equipment_type_id], [equipment_category_id], [name], [tracking_type], [base_maintenance_interval_hours],
 [total_quantity], [damaged_quantity], [available_quantity], [reserved_quantity], [in_use_quantity], [missing_quantity],
 [description], [created_at]) VALUES
(1, 1, N'Shovel', N'QuantityBased', NULL, 50, 2, 35, 3, 8, 2, N'Field work shovels', '2026-05-01'),
(2, 5, N'Work Gloves', N'QuantityBased', NULL, 100, 5, 72, 8, 12, 3, N'Protective work gloves', '2026-05-01'),
(3, 2, N'Survey Drone', N'Individual', 200, 3, 0, 1, 1, 1, 0, N'DJI M300 survey drone', '2026-05-01'),
(4, 2, N'Soil Moisture Sensor', N'QuantityBased', 500, 15, 1, 10, 2, 2, 0, N'Soil moisture sensors', '2026-05-01'),
(5, 2, N'GPS Device', N'Individual', 300, 2, 0, 1, 0, 0, 0, N'RTK GPS positioning device', '2026-05-01');

SET IDENTITY_INSERT [dbo].[EquipmentType] OFF;

-- -----------------------------------------------------------------------------
-- 6. EquipmentInstance (5) - IDs 1-5
-- -----------------------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[EquipmentInstance] ON;

INSERT INTO [dbo].[EquipmentInstance]
([equipment_instance_id], [equipment_type_id], [asset_code], [serial_number], [total_usage_hour], [last_maintenance_date],
 [usage_hours_since_last_maintenance], [condition_level], [status], [effective_interval_hour], [maintenance_count], [note], [created_at]) VALUES
(1, 3, N'DRONE-001', N'DJI-M300-2024-A91X', 120, '2026-05-10', 45, N'Good', N'InUse', 160, 3, N'Assigned to soil moisture experiment', '2026-05-01'),
(2, 3, N'DRONE-002', N'DJI-M300-2025-B22Y', 30, '2026-04-15', 30, N'Good', N'Available', 200, 1, N'Preferred for new experiment allocations', '2026-05-01'),
(3, 3, N'DRONE-003', N'DJI-M300-2023-C33Z', 210, '2026-05-01', 80, N'Fair', N'Reserved', 120, 6, N'Approaching scheduled maintenance', '2026-05-01'),
(4, 5, N'GPS-001', N'TRIMBLE-R12-7788', 85, '2026-03-20', 25, N'Good', N'Available', 240, 2, N'Used for mapping surveys', '2026-05-01'),
(5, 5, N'GPS-002', N'TRIMBLE-R12-9901', 150, '2026-01-10', 90, N'Fair', N'Maintenance', 180, 4, N'Under routine maintenance', '2026-05-01');

SET IDENTITY_INSERT [dbo].[EquipmentInstance] OFF;

-- -----------------------------------------------------------------------------
-- 7. EquipmentSubstitution (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[EquipmentSubstitution]
([primary_equipment_type_id], [sub_equipment_type_id], [efficiency_rate], [time_multiplier], [note], [created_at]) VALUES
(3, 5, 0.75, 1.20, N'When no drone is available, GPS plus manual survey may be used with lower efficiency', '2026-05-01'),
(5, 3, 0.85, 1.10, N'Drone may partially replace GPS positioning tasks', '2026-05-01'),
(1, 2, 0.90, 1.00, N'Gloves support safer shovel operations as auxiliary substitute', '2026-05-01'),
(4, 1, 0.80, 1.15, N'When sensors are short, increase manual sampling with shovels', '2026-05-01'),
(2, 1, 0.95, 1.00, N'Temporary glove substitute when preferred sizes are unavailable', '2026-05-01');

-- -----------------------------------------------------------------------------
-- 8. User (5) - IDs 1-5
-- 1 admin | 2 manager | 3 researcher01 | 4 technician01 | 5 student01
-- -----------------------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[User] ON;

INSERT INTO [dbo].[User]
([user_id], [full_name], [username], [password_hash], [email], [role_id], [created_at]) VALUES
(1, N'Le Van Admin', N'admin', N'$2a$11$eJRZPKWYC4HAgIjOrQuvv.xUkkyhG4iYPAOytFLTIouVD.uLv3qOa', N'admin@frpam.edu.vn', 1, '2026-05-01'),
(2, N'Tran Thi Manager', N'manager', N'$2a$11$eJRZPKWYC4HAgIjOrQuvv.xUkkyhG4iYPAOytFLTIouVD.uLv3qOa', N'manager@frpam.edu.vn', 2, '2026-05-01'),
(3, N'Nguyen Van Researcher', N'researcher01', N'$2a$11$eJRZPKWYC4HAgIjOrQuvv.xUkkyhG4iYPAOytFLTIouVD.uLv3qOa', N'researcher01@frpam.edu.vn', 3, '2026-05-01'),
(4, N'Pham Van Technician', N'technician01', N'$2a$11$eJRZPKWYC4HAgIjOrQuvv.xUkkyhG4iYPAOytFLTIouVD.uLv3qOa', N'technician01@frpam.edu.vn', 4, '2026-05-01'),
(5, N'Hoang Thi Student', N'student01', N'$2a$11$eJRZPKWYC4HAgIjOrQuvv.xUkkyhG4iYPAOytFLTIouVD.uLv3qOa', N'student01@frpam.edu.vn', 5, '2026-05-01');

SET IDENTITY_INSERT [dbo].[User] OFF;

-- -----------------------------------------------------------------------------
-- 9. HumanResourceProfile (5) - IDs 1-5
-- hr 1 manager | 2 researcher | 3 technician | 4 student | 5 admin
-- -----------------------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[HumanResourceProfile] ON;

INSERT INTO [dbo].[HumanResourceProfile]
([human_resource_id], [user_id], [max_working_hours_per_day], [current_workload], [status], [created_at]) VALUES
(1, 2, 8, 2, N'Available', '2026-05-01'),
(2, 3, 8, 4, N'Busy', '2026-05-01'),
(3, 4, 8, 6, N'Busy', '2026-05-01'),
(4, 5, 6, 3, N'Available', '2026-05-01'),
(5, 1, 8, 0, N'Unavailable', '2026-05-01');

SET IDENTITY_INSERT [dbo].[HumanResourceProfile] OFF;

-- -----------------------------------------------------------------------------
-- 10. HumanResourceSkill (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[HumanResourceSkill] ([human_resource_id], [skill_id], [skill_level]) VALUES
(3, 1, N'Advanced'),
(3, 4, N'Advanced'),
(4, 5, N'Intermediate'),
(4, 2, N'Beginner'),
(2, 3, N'Intermediate');

-- -----------------------------------------------------------------------------
-- 11. LandResource (5) - IDs 1-5
-- -----------------------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[LandResource] ON;

INSERT INTO [dbo].[LandResource]
([land_id], [area_id], [land_code], [area_size], [location], [soil_type], [status], [created_at]) VALUES
(1, 1, N'PLOT-001', 200.00, N'Zone A - Plot 1', N'Sandy Soil', N'InUse', '2026-05-01'),
(2, 1, N'PLOT-002', 180.00, N'Zone A - Plot 2', N'Sandy Soil', N'Available', '2026-05-01'),
(3, 2, N'PLOT-003', 250.00, N'Zone B - Plot 1', N'Loam', N'Reserved', '2026-05-01'),
(4, 3, N'PLOT-004', 150.00, N'Zone C - Plot 1', N'Clay Soil', N'Available', '2026-05-01'),
(5, 4, N'PLOT-005', 120.00, N'Zone D - Nursery', N'Peat Soil', N'Maintenance', '2026-05-01');

SET IDENTITY_INSERT [dbo].[LandResource] OFF;

-- -----------------------------------------------------------------------------
-- 12. Experiment (5) - IDs 1-5 | researcher_id = 3
-- -----------------------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Experiment] ON;

INSERT INTO [dbo].[Experiment]
([experiment_id], [experiment_name], [description], [researcher_id], [expect_start_date], [expect_end_date], [deadline], [priority], [status], [created_at]) VALUES
(1, N'Soil Moisture Monitoring Experiment', N'Monitor soil moisture in Zone A using drones and sensors', 3, '2026-06-01', '2026-06-10', '2026-06-15', 1, N'Approved', '2026-05-20'),
(2, N'Tree Growth Analysis', N'Analyze acacia tree growth in Zone B', 3, '2026-06-15', '2026-07-15', '2026-07-20', 2, N'Pending', '2026-05-22'),
(3, N'Irrigation Efficiency Test', N'Evaluate irrigation efficiency in Zone C', 3, '2026-05-25', '2026-06-05', '2026-06-10', 2, N'InProgress', '2026-05-18'),
(4, N'Drone Mapping Survey', N'3D mapping survey of Zone E', 3, '2026-07-01', '2026-07-10', '2026-07-15', 3, N'Draft', '2026-05-25'),
(5, N'Seed Germination Study', N'Seed germination research in the nursery', 3, '2026-04-01', '2026-04-30', '2026-05-05', 4, N'Completed', '2026-03-28');

SET IDENTITY_INSERT [dbo].[Experiment] OFF;

-- -----------------------------------------------------------------------------
-- 13. ExperimentLandRequirement (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[ExperimentLandRequirement]
([experiment_id], [required_area], [required_soil_type], [note], [created_at]) VALUES
(1, 200.00, N'Sandy Soil', N'Fixed plot for the entire experiment duration', '2026-05-20'),
(2, 250.00, N'Loam', N'Large area required for multiple planting plots', '2026-05-22'),
(3, 150.00, N'Clay Soil', N'Irrigation trial zone', '2026-05-18'),
(4, 100.00, N'Loam', N'Drone survey area', '2026-05-25'),
(5, 120.00, N'Peat Soil', N'Nursery seed germination area', '2026-03-28');

-- -----------------------------------------------------------------------------
-- 14. ExperimentEquipmentRequirement (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[ExperimentEquipmentRequirement]
([experiment_id], [equipment_type_id], [quantity], [allow_substitute], [min_acceptable_efficiency], [note], [created_at]) VALUES
(1, 3, 1, 1, 0.75, N'One survey drone required', '2026-05-20'),
(1, 1, 5, 0, NULL, N'Five shovels for manual sampling', '2026-05-20'),
(1, 4, 3, 1, 0.80, N'Three soil moisture sensors', '2026-05-20'),
(2, 1, 10, 0, NULL, N'Ten shovels for growth experiment', '2026-05-22'),
(3, 4, 5, 1, 0.85, N'Five sensors for irrigation monitoring', '2026-05-18');

-- -----------------------------------------------------------------------------
-- 15. ExperimentHumanRequirement (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[ExperimentHumanRequirement]
([experiment_id], [role_id], [quantity], [required_skill_id], [working_hours_per_day], [note], [created_at]) VALUES
(1, 4, 1, 1, 8, N'Technician for drone operation', '2026-05-20'),
(1, 5, 2, 5, 6, N'Two students for data collection support', '2026-05-20'),
(2, 4, 2, 4, 8, N'Two technicians for field measurement', '2026-05-22'),
(3, 5, 3, 2, 6, N'Three students for soil sampling', '2026-05-18'),
(4, 4, 1, 3, 8, N'Technician for GIS and drone mapping', '2026-05-25');

-- -----------------------------------------------------------------------------
-- 16. ExperimentPhase (5) - IDs 1-5
-- -----------------------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[ExperimentPhase] ON;

INSERT INTO [dbo].[ExperimentPhase]
([phase_id], [experiment_id], [phase_name], [phase_description], [phase_order], [expected_start_date], [expected_end_date], [status], [created_at]) VALUES
(1, 1, N'Site Preparation', N'Prepare plot and install sensors', 1, '2026-06-01', '2026-06-03', N'Completed', '2026-05-20'),
(2, 1, N'Field Monitoring', N'Drone flights and moisture data collection', 2, '2026-06-04', '2026-06-10', N'InProgress', '2026-05-20'),
(3, 3, N'Irrigation Setup', N'Install trial irrigation system', 1, '2026-05-25', '2026-05-28', N'Completed', '2026-05-18'),
(4, 3, N'Data Collection', N'Measure irrigation performance', 2, '2026-05-29', '2026-06-05', N'InProgress', '2026-05-18'),
(5, 2, N'Planting Phase', N'Plant acacia seedlings for growth trial', 1, '2026-06-15', '2026-06-25', N'Planned', '2026-05-22');

SET IDENTITY_INSERT [dbo].[ExperimentPhase] OFF;

-- -----------------------------------------------------------------------------
-- 17. PhaseEquipmentRequirement (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[PhaseEquipmentRequirement]
([phase_id], [equipment_type_id], [quantity], [note], [created_at]) VALUES
(1, 1, 3, N'Shovels for experiment 1 site preparation', '2026-05-20'),
(2, 3, 1, N'Drone for monitoring phase', '2026-05-20'),
(2, 4, 3, N'Sensors for monitoring phase', '2026-05-20'),
(3, 4, 2, N'Sensors for irrigation setup', '2026-05-18'),
(4, 1, 5, N'Shovels for irrigation measurement support', '2026-05-18');

-- -----------------------------------------------------------------------------
-- 18. PhaseHumanRequirement (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[PhaseHumanRequirement]
([phase_id], [role_id], [quantity], [required_skill_id], [note], [created_at]) VALUES
(1, 5, 2, 2, N'Students prepare the plot', '2026-05-20'),
(2, 4, 1, 1, N'Technician operates the drone', '2026-05-20'),
(2, 5, 1, 5, N'Student records field data', '2026-05-20'),
(3, 4, 1, 4, N'Technician installs sensors', '2026-05-18'),
(4, 5, 2, 5, N'Students collect irrigation data', '2026-05-18');

-- -----------------------------------------------------------------------------
-- 19. AllocationPlan (5) - IDs 1-5
-- -----------------------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[AllocationPlan] ON;

INSERT INTO [dbo].[AllocationPlan]
([allocation_plan_id], [experiment_id], [fitness_score], [created_by], [approve_by], [approve_status], [approved_at], [created_at]) VALUES
(1, 1, 92.5, 2, 2, N'Approved', '2026-05-25', '2026-05-24'),
(2, 3, 78.0, 2, 2, N'Approved', '2026-05-20', '2026-05-19'),
(3, 2, 65.0, 2, NULL, N'Pending', NULL, '2026-05-23'),
(4, 4, 55.0, 2, NULL, N'Pending', NULL, '2026-05-26'),
(5, 5, 88.0, 2, 2, N'Approved', '2026-04-01', '2026-03-30');

SET IDENTITY_INSERT [dbo].[AllocationPlan] OFF;

-- -----------------------------------------------------------------------------
-- 20. AllocationLandDetail (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[AllocationLandDetail]
([allocation_plan_id], [land_id], [exp_land_req_id], [start_date], [end_date], [status], [created_at]) VALUES
(1, 1, 1, '2026-06-01', '2026-06-10', N'Active', '2026-05-25'),
(2, 4, 3, '2026-05-25', '2026-06-05', N'Active', '2026-05-20'),
(3, 3, 2, '2026-06-15', '2026-07-15', N'Planned', '2026-05-23'),
(4, 2, 4, '2026-07-01', '2026-07-10', N'Planned', '2026-05-26'),
(5, 5, 5, '2026-04-01', '2026-04-30', N'Completed', '2026-04-01');

-- -----------------------------------------------------------------------------
-- 21. AllocationEquipmentDetail (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[AllocationEquipmentDetail]
([allocation_plan_id], [exp_equipment_req_id], [phase_equipment_req_id], [allocated_equipment_type_id],
 [equipment_instance_id], [quantity], [is_substitute], [efficiency_rate], [start_date], [end_date], [status], [created_at]) VALUES
(1, NULL, 2, 3, 1, 1, 0, 1.0, '2026-06-04', '2026-06-10', N'Active', '2026-05-25'),
(1, NULL, 1, 1, NULL, 3, 0, 1.0, '2026-06-01', '2026-06-03', N'Completed', '2026-05-25'),
(1, 3, NULL, 4, NULL, 3, 0, 1.0, '2026-06-01', '2026-06-10', N'Active', '2026-05-25'),
(2, 5, NULL, 4, NULL, 5, 0, 1.0, '2026-05-25', '2026-06-05', N'Active', '2026-05-20'),
(3, 4, NULL, 1, NULL, 10, 0, 1.0, '2026-06-15', '2026-07-15', N'Planned', '2026-05-23');

-- -----------------------------------------------------------------------------
-- 22. AllocationHumanDetail (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[AllocationHumanDetail]
([allocation_plan_id], [exp_human_req_id], [phase_human_req_id], [human_resource_id],
 [working_hours], [start_date], [end_date], [status], [created_at]) VALUES
(1, 1, NULL, 3, 16, '2026-06-01', '2026-06-10', N'Active', '2026-05-25'),
(1, 2, NULL, 4, 12, '2026-06-01', '2026-06-10', N'Active', '2026-05-25'),
(1, NULL, 2, 3, 8, '2026-06-04', '2026-06-10', N'Active', '2026-05-25'),
(2, NULL, 4, 4, 12, '2026-05-29', '2026-06-05', N'Active', '2026-05-20'),
(2, NULL, 3, 3, 8, '2026-05-25', '2026-05-28', N'Completed', '2026-05-20');

-- -----------------------------------------------------------------------------
-- 23. EquipmentShortageLog (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[EquipmentShortageLog]
([allocation_plan_id], [exp_equipment_req_id], [phase_equipment_req_id], [shortage_quantity], [created_at]) VALUES
(3, 4, NULL, 2, '2026-05-23'),
(1, 2, NULL, 2, '2026-05-24'),
(2, 5, NULL, 1, '2026-05-19'),
(2, NULL, 5, 2, '2026-05-20'),
(1, NULL, 3, 1, '2026-05-25');

-- -----------------------------------------------------------------------------
-- 24. Schedule (5)
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[Schedule]
([allocation_plan_id], [phase_id], [title], [description], [start_date], [end_date],
 [status], [created_by], [assigned_human_resource_id], [notes], [priority], [created_at]) VALUES
(1, 1, N'Prepare PLOT-001', N'Install sensors and mark experiment boundaries', '2026-06-01', '2026-06-03', N'Completed', 2, 4, N'Completed on schedule', 1, '2026-05-25'),
(1, 2, N'Drone Survey Flight', N'Technician flies drone to capture moisture data', '2026-06-04', '2026-06-10', N'InProgress', 2, 3, N'DRONE-001 in use', 1, '2026-05-25'),
(2, 3, N'Install Irrigation System', N'Install sensors and trial irrigation lines', '2026-05-25', '2026-05-28', N'Completed', 2, 3, NULL, 2, '2026-05-20'),
(2, 4, N'Collect Irrigation Data', N'Students record flow rate and soil moisture', '2026-05-29', '2026-06-05', N'InProgress', 2, 4, NULL, 2, '2026-05-20'),
(3, 5, N'Acacia Planting', N'Prepare planting for tree growth experiment', '2026-06-15', '2026-06-25', N'Planned', 2, 3, N'Awaiting final allocation approval', 2, '2026-05-23');

-- -----------------------------------------------------------------------------
-- 25. Notification (5)
-- user_id: 1 Admin, 2 Manager, 3 Researcher, 4 Technician, 5 Student
-- -----------------------------------------------------------------------------
INSERT INTO [dbo].[Notification]
([user_id], [title], [message], [notification_type], [reference_type], [reference_id],
 [is_read], [read_at], [is_deleted], [deleted_at], [created_at]) VALUES
(3, N'Allocation Plan Approved', N'Your allocation plan for Soil Moisture Monitoring Experiment has been approved. Fitness score: 92.5.', N'ExperimentApproved', N'AllocationPlan', 1, 1, '2026-05-25 10:30:00', 0, NULL, '2026-05-25 09:00:00'),
(3, N'Equipment Shortage Detected', N'Two shovels are short for your experiment requirements. The system suggests reviewing substitute options.', N'ConflictDetected', N'Experiment', 1, 0, NULL, 0, NULL, '2026-05-24 14:15:00'),
(2, N'Experiment Request Pending Review', N'Tree Growth Analysis is waiting for manager review and resource approval.', N'ExperimentPending', N'Experiment', 2, 0, NULL, 0, NULL, '2026-05-22 08:45:00'),
(4, N'Schedule Assigned', N'You have been assigned to Drone Survey Flight from 2026-06-04 to 2026-06-10.', N'ScheduleAssigned', N'Schedule', 2, 0, NULL, 0, NULL, '2026-05-25 11:00:00'),
(5, N'Schedule Assigned', N'You have been assigned to Prepare PLOT-001 from 2026-06-01 to 2026-06-03.', N'ScheduleAssigned', N'Schedule', 1, 1, '2026-06-03 17:00:00', 0, NULL, '2026-05-25 11:05:00');

COMMIT TRANSACTION;

PRINT N'Seed data loaded successfully.';
PRINT N'Demo accounts: admin / manager / researcher01 / technician01 / student01';
PRINT N'Shared password: Password123!';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT N'Seed data FAILED. Transaction rolled back.';
    PRINT ERROR_MESSAGE();
    THROW;
END CATCH;
GO
