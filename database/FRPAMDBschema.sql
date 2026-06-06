-- =============================================================================
-- Forestry Resource Planning and Allocation Management System (FRPAM)
-- MySQL 8.x - Script khởi tạo database FRPAMDB
-- Chạy toàn bộ file: mysql -u root -p < database/schema.sql
-- =============================================================================

-- -----------------------------------------------------------------------------
-- 1. Session settings
-- -----------------------------------------------------------------------------
SET NAMES utf8mb4;
SET CHARACTER SET utf8mb4;
SET time_zone = '+07:00';
SET FOREIGN_KEY_CHECKS = 0;
SET UNIQUE_CHECKS = 0;
SET SQL_MODE = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------------------------------
-- 2. Tạo lại database FRPAMDB
-- -----------------------------------------------------------------------------
DROP DATABASE IF EXISTS `FRPAMDB`;

CREATE DATABASE `FRPAMDB`
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE `FRPAMDB`;

-- -----------------------------------------------------------------------------
-- 3. Drop tables (thứ tự con → cha, an toàn khi FOREIGN_KEY_CHECKS = 0)
-- -----------------------------------------------------------------------------
DROP TABLE IF EXISTS `Schedule`;
DROP TABLE IF EXISTS `EquipmentShortageLog`;
DROP TABLE IF EXISTS `AllocationLandDetail`;
DROP TABLE IF EXISTS `AllocationHumanDetail`;
DROP TABLE IF EXISTS `AllocationEquipmentDetail`;
DROP TABLE IF EXISTS `AllocationPlan`;
DROP TABLE IF EXISTS `PhaseHumanRequirement`;
DROP TABLE IF EXISTS `PhaseEquipmentRequirement`;
DROP TABLE IF EXISTS `ExperimentPhase`;
DROP TABLE IF EXISTS `ExperimentLandRequirement`;
DROP TABLE IF EXISTS `Experiment`;
DROP TABLE IF EXISTS `EquipmentInstance`;
DROP TABLE IF EXISTS `EquipmentType`;
DROP TABLE IF EXISTS `LandResource`;
DROP TABLE IF EXISTS `HumanResourceSkill`;
DROP TABLE IF EXISTS `HumanResourceProfile`;
DROP TABLE IF EXISTS `User`;
DROP TABLE IF EXISTS `EquipmentCategory`;
DROP TABLE IF EXISTS `Area`;
DROP TABLE IF EXISTS `Skill`;
DROP TABLE IF EXISTS `Role`;

-- -----------------------------------------------------------------------------
-- 4. Tạo tables
-- -----------------------------------------------------------------------------

-- =========================
-- 4.1. Bảng tham chiếu (không phụ thuộc FK)
-- =========================

CREATE TABLE `Role` (
  `role_id` INT NOT NULL AUTO_INCREMENT,
  `role_name` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`role_id`)
);

CREATE TABLE `Skill` (
  `skill_id` INT NOT NULL AUTO_INCREMENT,
  `skill_name` VARCHAR(150) NOT NULL,
  `description` TEXT NULL,
  PRIMARY KEY (`skill_id`)
);

CREATE TABLE `Area` (
  `area_id` INT NOT NULL AUTO_INCREMENT,
  `area_name` VARCHAR(150) NOT NULL,
  `description` TEXT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`area_id`)
);

CREATE TABLE `EquipmentCategory` (
  `equipment_category_id` INT NOT NULL AUTO_INCREMENT,
  `category_name` VARCHAR(150) NOT NULL,
  `description` TEXT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`equipment_category_id`)
);

-- =========================
-- 4.2. User & nhân lực
-- =========================

CREATE TABLE `User` (
  `user_id` INT NOT NULL AUTO_INCREMENT,
  `full_name` VARCHAR(200) NOT NULL,
  `username` VARCHAR(100) NOT NULL,
  `password` VARCHAR(255) NOT NULL,
  `email` VARCHAR(255) NOT NULL,
  `role_id` INT NOT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `uk_user_username` (`username`),
  UNIQUE KEY `uk_user_email` (`email`),
  CONSTRAINT `fk_user_role`
    FOREIGN KEY (`role_id`) REFERENCES `Role` (`role_id`)
);

CREATE TABLE `HumanResourceProfile` (
  `human_resource_id` INT NOT NULL AUTO_INCREMENT,
  `user_id` INT NOT NULL,
  `max_working_hours_per_day` DECIMAL(5, 2) NOT NULL DEFAULT 8.00,
  `current_workload` DECIMAL(5, 2) NOT NULL DEFAULT 0.00,
  `status` ENUM('available', 'busy', 'unavailable') NOT NULL DEFAULT 'available',
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`human_resource_id`),
  UNIQUE KEY `uk_human_resource_user` (`user_id`),
  CONSTRAINT `fk_human_resource_user`
    FOREIGN KEY (`user_id`) REFERENCES `User` (`user_id`)
);

CREATE TABLE `HumanResourceSkill` (
  `human_resource_skill_id` INT NOT NULL AUTO_INCREMENT,
  `human_resource_id` INT NOT NULL,
  `skill_id` INT NOT NULL,
  `skill_level` ENUM('beginner', 'intermediate', 'advanced', 'expert') NOT NULL DEFAULT 'beginner',
  PRIMARY KEY (`human_resource_skill_id`),
  UNIQUE KEY `uk_human_resource_skill` (`human_resource_id`, `skill_id`),
  CONSTRAINT `fk_hrs_human_resource`
    FOREIGN KEY (`human_resource_id`) REFERENCES `HumanResourceProfile` (`human_resource_id`),
  CONSTRAINT `fk_hrs_skill`
    FOREIGN KEY (`skill_id`) REFERENCES `Skill` (`skill_id`)
);

-- =========================
-- 4.3. Tài nguyên đất & thiết bị
-- =========================

CREATE TABLE `LandResource` (
  `land_id` INT NOT NULL AUTO_INCREMENT,
  `area_id` INT NOT NULL,
  `land_code` VARCHAR(50) NOT NULL,
  `area_size` DECIMAL(12, 2) NOT NULL,
  `location` VARCHAR(255) NULL,
  `soil_type` VARCHAR(100) NULL,
  `status` ENUM('available', 'reserved', 'in_use', 'maintenance') NOT NULL DEFAULT 'available',
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`land_id`),
  UNIQUE KEY `uk_land_code` (`land_code`),
  CONSTRAINT `fk_land_area`
    FOREIGN KEY (`area_id`) REFERENCES `Area` (`area_id`)
);

CREATE TABLE `EquipmentType` (
  `equipment_type_id` INT NOT NULL AUTO_INCREMENT,
  `equipment_category_id` INT NOT NULL,
  `name` VARCHAR(150) NOT NULL,
  `tracking_type` ENUM('quantity', 'instance') NOT NULL DEFAULT 'quantity',
  `base_maintenance_interval_hours` DECIMAL(10, 2) NULL,
  `total_quantity` INT NOT NULL DEFAULT 0,
  `damaged_quantity` INT NOT NULL DEFAULT 0,
  `available_quantity` INT NOT NULL DEFAULT 0,
  `reserved_quantity` INT NOT NULL DEFAULT 0,
  `in_use_quantity` INT NOT NULL DEFAULT 0,
  `missing_quantity` INT NOT NULL DEFAULT 0,
  `description` TEXT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`equipment_type_id`),
  CONSTRAINT `fk_equipment_type_category`
    FOREIGN KEY (`equipment_category_id`) REFERENCES `EquipmentCategory` (`equipment_category_id`)
);

CREATE TABLE `EquipmentInstance` (
  `equipment_instance_id` INT NOT NULL AUTO_INCREMENT,
  `equipment_type_id` INT NOT NULL,
  `asset_code` VARCHAR(100) NOT NULL,
  `serial_number` VARCHAR(100) NULL,
  `total_usage_hour` DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
  `last_maintenance_date` DATETIME NULL,
  `usage_hours_since_last_maintenance` DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
  `condition_level` ENUM('excellent', 'good', 'fair', 'poor') NOT NULL DEFAULT 'good',
  `status` ENUM('available', 'in_use', 'maintenance', 'damaged', 'missing') NOT NULL DEFAULT 'available',
  `effective_interval_hour` DECIMAL(10, 2) NULL,
  `maintenance_count` INT NOT NULL DEFAULT 0,
  `note` TEXT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`equipment_instance_id`),
  UNIQUE KEY `uk_equipment_asset_code` (`asset_code`),
  CONSTRAINT `fk_equipment_instance_type`
    FOREIGN KEY (`equipment_type_id`) REFERENCES `EquipmentType` (`equipment_type_id`)
);

-- =========================
-- 4.4. Thí nghiệm & yêu cầu tài nguyên
-- =========================

CREATE TABLE `Experiment` (
  `experiment_id` INT NOT NULL AUTO_INCREMENT,
  `experiment_name` VARCHAR(200) NOT NULL,
  `description` TEXT NULL,
  `researcher_id` INT NOT NULL,
  `expect_start_date` DATETIME NULL,
  `expect_end_date` DATETIME NULL,
  `deadline` DATETIME NULL,
  `priority` INT NOT NULL DEFAULT 0,
  `status` ENUM('draft', 'pending', 'approved', 'in_progress', 'completed', 'cancelled') NOT NULL DEFAULT 'draft',
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`experiment_id`),
  CONSTRAINT `fk_experiment_researcher`
    FOREIGN KEY (`researcher_id`) REFERENCES `User` (`user_id`)
);

CREATE TABLE `ExperimentLandRequirement` (
  `exp_land_requirement_id` INT NOT NULL AUTO_INCREMENT,
  `experiment_id` INT NOT NULL,
  `required_area` DECIMAL(12, 2) NOT NULL,
  `required_soil_type` VARCHAR(100) NULL,
  `note` TEXT NULL,
  `is_fixed_for_experiment` BOOLEAN NOT NULL DEFAULT TRUE,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`exp_land_requirement_id`),
  CONSTRAINT `fk_exp_land_requirement_experiment`
    FOREIGN KEY (`experiment_id`) REFERENCES `Experiment` (`experiment_id`)
);

CREATE TABLE `ExperimentPhase` (
  `phase_id` INT NOT NULL AUTO_INCREMENT,
  `experiment_id` INT NOT NULL,
  `phase_name` VARCHAR(150) NOT NULL,
  `phase_description` TEXT NULL,
  `phase_order` INT NOT NULL,
  `expected_start_date` DATETIME NULL,
  `expected_end_date` DATETIME NULL,
  `status` ENUM('planned', 'in_progress', 'completed', 'cancelled') NOT NULL DEFAULT 'planned',
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`phase_id`),
  CONSTRAINT `fk_phase_experiment`
    FOREIGN KEY (`experiment_id`) REFERENCES `Experiment` (`experiment_id`)
);

CREATE TABLE `PhaseEquipmentRequirement` (
  `equipment_requirement_id` INT NOT NULL AUTO_INCREMENT,
  `phase_id` INT NOT NULL,
  `equipment_type_id` INT NOT NULL,
  `quantity` INT NOT NULL,
  `note` TEXT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`equipment_requirement_id`),
  CONSTRAINT `fk_phase_equipment_requirement_phase`
    FOREIGN KEY (`phase_id`) REFERENCES `ExperimentPhase` (`phase_id`),
  CONSTRAINT `fk_phase_equipment_requirement_type`
    FOREIGN KEY (`equipment_type_id`) REFERENCES `EquipmentType` (`equipment_type_id`)
);

CREATE TABLE `PhaseHumanRequirement` (
  `human_requirement_id` INT NOT NULL AUTO_INCREMENT,
  `phase_id` INT NOT NULL,
  `role_id` INT NOT NULL,
  `quantity` INT NOT NULL,
  `required_skill_id` INT NULL,
  `note` TEXT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`human_requirement_id`),
  CONSTRAINT `fk_phase_human_requirement_phase`
    FOREIGN KEY (`phase_id`) REFERENCES `ExperimentPhase` (`phase_id`),
  CONSTRAINT `fk_phase_human_requirement_role`
    FOREIGN KEY (`role_id`) REFERENCES `Role` (`role_id`),
  CONSTRAINT `fk_phase_human_requirement_skill`
    FOREIGN KEY (`required_skill_id`) REFERENCES `Skill` (`skill_id`)
);

-- =========================
-- 4.5. Kế hoạch phân bổ
-- =========================

CREATE TABLE `AllocationPlan` (
  `allocation_plan_id` INT NOT NULL AUTO_INCREMENT,
  `experiment_id` INT NOT NULL,
  `fitness_score` DECIMAL(10, 4) NULL,
  `created_by` INT NOT NULL,
  `approve_by` INT NULL,
  `approve_status` ENUM('pending', 'approved', 'rejected') NOT NULL DEFAULT 'pending',
  `approved_at` DATETIME NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`allocation_plan_id`),
  CONSTRAINT `fk_allocation_plan_experiment`
    FOREIGN KEY (`experiment_id`) REFERENCES `Experiment` (`experiment_id`),
  CONSTRAINT `fk_allocation_plan_created_by`
    FOREIGN KEY (`created_by`) REFERENCES `User` (`user_id`),
  CONSTRAINT `fk_allocation_plan_approve_by`
    FOREIGN KEY (`approve_by`) REFERENCES `User` (`user_id`)
);

CREATE TABLE `AllocationEquipmentDetail` (
  `allocation_equipment_detail_id` INT NOT NULL AUTO_INCREMENT,
  `allocation_plan_id` INT NOT NULL,
  `phase_id` INT NOT NULL,
  `equipment_requirement_id` INT NOT NULL,
  `equipment_type_id` INT NOT NULL,
  `equipment_instance_id` INT NULL,
  `quantity` INT NOT NULL DEFAULT 1,
  `start_date` DATETIME NOT NULL,
  `end_date` DATETIME NOT NULL,
  `status` ENUM('planned', 'active', 'completed', 'cancelled') NOT NULL DEFAULT 'planned',
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`allocation_equipment_detail_id`),
  CONSTRAINT `fk_alloc_equipment_plan`
    FOREIGN KEY (`allocation_plan_id`) REFERENCES `AllocationPlan` (`allocation_plan_id`),
  CONSTRAINT `fk_alloc_equipment_phase`
    FOREIGN KEY (`phase_id`) REFERENCES `ExperimentPhase` (`phase_id`),
  CONSTRAINT `fk_alloc_equipment_requirement`
    FOREIGN KEY (`equipment_requirement_id`) REFERENCES `PhaseEquipmentRequirement` (`equipment_requirement_id`),
  CONSTRAINT `fk_alloc_equipment_type`
    FOREIGN KEY (`equipment_type_id`) REFERENCES `EquipmentType` (`equipment_type_id`),
  CONSTRAINT `fk_alloc_equipment_instance`
    FOREIGN KEY (`equipment_instance_id`) REFERENCES `EquipmentInstance` (`equipment_instance_id`)
);

CREATE TABLE `AllocationHumanDetail` (
  `allocation_human_detail_id` INT NOT NULL AUTO_INCREMENT,
  `allocation_plan_id` INT NOT NULL,
  `phase_id` INT NOT NULL,
  `human_requirement_id` INT NOT NULL,
  `human_resource_id` INT NOT NULL,
  `working_hours` DECIMAL(6, 2) NOT NULL,
  `start_date` DATETIME NOT NULL,
  `end_date` DATETIME NOT NULL,
  `status` ENUM('planned', 'active', 'completed', 'cancelled') NOT NULL DEFAULT 'planned',
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`allocation_human_detail_id`),
  CONSTRAINT `fk_alloc_human_plan`
    FOREIGN KEY (`allocation_plan_id`) REFERENCES `AllocationPlan` (`allocation_plan_id`),
  CONSTRAINT `fk_alloc_human_phase`
    FOREIGN KEY (`phase_id`) REFERENCES `ExperimentPhase` (`phase_id`),
  CONSTRAINT `fk_alloc_human_requirement`
    FOREIGN KEY (`human_requirement_id`) REFERENCES `PhaseHumanRequirement` (`human_requirement_id`),
  CONSTRAINT `fk_alloc_human_resource`
    FOREIGN KEY (`human_resource_id`) REFERENCES `HumanResourceProfile` (`human_resource_id`)
);

CREATE TABLE `AllocationLandDetail` (
  `allocation_land_detail_id` INT NOT NULL AUTO_INCREMENT,
  `allocation_plan_id` INT NOT NULL,
  `land_id` INT NOT NULL,
  `exp_land_requirement_id` INT NOT NULL,
  `start_date` DATETIME NOT NULL,
  `end_date` DATETIME NOT NULL,
  `status` ENUM('planned', 'active', 'completed', 'cancelled') NOT NULL DEFAULT 'planned',
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`allocation_land_detail_id`),
  CONSTRAINT `fk_alloc_land_plan`
    FOREIGN KEY (`allocation_plan_id`) REFERENCES `AllocationPlan` (`allocation_plan_id`),
  CONSTRAINT `fk_alloc_land_resource`
    FOREIGN KEY (`land_id`) REFERENCES `LandResource` (`land_id`),
  CONSTRAINT `fk_alloc_land_requirement`
    FOREIGN KEY (`exp_land_requirement_id`) REFERENCES `ExperimentLandRequirement` (`exp_land_requirement_id`)
);

CREATE TABLE `EquipmentShortageLog` (
  `shortage_log_id` INT NOT NULL AUTO_INCREMENT,
  `allocation_plan_id` INT NOT NULL,
  `equipment_requirement_id` INT NOT NULL,
  `shortage_quantity` INT NOT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`shortage_log_id`),
  CONSTRAINT `fk_shortage_plan`
    FOREIGN KEY (`allocation_plan_id`) REFERENCES `AllocationPlan` (`allocation_plan_id`),
  CONSTRAINT `fk_shortage_requirement`
    FOREIGN KEY (`equipment_requirement_id`) REFERENCES `PhaseEquipmentRequirement` (`equipment_requirement_id`)
);

-- =========================
-- 4.6. Lịch thực hiện
-- =========================

CREATE TABLE `Schedule` (
  `schedule_id` INT NOT NULL AUTO_INCREMENT,
  `allocation_plan_id` INT NOT NULL,
  `phase_id` INT NOT NULL,
  `title` VARCHAR(200) NOT NULL,
  `description` TEXT NULL,
  `start_date` DATETIME NOT NULL,
  `end_date` DATETIME NOT NULL,
  `status` ENUM('scheduled', 'in_progress', 'completed', 'cancelled') NOT NULL DEFAULT 'scheduled',
  `created_by` INT NOT NULL,
  `assign_to` INT NULL,
  `notes` TEXT NULL,
  `priority` INT NOT NULL DEFAULT 0,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`schedule_id`),
  CONSTRAINT `fk_schedule_plan`
    FOREIGN KEY (`allocation_plan_id`) REFERENCES `AllocationPlan` (`allocation_plan_id`),
  CONSTRAINT `fk_schedule_phase`
    FOREIGN KEY (`phase_id`) REFERENCES `ExperimentPhase` (`phase_id`),
  CONSTRAINT `fk_schedule_created_by`
    FOREIGN KEY (`created_by`) REFERENCES `User` (`user_id`),
  CONSTRAINT `fk_schedule_assign_to`
    FOREIGN KEY (`assign_to`) REFERENCES `User` (`user_id`)
);

-- -----------------------------------------------------------------------------
-- 5. Khôi phục session settings
-- -----------------------------------------------------------------------------
SET FOREIGN_KEY_CHECKS = 1;
SET UNIQUE_CHECKS = 1;
