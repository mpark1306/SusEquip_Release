// using System.ComponentModel.DataAnnotations;
//
// namespace SusEquip.Data.Models
// {
//     public class MachineTypeModel
//     {
//         // Primary key (Identity)
//         public int ModelId { get; set; }
//
//         [StringLength(50)]
//         public string Firm { get; set; }
//
//         [StringLength(255)]
//         public string ModelName { get; set; }
//
//         [Required(ErrorMessage = "Type is required.")]
//         [StringLength(50)]
//         public string Type { get; set; }
//
//         // Bit columns default to false (0)
//         public bool Modified { get; set; }
//
//         [StringLength(255)]
//         public string ModSpecifics { get; set; }
//
//         public bool NonStandard { get; set; }
//
//         [StringLength(255)]
//         public string NonStandardSpecifics { get; set; }
//
//         [Required(ErrorMessage = "RAM is required.")]
//         public int Ram { get; set; }
//
//         public bool Gpu { get; set; }
//
//         [StringLength(50)]
//         public string GpuSpecifics { get; set; }
//
//         [Required(ErrorMessage = "Drive GB is required.")]
//         public int DriveGB { get; set; }
//
//         [StringLength(10)]
//         public string DriveType { get; set; }
//
//         public int? StorageGB { get; set; }
//         
//         // StorageType is an INT in DB; you can decide if you want an enum or just int?
//         public int? StorageType { get; set; }
//
//         public bool IsTouch { get; set; }
//
//         // Default 15 in DB, but for required logic in the form, you can do:
//         [Required(ErrorMessage = "Screen Inch is required.")]
//         public int ScreenInch { get; set; } = 15;  // default to 15
//
//         [StringLength(20)]
//         public string KeyboardLanguage { get; set; }
//
//         public bool DedicatedStorage { get; set; }
//     }
// }