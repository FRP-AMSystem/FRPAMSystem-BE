CREATE TABLE EquipmentHandover (
    handover_id INT IDENTITY(1,1) PRIMARY KEY,

    allocation_equipment_detail_id INT NOT NULL,

    equipment_instance_id INT NULL,

    handed_over_by INT NOT NULL,
    received_by INT NOT NULL,

    handover_date DATETIME2 NOT NULL,

    quantity INT NOT NULL,

    condition_before NVARCHAR(100) NULL,
    note NVARCHAR(MAX) NULL,

    status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    confirmed_at DATETIME2 NULL,

    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,

    CONSTRAINT FK_EquipmentHandover_AllocationEquipmentDetail
        FOREIGN KEY (allocation_equipment_detail_id)
        REFERENCES AllocationEquipmentDetail(allocation_equipment_detail_id),

    CONSTRAINT FK_EquipmentHandover_EquipmentInstance
        FOREIGN KEY (equipment_instance_id)
        REFERENCES EquipmentInstance(equipment_instance_id),

    CONSTRAINT FK_EquipmentHandover_HandedOverBy
        FOREIGN KEY (handed_over_by)
        REFERENCES [User](user_id),

    CONSTRAINT FK_EquipmentHandover_ReceivedBy
        FOREIGN KEY (received_by)
        REFERENCES [User](user_id),

    CONSTRAINT CK_EquipmentHandover_Quantity
        CHECK (quantity > 0)
);
CREATE TABLE EquipmentReturn (
    return_id INT IDENTITY(1,1) PRIMARY KEY,

    allocation_equipment_detail_id INT NOT NULL,

    equipment_instance_id INT NULL,

    returned_by INT NOT NULL,
    received_by INT NOT NULL,

    return_date DATETIME2 NOT NULL,

    quantity INT NOT NULL,

    condition_after NVARCHAR(100) NOT NULL,

    is_damaged BIT NOT NULL DEFAULT 0,
    damage_description NVARCHAR(MAX) NULL,

    note NVARCHAR(MAX) NULL,

    status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    confirmed_at DATETIME2 NULL,

    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,

    CONSTRAINT FK_EquipmentReturn_AllocationEquipmentDetail
        FOREIGN KEY (allocation_equipment_detail_id)
        REFERENCES AllocationEquipmentDetail(allocation_equipment_detail_id),

    CONSTRAINT FK_EquipmentReturn_EquipmentInstance
        FOREIGN KEY (equipment_instance_id)
        REFERENCES EquipmentInstance(equipment_instance_id),

    CONSTRAINT FK_EquipmentReturn_ReturnedBy
        FOREIGN KEY (returned_by)
        REFERENCES [User](user_id),

    CONSTRAINT FK_EquipmentReturn_ReceivedBy
        FOREIGN KEY (received_by)
        REFERENCES [User](user_id),

    CONSTRAINT CK_EquipmentReturn_Quantity
        CHECK (quantity > 0),

    CONSTRAINT CK_EquipmentReturn_DamageDescription
        CHECK (
            is_damaged = 0
            OR damage_description IS NOT NULL
        )
);
CREATE TABLE EquipmentExtensionRequest (
    extension_request_id INT IDENTITY(1,1) PRIMARY KEY,

    allocation_equipment_detail_id INT NOT NULL,

    requested_by INT NOT NULL,

    original_end_date DATETIME2 NOT NULL,
    requested_end_date DATETIME2 NOT NULL,

    reason NVARCHAR(MAX) NOT NULL,

    status NVARCHAR(50) NOT NULL DEFAULT 'Pending',

    reviewed_by INT NULL,
    reviewed_at DATETIME2 NULL,

    rejection_reason NVARCHAR(MAX) NULL,

    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,

    CONSTRAINT FK_EquipmentExtensionRequest_AllocationEquipmentDetail
        FOREIGN KEY (allocation_equipment_detail_id)
        REFERENCES AllocationEquipmentDetail(allocation_equipment_detail_id),

    CONSTRAINT FK_EquipmentExtensionRequest_RequestedBy
        FOREIGN KEY (requested_by)
        REFERENCES [User](user_id),

    CONSTRAINT FK_EquipmentExtensionRequest_ReviewedBy
        FOREIGN KEY (reviewed_by)
        REFERENCES [User](user_id),

    CONSTRAINT CK_EquipmentExtensionRequest_EndDate
        CHECK (requested_end_date > original_end_date)
);
CREATE TABLE EquipmentChangeRequest (
    change_request_id INT IDENTITY(1,1) PRIMARY KEY,

    allocation_equipment_detail_id INT NOT NULL,

    current_equipment_instance_id INT NULL,

    requested_equipment_type_id INT NOT NULL,

    requested_equipment_instance_id INT NULL,

    requested_by INT NOT NULL,

    reason NVARCHAR(MAX) NOT NULL,

    status NVARCHAR(50) NOT NULL DEFAULT 'Pending',

    reviewed_by INT NULL,
    reviewed_at DATETIME2 NULL,

    rejection_reason NVARCHAR(MAX) NULL,

    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,

    CONSTRAINT FK_EquipmentChangeRequest_AllocationEquipmentDetail
        FOREIGN KEY (allocation_equipment_detail_id)
        REFERENCES AllocationEquipmentDetail(allocation_equipment_detail_id),

    CONSTRAINT FK_EquipmentChangeRequest_CurrentEquipment
        FOREIGN KEY (current_equipment_instance_id)
        REFERENCES EquipmentInstance(equipment_instance_id),

    CONSTRAINT FK_EquipmentChangeRequest_RequestedEquipmentType
        FOREIGN KEY (requested_equipment_type_id)
        REFERENCES EquipmentType(equipment_type_id),

    CONSTRAINT FK_EquipmentChangeRequest_RequestedEquipmentInstance
        FOREIGN KEY (requested_equipment_instance_id)
        REFERENCES EquipmentInstance(equipment_instance_id),

    CONSTRAINT FK_EquipmentChangeRequest_RequestedBy
        FOREIGN KEY (requested_by)
        REFERENCES [User](user_id),

    CONSTRAINT FK_EquipmentChangeRequest_ReviewedBy
        FOREIGN KEY (reviewed_by)
        REFERENCES [User](user_id)
);