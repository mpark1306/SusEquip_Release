-- Simple query to update expired "Modtaget (Ny)" entries to "Kasseret"
-- Run this directly on your database

INSERT INTO Equip (Entry_Date, Inst_No, Creator_Initials, App_Owner, Status, serial_no, MAC_Address1, MAC_Address2, UUID, Product_No, model_name_and_no, Department, PC_Name, Service_Start, Service_Ends, Note, MachineType)
SELECT CONVERT(VARCHAR(10), GETDATE(), 120) + ' ' + CONVERT(VARCHAR(8), GETDATE(), 108), Inst_No, Creator_Initials, App_Owner, 'Kasseret', serial_no, MAC_Address1, MAC_Address2, UUID, Product_No, model_name_and_no, Department, PC_Name, Service_Start, Service_Ends, 'Auto-updated: Service expired', MachineType
FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY Inst_No ORDER BY Entry_ID DESC) AS rn FROM Equip) e
WHERE rn = 1 AND Status = 'Modtaget (Ny)' AND Service_Ends IS NOT NULL AND Service_Ends != '' AND ISDATE(Service_Ends) = 1 AND CAST(Service_Ends AS DATE) < CAST(GETDATE() AS DATE);
