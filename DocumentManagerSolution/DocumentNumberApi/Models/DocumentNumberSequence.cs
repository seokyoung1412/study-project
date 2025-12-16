// DocumentNumberApi/Models/DocumentNumberSequence.cs
using System.ComponentModel.DataAnnotations;

namespace DocumentNumberApi.Models
{
    public class DocumentNumberSequence
    {
        [Key] // EF Core가 복합키를 더 명확하게 처리하도록 Key 속성 추가 (선택 사항)
        [MaxLength(255)] // 🚨 이 줄 추가
        public required string DocumentType { get; set; }

        [Key] // EF Core가 복합키를 더 명확하게 처리하도록 Key 속성 추가 (선택 사항)
        [MaxLength(255)] // 🚨 이 줄 추가
        public required string DepartmentCode { get; set; }

        public int LastSequenceNumber { get; set; }
    }
}