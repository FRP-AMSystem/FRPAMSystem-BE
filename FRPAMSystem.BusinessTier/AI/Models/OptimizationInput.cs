using FRPAMSystem.DataTier.Models;

namespace FRPAMSystem.BusinessTier.AI.Models
{
    public class OptimizationInput
    {
        public Experiment Experiment { get; set; } = null!;

        public IReadOnlyCollection<ExperimentPhase> ExperimentPhases { get; set; } = Array.Empty<ExperimentPhase>();

        public IReadOnlyCollection<LandResource> LandResources { get; set; } = Array.Empty<LandResource>();

        public IReadOnlyCollection<HumanResourceProfile> HumanResources { get; set; } = Array.Empty<HumanResourceProfile>();

        public IReadOnlyCollection<EquipmentInstance> EquipmentInstances { get; set; } = Array.Empty<EquipmentInstance>();

        public IReadOnlyCollection<Skill> Skills { get; set; } = Array.Empty<Skill>();

        public IReadOnlyCollection<ExperimentLandRequirement> ExperimentLandRequirements { get; set; } = Array.Empty<ExperimentLandRequirement>();

        public IReadOnlyCollection<ExperimentHumanRequirement> ExperimentHumanRequirements { get; set; } = Array.Empty<ExperimentHumanRequirement>();

        public IReadOnlyCollection<ExperimentEquipmentRequirement> ExperimentEquipmentRequirements { get; set; } = Array.Empty<ExperimentEquipmentRequirement>();

        public IReadOnlyCollection<PhaseHumanRequirement> PhaseHumanRequirements { get; set; } = Array.Empty<PhaseHumanRequirement>();

        public IReadOnlyCollection<PhaseEquipmentRequirement> PhaseEquipmentRequirements { get; set; } = Array.Empty<PhaseEquipmentRequirement>();

        public IReadOnlyCollection<Schedule> ExistingSchedules { get; set; } = Array.Empty<Schedule>();

        public IReadOnlyCollection<AllocationLandDetail> ExistingLandAllocations { get; set; } = Array.Empty<AllocationLandDetail>();

        public IReadOnlyCollection<AllocationHumanDetail> ExistingHumanAllocations { get; set; } = Array.Empty<AllocationHumanDetail>();

        public IReadOnlyCollection<AllocationEquipmentDetail> ExistingEquipmentAllocations { get; set; } = Array.Empty<AllocationEquipmentDetail>();

        public IReadOnlyCollection<EquipmentSubstitution> EquipmentSubstitutions { get; set; } = Array.Empty<EquipmentSubstitution>();

        public OptimizationSettings Settings { get; set; } = new();
    }
}
