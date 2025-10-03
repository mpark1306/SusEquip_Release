/*
 * SusEquip Equipment Management System
 * Copyright (c) 2025 Mark Parking (mpark@dtu.dk)
 * All rights reserved. READ-ONLY SHOWCASE LICENSE.
 * See LICENSE file for full terms.
 */

using SusEquip.Data.Exceptions;
using SusEquip.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SusEquip.Data.Services
{
    public class EquipmentService : IEquipmentServiceSync
    {
        private readonly DatabaseHelper _dbHelper;
        public EquipmentService(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public void AddEntry(EquipmentData equipmentData)
        {
            if (equipmentData == null) throw new EquipmentValidationException("Equipment data cannot be null", "equipmentData");
            
            using var connection = _dbHelper.GetConnection();
            // Updated query with standardized column names
            using var command = _dbHelper.CreateCommand(connection,
                "INSERT INTO Equip (Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, " +
                "MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, PC_Name, Service_Start, Service_Ends, Note, MachineType) " +
                "VALUES (@_entryDate, @_instNo, @_creator, @_appOwner, @_status, @_serialNr, @_mac1, @_mac2, @_uUID, @_productNo, @_modelNameAndNr, @_department, @_pcName, @_serviceStart, @_serviceEnd, @_note, @_machineType)");

            command.Parameters.AddWithValue("@_entryDate", equipmentData.Entry_Date);
            command.Parameters.AddWithValue("@_instNo", equipmentData.Inst_No);
            command.Parameters.AddWithValue("@_creator", equipmentData.Creator_Initials);
            command.Parameters.AddWithValue("@_appOwner", equipmentData.App_Owner);
            command.Parameters.AddWithValue("@_status", equipmentData.Status);
            command.Parameters.AddWithValue("@_serialNr", equipmentData.Serial_No);
            command.Parameters.AddWithValue("@_mac1", equipmentData.Mac_Address1);
            command.Parameters.AddWithValue("@_mac2", equipmentData.Mac_Address2);
            command.Parameters.AddWithValue("@_uUID", equipmentData.UUID);
            command.Parameters.AddWithValue("@_productNo", equipmentData.Product_No);
            command.Parameters.AddWithValue("@_modelNameAndNr", equipmentData.Model_Name_and_No);
            command.Parameters.AddWithValue("@_department", equipmentData.Department);
            command.Parameters.AddWithValue("@_pcName", equipmentData.PC_Name);
            command.Parameters.AddWithValue("@_serviceStart", equipmentData.Service_Start);
            command.Parameters.AddWithValue("@_serviceEnd", equipmentData.Service_Ends);
            command.Parameters.AddWithValue("@_note", equipmentData.Note);
            command.Parameters.AddWithValue("@_machineType", equipmentData.MachineType);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException(ex.Message, "AddEntry", "Equip", ex);
            }
        }
        public void InsertEntry(EquipmentData ed)
        {
            if (ed == null) throw new EquipmentValidationException("Equipment data cannot be null", "ed");
            
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection,
                "INSERT INTO Equip (Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, " +
                "MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, PC_Name, " +
                "Service_Start, Service_Ends, Note, MachineType) " +
                "VALUES (@EntryDate, @InstNo, @Creator, @AppOwner, @Status, @SerialNo, @Mac1, @Mac2, " +
                "@UUID, @ProductNo, @ModelNameAndNo, @Department, @PC_Name, @ServiceStart, @ServiceEnd, " +
                "@Note, @MachineType)");

            command.Parameters.AddWithValue("@EntryDate", ed.Entry_Date);
            command.Parameters.AddWithValue("@InstNo", ed.Inst_No);
            command.Parameters.AddWithValue("@Creator", ed.Creator_Initials);
            command.Parameters.AddWithValue("@AppOwner", ed.App_Owner);
            command.Parameters.AddWithValue("@Status", ed.Status);
            command.Parameters.AddWithValue("@SerialNo", ed.Serial_No);
            command.Parameters.AddWithValue("@Mac1", ed.Mac_Address1);
            command.Parameters.AddWithValue("@Mac2", ed.Mac_Address2);
            command.Parameters.AddWithValue("@UUID", ed.UUID);
            command.Parameters.AddWithValue("@ProductNo", ed.Product_No);
            command.Parameters.AddWithValue("@ModelNameAndNo", ed.Model_Name_and_No);
            command.Parameters.AddWithValue("@Department", ed.Department);
            command.Parameters.AddWithValue("@PC_Name", ed.PC_Name);
            command.Parameters.AddWithValue("@ServiceStart", ed.Service_Start);
            command.Parameters.AddWithValue("@ServiceEnd", ed.Service_Ends);
            command.Parameters.AddWithValue("@Note", ed.Note);
            command.Parameters.AddWithValue("@MachineType", (object)ed.MachineType ?? DBNull.Value);

            try
            {
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException(ex.Message, "InsertEntry", "Equip", ex);
            }
        }

        public void UpdateLatestEntry(EquipmentData ed)
        {
            if (ed == null) throw new EquipmentValidationException("Equipment data cannot be null", "ed");
            
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, @"
                UPDATE Equip 
                SET Entry_Date = @EntryDate, 
                    creator_initials = @Creator, 
                    app_owner = @AppOwner, 
                    status = @Status, 
                    serial_no = @SerialNo, 
                    MAC_Address1 = @Mac1, 
                    MAC_Address2 = @Mac2, 
                    UUID = @UUID, 
                    Product_no = @ProductNo, 
                    model_name_and_no = @ModelNameAndNo, 
                    Department = @Department, 
                    PC_Name = @PC_Name, 
                    Service_Start = @ServiceStart, 
                    Service_Ends = @ServiceEnd, 
                    Note = @Note, 
                    MachineType = @MachineType
                WHERE Entry_Id = @EntryId");

            command.Parameters.AddWithValue("@EntryDate", ed.Entry_Date);
            command.Parameters.AddWithValue("@Creator", ed.Creator_Initials);
            command.Parameters.AddWithValue("@AppOwner", ed.App_Owner);
            command.Parameters.AddWithValue("@Status", ed.Status);
            command.Parameters.AddWithValue("@SerialNo", ed.Serial_No);
            command.Parameters.AddWithValue("@Mac1", ed.Mac_Address1);
            command.Parameters.AddWithValue("@Mac2", ed.Mac_Address2);
            command.Parameters.AddWithValue("@UUID", ed.UUID);
            command.Parameters.AddWithValue("@ProductNo", ed.Product_No);
            command.Parameters.AddWithValue("@ModelNameAndNo", ed.Model_Name_and_No);
            command.Parameters.AddWithValue("@Department", ed.Department);
            command.Parameters.AddWithValue("@PC_Name", ed.PC_Name);
            command.Parameters.AddWithValue("@ServiceStart", ed.Service_Start);
            command.Parameters.AddWithValue("@ServiceEnd", ed.Service_Ends);
            command.Parameters.AddWithValue("@Note", ed.Note);
            command.Parameters.AddWithValue("@MachineType", (object)ed.MachineType ?? DBNull.Value);
            command.Parameters.AddWithValue("@EntryId", ed.EntryId);

            try
            {
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException(ex.Message, "UpdateLatestEntry", "Equip", ex);
            }
        }


        public int GetNextInstNo()
        {
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, "SELECT MAX(inst_no) FROM Equip");

            connection.Open();
            object result = command.ExecuteScalar();
            return result != DBNull.Value ? Convert.ToInt32(result) + 1 : 1;
        }


        public void DeleteEntry(int inst_no, int entry_id)
        {
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection,
                "DELETE FROM Equip WHERE inst_no = @inst_no AND Entry_Id = @entry_id");

            command.Parameters.AddWithValue("@inst_no", inst_no);
            command.Parameters.AddWithValue("@entry_id", entry_id);

            connection.Open();
            command.ExecuteNonQuery();
        }

        public List<EquipmentData> GetEquipmentSorted(int inst_no)
        {
            List<EquipmentData> equipment = new List<EquipmentData>();
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection,
                "SELECT Entry_Id, Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, " +
                "MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, PC_Name, " +
                "Service_Start, Service_Ends, Note, MachineType " +
                "FROM Equip WHERE inst_no = @inst_no");

            command.Parameters.AddWithValue("@inst_no", inst_no);
            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                EquipmentData item = new EquipmentData
                {
                    EntryId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    Entry_Date = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Inst_No = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    Creator_Initials = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    App_Owner = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Serial_No = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    Mac_Address1 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    Mac_Address2 = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    UUID = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    Product_No = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                    Model_Name_and_No = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                    Department = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                    PC_Name = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
                    Service_Start = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
                    Service_Ends = reader.IsDBNull(15) ? string.Empty : reader.GetString(15),
                    Note = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
                    MachineType = reader.IsDBNull(17) ? string.Empty : reader.GetString(17)
                };
                equipment.Add(item);
            }
            return equipment;
        }



        public List<EquipmentData> GetEquipSortedByEntry(int inst_no)
        {
            List<EquipmentData> equipmentList = new List<EquipmentData>();
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection,
                "SELECT Entry_Id, Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, " +
                "MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, PC_Name, Service_Start, Service_Ends, Note, MachineType " +
                "FROM Equip WHERE inst_no = @inst_no AND Entry_Id = (SELECT MAX(Entry_Id) FROM Equip WHERE inst_no = @inst_no)");

            command.Parameters.AddWithValue("@inst_no", inst_no);
            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                EquipmentData item = new EquipmentData
                {
                    EntryId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    Entry_Date = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Inst_No = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    Creator_Initials = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    App_Owner = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Serial_No = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    Mac_Address1 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    Mac_Address2 = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    UUID = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    Product_No = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                    Model_Name_and_No = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                    Department = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                    PC_Name = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
                    Service_Start = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
                    Service_Ends = reader.IsDBNull(15) ? string.Empty : reader.GetString(15),
                    Note = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
                    MachineType = reader.IsDBNull(17) ? string.Empty : reader.GetString(17)
                };
                equipmentList.Add(item);
            }
            return equipmentList;
        }


        public List<EquipmentData> GetEquipment()
        {
            List<EquipmentData> devices = new List<EquipmentData>();
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection,
                "SELECT Entry_Id, Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, " +
                "MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, PC_Name, " +
                "Service_Start, Service_Ends, Note, MachineType " +
                "FROM Equip"
            );

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                EquipmentData item = new EquipmentData
                {
                    EntryId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    Entry_Date = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Inst_No = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    Creator_Initials = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    App_Owner = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Serial_No = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    Mac_Address1 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    Mac_Address2 = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    UUID = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    Product_No = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                    Model_Name_and_No = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                    Department = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                    PC_Name = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
                    Service_Start = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
                    Service_Ends = reader.IsDBNull(15) ? string.Empty : reader.GetString(15),
                    Note = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
                    MachineType = reader.IsDBNull(17) ? string.Empty : reader.GetString(17)
                };
                devices.Add(item);
            }
            return devices;
        }


        public List<MachineData> GetMachines()
        {
            List<MachineData> machines = new List<MachineData>();
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection,
                @"
                ;WITH temp AS (
                    SELECT e.inst_no, e.Entry_Id,
                           ROW_NUMBER() OVER (PARTITION BY e.inst_no ORDER BY e.Entry_Id DESC) AS rank
                    FROM Equip e
                ),
                ranked AS (
                    SELECT inst_no, Entry_Id FROM temp WHERE rank = 1
                )
                SELECT 
                    e.Entry_Id, 
                    e.Entry_Date, 
                    e.inst_no, 
                    e.creator_initials, 
                    e.app_owner, 
                    e.status, 
                    e.serial_no, 
                    e.MAC_Address1, 
                    e.MAC_Address2, 
                    e.UUID, 
                    e.Product_no, 
                    e.model_name_and_no, 
                    e.Department, 
                    e.PC_Name, 
                    e.Service_Start, 
                    e.Service_Ends, 
                    e.Note,
                    e.MachineType
                FROM Equip e
                JOIN ranked ON e.Entry_Id = ranked.Entry_Id;"
            );

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                MachineData item = new MachineData
                {
                    EntryId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    Entry_Date = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Inst_No = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    Creator_Initials = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    App_Owner = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Serial_No = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    Mac_Address1 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    Mac_Address2 = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    UUID = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    Product_No = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                    Model_Name_and_No = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                    Department = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                    PC_Name = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
                    Service_Start = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
                    Service_Ends = reader.IsDBNull(15) ? string.Empty : reader.GetString(15),
                    Note = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
                    MachineType = reader.IsDBNull(17) ? string.Empty : reader.GetString(17)
                };
                machines.Add(item);
            }

            return machines;
        }
        public List<MachineData> GetNewMachines()
        {
            List<MachineData> machines = new List<MachineData>();
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection,
                @"
        ;WITH temp AS (
            SELECT e.inst_no, e.Entry_Id,
                   ROW_NUMBER() OVER (PARTITION BY e.inst_no ORDER BY e.Entry_Id DESC) AS rank
            FROM Equip e
        ),
        ranked AS (
            SELECT inst_no, Entry_Id FROM temp WHERE rank = 1
        )
        SELECT 
            e.Entry_Id, 
            e.Entry_Date, 
            e.inst_no, 
            e.creator_initials, 
            e.app_owner, 
            e.status, 
            e.serial_no, 
            e.MAC_Address1, 
            e.MAC_Address2, 
            e.UUID, 
            e.Product_no, 
            e.model_name_and_no, 
            e.Department, 
            e.PC_Name, 
            e.Service_Start, 
            e.Service_Ends, 
            e.Note,
            e.MachineType
        FROM Equip e
        JOIN ranked ON e.Entry_Id = ranked.Entry_Id
        WHERE e.status = 'Modtaget (Ny)' 
        AND (e.Service_Ends IS NULL OR e.Service_Ends = '' OR 
             TRY_CAST(e.Service_Ends AS DATE) IS NULL OR 
             TRY_CAST(e.Service_Ends AS DATE) > GETDATE()) ;"
            );

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                MachineData item = new MachineData
                {
                    EntryId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    Entry_Date = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Inst_No = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    Creator_Initials = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    App_Owner = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Serial_No = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    Mac_Address1 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    Mac_Address2 = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    UUID = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    Product_No = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                    Model_Name_and_No = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                    Department = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                    PC_Name = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
                    Service_Start = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
                    Service_Ends = reader.IsDBNull(15) ? string.Empty : reader.GetString(15),
                    Note = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
                    MachineType = reader.IsDBNull(17) ? string.Empty : reader.GetString(17)
                };
                machines.Add(item);
            }

            return machines;
        }

        public List<MachineData> GetUsedMachines()
        {
            List<MachineData> machines = new List<MachineData>();
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection,
                @"
        ;WITH temp AS (
            SELECT e.inst_no, e.Entry_Id,
                   ROW_NUMBER() OVER (PARTITION BY e.inst_no ORDER BY e.Entry_Id DESC) AS rank
            FROM Equip e
        ),
        ranked AS (
            SELECT inst_no, Entry_Id FROM temp WHERE rank = 1
        )
        SELECT 
            e.Entry_Id, 
            e.Entry_Date, 
            e.inst_no, 
            e.creator_initials, 
            e.app_owner, 
            e.status, 
            e.serial_no, 
            e.MAC_Address1, 
            e.MAC_Address2, 
            e.UUID, 
            e.Product_no, 
            e.model_name_and_no, 
            e.Department, 
            e.PC_Name, 
            e.Service_Start, 
            e.Service_Ends, 
            e.Note,
            e.MachineType
        FROM Equip e
        JOIN ranked ON e.Entry_Id = ranked.Entry_Id
        WHERE e.status = 'På Lager (Brugt)';"
            );

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                MachineData item = new MachineData
                {
                    EntryId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    Entry_Date = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Inst_No = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    Creator_Initials = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    App_Owner = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Serial_No = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    Mac_Address1 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    Mac_Address2 = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    UUID = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    Product_No = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                    Model_Name_and_No = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                    Department = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                    PC_Name = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
                    Service_Start = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
                    Service_Ends = reader.IsDBNull(15) ? string.Empty : reader.GetString(15),
                    Note = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
                    MachineType = reader.IsDBNull(17) ? string.Empty : reader.GetString(17)
                };
                machines.Add(item);
            }

            return machines;
        }
        public List<MachineData> GetQuarantineMachines()
        {
            List<MachineData> machines = new List<MachineData>();
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection,
                @"
        ;WITH temp AS (
            SELECT e.inst_no, e.Entry_Id,
                   ROW_NUMBER() OVER (PARTITION BY e.inst_no ORDER BY e.Entry_Id DESC) AS rank
            FROM Equip e
        ),
        ranked AS (
            SELECT inst_no, Entry_Id FROM temp WHERE rank = 1
        )
        SELECT 
            e.Entry_Id, 
            e.Entry_Date, 
            e.inst_no, 
            e.creator_initials, 
            e.app_owner, 
            e.status, 
            e.serial_no, 
            e.MAC_Address1, 
            e.MAC_Address2, 
            e.UUID, 
            e.Product_no, 
            e.model_name_and_no, 
            e.Department, 
            e.PC_Name, 
            e.Service_Start, 
            e.Service_Ends, 
            e.Note,
            e.MachineType
        FROM Equip e
        JOIN ranked ON e.Entry_Id = ranked.Entry_Id
        WHERE e.status = 'Karantæne';"
            );


            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                MachineData item = new MachineData
                {
                    EntryId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    Entry_Date = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Inst_No = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    Creator_Initials = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    App_Owner = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Serial_No = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    Mac_Address1 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    Mac_Address2 = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    UUID = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    Product_No = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                    Model_Name_and_No = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                    Department = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                    PC_Name = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
                    Service_Start = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
                    Service_Ends = reader.IsDBNull(15) ? string.Empty : reader.GetString(15),
                    Note = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
                    MachineType = reader.IsDBNull(17) ? string.Empty : reader.GetString(17)
                };
                machines.Add(item);
            }

            return machines;
        }

        public List<MachineData> GetActiveMachines()
        {
            List<MachineData> machines = new List<MachineData>();
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection,
                @"
        ;WITH temp AS (
            SELECT e.inst_no, e.Entry_Id,
                   ROW_NUMBER() OVER (PARTITION BY e.inst_no ORDER BY e.Entry_Id DESC) AS rank
            FROM Equip e
        ),
        ranked AS (
            SELECT inst_no, Entry_Id FROM temp WHERE rank = 1
        )
        SELECT 
            e.Entry_Id, 
            e.Entry_Date, 
            e.inst_no, 
            e.creator_initials, 
            e.app_owner, 
            e.status, 
            e.serial_no, 
            e.MAC_Address1, 
            e.MAC_Address2, 
            e.UUID, 
            e.Product_no, 
            e.model_name_and_no, 
            e.Department, 
            e.PC_Name, 
            e.Service_Start, 
            e.Service_Ends, 
            e.Note,
            e.MachineType
        FROM Equip e
        JOIN ranked ON e.Entry_Id = ranked.Entry_Id
        WHERE e.status = 'Hos Bruger'
            AND (e.Service_Ends IS NULL OR e.Service_Ends = '' OR TRY_CAST(e.Service_Ends AS DATE) IS NULL OR TRY_CAST(e.Service_Ends AS DATE) > GETDATE());"
            );


            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                MachineData item = new MachineData
                {
                    EntryId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    Entry_Date = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Inst_No = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    Creator_Initials = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    App_Owner = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Serial_No = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    Mac_Address1 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    Mac_Address2 = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    UUID = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    Product_No = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                    Model_Name_and_No = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                    Department = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                    PC_Name = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
                    Service_Start = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
                    Service_Ends = reader.IsDBNull(15) ? string.Empty : reader.GetString(15),
                    Note = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
                    MachineType = reader.IsDBNull(17) ? string.Empty : reader.GetString(17)
                };
                machines.Add(item);
            }

            return machines;
        }




        public bool IsInstNoTaken(int instNo)
        {
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, "SELECT COUNT(*) FROM Equip WHERE inst_no = @instNo");
            command.Parameters.AddWithValue("@instNo", instNo);

            connection.Open();
            int count = (int)command.ExecuteScalar();
            return count > 0;
        }

        public bool IsSerialNoTakenInMachines(string serialNo)
        {
            var machines = GetMachines();
            return machines.Any(m => m.Serial_No == serialNo);
        }

        /// <summary>
        /// Get dashboard statistics using optimized SQL query that considers both status and Service_Ends date
        /// This provides accurate counts of truly active machines within their service period
        /// </summary>
        public (int activeCount, int newCount, int usedCount, int quarantinedCount) GetDashboardStatistics()
        {
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, @"
                WITH LatestEquipment AS (
                    SELECT e.*, ROW_NUMBER() OVER (PARTITION BY e.inst_no ORDER BY e.Entry_Id DESC) as rn
                    FROM Equip e
                )
                SELECT 
                    SUM(CASE 
                        WHEN status = 'Hos Bruger' 
                            AND (Service_Ends IS NULL OR Service_Ends = '' OR TRY_CAST(Service_Ends AS DATE) IS NULL OR TRY_CAST(Service_Ends AS DATE) > GETDATE())
                        THEN 1 ELSE 0 END) as ActiveCount,
                    SUM(CASE WHEN status = 'Modtaget (Ny)' THEN 1 ELSE 0 END) as NewCount,
                    SUM(CASE WHEN status = 'På Lager (Brugt)' THEN 1 ELSE 0 END) as UsedCount,
                    SUM(CASE WHEN status = 'Karantæne' THEN 1 ELSE 0 END) as QuarantinedCount
                FROM LatestEquipment 
                WHERE rn = 1");

            connection.Open();
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return (
                    activeCount: reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    newCount: reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    usedCount: reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    quarantinedCount: reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                );
            }

            return (0, 0, 0, 0);
        }

        /// <summary>
        /// Updates expired "Modtaget (Ny)" entries to "Kasseret" status
        /// Returns the number of records updated
        /// </summary>

        /// <summary>
        /// Gets computers whose service has been out of service since June this year
        /// </summary>
        public List<MachineData> GetMachinesOutOfServiceSinceJune()
        {
            List<MachineData> machinesOutOfService = new List<MachineData>();
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection,
                @"
                ;WITH temp AS (
                    SELECT e.inst_no, e.Entry_Id,
                           ROW_NUMBER() OVER (PARTITION BY e.inst_no ORDER BY e.Entry_Id DESC) AS rank
                    FROM Equip e
                ),
                ranked AS (
                    SELECT inst_no, Entry_Id FROM temp WHERE rank = 1
                )
                SELECT 
                    e.Entry_Id, 
                    e.Entry_Date, 
                    e.inst_no, 
                    e.creator_initials, 
                    e.app_owner, 
                    e.status, 
                    e.serial_no, 
                    e.MAC_Address1, 
                    e.MAC_Address2, 
                    e.UUID, 
                    e.Product_no, 
                    e.model_name_and_no, 
                    e.Department, 
                    e.PC_Name, 
                    e.Service_Start, 
                    e.Service_Ends, 
                    e.Note,
                    e.MachineType
                FROM Equip e
                JOIN ranked ON e.Entry_Id = ranked.Entry_Id
                WHERE TRY_CONVERT(DATE, e.Service_Ends, 120) >= '2025-06-01' 
                  AND TRY_CONVERT(DATE, e.Service_Ends, 120) < GETDATE();"
            );

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                MachineData item = new MachineData
                {
                    EntryId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    Entry_Date = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Inst_No = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    Creator_Initials = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    App_Owner = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Serial_No = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    Mac_Address1 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    Mac_Address2 = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    UUID = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    Product_No = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                    Model_Name_and_No = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                    Department = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                    PC_Name = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
                    Service_Start = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
                    Service_Ends = reader.IsDBNull(15) ? string.Empty : reader.GetString(15),
                    Note = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
                    MachineType = reader.IsDBNull(17) ? string.Empty : reader.GetString(17)
                };
                machinesOutOfService.Add(item);
            }

            return machinesOutOfService;
        }
    }   
}
