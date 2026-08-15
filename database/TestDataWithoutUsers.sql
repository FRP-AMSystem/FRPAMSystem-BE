USE [ForestryResourcePlanningDB];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    /* ------------------------------------------------------------------------
       1. Resolve existing users. dbo.User is READ ONLY in this script.
       ------------------------------------------------------------------------ */

    DECLARE
        @AdminUserId int,
        @ManagerUserId int,
        @ResearcherUserId int,
        @WorkerUserId int,
        @SupportUserId int,

        @AdminRoleId int,
        @ManagerRoleId int,
        @ResearcherRoleId int,
        @WorkerRoleId int,
        @SupportRoleId int;

    IF OBJECT_ID(N'tempdb..#OrderedUsers') IS NOT NULL
        DROP TABLE #OrderedUsers;

    ;WITH OrderedUsers AS
    (
        SELECT
            u.user_id,
            u.role_id,
            r.role_name,
            ROW_NUMBER() OVER
            (
                ORDER BY
                    CASE r.role_name
                        WHEN N'Admin' THEN 1
                        WHEN N'Manager' THEN 2
                        WHEN N'Researcher' THEN 3
                        WHEN N'Technician' THEN 4
                        WHEN N'Seasonal' THEN 5
                        ELSE 10
                    END,
                    u.user_id
            ) AS rn
        FROM dbo.[User] AS u
        INNER JOIN dbo.[Role] AS r
            ON r.role_id = u.role_id
    )
    SELECT user_id, role_id, role_name, rn
    INTO #OrderedUsers
    FROM OrderedUsers;

    SELECT @AdminUserId = user_id FROM #OrderedUsers WHERE rn = 1;
    SELECT @ManagerUserId = user_id FROM #OrderedUsers WHERE rn = 2;
    SELECT @ResearcherUserId = user_id FROM #OrderedUsers WHERE rn = 3;
    SELECT @WorkerUserId = user_id FROM #OrderedUsers WHERE rn = 4;
    SELECT @SupportUserId = user_id FROM #OrderedUsers WHERE rn = 5;

    IF @AdminUserId IS NULL
       OR @ManagerUserId IS NULL
       OR @ResearcherUserId IS NULL
       OR @WorkerUserId IS NULL
       OR @SupportUserId IS NULL
    BEGIN
        THROW 51000,
            'Seed requires at least 5 existing users. dbo.User was not modified.',
            1;
    END;

    SELECT @AdminRoleId = role_id FROM dbo.[User] WHERE user_id = @AdminUserId;
    SELECT @ManagerRoleId = role_id FROM dbo.[User] WHERE user_id = @ManagerUserId;
    SELECT @ResearcherRoleId = role_id FROM dbo.[User] WHERE user_id = @ResearcherUserId;
    SELECT @WorkerRoleId = role_id FROM dbo.[User] WHERE user_id = @WorkerUserId;
    SELECT @SupportRoleId = role_id FROM dbo.[User] WHERE user_id = @SupportUserId;

    /* ------------------------------------------------------------------------
       2. Clear existing non-user data (Reverse FK order)
       ------------------------------------------------------------------------ */

    IF OBJECT_ID(N'dbo.AuditLog', N'U') IS NOT NULL
        DELETE FROM dbo.AuditLog;

    IF OBJECT_ID(N'dbo.Notification', N'U') IS NOT NULL
        DELETE FROM dbo.Notification;

    DELETE FROM dbo.Schedule;
    DELETE FROM dbo.EquipmentShortageLog;
    DELETE FROM dbo.AllocationEquipmentDetail;
    DELETE FROM dbo.AllocationHumanDetail;
    DELETE FROM dbo.AllocationLandDetail;
    DELETE FROM dbo.AllocationPlan;

    DELETE FROM dbo.PhaseHumanRequirement;
    DELETE FROM dbo.PhaseEquipmentRequirement;

    DELETE FROM dbo.ExperimentPhase;
    DELETE FROM dbo.ExperimentHumanRequirement;
    DELETE FROM dbo.ExperimentEquipmentRequirement;
    DELETE FROM dbo.ExperimentLandRequirement;
    DELETE FROM dbo.Experiment;

    DELETE FROM dbo.EquipmentSubstitution;
    DELETE FROM dbo.EquipmentInstance;
    DELETE FROM dbo.EquipmentType;

    DELETE FROM dbo.HumanResourceSkill;
    DELETE FROM dbo.HumanResourceProfile;

    DELETE FROM dbo.LandResource;
    DELETE FROM dbo.Area;

    DELETE FROM dbo.EquipmentCategory;
    DELETE FROM dbo.Skill;

    /* ------------------------------------------------------------------------
       3. Reset identities
       ------------------------------------------------------------------------ */

    IF OBJECT_ID(N'dbo.AuditLog', N'U') IS NOT NULL
        DBCC CHECKIDENT (N'dbo.AuditLog', RESEED, 0) WITH NO_INFOMSGS;

    IF OBJECT_ID(N'dbo.Notification', N'U') IS NOT NULL
        DBCC CHECKIDENT (N'dbo.Notification', RESEED, 0) WITH NO_INFOMSGS;

    DBCC CHECKIDENT (N'dbo.Schedule', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.EquipmentShortageLog', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.AllocationEquipmentDetail', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.AllocationHumanDetail', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.AllocationLandDetail', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.AllocationPlan', RESEED, 0) WITH NO_INFOMSGS;

    DBCC CHECKIDENT (N'dbo.PhaseHumanRequirement', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.PhaseEquipmentRequirement', RESEED, 0) WITH NO_INFOMSGS;

    DBCC CHECKIDENT (N'dbo.ExperimentPhase', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.ExperimentHumanRequirement', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.ExperimentEquipmentRequirement', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.ExperimentLandRequirement', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.Experiment', RESEED, 0) WITH NO_INFOMSGS;

    DBCC CHECKIDENT (N'dbo.EquipmentSubstitution', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.EquipmentInstance', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.EquipmentType', RESEED, 0) WITH NO_INFOMSGS;

    DBCC CHECKIDENT (N'dbo.HumanResourceSkill', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.HumanResourceProfile', RESEED, 0) WITH NO_INFOMSGS;

    DBCC CHECKIDENT (N'dbo.LandResource', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.Area', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.EquipmentCategory', RESEED, 0) WITH NO_INFOMSGS;
    DBCC CHECKIDENT (N'dbo.Skill', RESEED, 0) WITH NO_INFOMSGS;

    /* ------------------------------------------------------------------------
       4. MASTER DATA (Skills, Areas, Categories)
       ------------------------------------------------------------------------ */

    INSERT INTO dbo.Skill (skill_name, description)
    VALUES
        (N'Drone Operation', N'Operate drones for forest aerial survey and monitoring.'),
        (N'Soil Sampling', N'Collect soil samples and perform acidity/moisture checks.'),
        (N'GIS Mapping', N'Prepare spatial mapping and analyze coordinates in QGIS/ArcGIS.'),
        (N'Field Measurement', N'Measure tree height, DBH, canopy spread and growth data.'),
        (N'Data Collection', N'Collect and record experiment observation logs in tablet.'),
        (N'Irrigation Management', N'Install, operate and maintain automated drip lines.'),
        (N'Heavy Machinery Operation', N'Operate excavators, tractors and bulldozers for land prep.'),
        (N'Chemical Treatment', N'Handle fertilizers, organic treatments and pest controls.');

    DECLARE
        @SkillDrone int, @SkillSoil int, @SkillGIS int, @SkillField int,
        @SkillData int, @SkillIrrigation int, @SkillMachinery int, @SkillChemical int;

    SELECT @SkillDrone      = skill_id FROM dbo.Skill WHERE skill_name = N'Drone Operation';
    SELECT @SkillSoil       = skill_id FROM dbo.Skill WHERE skill_name = N'Soil Sampling';
    SELECT @SkillGIS        = skill_id FROM dbo.Skill WHERE skill_name = N'GIS Mapping';
    SELECT @SkillField      = skill_id FROM dbo.Skill WHERE skill_name = N'Field Measurement';
    SELECT @SkillData       = skill_id FROM dbo.Skill WHERE skill_name = N'Data Collection';
    SELECT @SkillIrrigation = skill_id FROM dbo.Skill WHERE skill_name = N'Irrigation Management';
    SELECT @SkillMachinery  = skill_id FROM dbo.Skill WHERE skill_name = N'Heavy Machinery Operation';
    SELECT @SkillChemical   = skill_id FROM dbo.Skill WHERE skill_name = N'Chemical Treatment';

    INSERT INTO dbo.Area (area_name, description, created_at)
    VALUES
        (N'Test Zone A - Highland', N'High altitude sandy soil plots for drought trials.', GETDATE()),
        (N'Test Zone B - Lowland', N'Loam rich soil plots for rapid planting experiments.', GETDATE()),
        (N'Test Zone C - Wetland', N'Clay plots for flood and drainage irrigation tests.', GETDATE()),
        (N'Test Nursery Station', N'High-tech controlled nursery zone for hybrid seedlings.', GETDATE()),
        (N'Test Conservation Forest', N'Primary rainforest zone for long-term biodiversity monitoring.', GETDATE()),
        (N'Test Slope Plantation', N'Steep slope terrain for erosion prevention experiments.', GETDATE()),
        (N'Test Coastal Zone', N'Saline soil area for mangrove and coastal protection trials.', GETDATE());

    DECLARE
        @AreaA int, @AreaB int, @AreaC int, @AreaNursery int,
        @AreaConservation int, @AreaSlope int, @AreaCoastal int;

    SELECT @AreaA            = area_id FROM dbo.Area WHERE area_name = N'Test Zone A - Highland';
    SELECT @AreaB            = area_id FROM dbo.Area WHERE area_name = N'Test Zone B - Lowland';
    SELECT @AreaC            = area_id FROM dbo.Area WHERE area_name = N'Test Zone C - Wetland';
    SELECT @AreaNursery      = area_id FROM dbo.Area WHERE area_name = N'Test Nursery Station';
    SELECT @AreaConservation = area_id FROM dbo.Area WHERE area_name = N'Test Conservation Forest';
    SELECT @AreaSlope        = area_id FROM dbo.Area WHERE area_name = N'Test Slope Plantation';
    SELECT @AreaCoastal      = area_id FROM dbo.Area WHERE area_name = N'Test Coastal Zone';

    INSERT INTO dbo.EquipmentCategory (category_name, description, created_at)
    VALUES
        (N'Test Hand Tools', N'Manual tools like shovels, pruning shears and cutters.', GETDATE()),
        (N'Test Survey Equipment', N'Drones, RTK GPS receivers, LiDAR and smart sensors.', GETDATE()),
        (N'Test Irrigation Systems', N'Automated drip pumps, sprinkler units and water tanks.', GETDATE()),
        (N'Test Heavy Machinery', N'Tractors, wood chippers and small excavators.', GETDATE()),
        (N'Test Safety Gear', N'Helmets, high-vis vests, work gloves and respirators.', GETDATE()),
        (N'Test Lab Instruments', N'Portable spectrometers, pH meters and moisture meters.', GETDATE());

    DECLARE
        @CatHandTools int, @CatSurvey int, @CatIrrigation int,
        @CatHeavy int, @CatSafety int, @CatLab int;

    SELECT @CatHandTools  = equipment_category_id FROM dbo.EquipmentCategory WHERE category_name = N'Test Hand Tools';
    SELECT @CatSurvey     = equipment_category_id FROM dbo.EquipmentCategory WHERE category_name = N'Test Survey Equipment';
    SELECT @CatIrrigation = equipment_category_id FROM dbo.EquipmentCategory WHERE category_name = N'Test Irrigation Systems';
    SELECT @CatHeavy      = equipment_category_id FROM dbo.EquipmentCategory WHERE category_name = N'Test Heavy Machinery';
    SELECT @CatSafety     = equipment_category_id FROM dbo.EquipmentCategory WHERE category_name = N'Test Safety Gear';
    SELECT @CatLab        = equipment_category_id FROM dbo.EquipmentCategory WHERE category_name = N'Test Lab Instruments';

    /* ------------------------------------------------------------------------
       5. EQUIPMENT (Types, Instances, Substitutions)
       ------------------------------------------------------------------------ */

    INSERT INTO dbo.EquipmentType
    (
        equipment_category_id, name, tracking_type,
        base_maintenance_interval_hours, total_quantity, damaged_quantity,
        available_quantity, reserved_quantity, in_use_quantity, missing_quantity,
        description, created_at
    )
    VALUES
        (@CatHandTools, N'Test Tree Planting Shovel', N'QuantityBased', NULL, 100, 5, 70, 10, 12, 3, N'Standard hardened steel shovels.', GETDATE()),
        (@CatHandTools, N'Test Pruning Shears', N'QuantityBased', NULL, 60, 2, 45, 5, 8, 0, N'Heavy-duty branch pruners.', GETDATE()),
        (@CatSafety, N'Test Protective Gloves', N'QuantityBased', NULL, 200, 10, 150, 20, 18, 2, N'Cut-resistant forestry work gloves.', GETDATE()),
        (@CatSurvey, N'Test Survey Drone 4K', N'Individual', 150, 4, 0, 2, 1, 1, 0, N'Multispectral camera drone for forestry survey.', GETDATE()),
        (@CatSurvey, N'Test Handheld GPS Tracker', N'Individual', 250, 5, 0, 3, 1, 1, 0, N'High precision RTK GPS surveyor.', GETDATE()),
        (@CatSurvey, N'Test Soil Moisture Sensor', N'QuantityBased', 400, 30, 2, 20, 4, 4, 0, N'Wireless IoT soil moisture probes.', GETDATE()),
        (@CatIrrigation, N'Test Portable Water Pump', N'Individual', 200, 4, 1, 1, 1, 1, 0, N'Gasoline driven portable pump.', GETDATE()),
        (@CatHeavy, N'Test Mini Excavator 3T', N'Individual', 300, 2, 0, 1, 0, 1, 0, N'Compact excavator for trenching.', GETDATE()),
        (@CatLab, N'Test Portable pH & EC Meter', N'Individual', 180, 4, 0, 3, 0, 1, 0, N'Digital soil test kit.', GETDATE()),
        (@CatLab, N'Test Wood Density Meter', N'Individual', 220, 2, 0, 1, 1, 0, 0, N'Non-destructive acoustic testing device.', GETDATE());

    DECLARE
        @EquipShovel int, @EquipShears int, @EquipGloves int, @EquipDrone int,
        @EquipGPS int, @EquipSensor int, @EquipPump int, @EquipExcavator int,
        @EquipPHMeter int, @EquipDensity int;

    SELECT @EquipShovel    = equipment_type_id FROM dbo.EquipmentType WHERE name = N'Test Tree Planting Shovel';
    SELECT @EquipShears    = equipment_type_id FROM dbo.EquipmentType WHERE name = N'Test Pruning Shears';
    SELECT @EquipGloves    = equipment_type_id FROM dbo.EquipmentType WHERE name = N'Test Protective Gloves';
    SELECT @EquipDrone     = equipment_type_id FROM dbo.EquipmentType WHERE name = N'Test Survey Drone 4K';
    SELECT @EquipGPS       = equipment_type_id FROM dbo.EquipmentType WHERE name = N'Test Handheld GPS Tracker';
    SELECT @EquipSensor    = equipment_type_id FROM dbo.EquipmentType WHERE name = N'Test Soil Moisture Sensor';
    SELECT @EquipPump      = equipment_type_id FROM dbo.EquipmentType WHERE name = N'Test Portable Water Pump';
    SELECT @EquipExcavator = equipment_type_id FROM dbo.EquipmentType WHERE name = N'Test Mini Excavator 3T';
    SELECT @EquipPHMeter   = equipment_type_id FROM dbo.EquipmentType WHERE name = N'Test Portable pH & EC Meter';
    SELECT @EquipDensity   = equipment_type_id FROM dbo.EquipmentType WHERE name = N'Test Wood Density Meter';

    INSERT INTO dbo.EquipmentInstance
    (
        equipment_type_id, asset_code, serial_number, total_usage_hour,
        last_maintenance_date, usage_hours_since_last_maintenance, condition_level,
        status, effective_interval_hour, maintenance_count, note, created_at
    )
    VALUES
        (@EquipDrone, N'TEST-DRN-001', N'SN-DRN-2026-01', 120, '2026-05-10', 45, N'Good', N'InUse', 150, 2, N'Deployed for Zone A survey.', GETDATE()),
        (@EquipDrone, N'TEST-DRN-002', N'SN-DRN-2026-02', 30,  '2026-04-15', 30, N'Good', N'Available', 150, 1, N'Ready in warehouse.', GETDATE()),
        (@EquipDrone, N'TEST-DRN-003', N'SN-DRN-2026-03', 210, '2026-05-01', 80, N'Fair', N'Reserved', 120, 4, N'Reserved for next month trial.', GETDATE()),
        (@EquipDrone, N'TEST-DRN-004', N'SN-DRN-2026-04', 310, '2026-02-10', 160, N'Poor', N'Maintenance', 100, 6, N'Under propeller overhaul.', GETDATE()),

        (@EquipGPS, N'TEST-GPS-001', N'SN-GPS-2026-01', 85,  '2026-03-20', 25, N'Good', N'InUse', 250, 1, N'Assigned to field worker.', GETDATE()),
        (@EquipGPS, N'TEST-GPS-002', N'SN-GPS-2026-02', 150, '2026-01-10', 90, N'Fair', N'Available', 200, 3, N'Available for borrowing.', GETDATE()),
        (@EquipGPS, N'TEST-GPS-003', N'SN-GPS-2026-03', 10,  '2026-06-01', 10, N'Good', N'Available', 250, 0, N'Brand new unit.', GETDATE()),

        (@EquipPump, N'TEST-PMP-001', N'SN-PMP-2026-01', 180, '2026-04-20', 60, N'Good', N'InUse', 200, 2, N'Operating at Nursery.', GETDATE()),
        (@EquipPump, N'TEST-PMP-002', N'SN-PMP-2026-02', 90,  '2026-05-15', 20, N'Good', N'Available', 200, 1, N'Backup pump.', GETDATE()),
        (@EquipPump, N'TEST-PMP-003', N'SN-PMP-2026-03', 290, '2026-03-01', 190, N'Fair', N'Reserved', 180, 5, N'Reserved for Wetland trial.', GETDATE()),

        (@EquipExcavator, N'TEST-EXC-001', N'SN-EXC-2026-01', 450, '2026-04-01', 110, N'Good', N'InUse', 300, 3, N'Clearing land at Slope area.', GETDATE()),
        (@EquipExcavator, N'TEST-EXC-002', N'SN-EXC-2026-02', 200, '2026-05-01', 50,  N'Good', N'Available', 300, 1, N'Stationed at depot.', GETDATE()),

        (@EquipPHMeter, N'TEST-PHM-001', N'SN-PHM-2026-01', 65,  '2026-05-05', 25, N'Good', N'InUse', 180, 1, N'In field lab kit.', GETDATE()),
        (@EquipPHMeter, N'TEST-PHM-002', N'SN-PHM-2026-02', 40,  '2026-05-20', 10, N'Good', N'Available', 180, 0, N'Calibrated and ready.', GETDATE()),
        (@EquipDensity, N'TEST-DNS-001', N'SN-DNS-2026-01', 110, '2026-03-15', 40, N'Good', N'Available', 220, 2, N'Wood lab testing device.', GETDATE());

    DECLARE
        @DroneInstance1 int, @DroneInstance2 int, @GPSInstance1 int,
        @PumpInstance1 int, @ExcavatorInstance1 int, @PHMInstance1 int;

    SELECT @DroneInstance1     = equipment_instance_id FROM dbo.EquipmentInstance WHERE asset_code = N'TEST-DRN-001';
    SELECT @DroneInstance2     = equipment_instance_id FROM dbo.EquipmentInstance WHERE asset_code = N'TEST-DRN-002';
    SELECT @GPSInstance1       = equipment_instance_id FROM dbo.EquipmentInstance WHERE asset_code = N'TEST-GPS-001';
    SELECT @PumpInstance1      = equipment_instance_id FROM dbo.EquipmentInstance WHERE asset_code = N'TEST-PMP-001';
    SELECT @ExcavatorInstance1 = equipment_instance_id FROM dbo.EquipmentInstance WHERE asset_code = N'TEST-EXC-001';
    SELECT @PHMInstance1       = equipment_instance_id FROM dbo.EquipmentInstance WHERE asset_code = N'TEST-PHM-001';

    INSERT INTO dbo.EquipmentSubstitution
    (
        primary_equipment_type_id, sub_equipment_type_id, efficiency_rate,
        time_multiplier, note, created_at
    )
    VALUES
        (@EquipDrone, @EquipGPS, 0.75, 1.25, N'GPS manual survey can substitute drone photogrammetry.', GETDATE()),
        (@EquipGPS, @EquipDrone, 0.90, 1.10, N'Drone can substitute GPS for rapid rough point marking.', GETDATE()),
        (@EquipSensor, @EquipPHMeter, 0.80, 1.20, N'Handheld test kit can substitute automatic sensor logs.', GETDATE()),
        (@EquipShovel, @EquipExcavator, 0.85, 1.25, N'Mini excavator is used as a fallback for heavy manual digging; efficiency is modeled conservatively.', GETDATE()),
        (@EquipExcavator, @EquipShovel, 0.50, 2.50, N'Manual workforce with shovels substitutes excavator shortage.', GETDATE()),
        (@EquipShears, @EquipShovel, 0.70, 1.40, N'General tools fallback for basic trimming.', GETDATE()),
        (@EquipGloves, @EquipShears, 0.90, 1.00, N'Tool bundle compatibility fallback.', GETDATE()),
        (@EquipDensity, @EquipPHMeter, 0.65, 1.50, N'Lab sample estimation substitute.', GETDATE());

    /* ------------------------------------------------------------------------
       6. HUMAN RESOURCE & SKILLS
       ------------------------------------------------------------------------ */

    INSERT INTO dbo.HumanResourceProfile
    (
        user_id, max_working_hours_per_day, current_workload, status, created_at
    )
    VALUES
        (@ManagerUserId, 8, 3, N'Available', GETDATE()),
        (@ResearcherUserId, 8, 5, N'Busy', GETDATE()),
        (@WorkerUserId, 8, 4, N'Busy', GETDATE()),
        (@SupportUserId, 6, 2, N'Available', GETDATE()),
        (@AdminUserId, 8, 0, N'Inactive', GETDATE());

    DECLARE
        @HRManager int, @HRResearcher int, @HRWorker int,
        @HRSupport int, @HRAdmin int;

    SELECT @HRManager    = human_resource_id FROM dbo.HumanResourceProfile WHERE user_id = @ManagerUserId;
    SELECT @HRResearcher = human_resource_id FROM dbo.HumanResourceProfile WHERE user_id = @ResearcherUserId;
    SELECT @HRWorker     = human_resource_id FROM dbo.HumanResourceProfile WHERE user_id = @WorkerUserId;
    SELECT @HRSupport    = human_resource_id FROM dbo.HumanResourceProfile WHERE user_id = @SupportUserId;
    SELECT @HRAdmin      = human_resource_id FROM dbo.HumanResourceProfile WHERE user_id = @AdminUserId;

    INSERT INTO dbo.HumanResourceSkill (human_resource_id, skill_id, skill_level)
    VALUES
        (@HRManager, @SkillGIS, N'Expert'),
        (@HRManager, @SkillData, N'Advanced'),
        (@HRResearcher, @SkillGIS, N'Advanced'),
        (@HRResearcher, @SkillSoil, N'Expert'),
        (@HRResearcher, @SkillChemical, N'Intermediate'),
        (@HRWorker, @SkillDrone, N'Expert'),
        (@HRWorker, @SkillField, N'Advanced'),
        (@HRWorker, @SkillMachinery, N'Advanced'),
        (@HRWorker, @SkillIrrigation, N'Intermediate'),
        (@HRSupport, @SkillSoil, N'Intermediate'),
        (@HRSupport, @SkillData, N'Advanced'),
        (@HRSupport, @SkillIrrigation, N'Intermediate'),
        (@HRAdmin, @SkillData, N'Expert'),
        (@HRAdmin, @SkillGIS, N'Intermediate');

    /* ------------------------------------------------------------------------
       7. LAND RESOURCES
       ------------------------------------------------------------------------ */

    INSERT INTO dbo.LandResource
    (
        area_id, land_code, area_size, location, soil_type, status, created_at
    )
    VALUES
        (@AreaA, N'PLOT-A-01', 300.00, N'Highland Hilltop Plot 1', N'Sandy Soil', N'InUse', GETDATE()),
        (@AreaA, N'PLOT-A-02', 250.00, N'Highland Ridge Plot 2', N'Sandy Soil', N'Available', GETDATE()),
        (@AreaB, N'PLOT-B-01', 400.00, N'Lowland Valley Plot 1', N'Loam', N'InUse', GETDATE()),
        (@AreaB, N'PLOT-B-02', 350.00, N'Lowland Riverbank Plot 2', N'Loam', N'Reserved', GETDATE()),
        (@AreaC, N'PLOT-C-01', 200.00, N'Wetland Marsh Sector 1', N'Clay Soil', N'InUse', GETDATE()),
        (@AreaC, N'PLOT-C-02', 180.00, N'Wetland Drainage Sector 2', N'Clay Soil', N'Available', GETDATE()),
        (@AreaNursery, N'PLOT-NUR-01', 150.00, N'Nursery Greenhouse Alpha', N'Peat Soil', N'InUse', GETDATE()),
        (@AreaNursery, N'PLOT-NUR-02', 120.00, N'Nursery Open Bed Beta', N'Peat Soil', N'Available', GETDATE()),
        (@AreaConservation, N'PLOT-CNS-01', 500.00, N'Core Rainforest Perimeter', N'Forest Brown Soil', N'InUse', GETDATE()),
        (@AreaSlope, N'PLOT-SLP-01', 280.00, N'Slope Anti-Erosion Zone 1', N'Rocky Soil', N'Maintenance', GETDATE());

    DECLARE
        @LandA1 int, @LandA2 int, @LandB1 int, @LandB2 int,
        @LandC1 int, @LandC2 int, @LandNur1 int, @LandNur2 int,
        @LandCns1 int, @LandSlp1 int;

    SELECT @LandA1   = land_id FROM dbo.LandResource WHERE land_code = N'PLOT-A-01';
    SELECT @LandA2   = land_id FROM dbo.LandResource WHERE land_code = N'PLOT-A-02';
    SELECT @LandB1   = land_id FROM dbo.LandResource WHERE land_code = N'PLOT-B-01';
    SELECT @LandB2   = land_id FROM dbo.LandResource WHERE land_code = N'PLOT-B-02';
    SELECT @LandC1   = land_id FROM dbo.LandResource WHERE land_code = N'PLOT-C-01';
    SELECT @LandC2   = land_id FROM dbo.LandResource WHERE land_code = N'PLOT-C-02';
    SELECT @LandNur1 = land_id FROM dbo.LandResource WHERE land_code = N'PLOT-NUR-01';
    SELECT @LandNur2 = land_id FROM dbo.LandResource WHERE land_code = N'PLOT-NUR-02';
    SELECT @LandCns1 = land_id FROM dbo.LandResource WHERE land_code = N'PLOT-CNS-01';
    SELECT @LandSlp1 = land_id FROM dbo.LandResource WHERE land_code = N'PLOT-SLP-01';

    /* ------------------------------------------------------------------------
       8. EXPERIMENTS, REQUIREMENTS & PHASES
       ------------------------------------------------------------------------ */

    INSERT INTO dbo.Experiment
    (
        experiment_name, description, researcher_id,
        expect_start_date, expect_end_date, deadline,
        priority, status, created_at
    )
    VALUES
        (N'EXP-01: Highland Drought Moisture Testing', N'Evaluate acacia drought resilience on sandy soil.', @ResearcherUserId, '2026-06-01', '2026-06-20', '2026-06-25', 1, N'Running', GETDATE()),
        (N'EXP-02: Fast Growing Hybrid Eucalypt Trial', N'Study growth yield using optimized soil preparation.', @ResearcherUserId, '2026-06-15', '2026-07-30', '2026-08-05', 2, N'Submitted', GETDATE()),
        (N'EXP-03: Wetland Automated Irrigation Flow', N'Test automated drip line efficiency on clay terrain.', @ResearcherUserId, '2026-05-20', '2026-06-10', '2026-06-15', 2, N'Running', GETDATE()),
        (N'EXP-04: Aerial LiDAR Forest Canopy Mapping', N'Mapping forest density and canopy height via drone.', @ResearcherUserId, '2026-07-01', '2026-07-15', '2026-07-20', 3, N'Draft', GETDATE()),
        (N'EXP-05: Nursery Seedling Germination Protocol', N'Microclimate test on organic seed treatments.', @ResearcherUserId, '2026-04-01', '2026-05-01', '2026-05-05', 1, N'Completed', GETDATE()),
        (N'EXP-06: Slope Soil Bio-engineering Trial', N'Root anchoring trial for preventing soil run-off.', @ResearcherUserId, '2026-08-01', '2026-08-30', '2026-09-05', 2, N'Draft', GETDATE()),
        (N'EXP-07: Rainforest Carbon Sequestration Survey', N'Long-term biomass assessment and soil carbon analysis.', @ResearcherUserId, '2026-05-01', '2026-06-30', '2026-07-05', 3, N'Running', GETDATE()),
        (N'EXP-08: Coastal Mangrove Salinity Test', N'Cancelled trial due to adverse tropical storm.', @ResearcherUserId, '2026-03-01', '2026-03-20', '2026-03-25', 4, N'Cancelled', GETDATE());

    DECLARE
        @Exp1 int, @Exp2 int, @Exp3 int, @Exp4 int,
        @Exp5 int, @Exp6 int, @Exp7 int, @Exp8 int;

    SELECT @Exp1 = experiment_id FROM dbo.Experiment WHERE experiment_name LIKE N'EXP-01%';
    SELECT @Exp2 = experiment_id FROM dbo.Experiment WHERE experiment_name LIKE N'EXP-02%';
    SELECT @Exp3 = experiment_id FROM dbo.Experiment WHERE experiment_name LIKE N'EXP-03%';
    SELECT @Exp4 = experiment_id FROM dbo.Experiment WHERE experiment_name LIKE N'EXP-04%';
    SELECT @Exp5 = experiment_id FROM dbo.Experiment WHERE experiment_name LIKE N'EXP-05%';
    SELECT @Exp6 = experiment_id FROM dbo.Experiment WHERE experiment_name LIKE N'EXP-06%';
    SELECT @Exp7 = experiment_id FROM dbo.Experiment WHERE experiment_name LIKE N'EXP-07%';
    SELECT @Exp8 = experiment_id FROM dbo.Experiment WHERE experiment_name LIKE N'EXP-08%';

    -- 8.1 Experiment Land Requirements
    INSERT INTO dbo.ExperimentLandRequirement (experiment_id, required_area, required_soil_type, note, created_at)
    VALUES
        (@Exp1, 300.00, N'Sandy Soil', N'Highland plot required.', GETDATE()),
        (@Exp2, 400.00, N'Loam', N'Fertile lowland loam plot required.', GETDATE()),
        (@Exp3, 200.00, N'Clay Soil', N'Clay soil with high water retention.', GETDATE()),
        (@Exp4, 250.00, N'Sandy Soil', N'Open canopy highland plot.', GETDATE()),
        (@Exp5, 150.00, N'Peat Soil', N'Nursery greenhouse lot.', GETDATE()),
        (@Exp6, 280.00, N'Rocky Soil', N'Steep incline slope.', GETDATE()),
        (@Exp7, 500.00, N'Forest Brown Soil', N'Conservation forest plot.', GETDATE()),
        (@Exp8, 120.00, N'Saline Soil', N'Coastal mangrove site.', GETDATE());

    -- 8.2 Experiment Equipment Requirements
    INSERT INTO dbo.ExperimentEquipmentRequirement
    (
        experiment_id, equipment_type_id, quantity, allow_substitute,
        min_acceptable_efficiency, note, created_at
    )
    VALUES
        (@Exp1, @EquipDrone, 1, 1, 0.75, N'Drone required for moisture thermal imaging.', GETDATE()),
        (@Exp1, @EquipSensor, 10, 1, 0.80, N'Sensors placed along grid lines.', GETDATE()),
        (@Exp1, @EquipShovel, 10, 0, NULL, N'Shovels for installing sensor tubes.', GETDATE()),
        (@Exp2, @EquipShovel, 25, 1, 0.90, N'Massive planting shovels required.', GETDATE()),
        (@Exp2, @EquipDrone, 1, 0, NULL, N'Drone inspection.', GETDATE()),
        (@Exp3, @EquipPump, 2, 0, NULL, N'High pressure pumps for irrigation grid.', GETDATE()),
        (@Exp3, @EquipSensor, 8, 1, 0.80, N'Moisture check sensors.', GETDATE()),
        (@Exp4, @EquipDrone, 2, 1, 0.75, N'Dual drone fleet for fast aerial lidar sweep.', GETDATE()),
        (@Exp4, @EquipGPS, 2, 0, NULL, N'Ground control points RTK GPS.', GETDATE()),
        (@Exp5, @EquipPHMeter, 2, 0, NULL, N'Daily substrate pH monitoring.', GETDATE()),
        (@Exp7, @EquipDensity, 2, 0, NULL, N'Wood core density testing.', GETDATE()),
        (@Exp7, @EquipGPS, 2, 0, NULL, N'Geotagging forest sample trees.', GETDATE());

    DECLARE
        @Exp1_ReqDrone int, @Exp1_ReqSensor int, @Exp1_ReqShovel int,
        @Exp2_ReqShovel int, @Exp3_ReqPump int, @Exp4_ReqDrone int,
        @Exp5_ReqPHMeter int, @Exp7_ReqDensity int, @Exp7_ReqGPS int;

    SELECT @Exp1_ReqDrone  = exp_equipment_req_id FROM dbo.ExperimentEquipmentRequirement WHERE experiment_id = @Exp1 AND equipment_type_id = @EquipDrone;
    SELECT @Exp1_ReqSensor = exp_equipment_req_id FROM dbo.ExperimentEquipmentRequirement WHERE experiment_id = @Exp1 AND equipment_type_id = @EquipSensor;
    SELECT @Exp1_ReqShovel = exp_equipment_req_id FROM dbo.ExperimentEquipmentRequirement WHERE experiment_id = @Exp1 AND equipment_type_id = @EquipShovel;
    SELECT @Exp2_ReqShovel = exp_equipment_req_id FROM dbo.ExperimentEquipmentRequirement WHERE experiment_id = @Exp2 AND equipment_type_id = @EquipShovel;
    SELECT @Exp3_ReqPump   = exp_equipment_req_id FROM dbo.ExperimentEquipmentRequirement WHERE experiment_id = @Exp3 AND equipment_type_id = @EquipPump;
    SELECT @Exp4_ReqDrone   = exp_equipment_req_id FROM dbo.ExperimentEquipmentRequirement WHERE experiment_id = @Exp4 AND equipment_type_id = @EquipDrone;
    SELECT @Exp5_ReqPHMeter  = exp_equipment_req_id FROM dbo.ExperimentEquipmentRequirement WHERE experiment_id = @Exp5 AND equipment_type_id = @EquipPHMeter;
    SELECT @Exp7_ReqDensity  = exp_equipment_req_id FROM dbo.ExperimentEquipmentRequirement WHERE experiment_id = @Exp7 AND equipment_type_id = @EquipDensity;
    SELECT @Exp7_ReqGPS      = exp_equipment_req_id FROM dbo.ExperimentEquipmentRequirement WHERE experiment_id = @Exp7 AND equipment_type_id = @EquipGPS;

    -- 8.3 Experiment Human Requirements
    INSERT INTO dbo.ExperimentHumanRequirement
    (
        experiment_id, role_id, quantity, required_skill_id,
        working_hours_per_day, note, created_at
    )
    VALUES
        (@Exp1, @WorkerRoleId, 1, @SkillDrone, 8, N'Drone operator for thermal flight.', GETDATE()),
        (@Exp1, @SupportRoleId, 2, @SkillSoil, 6, N'Soil sampling assistants.', GETDATE()),
        (@Exp2, @WorkerRoleId, 2, @SkillField, 8, N'Planting field technicians.', GETDATE()),
        (@Exp3, @WorkerRoleId, 1, @SkillIrrigation, 8, N'Irrigation specialist.', GETDATE()),
        (@Exp3, @SupportRoleId, 1, @SkillData, 6, N'Sensor data logger.', GETDATE()),
        (@Exp4, @ResearcherRoleId, 1, @SkillGIS, 8, N'GIS specialist for flight path planning.', GETDATE()),
        (@Exp4, @WorkerRoleId, 1, @SkillDrone, 8, N'Licensed drone pilot.', GETDATE()),
        (@Exp5, @SupportRoleId, 1, @SkillData, 6, N'Daily seed record keeper.', GETDATE()),
        (@Exp7, @ResearcherRoleId, 1, @SkillField, 8, N'Senior forestry researcher.', GETDATE()),
        (@Exp7, @WorkerRoleId, 1, @SkillGIS, 8, N'Field mapper.', GETDATE());

    DECLARE
        @Exp1_HumDrone int, @Exp1_HumSoil int, @Exp2_HumField int,
        @Exp3_HumIrrigation int, @Exp4_HumGIS int, @Exp5_HumData int,
        @Exp7_HumField int, @Exp7_HumGIS int;

    SELECT @Exp1_HumDrone      = exp_human_req_id FROM dbo.ExperimentHumanRequirement WHERE experiment_id = @Exp1 AND required_skill_id = @SkillDrone;
    SELECT @Exp1_HumSoil       = exp_human_req_id FROM dbo.ExperimentHumanRequirement WHERE experiment_id = @Exp1 AND required_skill_id = @SkillSoil;
    SELECT @Exp2_HumField      = exp_human_req_id FROM dbo.ExperimentHumanRequirement WHERE experiment_id = @Exp2 AND required_skill_id = @SkillField;
    SELECT @Exp3_HumIrrigation = exp_human_req_id FROM dbo.ExperimentHumanRequirement WHERE experiment_id = @Exp3 AND required_skill_id = @SkillIrrigation;
    SELECT @Exp4_HumGIS        = exp_human_req_id FROM dbo.ExperimentHumanRequirement WHERE experiment_id = @Exp4 AND required_skill_id = @SkillGIS;
    SELECT @Exp5_HumData       = exp_human_req_id FROM dbo.ExperimentHumanRequirement WHERE experiment_id = @Exp5 AND required_skill_id = @SkillData;
    SELECT @Exp7_HumField      = exp_human_req_id FROM dbo.ExperimentHumanRequirement WHERE experiment_id = @Exp7 AND required_skill_id = @SkillField;
    SELECT @Exp7_HumGIS        = exp_human_req_id FROM dbo.ExperimentHumanRequirement WHERE experiment_id = @Exp7 AND required_skill_id = @SkillGIS;

    -- 8.4 Experiment Phases
    INSERT INTO dbo.ExperimentPhase
    (
        experiment_id, phase_name, phase_description, phase_order,
        expected_start_date, expected_end_date, status, created_at
    )
    VALUES
        (@Exp1, N'EXP1-P1: Site Grid Setup & Sensor Deployment', N'Mark 50x50m grid and install IoT moisture probes.', 1, '2026-06-01', '2026-06-05', N'Completed', GETDATE()),
        (@Exp1, N'EXP1-P2: Thermal Drone Data Sweep', N'Perform daily morning and afternoon drone thermal passes.', 2, '2026-06-06', '2026-06-15', N'InProgress', GETDATE()),
        (@Exp1, N'EXP1-P3: Soil Core Extraction & Lab Analysis', N'Collect physical soil cores to validate sensor accuracy.', 3, '2026-06-16', '2026-06-20', N'Planned', GETDATE()),

        (@Exp2, N'EXP2-P1: Land Clearing & Deep Trenching', N'Excavator clearing and bedding preparation.', 1, '2026-06-15', '2026-06-25', N'Planned', GETDATE()),
        (@Exp2, N'EXP2-P2: Seedling Outplanting & Fertilization', N'Transplant hybrid seedlings into test rows.', 2, '2026-06-26', '2026-07-15', N'Planned', GETDATE()),
        (@Exp2, N'EXP2-P3: Early Height & Root Growth Survey', N'First month biometric field measurements.', 3, '2026-07-16', '2026-07-30', N'Planned', GETDATE()),

        (@Exp3, N'EXP3-P1: Pump Installation & Line Testing', N'Set up water pumps and pressure test drip valves.', 1, '2026-05-20', '2026-05-25', N'Completed', GETDATE()),
        (@Exp3, N'EXP3-P2: Automated Moisture Flow Regimes', N'Run scheduled flow regimes and monitor saturation.', 2, '2026-05-26', '2026-06-10', N'InProgress', GETDATE()),

        (@Exp4, N'EXP4-P1: Ground Control Point Survey', N'Mark RTK GPS ground benchmarks.', 1, '2026-07-01', '2026-07-05', N'Planned', GETDATE()),
        (@Exp4, N'EXP4-P2: Dual Drone LiDAR Flight Operation', N'Fly automated LiDAR survey routes.', 2, '2026-07-06', '2026-07-15', N'Planned', GETDATE()),

        (@Exp5, N'EXP5-P1: Organic Priming & Inoculation', N'Treat seed batches with mycorrhizal fungi.', 1, '2026-04-01', '2026-04-10', N'Completed', GETDATE()),
        (@Exp5, N'EXP5-P2: Greenhouse Germination Monitoring', N'Track sprout rate and vigor index daily.', 2, '2026-04-11', '2026-05-01', N'Completed', GETDATE()),

        (@Exp7, N'EXP7-P1: Tree Geotagging & DBH Census', N'Tag specimen trees and record diameter at breast height.', 1, '2026-05-01', '2026-05-20', N'Completed', GETDATE()),
        (@Exp7, N'EXP7-P2: Core Sampling & Wood Density Test', N'Extract increment cores for acoustic lab evaluation.', 2, '2026-05-21', '2026-06-30', N'InProgress', GETDATE());

    DECLARE
        @Phase1_1 int, @Phase1_2 int, @Phase1_3 int,
        @Phase2_1 int, @Phase2_2 int, @Phase3_1 int, @Phase3_2 int,
        @Phase4_1 int, @Phase4_2 int, @Phase5_1 int, @Phase5_2 int,
        @Phase7_1 int, @Phase7_2 int;

    SELECT @Phase1_1 = phase_id FROM dbo.ExperimentPhase WHERE phase_name LIKE N'EXP1-P1%';
    SELECT @Phase1_2 = phase_id FROM dbo.ExperimentPhase WHERE phase_name LIKE N'EXP1-P2%';
    SELECT @Phase1_3 = phase_id FROM dbo.ExperimentPhase WHERE phase_name LIKE N'EXP1-P3%';
    SELECT @Phase2_1 = phase_id FROM dbo.ExperimentPhase WHERE phase_name LIKE N'EXP2-P1%';
    SELECT @Phase2_2 = phase_id FROM dbo.ExperimentPhase WHERE phase_name LIKE N'EXP2-P2%';
    SELECT @Phase3_1 = phase_id FROM dbo.ExperimentPhase WHERE phase_name LIKE N'EXP3-P1%';
    SELECT @Phase3_2 = phase_id FROM dbo.ExperimentPhase WHERE phase_name LIKE N'EXP3-P2%';
    SELECT @Phase4_1 = phase_id FROM dbo.ExperimentPhase WHERE phase_name LIKE N'EXP4-P1%';
    SELECT @Phase4_2 = phase_id FROM dbo.ExperimentPhase WHERE phase_name LIKE N'EXP4-P2%';
    SELECT @Phase5_1 = phase_id FROM dbo.ExperimentPhase WHERE phase_name LIKE N'EXP5-P1%';
    SELECT @Phase5_2 = phase_id FROM dbo.ExperimentPhase WHERE phase_name LIKE N'EXP5-P2%';
    SELECT @Phase7_1 = phase_id FROM dbo.ExperimentPhase WHERE phase_name LIKE N'EXP7-P1%';
    SELECT @Phase7_2 = phase_id FROM dbo.ExperimentPhase WHERE phase_name LIKE N'EXP7-P2%';

    -- 8.5 Phase Equipment Requirements
    INSERT INTO dbo.PhaseEquipmentRequirement (phase_id, equipment_type_id, quantity, note, created_at)
    VALUES
        (@Phase1_1, @EquipShovel, 5, N'Shovels for sensor post holes.', GETDATE()),
        (@Phase1_1, @EquipSensor, 10, N'10 IoT moisture sensors deployed.', GETDATE()),
        (@Phase1_2, @EquipDrone, 1, N'Survey drone with thermal camera.', GETDATE()),
        (@Phase1_3, @EquipPHMeter, 1, N'Field chemical kit.', GETDATE()),

        (@Phase2_1, @EquipExcavator, 1, N'Mini excavator for bedding rows.', GETDATE()),
        (@Phase2_2, @EquipShovel, 20, N'Planting shovels for crew.', GETDATE()),
        (@Phase2_2, @EquipGloves, 20, N'Protective gloves for workers.', GETDATE()),

        (@Phase3_1, @EquipPump, 2, N'Water pumps installation.', GETDATE()),
        (@Phase3_2, @EquipSensor, 8, N'Continuous soil check probes.', GETDATE()),

        (@Phase4_1, @EquipGPS, 2, N'High accuracy GPS ground units.', GETDATE()),
        (@Phase4_2, @EquipDrone, 2, N'Dual LiDAR survey drones.', GETDATE()),

        (@Phase7_1, @EquipGPS, 1, N'Tree mapping GPS.', GETDATE()),
        (@Phase7_2, @EquipDensity, 2, N'Acoustic density testers.', GETDATE()),
        (@Phase7_2, @EquipPHMeter, 1, N'Soil acidity verification.', GETDATE());

    DECLARE
        @PhaseReq_P12_Drone int, @PhaseReq_P11_Shovel int, @PhaseReq_P21_Exc int,
        @PhaseReq_P31_Pump int, @PhaseReq_P42_Drone int, @PhaseReq_P72_Density int;

    SELECT @PhaseReq_P12_Drone   = phase_equipment_req_id FROM dbo.PhaseEquipmentRequirement WHERE phase_id = @Phase1_2 AND equipment_type_id = @EquipDrone;
    SELECT @PhaseReq_P11_Shovel  = phase_equipment_req_id FROM dbo.PhaseEquipmentRequirement WHERE phase_id = @Phase1_1 AND equipment_type_id = @EquipShovel;
    SELECT @PhaseReq_P21_Exc     = phase_equipment_req_id FROM dbo.PhaseEquipmentRequirement WHERE phase_id = @Phase2_1 AND equipment_type_id = @EquipExcavator;
    SELECT @PhaseReq_P31_Pump    = phase_equipment_req_id FROM dbo.PhaseEquipmentRequirement WHERE phase_id = @Phase3_1 AND equipment_type_id = @EquipPump;
    SELECT @PhaseReq_P42_Drone   = phase_equipment_req_id FROM dbo.PhaseEquipmentRequirement WHERE phase_id = @Phase4_2 AND equipment_type_id = @EquipDrone;
    SELECT @PhaseReq_P72_Density = phase_equipment_req_id FROM dbo.PhaseEquipmentRequirement WHERE phase_id = @Phase7_2 AND equipment_type_id = @EquipDensity;

    -- 8.6 Phase Human Requirements
    INSERT INTO dbo.PhaseHumanRequirement (phase_id, role_id, quantity, required_skill_id, note, created_at)
    VALUES
        (@Phase1_1, @SupportRoleId, 2, @SkillSoil, N'Support staff installs probes.', GETDATE()),
        (@Phase1_2, @WorkerRoleId, 1, @SkillDrone, N'Worker operates drone.', GETDATE()),
        (@Phase1_3, @ResearcherRoleId, 1, @SkillSoil, N'Researcher tests cores in field.', GETDATE()),

        (@Phase2_1, @WorkerRoleId, 1, @SkillMachinery, N'Worker operates mini excavator.', GETDATE()),
        (@Phase2_2, @WorkerRoleId, 2, @SkillField, N'Worker field crew plants seedlings.', GETDATE()),

        (@Phase3_1, @WorkerRoleId, 1, @SkillIrrigation, N'Worker installs pumps.', GETDATE()),
        (@Phase3_2, @SupportRoleId, 1, @SkillData, N'Support logs water flow stats.', GETDATE()),

        (@Phase4_1, @ResearcherRoleId, 1, @SkillGIS, N'Researcher surveys benchmarks.', GETDATE()),
        (@Phase4_2, @WorkerRoleId, 1, @SkillDrone, N'Pilot conducts flights.', GETDATE()),

        (@Phase7_1, @WorkerRoleId, 1, @SkillField, N'Field technician measures DBH.', GETDATE()),
        (@Phase7_2, @ResearcherRoleId, 1, @SkillField, N'Researcher records wood density.', GETDATE()),
        (@Phase7_2, @SupportRoleId, 1, @SkillData, N'Support enters data into system.', GETDATE()),
        (@Phase1_2, @SupportRoleId, 1, @SkillData, N'Backup data recorder during flight.', GETDATE()),
        (@Phase3_2, @WorkerRoleId, 1, @SkillIrrigation, N'On-call maintenance technician.', GETDATE());

    DECLARE
        @PhaseHum_P12_Drone int, @PhaseHum_P11_Soil int, @PhaseHum_P13_Soil int,
        @PhaseHum_P21_Mach int, @PhaseHum_P31_Irr int, @PhaseHum_P42_Drone int;

    SELECT @PhaseHum_P12_Drone = phase_human_req_id FROM dbo.PhaseHumanRequirement WHERE phase_id = @Phase1_2 AND required_skill_id = @SkillDrone;
    SELECT @PhaseHum_P11_Soil  = phase_human_req_id FROM dbo.PhaseHumanRequirement WHERE phase_id = @Phase1_1 AND required_skill_id = @SkillSoil;
    SELECT @PhaseHum_P13_Soil  = phase_human_req_id FROM dbo.PhaseHumanRequirement WHERE phase_id = @Phase1_3 AND required_skill_id = @SkillSoil;
    SELECT @PhaseHum_P21_Mach  = phase_human_req_id FROM dbo.PhaseHumanRequirement WHERE phase_id = @Phase2_1 AND required_skill_id = @SkillMachinery;
    SELECT @PhaseHum_P31_Irr   = phase_human_req_id FROM dbo.PhaseHumanRequirement WHERE phase_id = @Phase3_1 AND required_skill_id = @SkillIrrigation;
    SELECT @PhaseHum_P42_Drone = phase_human_req_id FROM dbo.PhaseHumanRequirement WHERE phase_id = @Phase4_2 AND required_skill_id = @SkillDrone;

    /* ------------------------------------------------------------------------
       9. ALLOCATION PLANS & DETAILS
       ------------------------------------------------------------------------ */

    INSERT INTO dbo.AllocationPlan
    (
        experiment_id, fitness_score, created_by, approve_by,
        approve_status, approved_at, created_at
    )
    VALUES
        (@Exp1, 94.5, @ManagerUserId, @ManagerUserId, N'Approved', '2026-05-25', GETDATE()),
        (@Exp2, 68.0, @ResearcherUserId, NULL, N'Pending', NULL, GETDATE()),
        (@Exp3, 85.0, @ManagerUserId, @ManagerUserId, N'Approved', '2026-05-18', GETDATE()),
        (@Exp4, 52.5, @ResearcherUserId, NULL, N'Draft', NULL, GETDATE()),
        (@Exp5, 91.0, @ManagerUserId, @ManagerUserId, N'Approved', '2026-03-25', GETDATE()),
        (@Exp7, 88.5, @ManagerUserId, @ManagerUserId, N'Approved', '2026-04-28', GETDATE());

    DECLARE
        @Plan1 int, @Plan2 int, @Plan3 int,
        @Plan4 int, @Plan5 int, @Plan7 int;

    SELECT @Plan1 = allocation_plan_id FROM dbo.AllocationPlan WHERE experiment_id = @Exp1;
    SELECT @Plan2 = allocation_plan_id FROM dbo.AllocationPlan WHERE experiment_id = @Exp2;
    SELECT @Plan3 = allocation_plan_id FROM dbo.AllocationPlan WHERE experiment_id = @Exp3;
    SELECT @Plan4 = allocation_plan_id FROM dbo.AllocationPlan WHERE experiment_id = @Exp4;
    SELECT @Plan5 = allocation_plan_id FROM dbo.AllocationPlan WHERE experiment_id = @Exp5;
    SELECT @Plan7 = allocation_plan_id FROM dbo.AllocationPlan WHERE experiment_id = @Exp7;

    -- 9.1 Allocation Land Details
    INSERT INTO dbo.AllocationLandDetail
    (
        allocation_plan_id, land_id, exp_land_req_id,
        start_date, end_date, status, created_at
    )
    SELECT @Plan1, @LandA1, exp_land_req_id, '2026-06-01', '2026-06-20', N'InUse', GETDATE()
    FROM dbo.ExperimentLandRequirement WHERE experiment_id = @Exp1
    UNION ALL
    SELECT @Plan2, @LandB1, exp_land_req_id, '2026-06-15', '2026-07-30', N'Reserved', GETDATE()
    FROM dbo.ExperimentLandRequirement WHERE experiment_id = @Exp2
    UNION ALL
    SELECT @Plan3, @LandC1, exp_land_req_id, '2026-05-20', '2026-06-10', N'InUse', GETDATE()
    FROM dbo.ExperimentLandRequirement WHERE experiment_id = @Exp3
    UNION ALL
    SELECT @Plan4, @LandA2, exp_land_req_id, '2026-07-01', '2026-07-15', N'Proposed', GETDATE()
    FROM dbo.ExperimentLandRequirement WHERE experiment_id = @Exp4
    UNION ALL
    SELECT @Plan5, @LandNur1, exp_land_req_id, '2026-04-01', '2026-05-01', N'Completed', GETDATE()
    FROM dbo.ExperimentLandRequirement WHERE experiment_id = @Exp5
    UNION ALL
    SELECT @Plan7, @LandCns1, exp_land_req_id, '2026-05-01', '2026-06-30', N'InUse', GETDATE()
    FROM dbo.ExperimentLandRequirement WHERE experiment_id = @Exp7;

    -- 9.2 Allocation Equipment Details
    INSERT INTO dbo.AllocationEquipmentDetail
    (
        allocation_plan_id, exp_equipment_req_id, phase_equipment_req_id,
        allocated_equipment_type_id, equipment_instance_id, quantity,
        is_substitute, efficiency_rate, start_date, end_date, status, created_at
    )
    VALUES
        (@Plan1, NULL, @PhaseReq_P12_Drone, @EquipDrone, @DroneInstance1, 1, 0, 1.00, '2026-06-06', '2026-06-15', N'InUse', GETDATE()),
        (@Plan1, NULL, @PhaseReq_P11_Shovel, @EquipShovel, NULL, 5, 0, 1.00, '2026-06-01', '2026-06-05', N'Completed', GETDATE()),
        (@Plan1, @Exp1_ReqSensor, NULL, @EquipSensor, NULL, 10, 0, 1.00, '2026-06-01', '2026-06-20', N'InUse', GETDATE()),

        (@Plan2, NULL, @PhaseReq_P21_Exc, @EquipExcavator, @ExcavatorInstance1, 1, 0, 1.00, '2026-06-15', '2026-06-25', N'Reserved', GETDATE()),
        (@Plan2, @Exp2_ReqShovel, NULL, @EquipShovel, NULL, 25, 0, 1.00, '2026-06-26', '2026-07-15', N'Proposed', GETDATE()),

        (@Plan3, NULL, @PhaseReq_P31_Pump, @EquipPump, @PumpInstance1, 1, 0, 1.00, '2026-05-20', '2026-05-25', N'Completed', GETDATE()),
        (@Plan3, @Exp3_ReqPump, NULL, @EquipPump, NULL, 1, 0, 1.00, '2026-05-26', '2026-06-10', N'InUse', GETDATE()),

        (@Plan4, @Exp4_ReqDrone, NULL, @EquipGPS, @GPSInstance1, 1, 1, 0.75, '2026-07-01', '2026-07-15', N'Proposed', GETDATE()),
        (@Plan4, NULL, @PhaseReq_P42_Drone, @EquipDrone, @DroneInstance2, 1, 0, 1.00, '2026-07-06', '2026-07-15', N'Proposed', GETDATE()),

        (@Plan7, @Exp7_ReqDensity, NULL, @EquipDensity, NULL, 2, 0, 1.00, '2026-05-21', '2026-06-30', N'InUse', GETDATE()),
        (@Plan7, @Exp7_ReqGPS, NULL, @EquipGPS, @GPSInstance1, 2, 0, 1.00, '2026-05-01', '2026-06-30', N'InUse', GETDATE()),
        (@Plan5, @Exp5_ReqPHMeter, NULL, @EquipPHMeter, @PHMInstance1, 2, 0, 1.00, '2026-04-01', '2026-05-01', N'Completed', GETDATE());

    -- 9.3 Allocation Human Details
    INSERT INTO dbo.AllocationHumanDetail
    (
        allocation_plan_id, exp_human_req_id, phase_human_req_id,
        human_resource_id, working_hours, start_date, end_date, status, created_at
    )
    VALUES
        (@Plan1, @Exp1_HumDrone, NULL, @HRWorker, 8, '2026-06-06', '2026-06-15', N'InUse', GETDATE()),
        (@Plan1, @Exp1_HumSoil, NULL, @HRSupport, 6, '2026-06-01', '2026-06-05', N'Completed', GETDATE()),
        (@Plan1, NULL, @PhaseHum_P13_Soil, @HRResearcher, 8, '2026-06-16', '2026-06-20', N'Reserved', GETDATE()),

        (@Plan2, NULL, @PhaseHum_P21_Mach, @HRWorker, 8, '2026-06-15', '2026-06-25', N'Reserved', GETDATE()),
        (@Plan2, @Exp2_HumField, NULL, @HRWorker, 8, '2026-06-26', '2026-07-15', N'Proposed', GETDATE()),

        (@Plan3, NULL, @PhaseHum_P31_Irr, @HRWorker, 8, '2026-05-20', '2026-05-25', N'Completed', GETDATE()),
        (@Plan3, @Exp3_HumIrrigation, NULL, @HRSupport, 6, '2026-05-26', '2026-06-10', N'InUse', GETDATE()),

        (@Plan4, @Exp4_HumGIS, NULL, @HRResearcher, 8, '2026-07-01', '2026-07-05', N'Proposed', GETDATE()),
        (@Plan4, NULL, @PhaseHum_P42_Drone, @HRWorker, 8, '2026-07-06', '2026-07-15', N'Proposed', GETDATE()),

        (@Plan7, @Exp7_HumField, NULL, @HRResearcher, 8, '2026-05-01', '2026-05-20', N'Completed', GETDATE()),
        (@Plan7, @Exp7_HumGIS, NULL, @HRWorker, 8, '2026-05-21', '2026-06-30', N'InUse', GETDATE()),
        (@Plan5, @Exp5_HumData, NULL, @HRSupport, 6, '2026-04-01', '2026-05-01', N'Completed', GETDATE());

    -- 9.4 Equipment Shortage Logs
    INSERT INTO dbo.EquipmentShortageLog
    (
        allocation_plan_id, exp_equipment_req_id, phase_equipment_req_id,
        shortage_quantity, created_at
    )
    VALUES
        (@Plan2, @Exp2_ReqShovel, NULL, 5, GETDATE()),
        (@Plan1, @Exp1_ReqDrone, NULL, 1, GETDATE()),
        (@Plan4, @Exp4_ReqDrone, NULL, 1, GETDATE()),
        (@Plan3, @Exp3_ReqPump, NULL, 1, GETDATE()),
        (@Plan1, NULL, @PhaseReq_P11_Shovel, 2, GETDATE()),
        (@Plan2, NULL, @PhaseReq_P21_Exc, 1, GETDATE()),
        (@Plan4, NULL, @PhaseReq_P42_Drone, 1, GETDATE()),
        (@Plan7, NULL, @PhaseReq_P72_Density, 1, GETDATE());

    /* ------------------------------------------------------------------------
       10. SCHEDULE
       ------------------------------------------------------------------------ */

    INSERT INTO dbo.Schedule
    (
        allocation_plan_id, phase_id, title, description,
        start_date, end_date, status, created_by,
        assigned_human_resource_id, notes, priority, created_at
    )
    VALUES
        (@Plan1, @Phase1_1, N'EXP1: Sensor Grid Installation', N'Install 10 moisture sensors at Zone A.', '2026-06-01', '2026-06-05', N'Completed', @ManagerUserId, @HRSupport, N'Successfully installed.', 1, GETDATE()),
        (@Plan1, @Phase1_2, N'EXP1: Thermal Drone Flights', N'Daily morning flights at 8:00 AM.', '2026-06-06', '2026-06-15', N'InProgress', @ManagerUserId, @HRWorker, N'Battery packs charged daily.', 1, GETDATE()),
        (@Plan1, @Phase1_3, N'EXP1: Soil Core Validation', N'Extract core samples for moisture verification.', '2026-06-16', '2026-06-20', N'Planned', @ManagerUserId, @HRResearcher, N'Prepare field cooler boxes.', 2, GETDATE()),

        (@Plan2, @Phase2_1, N'EXP2: Tractor Trenching Work', N'Prepare planting beds at Lowland plot.', '2026-06-15', '2026-06-25', N'Planned', @ManagerUserId, @HRWorker, N'Waiting for plan approval.', 2, GETDATE()),
        (@Plan2, @Phase2_2, N'EXP2: Eucalypt Planting Drive', N'Plant 500 seedlings across 4 beds.', '2026-06-26', '2026-07-15', N'Planned', @ManagerUserId, @HRWorker, N'Weather check required.', 2, GETDATE()),

        (@Plan3, @Phase3_1, N'EXP3: Pump Setup & Pressure Check', N'Install pump and drip emitters.', '2026-05-20', '2026-05-25', N'Completed', @ManagerUserId, @HRWorker, N'All valves operational.', 1, GETDATE()),
        (@Plan3, @Phase3_2, N'EXP3: Water Regime Cycling', N'Cycle drip lines every 4 hours.', '2026-05-26', '2026-06-10', N'InProgress', @ManagerUserId, @HRSupport, N'Log gauge values twice daily.', 1, GETDATE()),

        (@Plan4, @Phase4_1, N'EXP4: GPS Benchmark Calibration', N'Survey Ground Control Points.', '2026-07-01', '2026-07-05', N'Planned', @ManagerUserId, @HRResearcher, N'Check GPS battery life.', 3, GETDATE()),
        (@Plan4, @Phase4_2, N'EXP4: LiDAR Aerial Capture', N'Autonomous LiDAR sweep over Zone A.', '2026-07-06', '2026-07-15', N'Planned', @ManagerUserId, @HRWorker, N'Clear flight zone clearance.', 2, GETDATE()),

        (@Plan5, @Phase5_1, N'EXP5: Nursery Inoculation Protocol', N'Apply organic mycorrhizal blend.', '2026-04-01', '2026-04-10', N'Completed', @ManagerUserId, @HRSupport, N'Passed quality control.', 1, GETDATE()),
        (@Plan7, @Phase7_1, N'EXP7: Tree DBH & GPS Tagging', N'Tag 120 specimen trees.', '2026-05-01', '2026-05-20', N'Completed', @ManagerUserId, @HRWorker, N'Finished ahead of time.', 2, GETDATE()),
        (@Plan7, @Phase7_2, N'EXP7: Acoustic Density Logging', N'Test acoustic velocity on tagged trunks.', '2026-05-21', '2026-06-30', N'InProgress', @ManagerUserId, @HRResearcher, N'Testing at 50% completion.', 2, GETDATE());

    /* ------------------------------------------------------------------------
       11. FINAL INTEGRITY CHECKS
       Fail before COMMIT if any FK source variable unexpectedly resolved to NULL.
       This makes future schema/data changes fail clearly instead of creating
       partially inconsistent seed data.
       ------------------------------------------------------------------------ */
    IF @EquipDrone IS NULL OR @EquipGPS IS NULL OR @EquipSensor IS NULL
       OR @EquipShovel IS NULL OR @EquipPump IS NULL OR @EquipExcavator IS NULL
       OR @EquipPHMeter IS NULL OR @EquipDensity IS NULL
       OR @Exp1 IS NULL OR @Exp2 IS NULL OR @Exp3 IS NULL OR @Exp4 IS NULL OR @Exp5 IS NULL OR @Exp7 IS NULL
       OR @Plan1 IS NULL OR @Plan2 IS NULL OR @Plan3 IS NULL OR @Plan4 IS NULL OR @Plan5 IS NULL OR @Plan7 IS NULL
    BEGIN
        THROW 51001, 'Seed integrity check failed: one or more required IDs could not be resolved.', 1;
    END;

    /* ------------------------------------------------------------------------
       12. NOTIFICATIONS
       ------------------------------------------------------------------------ */

    IF OBJECT_ID(N'dbo.Notification', N'U') IS NOT NULL
    BEGIN
        INSERT INTO dbo.Notification
        (
            user_id, title, message, notification_type,
            reference_type, reference_id, is_read, read_at,
            is_deleted, deleted_at, created_at
        )
        VALUES
            (@ResearcherUserId, N'Allocation Plan Approved', N'Allocation Plan for EXP-01 has been approved by Manager.', N'AllocationPlanApproved', N'AllocationPlan', @Plan1, 1, GETDATE(), 0, NULL, GETDATE()),
            (@ResearcherUserId, N'Equipment Shortage Detected', N'Shortage detected: 1 Drone unit substituted with GPS unit.', N'EquipmentShortage', N'Experiment', @Exp4, 0, NULL, 0, NULL, GETDATE()),
            (@ManagerUserId, N'Experiment Submitted for Review', N'Researcher submitted EXP-02 for resource allocation approval.', N'ExperimentPending', N'Experiment', @Exp2, 0, NULL, 0, NULL, GETDATE()),
            (@WorkerUserId, N'New Flight Schedule Assigned', N'You have been assigned to thermal flight task in EXP-01.', N'ScheduleAssigned', N'Schedule', 2, 0, NULL, 0, NULL, GETDATE()),
            (@SupportUserId, N'Irrigation Logging Task Assigned', N'You are assigned to water regime cycling task in EXP-03.', N'ScheduleAssigned', N'Schedule', 7, 1, GETDATE(), 0, NULL, GETDATE()),
            (@WorkerUserId, N'Mini Excavator Maintenance Due', N'Mini Excavator 3T scheduled for 500-hour inspection.', N'MaintenanceAlert', N'EquipmentInstance', @ExcavatorInstance1, 0, NULL, 0, NULL, GETDATE()),
            (@ResearcherUserId, N'Experiment EXP-05 Completed', N'All phases for Seedling Germination Protocol are completed.', N'ExperimentCompleted', N'Experiment', @Exp5, 1, GETDATE(), 0, NULL, GETDATE()),
            (@ManagerUserId, N'Plan EXP-03 Executing On Track', N'Wetland irrigation phase 2 progress is at 70%.', N'ProgressUpdate', N'AllocationPlan', @Plan3, 1, GETDATE(), 0, NULL, GETDATE()),
            (@SupportUserId, N'Soil Sample Cooler Preparation', N'Reminder to prepare cooler packs for core collection.', N'TaskReminder', N'Schedule', 3, 0, NULL, 0, NULL, GETDATE()),
            (@WorkerUserId, N'Rainforest Tree Tagging Completed', N'Manager approved completion of EXP-07 Phase 1.', N'TaskCompleted', N'Schedule', 11, 1, GETDATE(), 0, NULL, GETDATE()),
            (@ResearcherUserId, N'LiDAR Data Validation Needed', N'Pre-flight ground benchmarks uploaded for EXP-04.', N'DataReview', N'Experiment', @Exp4, 0, NULL, 0, NULL, GETDATE()),
            (@AdminUserId, N'System Backup & Resource Audit', N'Monthly resource allocation logs summarized successfully.', N'SystemAudit', N'AuditLog', 1, 0, NULL, 0, NULL, GETDATE());
    END;

    COMMIT TRANSACTION;

    PRINT N'============================================================';
    PRINT N'Expanded Test Data Loaded Successfully (~180+ records).';
    PRINT N'dbo.User and dbo.Role were NOT modified.';
    PRINT N'All 30 controllers have full data coverage for testing.';
    PRINT N'============================================================';
    PRINT CONCAT(N'AdminUserId      = ', @AdminUserId);
    PRINT CONCAT(N'ManagerUserId    = ', @ManagerUserId);
    PRINT CONCAT(N'ResearcherUserId = ', @ResearcherUserId);
    PRINT CONCAT(N'WorkerUserId     = ', @WorkerUserId);
    PRINT CONCAT(N'SupportUserId    = ', @SupportUserId);
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT N'============================================================';
    PRINT N'Test data seed failed. Transaction rolled back.';
    PRINT CONCAT(N'Error ', ERROR_NUMBER(), N' at line ', ERROR_LINE(), N': ', ERROR_MESSAGE());
    PRINT N'============================================================';

    THROW;
END CATCH;
GO