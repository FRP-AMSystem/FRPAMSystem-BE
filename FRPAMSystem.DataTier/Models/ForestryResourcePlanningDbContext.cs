using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.DataTier.Models;

public partial class ForestryResourcePlanningDbContext : DbContext
{
    public ForestryResourcePlanningDbContext()
    {
    }

    public ForestryResourcePlanningDbContext(DbContextOptions<ForestryResourcePlanningDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AllocationEquipmentDetail> AllocationEquipmentDetails { get; set; }

    public virtual DbSet<AllocationHumanDetail> AllocationHumanDetails { get; set; }

    public virtual DbSet<AllocationLandDetail> AllocationLandDetails { get; set; }

    public virtual DbSet<AllocationPlan> AllocationPlans { get; set; }

    public virtual DbSet<Area> Areas { get; set; }

    public virtual DbSet<EquipmentCategory> EquipmentCategories { get; set; }

    public virtual DbSet<EquipmentInstance> EquipmentInstances { get; set; }

    public virtual DbSet<EquipmentShortageLog> EquipmentShortageLogs { get; set; }

    public virtual DbSet<EquipmentSubstitution> EquipmentSubstitutions { get; set; }

    public virtual DbSet<EquipmentType> EquipmentTypes { get; set; }

    public virtual DbSet<Experiment> Experiments { get; set; }

    public virtual DbSet<ExperimentEquipmentRequirement> ExperimentEquipmentRequirements { get; set; }

    public virtual DbSet<ExperimentHumanRequirement> ExperimentHumanRequirements { get; set; }

    public virtual DbSet<ExperimentLandRequirement> ExperimentLandRequirements { get; set; }

    public virtual DbSet<ExperimentPhase> ExperimentPhases { get; set; }

    public virtual DbSet<HumanResourceProfile> HumanResourceProfiles { get; set; }

    public virtual DbSet<HumanResourceSkill> HumanResourceSkills { get; set; }

    public virtual DbSet<LandResource> LandResources { get; set; }

    public virtual DbSet<PhaseEquipmentRequirement> PhaseEquipmentRequirements { get; set; }

    public virtual DbSet<PhaseHumanRequirement> PhaseHumanRequirements { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string is injected via DI (appsettings.json).
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AllocationEquipmentDetail>(entity =>
        {
            entity.HasKey(e => e.AllocationEquipmentDetailId).HasName("PK__Allocati__1998F72A40624FFB");

            entity.ToTable("AllocationEquipmentDetail");

            entity.Property(e => e.AllocationEquipmentDetailId).HasColumnName("allocation_equipment_detail_id");
            entity.Property(e => e.AllocatedEquipmentTypeId).HasColumnName("allocated_equipment_type_id");
            entity.Property(e => e.AllocationPlanId).HasColumnName("allocation_plan_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.EfficiencyRate)
                .HasDefaultValue(1.0)
                .HasColumnName("efficiency_rate");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.EquipmentInstanceId).HasColumnName("equipment_instance_id");
            entity.Property(e => e.ExpEquipmentReqId).HasColumnName("exp_equipment_req_id");
            entity.Property(e => e.IsSubstitute).HasColumnName("is_substitute");
            entity.Property(e => e.PhaseEquipmentReqId).HasColumnName("phase_equipment_req_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.AllocatedEquipmentType).WithMany(p => p.AllocationEquipmentDetails)
                .HasForeignKey(d => d.AllocatedEquipmentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AllocationEquipmentDetail_AllocatedEquipmentType");

            entity.HasOne(d => d.AllocationPlan).WithMany(p => p.AllocationEquipmentDetails)
                .HasForeignKey(d => d.AllocationPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AllocationEquipmentDetail_AllocationPlan");

            entity.HasOne(d => d.EquipmentInstance).WithMany(p => p.AllocationEquipmentDetails)
                .HasForeignKey(d => d.EquipmentInstanceId)
                .HasConstraintName("FK_AllocationEquipmentDetail_EquipmentInstance");

            entity.HasOne(d => d.ExpEquipmentReq).WithMany(p => p.AllocationEquipmentDetails)
                .HasForeignKey(d => d.ExpEquipmentReqId)
                .HasConstraintName("FK_AllocationEquipmentDetail_ExperimentEquipmentRequirement");

            entity.HasOne(d => d.PhaseEquipmentReq).WithMany(p => p.AllocationEquipmentDetails)
                .HasForeignKey(d => d.PhaseEquipmentReqId)
                .HasConstraintName("FK_AllocationEquipmentDetail_PhaseEquipmentRequirement");
        });

        modelBuilder.Entity<AllocationHumanDetail>(entity =>
        {
            entity.HasKey(e => e.AllocationHumanDetailId).HasName("PK__Allocati__68BC824CA3DDF6D2");

            entity.ToTable("AllocationHumanDetail");

            entity.Property(e => e.AllocationHumanDetailId).HasColumnName("allocation_human_detail_id");
            entity.Property(e => e.AllocationPlanId).HasColumnName("allocation_plan_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.ExpHumanReqId).HasColumnName("exp_human_req_id");
            entity.Property(e => e.HumanResourceId).HasColumnName("human_resource_id");
            entity.Property(e => e.PhaseHumanReqId).HasColumnName("phase_human_req_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.WorkingHours).HasColumnName("working_hours");

            entity.HasOne(d => d.AllocationPlan).WithMany(p => p.AllocationHumanDetails)
                .HasForeignKey(d => d.AllocationPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AllocationHumanDetail_AllocationPlan");

            entity.HasOne(d => d.ExpHumanReq).WithMany(p => p.AllocationHumanDetails)
                .HasForeignKey(d => d.ExpHumanReqId)
                .HasConstraintName("FK_AllocationHumanDetail_ExperimentHumanRequirement");

            entity.HasOne(d => d.HumanResource).WithMany(p => p.AllocationHumanDetails)
                .HasForeignKey(d => d.HumanResourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AllocationHumanDetail_HumanResourceProfile");

            entity.HasOne(d => d.PhaseHumanReq).WithMany(p => p.AllocationHumanDetails)
                .HasForeignKey(d => d.PhaseHumanReqId)
                .HasConstraintName("FK_AllocationHumanDetail_PhaseHumanRequirement");
        });

        modelBuilder.Entity<AllocationLandDetail>(entity =>
        {
            entity.HasKey(e => e.AllocationLandDetailId).HasName("PK__Allocati__F6D6308A55006AF3");

            entity.ToTable("AllocationLandDetail");

            entity.Property(e => e.AllocationLandDetailId).HasColumnName("allocation_land_detail_id");
            entity.Property(e => e.AllocationPlanId).HasColumnName("allocation_plan_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.ExpLandReqId).HasColumnName("exp_land_req_id");
            entity.Property(e => e.LandId).HasColumnName("land_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.AllocationPlan).WithMany(p => p.AllocationLandDetails)
                .HasForeignKey(d => d.AllocationPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AllocationLandDetail_AllocationPlan");

            entity.HasOne(d => d.ExpLandReq).WithMany(p => p.AllocationLandDetails)
                .HasForeignKey(d => d.ExpLandReqId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AllocationLandDetail_ExperimentLandRequirement");

            entity.HasOne(d => d.Land).WithMany(p => p.AllocationLandDetails)
                .HasForeignKey(d => d.LandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AllocationLandDetail_LandResource");
        });

        modelBuilder.Entity<AllocationPlan>(entity =>
        {
            entity.HasKey(e => e.AllocationPlanId).HasName("PK__Allocati__124B7D5C38F76AE2");

            entity.ToTable("AllocationPlan");

            entity.Property(e => e.AllocationPlanId).HasColumnName("allocation_plan_id");
            entity.Property(e => e.ApproveBy).HasColumnName("approve_by");
            entity.Property(e => e.ApproveStatus)
                .HasMaxLength(50)
                .HasColumnName("approve_status");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.ExperimentId).HasColumnName("experiment_id");
            entity.Property(e => e.FitnessScore).HasColumnName("fitness_score");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.ApproveByNavigation).WithMany(p => p.AllocationPlanApproveByNavigations)
                .HasForeignKey(d => d.ApproveBy)
                .HasConstraintName("FK_AllocationPlan_ApproveBy");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.AllocationPlanCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_AllocationPlan_CreatedBy");

            entity.HasOne(d => d.Experiment).WithMany(p => p.AllocationPlans)
                .HasForeignKey(d => d.ExperimentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AllocationPlan_Experiment");
        });

        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.AreaId).HasName("PK__Area__985D6D6B0D01DD39");

            entity.ToTable("Area");

            entity.Property(e => e.AreaId).HasColumnName("area_id");
            entity.Property(e => e.AreaName)
                .HasMaxLength(255)
                .HasColumnName("area_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<EquipmentCategory>(entity =>
        {
            entity.HasKey(e => e.EquipmentCategoryId).HasName("PK__Equipmen__2AE5B95065BAC7B3");

            entity.ToTable("EquipmentCategory");

            entity.Property(e => e.EquipmentCategoryId).HasColumnName("equipment_category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(255)
                .HasColumnName("category_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<EquipmentInstance>(entity =>
        {
            entity.HasKey(e => e.EquipmentInstanceId).HasName("PK__Equipmen__6957C6342C4C5733");

            entity.ToTable("EquipmentInstance");

            entity.HasIndex(e => e.AssetCode, "UQ_EquipmentInstance_asset_code").IsUnique();

            entity.Property(e => e.EquipmentInstanceId).HasColumnName("equipment_instance_id");
            entity.Property(e => e.AssetCode)
                .HasMaxLength(100)
                .HasColumnName("asset_code");
            entity.Property(e => e.ConditionLevel)
                .HasMaxLength(50)
                .HasColumnName("condition_level");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.EffectiveIntervalHour).HasColumnName("effective_interval_hour");
            entity.Property(e => e.EquipmentTypeId).HasColumnName("equipment_type_id");
            entity.Property(e => e.LastMaintenanceDate).HasColumnName("last_maintenance_date");
            entity.Property(e => e.MaintenanceCount).HasColumnName("maintenance_count");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(100)
                .HasColumnName("serial_number");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.TotalUsageHour).HasColumnName("total_usage_hour");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UsageHoursSinceLastMaintenance).HasColumnName("usage_hours_since_last_maintenance");

            entity.HasOne(d => d.EquipmentType).WithMany(p => p.EquipmentInstances)
                .HasForeignKey(d => d.EquipmentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EquipmentInstance_EquipmentType");
        });

        modelBuilder.Entity<EquipmentShortageLog>(entity =>
        {
            entity.HasKey(e => e.ShortageLogId).HasName("PK__Equipmen__606DC4EA5DBE510E");

            entity.ToTable("EquipmentShortageLog");

            entity.Property(e => e.ShortageLogId).HasColumnName("shortage_log_id");
            entity.Property(e => e.AllocationPlanId).HasColumnName("allocation_plan_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpEquipmentReqId).HasColumnName("exp_equipment_req_id");
            entity.Property(e => e.PhaseEquipmentReqId).HasColumnName("phase_equipment_req_id");
            entity.Property(e => e.ShortageQuantity).HasColumnName("shortage_quantity");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.AllocationPlan).WithMany(p => p.EquipmentShortageLogs)
                .HasForeignKey(d => d.AllocationPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EquipmentShortageLog_AllocationPlan");

            entity.HasOne(d => d.ExpEquipmentReq).WithMany(p => p.EquipmentShortageLogs)
                .HasForeignKey(d => d.ExpEquipmentReqId)
                .HasConstraintName("FK_EquipmentShortageLog_ExperimentEquipmentRequirement");

            entity.HasOne(d => d.PhaseEquipmentReq).WithMany(p => p.EquipmentShortageLogs)
                .HasForeignKey(d => d.PhaseEquipmentReqId)
                .HasConstraintName("FK_EquipmentShortageLog_PhaseEquipmentRequirement");
        });

        modelBuilder.Entity<EquipmentSubstitution>(entity =>
        {
            entity.HasKey(e => e.EquipmentSubId).HasName("PK__Equipmen__B95AF311E4E069C5");

            entity.ToTable("EquipmentSubstitution");

            entity.HasIndex(e => new { e.PrimaryEquipmentTypeId, e.SubEquipmentTypeId }, "UQ_EquipmentSubstitution").IsUnique();

            entity.Property(e => e.EquipmentSubId).HasColumnName("equipment_sub_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.EfficiencyRate).HasColumnName("efficiency_rate");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.PrimaryEquipmentTypeId).HasColumnName("primary_equipment_type_id");
            entity.Property(e => e.SubEquipmentTypeId).HasColumnName("sub_equipment_type_id");
            entity.Property(e => e.TimeMultiplier).HasColumnName("time_multiplier");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.PrimaryEquipmentType).WithMany(p => p.EquipmentSubstitutionPrimaryEquipmentTypes)
                .HasForeignKey(d => d.PrimaryEquipmentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EquipmentSubstitution_PrimaryEquipmentType");

            entity.HasOne(d => d.SubEquipmentType).WithMany(p => p.EquipmentSubstitutionSubEquipmentTypes)
                .HasForeignKey(d => d.SubEquipmentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EquipmentSubstitution_SubEquipmentType");
        });

        modelBuilder.Entity<EquipmentType>(entity =>
        {
            entity.HasKey(e => e.EquipmentTypeId).HasName("PK__Equipmen__D8B1EC058802FA02");

            entity.ToTable("EquipmentType");

            entity.Property(e => e.EquipmentTypeId).HasColumnName("equipment_type_id");
            entity.Property(e => e.AvailableQuantity).HasColumnName("available_quantity");
            entity.Property(e => e.BaseMaintenanceIntervalHours).HasColumnName("base_maintenance_interval_hours");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DamagedQuantity).HasColumnName("damaged_quantity");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EquipmentCategoryId).HasColumnName("equipment_category_id");
            entity.Property(e => e.InUseQuantity).HasColumnName("in_use_quantity");
            entity.Property(e => e.MissingQuantity).HasColumnName("missing_quantity");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ReservedQuantity).HasColumnName("reserved_quantity");
            entity.Property(e => e.TotalQuantity).HasColumnName("total_quantity");
            entity.Property(e => e.TrackingType)
                .HasMaxLength(50)
                .HasColumnName("tracking_type");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.EquipmentCategory).WithMany(p => p.EquipmentTypes)
                .HasForeignKey(d => d.EquipmentCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EquipmentType_EquipmentCategory");
        });

        modelBuilder.Entity<Experiment>(entity =>
        {
            entity.HasKey(e => e.ExperimentId).HasName("PK__Experime__38C6E36F573A4911");

            entity.ToTable("Experiment");

            entity.Property(e => e.ExperimentId).HasColumnName("experiment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Deadline).HasColumnName("deadline");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ExpectEndDate).HasColumnName("expect_end_date");
            entity.Property(e => e.ExpectStartDate).HasColumnName("expect_start_date");
            entity.Property(e => e.ExperimentName)
                .HasMaxLength(255)
                .HasColumnName("experiment_name");
            entity.Property(e => e.Priority)
                .HasDefaultValue(2)
                .HasColumnName("priority");
            entity.Property(e => e.ResearcherId).HasColumnName("researcher_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Researcher).WithMany(p => p.Experiments)
                .HasForeignKey(d => d.ResearcherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Experiment_Researcher");
        });

        modelBuilder.Entity<ExperimentEquipmentRequirement>(entity =>
        {
            entity.HasKey(e => e.ExpEquipmentReqId).HasName("PK__Experime__CF7174CDE1CB8C95");

            entity.ToTable("ExperimentEquipmentRequirement");

            entity.Property(e => e.ExpEquipmentReqId).HasColumnName("exp_equipment_req_id");
            entity.Property(e => e.AllowSubstitute).HasColumnName("allow_substitute");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.EquipmentTypeId).HasColumnName("equipment_type_id");
            entity.Property(e => e.ExperimentId).HasColumnName("experiment_id");
            entity.Property(e => e.MinAcceptableEfficiency).HasColumnName("min_acceptable_efficiency");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.EquipmentType).WithMany(p => p.ExperimentEquipmentRequirements)
                .HasForeignKey(d => d.EquipmentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExperimentEquipmentRequirement_EquipmentType");

            entity.HasOne(d => d.Experiment).WithMany(p => p.ExperimentEquipmentRequirements)
                .HasForeignKey(d => d.ExperimentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExperimentEquipmentRequirement_Experiment");
        });

        modelBuilder.Entity<ExperimentHumanRequirement>(entity =>
        {
            entity.HasKey(e => e.ExpHumanReqId).HasName("PK__Experime__F0CBE750BEEC738C");

            entity.ToTable("ExperimentHumanRequirement");

            entity.Property(e => e.ExpHumanReqId).HasColumnName("exp_human_req_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.ExperimentId).HasColumnName("experiment_id");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.RequiredSkillId).HasColumnName("required_skill_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.WorkingHoursPerDay).HasColumnName("working_hours_per_day");

            entity.HasOne(d => d.Experiment).WithMany(p => p.ExperimentHumanRequirements)
                .HasForeignKey(d => d.ExperimentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExperimentHumanRequirement_Experiment");

            entity.HasOne(d => d.RequiredSkill).WithMany(p => p.ExperimentHumanRequirements)
                .HasForeignKey(d => d.RequiredSkillId)
                .HasConstraintName("FK_ExperimentHumanRequirement_Skill");

            entity.HasOne(d => d.Role).WithMany(p => p.ExperimentHumanRequirements)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExperimentHumanRequirement_Role");
        });

        modelBuilder.Entity<ExperimentLandRequirement>(entity =>
        {
            entity.HasKey(e => e.ExpLandReqId).HasName("PK__Experime__37A1D3A05F493063");

            entity.ToTable("ExperimentLandRequirement");

            entity.Property(e => e.ExpLandReqId).HasColumnName("exp_land_req_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.ExperimentId).HasColumnName("experiment_id");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.RequiredArea)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("required_area");
            entity.Property(e => e.RequiredSoilType)
                .HasMaxLength(100)
                .HasColumnName("required_soil_type");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Experiment).WithMany(p => p.ExperimentLandRequirements)
                .HasForeignKey(d => d.ExperimentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExperimentLandRequirement_Experiment");
        });

        modelBuilder.Entity<ExperimentPhase>(entity =>
        {
            entity.HasKey(e => e.PhaseId).HasName("PK__Experime__7B3D6DF256361D87");

            entity.ToTable("ExperimentPhase");

            entity.HasIndex(e => new { e.ExperimentId, e.PhaseOrder }, "UQ_ExperimentPhase_Order").IsUnique();

            entity.Property(e => e.PhaseId).HasColumnName("phase_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpectedEndDate).HasColumnName("expected_end_date");
            entity.Property(e => e.ExpectedStartDate).HasColumnName("expected_start_date");
            entity.Property(e => e.ExperimentId).HasColumnName("experiment_id");
            entity.Property(e => e.PhaseDescription).HasColumnName("phase_description");
            entity.Property(e => e.PhaseName)
                .HasMaxLength(255)
                .HasColumnName("phase_name");
            entity.Property(e => e.PhaseOrder).HasColumnName("phase_order");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Experiment).WithMany(p => p.ExperimentPhases)
                .HasForeignKey(d => d.ExperimentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExperimentPhase_Experiment");
        });

        modelBuilder.Entity<HumanResourceProfile>(entity =>
        {
            entity.HasKey(e => e.HumanResourceId).HasName("PK__HumanRes__65E948D7EDB7FAB7");

            entity.ToTable("HumanResourceProfile");

            entity.HasIndex(e => e.UserId, "UQ_HumanResourceProfile_user_id").IsUnique();

            entity.Property(e => e.HumanResourceId).HasColumnName("human_resource_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentWorkload).HasColumnName("current_workload");
            entity.Property(e => e.MaxWorkingHoursPerDay).HasColumnName("max_working_hours_per_day");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.HumanResourceProfile)
                .HasForeignKey<HumanResourceProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HumanResourceProfile_User");
        });

        modelBuilder.Entity<HumanResourceSkill>(entity =>
        {
            entity.HasKey(e => e.HumanResourceSkillId).HasName("PK__HumanRes__5941C19A1E9625CC");

            entity.ToTable("HumanResourceSkill");

            entity.HasIndex(e => new { e.HumanResourceId, e.SkillId }, "UQ_HumanResourceSkill").IsUnique();

            entity.Property(e => e.HumanResourceSkillId).HasColumnName("human_resource_skill_id");
            entity.Property(e => e.HumanResourceId).HasColumnName("human_resource_id");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.SkillLevel)
                .HasMaxLength(50)
                .HasColumnName("skill_level");

            entity.HasOne(d => d.HumanResource).WithMany(p => p.HumanResourceSkills)
                .HasForeignKey(d => d.HumanResourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HumanResourceSkill_HumanResourceProfile");

            entity.HasOne(d => d.Skill).WithMany(p => p.HumanResourceSkills)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HumanResourceSkill_Skill");
        });

        modelBuilder.Entity<LandResource>(entity =>
        {
            entity.HasKey(e => e.LandId).HasName("PK__LandReso__E146676D7EB5E8D0");

            entity.ToTable("LandResource");

            entity.HasIndex(e => e.LandCode, "UQ_LandResource_land_code").IsUnique();

            entity.Property(e => e.LandId).HasColumnName("land_id");
            entity.Property(e => e.AreaId).HasColumnName("area_id");
            entity.Property(e => e.AreaSize)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("area_size");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.LandCode)
                .HasMaxLength(100)
                .HasColumnName("land_code");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.SoilType)
                .HasMaxLength(100)
                .HasColumnName("soil_type");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Area).WithMany(p => p.LandResources)
                .HasForeignKey(d => d.AreaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LandResource_Area");
        });

        modelBuilder.Entity<PhaseEquipmentRequirement>(entity =>
        {
            entity.HasKey(e => e.PhaseEquipmentReqId).HasName("PK__PhaseEqu__5E7BBE0EC8979919");

            entity.ToTable("PhaseEquipmentRequirement");

            entity.Property(e => e.PhaseEquipmentReqId).HasColumnName("phase_equipment_req_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.EquipmentTypeId).HasColumnName("equipment_type_id");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.PhaseId).HasColumnName("phase_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.EquipmentType).WithMany(p => p.PhaseEquipmentRequirements)
                .HasForeignKey(d => d.EquipmentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhaseEquipmentRequirement_EquipmentType");

            entity.HasOne(d => d.Phase).WithMany(p => p.PhaseEquipmentRequirements)
                .HasForeignKey(d => d.PhaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhaseEquipmentRequirement_ExperimentPhase");
        });

        modelBuilder.Entity<PhaseHumanRequirement>(entity =>
        {
            entity.HasKey(e => e.PhaseHumanReqId).HasName("PK__PhaseHum__A63F450638DA2A6E");

            entity.ToTable("PhaseHumanRequirement");

            entity.Property(e => e.PhaseHumanReqId).HasColumnName("phase_human_req_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.PhaseId).HasColumnName("phase_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.RequiredSkillId).HasColumnName("required_skill_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Phase).WithMany(p => p.PhaseHumanRequirements)
                .HasForeignKey(d => d.PhaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhaseHumanRequirement_ExperimentPhase");

            entity.HasOne(d => d.RequiredSkill).WithMany(p => p.PhaseHumanRequirements)
                .HasForeignKey(d => d.RequiredSkillId)
                .HasConstraintName("FK_PhaseHumanRequirement_Skill");

            entity.HasOne(d => d.Role).WithMany(p => p.PhaseHumanRequirements)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhaseHumanRequirement_Role");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__760965CC81CA5BAB");

            entity.ToTable("Role");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(100)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__Schedule__C46A8A6FB43CE62B");

            entity.ToTable("Schedule");

            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.AllocationPlanId).HasColumnName("allocation_plan_id");
            entity.Property(e => e.AssignedHumanResourceId).HasColumnName("assigned_human_resource_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PhaseId).HasColumnName("phase_id");
            entity.Property(e => e.Priority)
                .HasDefaultValue(2)
                .HasColumnName("priority");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.AllocationPlan).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.AllocationPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Schedule_AllocationPlan");

            entity.HasOne(d => d.AssignedHumanResource).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.AssignedHumanResourceId)
                .HasConstraintName("FK_Schedule_AssignedHumanResource");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Schedule_CreatedBy");

            entity.HasOne(d => d.Phase).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.PhaseId)
                .HasConstraintName("FK_Schedule_ExperimentPhase");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.SkillId).HasName("PK__Skill__FBBA83796C7AA4BB");

            entity.ToTable("Skill");

            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.SkillName)
                .HasMaxLength(100)
                .HasColumnName("skill_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__B9BE370F54E40BCD");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ_User_email").IsUnique();

            entity.HasIndex(e => e.Username, "UQ_User_username").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
