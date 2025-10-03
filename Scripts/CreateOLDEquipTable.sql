-- Script to create OLD_Equip table for old machines
-- This table will store old machines that contain "ENV" in PC_Name or Department
-- and will have special Inst_No format like "O-{number}"

CREATE TABLE OLD_Equip
(
    Entry_Date        nvarchar(255),
    Inst_No           nvarchar(50),      -- Changed from int to nvarchar to support "O-{number}" format
    Creator_Initials  nvarchar(10),
    App_Owner         nvarchar(20),
    Status            nvarchar(80),
    serial_no         nvarchar(255),
    MAC_Address1      nvarchar(255),
    MAC_Address2      nvarchar(255),
    UUID              nvarchar(255),
    Product_No        nvarchar(255),
    model_name_and_no nvarchar(255),
    Department        nvarchar(255),
    PC_Name           nvarchar(255),
    Service_Start     nvarchar(255),
    Service_Ends      nvarchar(255),
    Note              nvarchar(255),
    Entry_ID          int identity,
    MachineType       nvarchar(50)
);

-- Create similar indexes for performance
CREATE INDEX IX_OLD_Equip_InstNo_EntryId ON OLD_Equip (Inst_No, Entry_ID);
CREATE INDEX IX_OLD_Equip_SerialNo ON OLD_Equip (serial_no);
CREATE INDEX IX_OLD_Equip_Status ON OLD_Equip (Status);
CREATE INDEX IX_OLD_Equip_Modtaget ON OLD_Equip (Status, Service_Ends) 
    INCLUDE (Entry_ID, Inst_No, PC_Name, App_Owner)
    WHERE [status] = 'Modtaget';
CREATE INDEX IX_OLD_Equip_PaLager ON OLD_Equip (Service_Ends) 
    INCLUDE (Entry_ID, Inst_No, PC_Name, App_Owner, Status)
    WHERE ([status] IN ('På Lager', 'På Lager (Brugt)')) AND ([PC_Name] IN ('', 'N/ A')) AND ([app_owner] IN ('', 'N/ A'));
CREATE INDEX IX_OLD_Equip_HosBruger ON OLD_Equip (Status) 
    INCLUDE (Entry_ID, Inst_No, PC_Name, App_Owner, Service_Start, Service_Ends);
CREATE INDEX IX_OLD_Equip_Karantæne ON OLD_Equip (Status) 
    INCLUDE (Entry_ID, Inst_No, PC_Name, App_Owner, Service_Start, Service_Ends);
