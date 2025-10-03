namespace SusEquip.Data.Models
{
    public class DTUPC_NewDevice
    {
        public string DeviceName { get; set; }
        public string MacAddress { get; set; }
        public string UUIDAddress { get; set; }
        public string OSDresourceID { get; set; }
        public string variable { get; set; }      // OU path
        public string PrimaryUsers { get; set; }
    }
}
