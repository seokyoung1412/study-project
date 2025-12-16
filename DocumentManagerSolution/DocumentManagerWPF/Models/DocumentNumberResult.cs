// DocumentManagerWPF/Models/DocumentNumberResult.cs
namespace DocumentManagerWPF.Models
{
    // API 응답 구조와 일치해야 합니다.
    public class DocumentNumberResult
    {
        public string? DocumentNumber { get; set; }
        public int Sequence { get; set; }
    }
}