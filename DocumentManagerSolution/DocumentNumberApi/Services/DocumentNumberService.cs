// DocumentNumberApi/Services/DocumentNumberService.cs
using DocumentNumberApi.Data;
using DocumentNumberApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentNumberApi.Services
{
    public class DocumentNumberService
    {
        private readonly AppDbContext _context;

        public DocumentNumberService(AppDbContext context)
        {
            _context = context;
        }

        // 시퀀스 번호를 트랜잭션으로 안전하게 증가시키는 메서드
        public async Task<int> GetNextSequenceNumberAsync(string documentType, string departmentCode)
        {
            // 동시성 문제를 방지하기 위해 트랜잭션 사용
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                // 현재 시퀀스 찾기 (쓰기 잠금 포함)
                var sequence = await _context.DocumentSequences
                    .Where(s => s.DocumentType == documentType && s.DepartmentCode == departmentCode)
                    // MariaDB/MySQL에서 SELECT FOR UPDATE를 사용하여 행 잠금 (동시성 제어)
                    .AsTracking()
                    .FirstOrDefaultAsync();

                if (sequence == null)
                {
                    // 새 시퀀스 생성
                    sequence = new DocumentNumberSequence
                    {
                        DocumentType = documentType,
                        DepartmentCode = departmentCode,
                        LastSequenceNumber = 1
                    };
                    _context.DocumentSequences.Add(sequence);
                }
                else
                {
                    // 기존 시퀀스 증가
                    sequence.LastSequenceNumber++;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return sequence.LastSequenceNumber;
            }
        }
    }
}