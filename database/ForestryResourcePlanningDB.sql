/*
================================================================================
  Forestry Resource Planning and Allocation Management System
  Script triển khai schema trên MonsterASP (SQL Server hosting)

  Cách chạy qua SSMS:
    1. Server name : db54885.databaseasp.net
    2. Authentication: SQL Server Authentication
    3. Login         : db54885
    4. Password      : (lấy từ MonsterASP control panel)
    5. Connect → chọn database [db54885] (không dùng master)
    6. Mở file này → Execute (F5)

  Lưu ý MonsterASP:
    - Hosting KHÔNG cho phép CREATE / DROP DATABASE
    - Script chỉ DROP TABLE + tạo lại schema trong database đã được cấp
    - Chạy lại script sẽ XÓA TOÀN BỘ dữ liệu hiện có trong db54885

  Local dev (SQL Express): tạo DB ForestryResourcePlanningDB trên máy local,
    đổi dòng USE [db54885] thành USE [ForestryResourcePlanningDB]
================================================================================
*/

USE [db54885];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET NOCOUNT ON;
GO

-- -----------------------------------------------------------------------------
-- Drop tables (thứ tự con → cha; MonsterASP không cho DROP DATABASE)
-- -----------------------------------------------------------------------------
IF OBJECT_ID(N'[dbo].[Schedule]', N'U') IS NOT NULL DROP TABLE [dbo].[Schedule];
GO
IF OBJECT_ID(N'[dbo].[EquipmentShortageLog]', N'U') IS NOT NULL DROP TABLE [dbo].[EquipmentShortageLog];
GO
IF OBJECT_ID(N'[dbo].[AllocationLandDetail]', N'U') IS NOT NULL DROP TABLE [dbo].[AllocationLandDetail];
GO
IF OBJECT_ID(N'[dbo].[AllocationHumanDetail]', N'U') IS NOT NULL DROP TABLE [dbo].[AllocationHumanDetail];
GO
IF OBJECT_ID(N'[dbo].[AllocationEquipmentDetail]', N'U') IS NOT NULL DROP TABLE [dbo].[AllocationEquipmentDetail];
GO
IF OBJECT_ID(N'[dbo].[AllocationPlan]', N'U') IS NOT NULL DROP TABLE [dbo].[AllocationPlan];
GO
IF OBJECT_ID(N'[dbo].[PhaseHumanRequirement]', N'U') IS NOT NULL DROP TABLE [dbo].[PhaseHumanRequirement];
GO
IF OBJECT_ID(N'[dbo].[PhaseEquipmentRequirement]', N'U') IS NOT NULL DROP TABLE [dbo].[PhaseEquipmentRequirement];
GO
IF OBJECT_ID(N'[dbo].[ExperimentPhase]', N'U') IS NOT NULL DROP TABLE [dbo].[ExperimentPhase];
GO
IF OBJECT_ID(N'[dbo].[ExperimentHumanRequirement]', N'U') IS NOT NULL DROP TABLE [dbo].[ExperimentHumanRequirement];
GO
IF OBJECT_ID(N'[dbo].[ExperimentEquipmentRequirement]', N'U') IS NOT NULL DROP TABLE [dbo].[ExperimentEquipmentRequirement];
GO
IF OBJECT_ID(N'[dbo].[ExperimentLandRequirement]', N'U') IS NOT NULL DROP TABLE [dbo].[ExperimentLandRequirement];
GO
IF OBJECT_ID(N'[dbo].[Experiment]', N'U') IS NOT NULL DROP TABLE [dbo].[Experiment];
GO
IF OBJECT_ID(N'[dbo].[EquipmentSubstitution]', N'U') IS NOT NULL DROP TABLE [dbo].[EquipmentSubstitution];
GO
IF OBJECT_ID(N'[dbo].[EquipmentInstance]', N'U') IS NOT NULL DROP TABLE [dbo].[EquipmentInstance];
GO
IF OBJECT_ID(N'[dbo].[EquipmentType]', N'U') IS NOT NULL DROP TABLE [dbo].[EquipmentType];
GO
IF OBJECT_ID(N'[dbo].[HumanResourceSkill]', N'U') IS NOT NULL DROP TABLE [dbo].[HumanResourceSkill];
GO
IF OBJECT_ID(N'[dbo].[HumanResourceProfile]', N'U') IS NOT NULL DROP TABLE [dbo].[HumanResourceProfile];
GO
IF OBJECT_ID(N'[dbo].[LandResource]', N'U') IS NOT NULL DROP TABLE [dbo].[LandResource];
GO
IF OBJECT_ID(N'[dbo].[User]', N'U') IS NOT NULL DROP TABLE [dbo].[User];
GO
IF OBJECT_ID(N'[dbo].[EquipmentCategory]', N'U') IS NOT NULL DROP TABLE [dbo].[EquipmentCategory];
GO
IF OBJECT_ID(N'[dbo].[Area]', N'U') IS NOT NULL DROP TABLE [dbo].[Area];
GO
IF OBJECT_ID(N'[dbo].[Skill]', N'U') IS NOT NULL DROP TABLE [dbo].[Skill];
GO
IF OBJECT_ID(N'[dbo].[Role]', N'U') IS NOT NULL DROP TABLE [dbo].[Role];
GO

-- -----------------------------------------------------------------------------
-- Create tables
-- -----------------------------------------------------------------------------
/****** Object:  Table [dbo].[AllocationEquipmentDetail]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AllocationEquipmentDetail](
	[allocation_equipment_detail_id] [int] IDENTITY(1,1) NOT NULL,
	[allocation_plan_id] [int] NOT NULL,
	[exp_equipment_req_id] [int] NULL,
	[phase_equipment_req_id] [int] NULL,
	[allocated_equipment_type_id] [int] NOT NULL,
	[equipment_instance_id] [int] NULL,
	[quantity] [int] NOT NULL,
	[is_substitute] [bit] NOT NULL,
	[efficiency_rate] [float] NOT NULL,
	[start_date] [datetime2](7) NOT NULL,
	[end_date] [datetime2](7) NOT NULL,
	[status] [nvarchar](50) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[allocation_equipment_detail_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AllocationHumanDetail]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AllocationHumanDetail](
	[allocation_human_detail_id] [int] IDENTITY(1,1) NOT NULL,
	[allocation_plan_id] [int] NOT NULL,
	[exp_human_req_id] [int] NULL,
	[phase_human_req_id] [int] NULL,
	[human_resource_id] [int] NOT NULL,
	[working_hours] [float] NOT NULL,
	[start_date] [datetime2](7) NOT NULL,
	[end_date] [datetime2](7) NOT NULL,
	[status] [nvarchar](50) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[allocation_human_detail_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AllocationLandDetail]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AllocationLandDetail](
	[allocation_land_detail_id] [int] IDENTITY(1,1) NOT NULL,
	[allocation_plan_id] [int] NOT NULL,
	[land_id] [int] NOT NULL,
	[exp_land_req_id] [int] NOT NULL,
	[start_date] [datetime2](7) NOT NULL,
	[end_date] [datetime2](7) NOT NULL,
	[status] [nvarchar](50) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[allocation_land_detail_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AllocationPlan]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AllocationPlan](
	[allocation_plan_id] [int] IDENTITY(1,1) NOT NULL,
	[experiment_id] [int] NOT NULL,
	[fitness_score] [float] NULL,
	[created_by] [int] NULL,
	[approve_by] [int] NULL,
	[approve_status] [nvarchar](50) NOT NULL,
	[approved_at] [datetime2](7) NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[allocation_plan_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Area]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Area](
	[area_id] [int] IDENTITY(1,1) NOT NULL,
	[area_name] [nvarchar](255) NOT NULL,
	[description] [nvarchar](max) NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[area_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EquipmentCategory]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EquipmentCategory](
	[equipment_category_id] [int] IDENTITY(1,1) NOT NULL,
	[category_name] [nvarchar](255) NOT NULL,
	[description] [nvarchar](max) NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[equipment_category_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EquipmentInstance]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EquipmentInstance](
	[equipment_instance_id] [int] IDENTITY(1,1) NOT NULL,
	[equipment_type_id] [int] NOT NULL,
	[asset_code] [nvarchar](100) NOT NULL,
	[serial_number] [nvarchar](100) NULL,
	[total_usage_hour] [float] NOT NULL,
	[last_maintenance_date] [datetime2](7) NULL,
	[usage_hours_since_last_maintenance] [float] NOT NULL,
	[condition_level] [nvarchar](50) NOT NULL,
	[status] [nvarchar](50) NOT NULL,
	[effective_interval_hour] [float] NULL,
	[maintenance_count] [int] NOT NULL,
	[note] [nvarchar](max) NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[equipment_instance_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_EquipmentInstance_asset_code] UNIQUE NONCLUSTERED 
(
	[asset_code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EquipmentShortageLog]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EquipmentShortageLog](
	[shortage_log_id] [int] IDENTITY(1,1) NOT NULL,
	[allocation_plan_id] [int] NOT NULL,
	[exp_equipment_req_id] [int] NULL,
	[phase_equipment_req_id] [int] NULL,
	[shortage_quantity] [int] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[shortage_log_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EquipmentSubstitution]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EquipmentSubstitution](
	[equipment_sub_id] [int] IDENTITY(1,1) NOT NULL,
	[primary_equipment_type_id] [int] NOT NULL,
	[sub_equipment_type_id] [int] NOT NULL,
	[efficiency_rate] [float] NOT NULL,
	[time_multiplier] [float] NOT NULL,
	[note] [nvarchar](max) NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[equipment_sub_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_EquipmentSubstitution] UNIQUE NONCLUSTERED 
(
	[primary_equipment_type_id] ASC,
	[sub_equipment_type_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EquipmentType]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EquipmentType](
	[equipment_type_id] [int] IDENTITY(1,1) NOT NULL,
	[equipment_category_id] [int] NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[tracking_type] [nvarchar](50) NOT NULL,
	[base_maintenance_interval_hours] [float] NULL,
	[total_quantity] [int] NOT NULL,
	[damaged_quantity] [int] NOT NULL,
	[available_quantity] [int] NOT NULL,
	[reserved_quantity] [int] NOT NULL,
	[in_use_quantity] [int] NOT NULL,
	[missing_quantity] [int] NOT NULL,
	[description] [nvarchar](max) NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[equipment_type_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Experiment]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Experiment](
	[experiment_id] [int] IDENTITY(1,1) NOT NULL,
	[experiment_name] [nvarchar](255) NOT NULL,
	[description] [nvarchar](max) NULL,
	[researcher_id] [int] NOT NULL,
	[expect_start_date] [datetime2](7) NOT NULL,
	[expect_end_date] [datetime2](7) NOT NULL,
	[deadline] [datetime2](7) NULL,
	[priority] [int] NOT NULL,
	[status] [nvarchar](50) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[experiment_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExperimentEquipmentRequirement]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExperimentEquipmentRequirement](
	[exp_equipment_req_id] [int] IDENTITY(1,1) NOT NULL,
	[experiment_id] [int] NOT NULL,
	[equipment_type_id] [int] NOT NULL,
	[quantity] [int] NOT NULL,
	[allow_substitute] [bit] NOT NULL,
	[min_acceptable_efficiency] [float] NULL,
	[note] [nvarchar](max) NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[exp_equipment_req_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExperimentHumanRequirement]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExperimentHumanRequirement](
	[exp_human_req_id] [int] IDENTITY(1,1) NOT NULL,
	[experiment_id] [int] NOT NULL,
	[role_id] [int] NOT NULL,
	[quantity] [int] NOT NULL,
	[required_skill_id] [int] NULL,
	[working_hours_per_day] [float] NULL,
	[note] [nvarchar](max) NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[exp_human_req_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExperimentLandRequirement]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExperimentLandRequirement](
	[exp_land_req_id] [int] IDENTITY(1,1) NOT NULL,
	[experiment_id] [int] NOT NULL,
	[required_area] [decimal](10, 2) NOT NULL,
	[required_soil_type] [nvarchar](100) NOT NULL,
	[note] [nvarchar](max) NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[exp_land_req_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExperimentPhase]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExperimentPhase](
	[phase_id] [int] IDENTITY(1,1) NOT NULL,
	[experiment_id] [int] NOT NULL,
	[phase_name] [nvarchar](255) NOT NULL,
	[phase_description] [nvarchar](max) NULL,
	[phase_order] [int] NOT NULL,
	[expected_start_date] [datetime2](7) NOT NULL,
	[expected_end_date] [datetime2](7) NOT NULL,
	[status] [nvarchar](50) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[phase_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_ExperimentPhase_Order] UNIQUE NONCLUSTERED 
(
	[experiment_id] ASC,
	[phase_order] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HumanResourceProfile]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HumanResourceProfile](
	[human_resource_id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NOT NULL,
	[max_working_hours_per_day] [float] NOT NULL,
	[current_workload] [float] NOT NULL,
	[status] [nvarchar](50) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[human_resource_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_HumanResourceProfile_user_id] UNIQUE NONCLUSTERED 
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HumanResourceSkill]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HumanResourceSkill](
	[human_resource_skill_id] [int] IDENTITY(1,1) NOT NULL,
	[human_resource_id] [int] NOT NULL,
	[skill_id] [int] NOT NULL,
	[skill_level] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[human_resource_skill_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_HumanResourceSkill] UNIQUE NONCLUSTERED 
(
	[human_resource_id] ASC,
	[skill_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LandResource]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LandResource](
	[land_id] [int] IDENTITY(1,1) NOT NULL,
	[area_id] [int] NOT NULL,
	[land_code] [nvarchar](100) NOT NULL,
	[area_size] [decimal](10, 2) NOT NULL,
	[location] [nvarchar](255) NULL,
	[soil_type] [nvarchar](100) NOT NULL,
	[status] [nvarchar](50) NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[land_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_LandResource_land_code] UNIQUE NONCLUSTERED 
(
	[land_code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PhaseEquipmentRequirement]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PhaseEquipmentRequirement](
	[phase_equipment_req_id] [int] IDENTITY(1,1) NOT NULL,
	[phase_id] [int] NOT NULL,
	[equipment_type_id] [int] NOT NULL,
	[quantity] [int] NOT NULL,
	[note] [nvarchar](max) NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[phase_equipment_req_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PhaseHumanRequirement]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PhaseHumanRequirement](
	[phase_human_req_id] [int] IDENTITY(1,1) NOT NULL,
	[phase_id] [int] NOT NULL,
	[role_id] [int] NOT NULL,
	[quantity] [int] NOT NULL,
	[required_skill_id] [int] NULL,
	[note] [nvarchar](max) NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[phase_human_req_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Role]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Role](
	[role_id] [int] IDENTITY(1,1) NOT NULL,
	[role_name] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[role_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Schedule]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Schedule](
	[schedule_id] [int] IDENTITY(1,1) NOT NULL,
	[allocation_plan_id] [int] NOT NULL,
	[phase_id] [int] NULL,
	[title] [nvarchar](255) NOT NULL,
	[description] [nvarchar](max) NULL,
	[start_date] [datetime2](7) NOT NULL,
	[end_date] [datetime2](7) NOT NULL,
	[status] [nvarchar](50) NOT NULL,
	[created_by] [int] NULL,
	[assigned_human_resource_id] [int] NULL,
	[notes] [nvarchar](max) NULL,
	[priority] [int] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[schedule_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Skill]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Skill](
	[skill_id] [int] IDENTITY(1,1) NOT NULL,
	[skill_name] [nvarchar](100) NOT NULL,
	[description] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[skill_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 6/7/2026 6:52:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[user_id] [int] IDENTITY(1,1) NOT NULL,
	[full_name] [nvarchar](255) NOT NULL,
	[username] [nvarchar](100) NOT NULL,
	[password_hash] [nvarchar](255) NOT NULL,
	[email] [nvarchar](255) NOT NULL,
	[role_id] [int] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[user_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_User_email] UNIQUE NONCLUSTERED 
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_User_username] UNIQUE NONCLUSTERED 
(
	[username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notification]    Script Date: 6/7/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notification](
	[notification_id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NOT NULL,
	[title] [nvarchar](255) NOT NULL,
	[message] [nvarchar](max) NOT NULL,
	[notification_type] [nvarchar](50) NOT NULL,
	[reference_type] [nvarchar](50) NULL,
	[reference_id] [int] NULL,
	[is_read] [bit] NOT NULL,
	[read_at] [datetime2](7) NULL,
	[is_deleted] [bit] NOT NULL,
	[deleted_at] [datetime2](7) NULL,
	[created_at] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[notification_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail] ADD  DEFAULT ((0)) FOR [is_substitute]
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail] ADD  DEFAULT ((1)) FOR [efficiency_rate]
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[AllocationHumanDetail] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[AllocationLandDetail] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[AllocationPlan] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[Area] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[EquipmentCategory] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[EquipmentInstance] ADD  DEFAULT ((0)) FOR [total_usage_hour]
GO
ALTER TABLE [dbo].[EquipmentInstance] ADD  DEFAULT ((0)) FOR [usage_hours_since_last_maintenance]
GO
ALTER TABLE [dbo].[EquipmentInstance] ADD  DEFAULT ((0)) FOR [maintenance_count]
GO
ALTER TABLE [dbo].[EquipmentInstance] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[EquipmentShortageLog] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[EquipmentSubstitution] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[EquipmentType] ADD  DEFAULT ((0)) FOR [total_quantity]
GO
ALTER TABLE [dbo].[EquipmentType] ADD  DEFAULT ((0)) FOR [damaged_quantity]
GO
ALTER TABLE [dbo].[EquipmentType] ADD  DEFAULT ((0)) FOR [available_quantity]
GO
ALTER TABLE [dbo].[EquipmentType] ADD  DEFAULT ((0)) FOR [reserved_quantity]
GO
ALTER TABLE [dbo].[EquipmentType] ADD  DEFAULT ((0)) FOR [in_use_quantity]
GO
ALTER TABLE [dbo].[EquipmentType] ADD  DEFAULT ((0)) FOR [missing_quantity]
GO
ALTER TABLE [dbo].[EquipmentType] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[Experiment] ADD  DEFAULT ((2)) FOR [priority]
GO
ALTER TABLE [dbo].[Experiment] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[ExperimentEquipmentRequirement] ADD  DEFAULT ((0)) FOR [allow_substitute]
GO
ALTER TABLE [dbo].[ExperimentEquipmentRequirement] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[ExperimentHumanRequirement] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[ExperimentLandRequirement] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[ExperimentPhase] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[HumanResourceProfile] ADD  DEFAULT ((0)) FOR [current_workload]
GO
ALTER TABLE [dbo].[HumanResourceProfile] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[LandResource] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[PhaseEquipmentRequirement] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[PhaseHumanRequirement] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[Schedule] ADD  DEFAULT ((2)) FOR [priority]
GO
ALTER TABLE [dbo].[Schedule] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT (getdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[Notification] ADD  DEFAULT ((0)) FOR [is_read]
GO
ALTER TABLE [dbo].[Notification] ADD  DEFAULT ((0)) FOR [is_deleted]
GO
ALTER TABLE [dbo].[Notification] ADD  DEFAULT (getdate()) FOR [created_at]
GO
CREATE NONCLUSTERED INDEX [IX_Notification_User_IsRead_CreatedAt]
ON [dbo].[Notification]([user_id] ASC, [is_read] ASC, [created_at] DESC)
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail]  WITH CHECK ADD  CONSTRAINT [FK_AllocationEquipmentDetail_AllocatedEquipmentType] FOREIGN KEY([allocated_equipment_type_id])
REFERENCES [dbo].[EquipmentType] ([equipment_type_id])
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail] CHECK CONSTRAINT [FK_AllocationEquipmentDetail_AllocatedEquipmentType]
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail]  WITH CHECK ADD  CONSTRAINT [FK_AllocationEquipmentDetail_AllocationPlan] FOREIGN KEY([allocation_plan_id])
REFERENCES [dbo].[AllocationPlan] ([allocation_plan_id])
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail] CHECK CONSTRAINT [FK_AllocationEquipmentDetail_AllocationPlan]
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail]  WITH CHECK ADD  CONSTRAINT [FK_AllocationEquipmentDetail_EquipmentInstance] FOREIGN KEY([equipment_instance_id])
REFERENCES [dbo].[EquipmentInstance] ([equipment_instance_id])
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail] CHECK CONSTRAINT [FK_AllocationEquipmentDetail_EquipmentInstance]
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail]  WITH CHECK ADD  CONSTRAINT [FK_AllocationEquipmentDetail_ExperimentEquipmentRequirement] FOREIGN KEY([exp_equipment_req_id])
REFERENCES [dbo].[ExperimentEquipmentRequirement] ([exp_equipment_req_id])
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail] CHECK CONSTRAINT [FK_AllocationEquipmentDetail_ExperimentEquipmentRequirement]
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail]  WITH CHECK ADD  CONSTRAINT [FK_AllocationEquipmentDetail_PhaseEquipmentRequirement] FOREIGN KEY([phase_equipment_req_id])
REFERENCES [dbo].[PhaseEquipmentRequirement] ([phase_equipment_req_id])
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail] CHECK CONSTRAINT [FK_AllocationEquipmentDetail_PhaseEquipmentRequirement]
GO
ALTER TABLE [dbo].[AllocationHumanDetail]  WITH CHECK ADD  CONSTRAINT [FK_AllocationHumanDetail_AllocationPlan] FOREIGN KEY([allocation_plan_id])
REFERENCES [dbo].[AllocationPlan] ([allocation_plan_id])
GO
ALTER TABLE [dbo].[AllocationHumanDetail] CHECK CONSTRAINT [FK_AllocationHumanDetail_AllocationPlan]
GO
ALTER TABLE [dbo].[AllocationHumanDetail]  WITH CHECK ADD  CONSTRAINT [FK_AllocationHumanDetail_ExperimentHumanRequirement] FOREIGN KEY([exp_human_req_id])
REFERENCES [dbo].[ExperimentHumanRequirement] ([exp_human_req_id])
GO
ALTER TABLE [dbo].[AllocationHumanDetail] CHECK CONSTRAINT [FK_AllocationHumanDetail_ExperimentHumanRequirement]
GO
ALTER TABLE [dbo].[AllocationHumanDetail]  WITH CHECK ADD  CONSTRAINT [FK_AllocationHumanDetail_HumanResourceProfile] FOREIGN KEY([human_resource_id])
REFERENCES [dbo].[HumanResourceProfile] ([human_resource_id])
GO
ALTER TABLE [dbo].[AllocationHumanDetail] CHECK CONSTRAINT [FK_AllocationHumanDetail_HumanResourceProfile]
GO
ALTER TABLE [dbo].[AllocationHumanDetail]  WITH CHECK ADD  CONSTRAINT [FK_AllocationHumanDetail_PhaseHumanRequirement] FOREIGN KEY([phase_human_req_id])
REFERENCES [dbo].[PhaseHumanRequirement] ([phase_human_req_id])
GO
ALTER TABLE [dbo].[AllocationHumanDetail] CHECK CONSTRAINT [FK_AllocationHumanDetail_PhaseHumanRequirement]
GO
ALTER TABLE [dbo].[AllocationLandDetail]  WITH CHECK ADD  CONSTRAINT [FK_AllocationLandDetail_AllocationPlan] FOREIGN KEY([allocation_plan_id])
REFERENCES [dbo].[AllocationPlan] ([allocation_plan_id])
GO
ALTER TABLE [dbo].[AllocationLandDetail] CHECK CONSTRAINT [FK_AllocationLandDetail_AllocationPlan]
GO
ALTER TABLE [dbo].[AllocationLandDetail]  WITH CHECK ADD  CONSTRAINT [FK_AllocationLandDetail_ExperimentLandRequirement] FOREIGN KEY([exp_land_req_id])
REFERENCES [dbo].[ExperimentLandRequirement] ([exp_land_req_id])
GO
ALTER TABLE [dbo].[AllocationLandDetail] CHECK CONSTRAINT [FK_AllocationLandDetail_ExperimentLandRequirement]
GO
ALTER TABLE [dbo].[AllocationLandDetail]  WITH CHECK ADD  CONSTRAINT [FK_AllocationLandDetail_LandResource] FOREIGN KEY([land_id])
REFERENCES [dbo].[LandResource] ([land_id])
GO
ALTER TABLE [dbo].[AllocationLandDetail] CHECK CONSTRAINT [FK_AllocationLandDetail_LandResource]
GO
ALTER TABLE [dbo].[AllocationPlan]  WITH CHECK ADD  CONSTRAINT [FK_AllocationPlan_ApproveBy] FOREIGN KEY([approve_by])
REFERENCES [dbo].[User] ([user_id])
GO
ALTER TABLE [dbo].[AllocationPlan] CHECK CONSTRAINT [FK_AllocationPlan_ApproveBy]
GO
ALTER TABLE [dbo].[AllocationPlan]  WITH CHECK ADD  CONSTRAINT [FK_AllocationPlan_CreatedBy] FOREIGN KEY([created_by])
REFERENCES [dbo].[User] ([user_id])
GO
ALTER TABLE [dbo].[AllocationPlan] CHECK CONSTRAINT [FK_AllocationPlan_CreatedBy]
GO
ALTER TABLE [dbo].[AllocationPlan]  WITH CHECK ADD  CONSTRAINT [FK_AllocationPlan_Experiment] FOREIGN KEY([experiment_id])
REFERENCES [dbo].[Experiment] ([experiment_id])
GO
ALTER TABLE [dbo].[AllocationPlan] CHECK CONSTRAINT [FK_AllocationPlan_Experiment]
GO
ALTER TABLE [dbo].[EquipmentInstance]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentInstance_EquipmentType] FOREIGN KEY([equipment_type_id])
REFERENCES [dbo].[EquipmentType] ([equipment_type_id])
GO
ALTER TABLE [dbo].[EquipmentInstance] CHECK CONSTRAINT [FK_EquipmentInstance_EquipmentType]
GO
ALTER TABLE [dbo].[EquipmentShortageLog]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentShortageLog_AllocationPlan] FOREIGN KEY([allocation_plan_id])
REFERENCES [dbo].[AllocationPlan] ([allocation_plan_id])
GO
ALTER TABLE [dbo].[EquipmentShortageLog] CHECK CONSTRAINT [FK_EquipmentShortageLog_AllocationPlan]
GO
ALTER TABLE [dbo].[EquipmentShortageLog]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentShortageLog_ExperimentEquipmentRequirement] FOREIGN KEY([exp_equipment_req_id])
REFERENCES [dbo].[ExperimentEquipmentRequirement] ([exp_equipment_req_id])
GO
ALTER TABLE [dbo].[EquipmentShortageLog] CHECK CONSTRAINT [FK_EquipmentShortageLog_ExperimentEquipmentRequirement]
GO
ALTER TABLE [dbo].[EquipmentShortageLog]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentShortageLog_PhaseEquipmentRequirement] FOREIGN KEY([phase_equipment_req_id])
REFERENCES [dbo].[PhaseEquipmentRequirement] ([phase_equipment_req_id])
GO
ALTER TABLE [dbo].[EquipmentShortageLog] CHECK CONSTRAINT [FK_EquipmentShortageLog_PhaseEquipmentRequirement]
GO
ALTER TABLE [dbo].[EquipmentSubstitution]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentSubstitution_PrimaryEquipmentType] FOREIGN KEY([primary_equipment_type_id])
REFERENCES [dbo].[EquipmentType] ([equipment_type_id])
GO
ALTER TABLE [dbo].[EquipmentSubstitution] CHECK CONSTRAINT [FK_EquipmentSubstitution_PrimaryEquipmentType]
GO
ALTER TABLE [dbo].[EquipmentSubstitution]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentSubstitution_SubEquipmentType] FOREIGN KEY([sub_equipment_type_id])
REFERENCES [dbo].[EquipmentType] ([equipment_type_id])
GO
ALTER TABLE [dbo].[EquipmentSubstitution] CHECK CONSTRAINT [FK_EquipmentSubstitution_SubEquipmentType]
GO
ALTER TABLE [dbo].[EquipmentType]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentType_EquipmentCategory] FOREIGN KEY([equipment_category_id])
REFERENCES [dbo].[EquipmentCategory] ([equipment_category_id])
GO
ALTER TABLE [dbo].[EquipmentType] CHECK CONSTRAINT [FK_EquipmentType_EquipmentCategory]
GO
ALTER TABLE [dbo].[Experiment]  WITH CHECK ADD  CONSTRAINT [FK_Experiment_Researcher] FOREIGN KEY([researcher_id])
REFERENCES [dbo].[User] ([user_id])
GO
ALTER TABLE [dbo].[Experiment] CHECK CONSTRAINT [FK_Experiment_Researcher]
GO
ALTER TABLE [dbo].[ExperimentEquipmentRequirement]  WITH CHECK ADD  CONSTRAINT [FK_ExperimentEquipmentRequirement_EquipmentType] FOREIGN KEY([equipment_type_id])
REFERENCES [dbo].[EquipmentType] ([equipment_type_id])
GO
ALTER TABLE [dbo].[ExperimentEquipmentRequirement] CHECK CONSTRAINT [FK_ExperimentEquipmentRequirement_EquipmentType]
GO
ALTER TABLE [dbo].[ExperimentEquipmentRequirement]  WITH CHECK ADD  CONSTRAINT [FK_ExperimentEquipmentRequirement_Experiment] FOREIGN KEY([experiment_id])
REFERENCES [dbo].[Experiment] ([experiment_id])
GO
ALTER TABLE [dbo].[ExperimentEquipmentRequirement] CHECK CONSTRAINT [FK_ExperimentEquipmentRequirement_Experiment]
GO
ALTER TABLE [dbo].[ExperimentHumanRequirement]  WITH CHECK ADD  CONSTRAINT [FK_ExperimentHumanRequirement_Experiment] FOREIGN KEY([experiment_id])
REFERENCES [dbo].[Experiment] ([experiment_id])
GO
ALTER TABLE [dbo].[ExperimentHumanRequirement] CHECK CONSTRAINT [FK_ExperimentHumanRequirement_Experiment]
GO
ALTER TABLE [dbo].[ExperimentHumanRequirement]  WITH CHECK ADD  CONSTRAINT [FK_ExperimentHumanRequirement_Role] FOREIGN KEY([role_id])
REFERENCES [dbo].[Role] ([role_id])
GO
ALTER TABLE [dbo].[ExperimentHumanRequirement] CHECK CONSTRAINT [FK_ExperimentHumanRequirement_Role]
GO
ALTER TABLE [dbo].[ExperimentHumanRequirement]  WITH CHECK ADD  CONSTRAINT [FK_ExperimentHumanRequirement_Skill] FOREIGN KEY([required_skill_id])
REFERENCES [dbo].[Skill] ([skill_id])
GO
ALTER TABLE [dbo].[ExperimentHumanRequirement] CHECK CONSTRAINT [FK_ExperimentHumanRequirement_Skill]
GO
ALTER TABLE [dbo].[ExperimentLandRequirement]  WITH CHECK ADD  CONSTRAINT [FK_ExperimentLandRequirement_Experiment] FOREIGN KEY([experiment_id])
REFERENCES [dbo].[Experiment] ([experiment_id])
GO
ALTER TABLE [dbo].[ExperimentLandRequirement] CHECK CONSTRAINT [FK_ExperimentLandRequirement_Experiment]
GO
ALTER TABLE [dbo].[ExperimentPhase]  WITH CHECK ADD  CONSTRAINT [FK_ExperimentPhase_Experiment] FOREIGN KEY([experiment_id])
REFERENCES [dbo].[Experiment] ([experiment_id])
GO
ALTER TABLE [dbo].[ExperimentPhase] CHECK CONSTRAINT [FK_ExperimentPhase_Experiment]
GO
ALTER TABLE [dbo].[HumanResourceProfile]  WITH CHECK ADD  CONSTRAINT [FK_HumanResourceProfile_User] FOREIGN KEY([user_id])
REFERENCES [dbo].[User] ([user_id])
GO
ALTER TABLE [dbo].[HumanResourceProfile] CHECK CONSTRAINT [FK_HumanResourceProfile_User]
GO
ALTER TABLE [dbo].[HumanResourceSkill]  WITH CHECK ADD  CONSTRAINT [FK_HumanResourceSkill_HumanResourceProfile] FOREIGN KEY([human_resource_id])
REFERENCES [dbo].[HumanResourceProfile] ([human_resource_id])
GO
ALTER TABLE [dbo].[HumanResourceSkill] CHECK CONSTRAINT [FK_HumanResourceSkill_HumanResourceProfile]
GO
ALTER TABLE [dbo].[HumanResourceSkill]  WITH CHECK ADD  CONSTRAINT [FK_HumanResourceSkill_Skill] FOREIGN KEY([skill_id])
REFERENCES [dbo].[Skill] ([skill_id])
GO
ALTER TABLE [dbo].[HumanResourceSkill] CHECK CONSTRAINT [FK_HumanResourceSkill_Skill]
GO
ALTER TABLE [dbo].[LandResource]  WITH CHECK ADD  CONSTRAINT [FK_LandResource_Area] FOREIGN KEY([area_id])
REFERENCES [dbo].[Area] ([area_id])
GO
ALTER TABLE [dbo].[LandResource] CHECK CONSTRAINT [FK_LandResource_Area]
GO
ALTER TABLE [dbo].[PhaseEquipmentRequirement]  WITH CHECK ADD  CONSTRAINT [FK_PhaseEquipmentRequirement_EquipmentType] FOREIGN KEY([equipment_type_id])
REFERENCES [dbo].[EquipmentType] ([equipment_type_id])
GO
ALTER TABLE [dbo].[PhaseEquipmentRequirement] CHECK CONSTRAINT [FK_PhaseEquipmentRequirement_EquipmentType]
GO
ALTER TABLE [dbo].[PhaseEquipmentRequirement]  WITH CHECK ADD  CONSTRAINT [FK_PhaseEquipmentRequirement_ExperimentPhase] FOREIGN KEY([phase_id])
REFERENCES [dbo].[ExperimentPhase] ([phase_id])
GO
ALTER TABLE [dbo].[PhaseEquipmentRequirement] CHECK CONSTRAINT [FK_PhaseEquipmentRequirement_ExperimentPhase]
GO
ALTER TABLE [dbo].[PhaseHumanRequirement]  WITH CHECK ADD  CONSTRAINT [FK_PhaseHumanRequirement_ExperimentPhase] FOREIGN KEY([phase_id])
REFERENCES [dbo].[ExperimentPhase] ([phase_id])
GO
ALTER TABLE [dbo].[PhaseHumanRequirement] CHECK CONSTRAINT [FK_PhaseHumanRequirement_ExperimentPhase]
GO
ALTER TABLE [dbo].[PhaseHumanRequirement]  WITH CHECK ADD  CONSTRAINT [FK_PhaseHumanRequirement_Role] FOREIGN KEY([role_id])
REFERENCES [dbo].[Role] ([role_id])
GO
ALTER TABLE [dbo].[PhaseHumanRequirement] CHECK CONSTRAINT [FK_PhaseHumanRequirement_Role]
GO
ALTER TABLE [dbo].[Notification]  WITH CHECK ADD  CONSTRAINT [FK_Notification_User] FOREIGN KEY([user_id])
REFERENCES [dbo].[User] ([user_id])
GO
ALTER TABLE [dbo].[Notification] CHECK CONSTRAINT [FK_Notification_User]
GO
ALTER TABLE [dbo].[PhaseHumanRequirement]  WITH CHECK ADD  CONSTRAINT [FK_PhaseHumanRequirement_Skill] FOREIGN KEY([required_skill_id])
REFERENCES [dbo].[Skill] ([skill_id])
GO
ALTER TABLE [dbo].[PhaseHumanRequirement] CHECK CONSTRAINT [FK_PhaseHumanRequirement_Skill]
GO
ALTER TABLE [dbo].[Schedule]  WITH CHECK ADD  CONSTRAINT [FK_Schedule_AllocationPlan] FOREIGN KEY([allocation_plan_id])
REFERENCES [dbo].[AllocationPlan] ([allocation_plan_id])
GO
ALTER TABLE [dbo].[Schedule] CHECK CONSTRAINT [FK_Schedule_AllocationPlan]
GO
ALTER TABLE [dbo].[Schedule]  WITH CHECK ADD  CONSTRAINT [FK_Schedule_AssignedHumanResource] FOREIGN KEY([assigned_human_resource_id])
REFERENCES [dbo].[HumanResourceProfile] ([human_resource_id])
GO
ALTER TABLE [dbo].[Schedule] CHECK CONSTRAINT [FK_Schedule_AssignedHumanResource]
GO
ALTER TABLE [dbo].[Schedule]  WITH CHECK ADD  CONSTRAINT [FK_Schedule_CreatedBy] FOREIGN KEY([created_by])
REFERENCES [dbo].[User] ([user_id])
GO
ALTER TABLE [dbo].[Schedule] CHECK CONSTRAINT [FK_Schedule_CreatedBy]
GO
ALTER TABLE [dbo].[Schedule]  WITH CHECK ADD  CONSTRAINT [FK_Schedule_ExperimentPhase] FOREIGN KEY([phase_id])
REFERENCES [dbo].[ExperimentPhase] ([phase_id])
GO
ALTER TABLE [dbo].[Schedule] CHECK CONSTRAINT [FK_Schedule_ExperimentPhase]
GO
ALTER TABLE [dbo].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_Role] FOREIGN KEY([role_id])
REFERENCES [dbo].[Role] ([role_id])
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [FK_User_Role]
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail]  WITH CHECK ADD  CONSTRAINT [CK_AllocationEquipmentDetail_DateRange] CHECK  (([end_date]>=[start_date]))
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail] CHECK CONSTRAINT [CK_AllocationEquipmentDetail_DateRange]
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail]  WITH CHECK ADD  CONSTRAINT [CK_AllocationEquipmentDetail_efficiency_rate] CHECK  (([efficiency_rate]>(0) AND [efficiency_rate]<=(1)))
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail] CHECK CONSTRAINT [CK_AllocationEquipmentDetail_efficiency_rate]
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail]  WITH CHECK ADD  CONSTRAINT [CK_AllocationEquipmentDetail_quantity] CHECK  (([quantity]>(0)))
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail] CHECK CONSTRAINT [CK_AllocationEquipmentDetail_quantity]
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail]  WITH CHECK ADD  CONSTRAINT [CK_AllocationEquipmentDetail_RequirementSource] CHECK  (([exp_equipment_req_id] IS NOT NULL AND [phase_equipment_req_id] IS NULL OR [exp_equipment_req_id] IS NULL AND [phase_equipment_req_id] IS NOT NULL))
GO
ALTER TABLE [dbo].[AllocationEquipmentDetail] CHECK CONSTRAINT [CK_AllocationEquipmentDetail_RequirementSource]
GO
ALTER TABLE [dbo].[AllocationHumanDetail]  WITH CHECK ADD  CONSTRAINT [CK_AllocationHumanDetail_DateRange] CHECK  (([end_date]>=[start_date]))
GO
ALTER TABLE [dbo].[AllocationHumanDetail] CHECK CONSTRAINT [CK_AllocationHumanDetail_DateRange]
GO
ALTER TABLE [dbo].[AllocationHumanDetail]  WITH CHECK ADD  CONSTRAINT [CK_AllocationHumanDetail_RequirementSource] CHECK  (([exp_human_req_id] IS NOT NULL AND [phase_human_req_id] IS NULL OR [exp_human_req_id] IS NULL AND [phase_human_req_id] IS NOT NULL))
GO
ALTER TABLE [dbo].[AllocationHumanDetail] CHECK CONSTRAINT [CK_AllocationHumanDetail_RequirementSource]
GO
ALTER TABLE [dbo].[AllocationHumanDetail]  WITH CHECK ADD  CONSTRAINT [CK_AllocationHumanDetail_working_hours] CHECK  (([working_hours]>(0)))
GO
ALTER TABLE [dbo].[AllocationHumanDetail] CHECK CONSTRAINT [CK_AllocationHumanDetail_working_hours]
GO
ALTER TABLE [dbo].[AllocationLandDetail]  WITH CHECK ADD  CONSTRAINT [CK_AllocationLandDetail_DateRange] CHECK  (([end_date]>=[start_date]))
GO
ALTER TABLE [dbo].[AllocationLandDetail] CHECK CONSTRAINT [CK_AllocationLandDetail_DateRange]
GO
ALTER TABLE [dbo].[AllocationPlan]  WITH CHECK ADD  CONSTRAINT [CK_AllocationPlan_fitness_score] CHECK  (([fitness_score] IS NULL OR [fitness_score]>=(0) AND [fitness_score]<=(100)))
GO
ALTER TABLE [dbo].[AllocationPlan] CHECK CONSTRAINT [CK_AllocationPlan_fitness_score]
GO
ALTER TABLE [dbo].[EquipmentInstance]  WITH CHECK ADD  CONSTRAINT [CK_EquipmentInstance_usage] CHECK  (([total_usage_hour]>=(0) AND [usage_hours_since_last_maintenance]>=(0) AND [maintenance_count]>=(0)))
GO
ALTER TABLE [dbo].[EquipmentInstance] CHECK CONSTRAINT [CK_EquipmentInstance_usage]
GO
ALTER TABLE [dbo].[EquipmentShortageLog]  WITH CHECK ADD  CONSTRAINT [CK_EquipmentShortageLog_RequirementSource] CHECK  (([exp_equipment_req_id] IS NOT NULL AND [phase_equipment_req_id] IS NULL OR [exp_equipment_req_id] IS NULL AND [phase_equipment_req_id] IS NOT NULL))
GO
ALTER TABLE [dbo].[EquipmentShortageLog] CHECK CONSTRAINT [CK_EquipmentShortageLog_RequirementSource]
GO
ALTER TABLE [dbo].[EquipmentShortageLog]  WITH CHECK ADD  CONSTRAINT [CK_EquipmentShortageLog_shortage_quantity] CHECK  (([shortage_quantity]>(0)))
GO
ALTER TABLE [dbo].[EquipmentShortageLog] CHECK CONSTRAINT [CK_EquipmentShortageLog_shortage_quantity]
GO
ALTER TABLE [dbo].[EquipmentSubstitution]  WITH CHECK ADD  CONSTRAINT [CK_EquipmentSubstitution_Efficiency] CHECK  (([efficiency_rate]>(0) AND [efficiency_rate]<=(1)))
GO
ALTER TABLE [dbo].[EquipmentSubstitution] CHECK CONSTRAINT [CK_EquipmentSubstitution_Efficiency]
GO
ALTER TABLE [dbo].[EquipmentSubstitution]  WITH CHECK ADD  CONSTRAINT [CK_EquipmentSubstitution_NotSame] CHECK  (([primary_equipment_type_id]<>[sub_equipment_type_id]))
GO
ALTER TABLE [dbo].[EquipmentSubstitution] CHECK CONSTRAINT [CK_EquipmentSubstitution_NotSame]
GO
ALTER TABLE [dbo].[EquipmentSubstitution]  WITH CHECK ADD  CONSTRAINT [CK_EquipmentSubstitution_TimeMultiplier] CHECK  (([time_multiplier]>=(1)))
GO
ALTER TABLE [dbo].[EquipmentSubstitution] CHECK CONSTRAINT [CK_EquipmentSubstitution_TimeMultiplier]
GO
ALTER TABLE [dbo].[EquipmentType]  WITH CHECK ADD  CONSTRAINT [CK_EquipmentType_quantities] CHECK  (([total_quantity]>=(0) AND [damaged_quantity]>=(0) AND [available_quantity]>=(0) AND [reserved_quantity]>=(0) AND [in_use_quantity]>=(0) AND [missing_quantity]>=(0)))
GO
ALTER TABLE [dbo].[EquipmentType] CHECK CONSTRAINT [CK_EquipmentType_quantities]
GO
ALTER TABLE [dbo].[EquipmentType]  WITH CHECK ADD  CONSTRAINT [CK_EquipmentType_tracking_type] CHECK  (([tracking_type]='Individual' OR [tracking_type]='QuantityBased'))
GO
ALTER TABLE [dbo].[EquipmentType] CHECK CONSTRAINT [CK_EquipmentType_tracking_type]
GO
ALTER TABLE [dbo].[Experiment]  WITH CHECK ADD  CONSTRAINT [CK_Experiment_DateRange] CHECK  (([expect_end_date]>=[expect_start_date]))
GO
ALTER TABLE [dbo].[Experiment] CHECK CONSTRAINT [CK_Experiment_DateRange]
GO
ALTER TABLE [dbo].[Experiment]  WITH CHECK ADD  CONSTRAINT [CK_Experiment_Priority] CHECK  (([priority]>=(1) AND [priority]<=(4)))
GO
ALTER TABLE [dbo].[Experiment] CHECK CONSTRAINT [CK_Experiment_Priority]
GO
ALTER TABLE [dbo].[ExperimentEquipmentRequirement]  WITH CHECK ADD  CONSTRAINT [CK_ExperimentEquipmentRequirement_min_efficiency] CHECK  (([min_acceptable_efficiency] IS NULL OR [min_acceptable_efficiency]>(0) AND [min_acceptable_efficiency]<=(1)))
GO
ALTER TABLE [dbo].[ExperimentEquipmentRequirement] CHECK CONSTRAINT [CK_ExperimentEquipmentRequirement_min_efficiency]
GO
ALTER TABLE [dbo].[ExperimentEquipmentRequirement]  WITH CHECK ADD  CONSTRAINT [CK_ExperimentEquipmentRequirement_quantity] CHECK  (([quantity]>(0)))
GO
ALTER TABLE [dbo].[ExperimentEquipmentRequirement] CHECK CONSTRAINT [CK_ExperimentEquipmentRequirement_quantity]
GO
ALTER TABLE [dbo].[ExperimentHumanRequirement]  WITH CHECK ADD  CONSTRAINT [CK_ExperimentHumanRequirement_quantity] CHECK  (([quantity]>(0)))
GO
ALTER TABLE [dbo].[ExperimentHumanRequirement] CHECK CONSTRAINT [CK_ExperimentHumanRequirement_quantity]
GO
ALTER TABLE [dbo].[ExperimentHumanRequirement]  WITH CHECK ADD  CONSTRAINT [CK_ExperimentHumanRequirement_working_hours] CHECK  (([working_hours_per_day] IS NULL OR [working_hours_per_day]>(0)))
GO
ALTER TABLE [dbo].[ExperimentHumanRequirement] CHECK CONSTRAINT [CK_ExperimentHumanRequirement_working_hours]
GO
ALTER TABLE [dbo].[ExperimentLandRequirement]  WITH CHECK ADD  CONSTRAINT [CK_ExperimentLandRequirement_required_area] CHECK  (([required_area]>(0)))
GO
ALTER TABLE [dbo].[ExperimentLandRequirement] CHECK CONSTRAINT [CK_ExperimentLandRequirement_required_area]
GO
ALTER TABLE [dbo].[ExperimentPhase]  WITH CHECK ADD  CONSTRAINT [CK_ExperimentPhase_DateRange] CHECK  (([expected_end_date]>=[expected_start_date]))
GO
ALTER TABLE [dbo].[ExperimentPhase] CHECK CONSTRAINT [CK_ExperimentPhase_DateRange]
GO
ALTER TABLE [dbo].[ExperimentPhase]  WITH CHECK ADD  CONSTRAINT [CK_ExperimentPhase_Order] CHECK  (([phase_order]>(0)))
GO
ALTER TABLE [dbo].[ExperimentPhase] CHECK CONSTRAINT [CK_ExperimentPhase_Order]
GO
ALTER TABLE [dbo].[HumanResourceProfile]  WITH CHECK ADD  CONSTRAINT [CK_HumanResourceProfile_current_workload] CHECK  (([current_workload]>=(0)))
GO
ALTER TABLE [dbo].[HumanResourceProfile] CHECK CONSTRAINT [CK_HumanResourceProfile_current_workload]
GO
ALTER TABLE [dbo].[HumanResourceProfile]  WITH CHECK ADD  CONSTRAINT [CK_HumanResourceProfile_max_working_hours] CHECK  (([max_working_hours_per_day]>(0)))
GO
ALTER TABLE [dbo].[HumanResourceProfile] CHECK CONSTRAINT [CK_HumanResourceProfile_max_working_hours]
GO
ALTER TABLE [dbo].[LandResource]  WITH CHECK ADD  CONSTRAINT [CK_LandResource_area_size] CHECK  (([area_size]>(0)))
GO
ALTER TABLE [dbo].[LandResource] CHECK CONSTRAINT [CK_LandResource_area_size]
GO
ALTER TABLE [dbo].[PhaseEquipmentRequirement]  WITH CHECK ADD  CONSTRAINT [CK_PhaseEquipmentRequirement_quantity] CHECK  (([quantity]>(0)))
GO
ALTER TABLE [dbo].[PhaseEquipmentRequirement] CHECK CONSTRAINT [CK_PhaseEquipmentRequirement_quantity]
GO
ALTER TABLE [dbo].[PhaseHumanRequirement]  WITH CHECK ADD  CONSTRAINT [CK_PhaseHumanRequirement_quantity] CHECK  (([quantity]>(0)))
GO
ALTER TABLE [dbo].[PhaseHumanRequirement] CHECK CONSTRAINT [CK_PhaseHumanRequirement_quantity]
GO
ALTER TABLE [dbo].[Schedule]  WITH CHECK ADD  CONSTRAINT [CK_Schedule_DateRange] CHECK  (([end_date]>=[start_date]))
GO
ALTER TABLE [dbo].[Schedule] CHECK CONSTRAINT [CK_Schedule_DateRange]
GO
ALTER TABLE [dbo].[Schedule]  WITH CHECK ADD  CONSTRAINT [CK_Schedule_Priority] CHECK  (([priority]>=(1) AND [priority]<=(4)))
GO
ALTER TABLE [dbo].[Schedule] CHECK CONSTRAINT [CK_Schedule_Priority]
GO

PRINT N'Schema ForestryResourcePlanningDB đã được tạo thành công trên db54885.';
GO
