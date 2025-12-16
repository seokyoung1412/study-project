// DocumentManagerWPF/Services/DocumentApiService.cs
using DocumentManagerWPF.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace DocumentManagerWPF.Services
{
    public class DocumentApiService
    {
        private readonly HttpClient _httpClient;

        public DocumentApiService()
        {
            // API 서버가 실행 중인 주소와 포트로 설정합니다.
            // Docker를 통해 http://localhost:8080으로 노출됩니다.
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8080/")
            };
        }

        // 문서 번호 생성 API를 호출합니다.
        public async Task<DocumentNumberResult?> GetNextDocumentNumberAsync(string documentType, string departmentCode)
        {
            try
            {
                string url = $"api/documentnumber/{documentType}/{departmentCode}";

                // HTTP GET 요청을 보내고 JSON 응답을 역직렬화합니다.
                var result = await _httpClient.GetFromJsonAsync<DocumentNumberResult>(url);

                return result;
            }
            catch (HttpRequestException ex)
            {
                // API 서버가 꺼져 있거나 연결에 실패한 경우 처리
                Console.WriteLine($"API 호출 중 오류 발생: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"예상치 못한 오류: {ex.Message}");
                return null;
            }
        }
    }
}