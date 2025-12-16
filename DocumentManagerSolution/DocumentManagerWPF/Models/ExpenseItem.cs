namespace DocumentManagerWPF.Models
{
    public class ExpenseItem
    {
        // 경고 CS8618 해결: null이 아님을 보장하기 위해 빈 문자열로 초기화합니다.
        public string ItemName { get; set; } = string.Empty;  // 항목 (예: 소모품)
        public decimal Amount { get; set; }   // 금액 (decimal은 null 허용 타입이 아니므로 경고 없음)

        // 경고 CS8618 해결
        public string Description { get; set; } = string.Empty; // 비고 (예: 사무용품 구입)
    }
}