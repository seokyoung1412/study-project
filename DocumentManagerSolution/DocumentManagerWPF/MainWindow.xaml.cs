using DocumentManagerWPF.Services;
using DocumentManagerWPF.Models;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Net.Http;
// System.Windows.Media 네임스페이스는 기본적으로 사용하지만, 
// Brushes, Color 사용 시 모호성 문제로 인해 아래 코드에서 명시적으로 사용합니다.
// using System.Windows.Media; 

namespace DocumentManagerWPF
{
    public partial class MainWindow : Window
    {
        // TotalAmountTextBlock에 표시될 한글 금액과 내부적으로 계산된 숫자 금액
        private decimal currentTotalAmountValue = 50000m;
        private string currentTotalAmountHangeul = "일금 오만 원정";

        private readonly DocumentApiService _api = new DocumentApiService();
        private const string DocumentType = "DS";


        public MainWindow()
        {
            InitializeComponent();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // 초기 5줄 지출 항목 생성
            InitializeExpenseItems(5);

            // 초기 합계 텍스트 설정
            UpdateTotalAmount();
        }

        // --------------------------------------------------------
        // 합계 계산 및 UI 업데이트 로직
        // --------------------------------------------------------
        private void UpdateTotalAmount()
        {
            decimal total = 0;

            // (기존의 금액 합산 로직 생략)

            // StackPanel의 모든 자식(Border)을 순회하여 금액 합산
            foreach (var borderChild in ExpenseItemsStackPanel.Children)
            {
                if (borderChild is System.Windows.Controls.Border border && border.Child is Grid rowGrid)
                {
                    // Grid 내의 컨트롤을 순회하여 금액 TextBox (Column 3)를 찾습니다.
                    foreach (var control in rowGrid.Children)
                    {
                        if (control is TextBox textBox && Grid.GetColumn(textBox) == 3)
                        {
                            // 콤마 제거 후 파싱
                            if (decimal.TryParse(textBox.Text.Replace(",", "").Replace(" ", ""), out decimal amount))
                            {
                                total += amount;
                            }
                        }
                    }
                }
            }

            // 합계 및 한글 합계 변수 업데이트
            currentTotalAmountValue = total;
            string hangeul = NumberToHangeul(total);
            currentTotalAmountHangeul = $"일금 {hangeul} 원정";

            // UI 업데이트 (이 부분이 핵심 수정 사항입니다.)

            // 1. 상단 (한글 금액) 업데이트: 한글 금액을 할당합니다.
            TotalAmountTextBlock.Text = currentTotalAmountHangeul;

            // 2. 상단 (숫자 금액 - 헤더) 업데이트: 숫자 금액을 원화 포맷으로 할당합니다.
            // XAML에서 TotalAmountValueTextBlockHeader의 Text="{Binding ElementName=...}" 바인딩이 없으므로 직접 값을 할당합니다.
            TotalAmountValueTextBlockHeader.Text = $"₩ {total.ToString("#,##0")} 원"; // 예: ₩ 10,005 원

            // 3. 하단 (숫자 금액 - 푸터) 업데이트: 숫자 금액을 콤마 포맷으로 할당합니다.
            TotalAmountValueTextBlockFooter.Text = total.ToString("#,##0"); // 예: 10,005
        }

        // --------------------------------------------------------
        // 동적 항목의 금액 변경 이벤트 핸들러
        // --------------------------------------------------------
        private void AmountTextBox_TextChanged_Dynamic(object sender, TextChangedEventArgs e)
        {
            // 금액 입력 형식 자동 포맷팅 
            if (sender is TextBox textBox)
            {
                string originalText = textBox.Text;
                string cleanText = originalText.Replace(",", "").Replace(" ", "");

                if (decimal.TryParse(cleanText, out decimal value))
                {
                    // TextChanged 이벤트 재진입 방지
                    textBox.TextChanged -= AmountTextBox_TextChanged_Dynamic;
                    textBox.Text = value.ToString("#,##0");
                    textBox.SelectionStart = textBox.Text.Length;
                    textBox.TextChanged += AmountTextBox_TextChanged_Dynamic;
                }
            }

            UpdateTotalAmount();
        }

        // --------------------------------------------------------
        // 초기 5줄 생성 및 줄 추가 로직
        // --------------------------------------------------------
        private void InitializeExpenseItems(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (i == 0)
                {
                    // 초기값 설정
                    AddExpenseRow("소모품", 50000, "사무용품 구입");
                }
                else
                {
                    AddExpenseRow("", 0, "");
                }
            }
        }

        // XAML에 정의된 이벤트 핸들러
        private void AddRowButton_Click(object sender, RoutedEventArgs e)
        {
            AddExpenseRow("", 0, "");
        }

        // --------------------------------------------------------
        // 동적 UI 요소 생성 로직 (AddExpenseRow)
        // --------------------------------------------------------
        private void AddExpenseRow(string itemName, decimal amount, string description)
        {
            Grid rowGrid = new Grid();
            rowGrid.Height = 25;

            // XAML의 헤더와 동일하게 12개 Column 정의를 시뮬레이션
            for (int i = 0; i < 12; i++)
            {
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }

            // 1. 항목 TextBox (Column 0, Span 3)
            TextBox itemTextBox = CreateTextBox(itemName, new Thickness(0), true, TextAlignment.Center);
            Grid.SetColumn(itemTextBox, 0);
            Grid.SetColumnSpan(itemTextBox, 3);

            // 2. 금액 TextBox (Column 3, Span 4)
            TextBox amountTextBox = CreateTextBox(amount.ToString("#,##0"), new Thickness(0), false, TextAlignment.Right);
            Grid.SetColumn(amountTextBox, 3);
            Grid.SetColumnSpan(amountTextBox, 4);
            amountTextBox.TextChanged += AmountTextBox_TextChanged_Dynamic;

            // 3. 비고 TextBox (Column 7, Span 5)
            TextBox descriptionTextBox = CreateTextBox(description, new Thickness(0), true, TextAlignment.Left);
            Grid.SetColumn(descriptionTextBox, 7);
            Grid.SetColumnSpan(descriptionTextBox, 5);

            // Grid에 컨트롤 추가
            rowGrid.Children.Add(itemTextBox);
            rowGrid.Children.Add(amountTextBox);
            rowGrid.Children.Add(descriptionTextBox);

            // Border로 감싸서 테두리 표시 
            System.Windows.Controls.Border rowBorder = new System.Windows.Controls.Border
            {
                BorderBrush = System.Windows.Media.Brushes.Gray, // 모호성 해결: System.Windows.Media.Brushes 사용 명시
                BorderThickness = new Thickness(0, 0, 0, 1), // 아래쪽에만 선을 그립니다.
                Child = rowGrid
            };

            ExpenseItemsStackPanel.Children.Add(rowBorder);
        }

        // --------------------------------------------------------
        // TextBox 생성 도우미 함수
        // --------------------------------------------------------
        private TextBox CreateTextBox(string text, Thickness margin, bool isKoreanInput, TextAlignment alignment = TextAlignment.Center)
        {
            TextBox textBox = new TextBox
            {
                Text = text,
                Margin = margin,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                BorderThickness = new Thickness(0), // 내부 Border 제거
                TextAlignment = alignment,
                Padding = new Thickness(5, 0, 5, 0) // 내부 여백 추가
            };

            if (isKoreanInput)
            {
                InputMethod.SetIsInputMethodEnabled(textBox, true);
                InputMethod.SetPreferredImeState(textBox, InputMethodState.On);
            }
            else
            {
                InputMethod.SetPreferredImeState(textBox, InputMethodState.Off);
            }

            return textBox;
        }

        // --------------------------------------------------------
        // 한글 금액 변환 함수
        // --------------------------------------------------------
        private string NumberToHangeul(decimal number)
        {
            if (number == 0) return "영";

            // 소수점 이하 버림
            long longNumber = (long)Math.Truncate(number);
            if (longNumber < 0) return "음수는 처리하지 않습니다.";

            string[] unit = { "", "만", "억", "조", "경" };
            string[] numStr = { "", "일", "이", "삼", "사", "오", "육", "칠", "팔", "구" };
            string[] posStr = { "", "십", "백", "천" }; // 4자리 단위 내에서의 위치

            string s = longNumber.ToString();
            string result = "";
            string currentBlockHangeul = ""; // 현재 4자리 블록의 한글 변환 결과
            int unitIndex = 0; // 단위 (만, 억, 조...) 인덱스

            for (int i = s.Length - 1; i >= 0; i--)
            {
                int n = s[i] - '0';
                int pos = (s.Length - 1 - i) % 4; // 4자리 블록 내에서의 위치 (0:1, 1:10, 2:100, 3:1000)

                // 1. 현재 자리의 한글 문자열 생성
                string tempHangeul = "";
                if (n > 0)
                {
                    // 4자리 블록 내에서 10, 100, 1000 자리일 때 '일'을 생략 (예: 일십 -> 십)
                    string currentNumStr = (n == 1 && pos > 0) ? "" : numStr[n];
                    tempHangeul = currentNumStr + posStr[pos];
                }

                // 2. 현재 4자리 블록의 한글에 추가
                currentBlockHangeul = tempHangeul + currentBlockHangeul;

                if (pos == 3 || i == 0) // 4자리 블록의 끝 (천의 자리) 또는 맨 앞자리 처리 후
                {
                    // 현재 4자리 블록 (일~천)에 숫자가 있고, '만', '억' 등의 단위가 있을 때 단위를 붙임
                    if (!string.IsNullOrEmpty(currentBlockHangeul))
                    {
                        // 현재 4자리 블록의 한글 변환 결과에 단위(만, 억, 조...)를 붙여 전체 결과에 추가
                        // 만, 억, 조... 단위는 현재 블록의 한글 변환 결과 뒤에 붙어야 합니다.
                        result = currentBlockHangeul + unit[unitIndex] + result;
                    }

                    // 다음 블록을 위해 준비
                    currentBlockHangeul = ""; // 현재 블록 한글 초기화
                    unitIndex++; // 다음 단위로 이동 (만 -> 억 -> 조...)
                }
            }

            // 최종 정리: 100000000 (일억) 이 '억'으로 변환될 수 있도록 '일' 제거 로직이 필요할 수 있으나,
            // 위 로직에서는 pos=3에서 단위가 붙으므로 '일'이 붙지 않습니다.
            // 만약 `10000`의 결과가 '일만'이 나온다면, 아래 처리를 추가합니다.
            // if (result.StartsWith("일") && result.Length > 1)
            // {
            //     result = result.Substring(1);
            // }

            return result.Trim();
        }

        // --------------------------------------------------------
        // [XAML Event] 문서 번호 생성 (DocumentNumberTextBlock_MouseLeftButtonDown)
        // --------------------------------------------------------
        private async void DocumentNumberTextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DocumentNumberTextBlock.Text = "[API 호출 중...]";
            await GenerateDocumentNumberAsync();
        }

        private async Task GenerateDocumentNumberAsync()
        {
            try
            {
                string departmentCode = DepartmentCodeTextBox.Text.ToUpper();

                // DocumentApiService 호출 (API 서버에서 DocumentNumberResult 객체를 받음)
                var result = await _api.GetNextDocumentNumberAsync(DocumentType, departmentCode);

                // 🚨 유효성 검사 🚨
                // 1. API 호출 자체가 실패하여 result가 null인 경우
                // 2. API 응답 객체는 받았으나, 핵심 필드인 DocumentNumber가 null이거나 비어있는 경우
                if (result == null || string.IsNullOrEmpty(result.DocumentNumber))
                {
                    throw new Exception("API 응답에 유효한 문서 번호가 포함되어 있지 않습니다. (API 서버 응답 오류 또는 데이터 누락)");
                }

                string documentNumber = result.DocumentNumber; // non-null 보장
                DocumentNumberTextBlock.Text = documentNumber;

                MessageBox.Show($"문서 번호가 API를 통해 생성되었습니다: {documentNumber}", "번호 생성 완료");
            }
            catch (Exception ex)
            {
                DocumentNumberTextBlock.Text = "[오류: API 호출 실패]";

                // HttpRequestException일 경우, 서버 연결 실패 가능성을 명시
                string errorMessage = ex is System.Net.Http.HttpRequestException
          ? "API 서버에 연결할 수 없습니다. 서버가 실행 중인지, 주소(http://localhost:8080)가 올바른지 확인하세요."
          : $"문서 번호 API 호출 중 오류 발생: {ex.Message}";

                MessageBox.Show(errorMessage, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --------------------------------------------------------
        // [XAML Event] 엑셀 저장 (ExportToExcel_Click)
        // --------------------------------------------------------
        private async void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            string documentNumber = DocumentNumberTextBlock.Text;

            // 문서 번호가 없거나 오류 상태이면, 생성을 시도합니다.
            if (documentNumber.Contains("문서번호") || documentNumber.Contains("오류") || string.IsNullOrWhiteSpace(documentNumber))
            {
                await GenerateDocumentNumberAsync();
                documentNumber = DocumentNumberTextBlock.Text;

                if (documentNumber.Contains("문서번호") || documentNumber.Contains("오류"))
                {
                    MessageBox.Show("문서 번호 생성에 실패하여 엑셀 저장을 취소합니다.", "저장 취소");
                    return;
                }
            }

            try
            {
                await ExportToExcelAsync(documentNumber);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"엑셀 저장 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --------------------------------------------------------
        // 엑셀 내보내기 로직 (요구사항 42가지 반영)
        // -------------------------------------------------------
        private async Task ExportToExcelAsync(string documentNumber)
        {
            // 1. 파일 저장 경로 설정 (SaveFileDialog)
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"{documentNumber}_지출결의서.xlsx"
            };

            if (saveFileDialog.ShowDialog() != true)
            {
                MessageBox.Show("엑셀 저장이 취소되었습니다.", "취소");
                return;
            }

            FileInfo fileInfo = new FileInfo(saveFileDialog.FileName);

            // StackPanel에서 모든 지출 항목 데이터 추출
            List<ExpenseItem> expenseItems = GetExpenseItemsFromUI();

            int startDataRow = 10;
            int maxRows = 26; // A10부터 A26까지 (17줄)
            int numRowsToPrint = Math.Min(expenseItems.Count, maxRows - startDataRow + 1);

            // 실제 데이터가 출력되는 마지막 행
            int lastDataRow = startDataRow + numRowsToPrint - 1;

            // 합계 행 위치 (데이터 바로 다음 행)
            int totalRow = lastDataRow + 1;

            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets.Add("지출결의서");

                // ** A. 열 너비 및 기본 스타일 설정 **
                worksheet.DefaultColWidth = 10;
                worksheet.Column(1).Width = 7;
                worksheet.Column(2).Width = 8;
                worksheet.Column(3).Width = 8;
                worksheet.Column(4).Width = 12;
                worksheet.Column(5).Width = 12;
                worksheet.Column(6).Width = 12;
                worksheet.Column(7).Width = 12;
                worksheet.Column(8).Width = 12;
                worksheet.Column(9).Width = 12;
                worksheet.Column(10).Width = 10;
                worksheet.Column(11).Width = 10;
                worksheet.Column(12).Width = 10;

                worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // 기본 정렬을 중앙으로 설정
                worksheet.Cells.Style.Font.Size = 10;

                // ** 1. 제목 및 결재 라인 (A1:L2) **

                // 1. A1:D2 셀 병합, 지출결의서 굵게 24pt
                worksheet.Cells["A1:D2"].Merge = true;
                worksheet.Cells["A1"].Value = "지출결의서";
                worksheet.Cells["A1"].Style.Font.Size = 24;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // 결재 라인 (E1:L2) 헤더
                worksheet.Cells["E1:E2"].Merge = true; // 2. E1,E2 기안
                worksheet.Cells["E1"].Value = "기안";
                worksheet.Cells["F1:G1"].Merge = true; // 3. F1,G1 기안자
                worksheet.Cells["F1"].Value = "기안자";
                worksheet.Cells["F2:G2"].Merge = true; // 4. F2,G2 공란 (사인/이름)
                worksheet.Cells["H1:H2"].Merge = true; // 5. H1,H2 결재
                worksheet.Cells["H1"].Value = "결재";
                worksheet.Cells["I1"].Value = "검토"; // 6. I1 검토
                worksheet.Cells["J1:K1"].Merge = true; // 7. J1,K1 확인
                worksheet.Cells["J1"].Value = "확인";
                worksheet.Cells["L1"].Value = "승인"; // 8. L1 승인
                // 9. I2, 10. J2:K2, 11. L2 공란 (사인/이름)
                worksheet.Cells["J2:K2"].Merge = true;

                worksheet.Cells["E1:L2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                // ** 2. 일금 영 원정 및 상단 합계 (Row 3) **

                // 12. A3:G3 셀 병합, 일금 {합계(한글로)} 원정 (Right)
                worksheet.Cells["A3:G3"].Merge = true;
                worksheet.Cells["A3"].Value = TotalAmountTextBlock.Text;
                worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // 13. H3:L3 셀 병합, \ [합계] (Right, 원화 포맷)
                worksheet.Cells["H3:L3"].Merge = true;
                worksheet.Cells["H3"].Value = currentTotalAmountValue;
                worksheet.Cells["H3"].Style.Numberformat.Format = "\\ ₩#,##0"; // 원화 기호와 숫자 포맷
                worksheet.Cells["H3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // ** 3. 문서 정보 테이블 (A4:L7) **

                // Row 4: 구분 / 문서번호 / 품의부서코드
                worksheet.Cells["A4"].Value = "구분";
                worksheet.Cells["B4:C4"].Merge = true;
                // 15. DivisionTextBox 값
                if (DivisionComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    worksheet.Cells["B4"].Value = selectedItem.Content.ToString();
                } 
                worksheet.Cells["D4:F4"].Merge = true;
                worksheet.Cells["D4"].Value = "문서번호";
                worksheet.Cells["G4:I4"].Merge = true;
                worksheet.Cells["G4"].Value = documentNumber; // 17. documentNumber 값 (Right)
                worksheet.Cells["G4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells["J4:K4"].Merge = true;
                worksheet.Cells["J4"].Value = "품의부서코드";
                worksheet.Cells["L4"].Value = DepartmentCodeTextBox.Text; // 19. DepartmentCodeTextBox 값 (Right)
                worksheet.Cells["L4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;


                // Row 5: 품의 / 품의인 / 처리
                worksheet.Cells["A5"].Value = "품의";
                worksheet.Cells["B5:C5"].Merge = true;
                worksheet.Cells["B5"].Value = SubmissionTextBox.Text; // 21. SubmissionTextBox (날짜)
                worksheet.Cells["D5:F5"].Merge = true;
                worksheet.Cells["D5"].Value = "품의인";
                worksheet.Cells["G5:I5"].Merge = true;
                worksheet.Cells["G5"].Value = SubmissionNameTextBox.Text; // 23. SubmissionNameTextBox 값 (Right)
                worksheet.Cells["G5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells["J5"].Value = "처리";
                worksheet.Cells["K5:L5"].Merge = true;
                worksheet.Cells["K5"].Value = IscompleteTextBox.Text; // 25. IscompleteTextBox 값 (Right)
                worksheet.Cells["K5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // Row 6: 결재 / 결의인 / 계정과목 (헤더)
                worksheet.Cells["A6"].Value = "결재";
                worksheet.Cells["B6:C6"].Merge = true;
                worksheet.Cells["B6"].Value = ApprovalTextBox.Text; // 27. ApprovalTextBox (날짜)
                worksheet.Cells["D6:F6"].Merge = true;
                worksheet.Cells["D6"].Value = "결의인";
                worksheet.Cells["G6:I6"].Merge = true;
                worksheet.Cells["G6"].Value = ApprovalNameTextBox.Text; // 29. ApprovalNameTextBox 값 (Right)
                worksheet.Cells["G6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells["J6:L6"].Merge = true;
                worksheet.Cells["J6"].Value = "계정과목";

                // Row 7: 지출 / 지출인 / 계정과목 (실제 값)
                worksheet.Cells["A7"].Value = "지출";
                worksheet.Cells["B7:C7"].Merge = true;
                worksheet.Cells["B7"].Value = ExpenditureTextBox.Text; // 32. ExpenditureTextBox (날짜)
                worksheet.Cells["D7:F7"].Merge = true;
                worksheet.Cells["D7"].Value = "지출인";
                worksheet.Cells["G7:I7"].Merge = true;
                worksheet.Cells["G7"].Value = ExpenditureNameTextBox.Text; // 34. ExpenditureNameTextBox 값 (Right)
                worksheet.Cells["G7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells["J7:L7"].Merge = true;
                worksheet.Cells["J7"].Value = AccountSubjectTextBox.Text; // 35. AccountSubjectTextBox 값 (Right)
                worksheet.Cells["J7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // 문서 정보 테이블 (A4:L7) 전체 셀에 중앙 정렬 적용 (우측 정렬은 위에서 별도 지정)
                worksheet.Cells["A4:L7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A4:A7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // A열 헤더 중앙

                // ** 4. 지출 내역 헤더 (Row 8, 9) **

                // 36. A8:L8 셀 병합, 지 출 내 역 (Center, Gray Background)
                worksheet.Cells["A8:L8"].Merge = true;
                worksheet.Cells["A8"].Value = "지 출 내 역";
                worksheet.Cells["A8:L8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A8:L8"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["A8:L8"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); // 모호성 해결: System.Drawing.Color 사용 명시

                // 37. A9:C9 셀 병합, 항목 (Center, Gray Background)
                worksheet.Cells["A9:C9"].Merge = true;
                worksheet.Cells["A9"].Value = "항 목";

                // 38. D9:G9 셀 병합, 금액 (Center, Gray Background)
                worksheet.Cells["D9:G9"].Merge = true;
                worksheet.Cells["D9"].Value = "금 액";

                // 39. H9:L9 셀 병합, 비고 (Center, Gray Background)
                worksheet.Cells["H9:L9"].Merge = true;
                worksheet.Cells["H9"].Value = "비 고";

                worksheet.Cells["A9:L9"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A9:L9"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["A9:L9"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); // 모호성 해결: System.Drawing.Color 사용 명시


                // ** 5. 지출 내역 데이터 (A10:L{totalRow-1}) - 동적 데이터 반영 **

                // 실제 데이터가 출력될 최대 행까지 반복 (A10 ~ A26)
                for (int i = 0; i < maxRows - startDataRow + 1; i++)
                {
                    int currentRow = startDataRow + i;
                    worksheet.Row(currentRow).Height = 20;

                    // 병합 설정
                    worksheet.Cells[$"A{currentRow}:C{currentRow}"].Merge = true;
                    worksheet.Cells[$"D{currentRow}:G{currentRow}"].Merge = true;
                    worksheet.Cells[$"H{currentRow}:L{currentRow}"].Merge = true;

                    // 정렬 설정
                    worksheet.Cells[$"A{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // 항목 중앙
                    worksheet.Cells[$"D{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right; // 금액 우측
                    worksheet.Cells[$"H{currentRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left; // 비고 좌측
                    worksheet.Row(currentRow).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    if (i < expenseItems.Count)
                    {
                        var item = expenseItems[i];

                        // 항목 (ItemName)
                        worksheet.Cells[$"A{currentRow}"].Value = item.ItemName;

                        // 금액 (Amount)
                        if (item.Amount > 0)
                        {
                            worksheet.Cells[$"D{currentRow}"].Value = item.Amount;
                            worksheet.Cells[$"D{currentRow}"].Style.Numberformat.Format = "#,##0";
                        }

                        // 비고 (Description)
                        worksheet.Cells[$"H{currentRow}"].Value = item.Description;
                    }
                }

                // ** 6. 합계 (A{totalRow}:L{totalRow}) **

                // 40. A{totalRow}:C{totalRow} 셀 병합, 합계 (Center, Gray Background)
                worksheet.Cells[$"A{totalRow}:C{totalRow}"].Merge = true;
                worksheet.Cells[$"A{totalRow}"].Value = "합계";
                worksheet.Cells[$"A{totalRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[$"A{totalRow}:L{totalRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[$"A{totalRow}:L{totalRow}"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); // 모호성 해결: System.Drawing.Color 사용 명시

                // 41. D{totalRow}:G{totalRow} 셀 병합, {자동계산된 합계} (Right, Number format)
                worksheet.Cells[$"D{totalRow}:G{totalRow}"].Merge = true;
                worksheet.Cells[$"D{totalRow}"].Value = currentTotalAmountValue;
                worksheet.Cells[$"D{totalRow}"].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[$"D{totalRow}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // 42. H{totalRow}:L{totalRow} 셀 병합, 공란
                worksheet.Cells[$"H{totalRow}:L{totalRow}"].Merge = true;

                // ** 7. 모든 셀에 얇은 테두리 적용 (A1:L{totalRow}) **
                var allCells = worksheet.Cells[$"A1:L{totalRow}"];
                allCells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // 3. 파일 저장
                await package.SaveAsync();
            }

            MessageBox.Show($"엑셀 파일이 성공적으로 저장되었습니다: {fileInfo.FullName}", "저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // --------------------------------------------------------
        // UI에서 지출 항목 데이터 추출 로직
        // --------------------------------------------------------
        private List<ExpenseItem> GetExpenseItemsFromUI()
        {
            List<ExpenseItem> items = new List<ExpenseItem>();

            foreach (var borderChild in ExpenseItemsStackPanel.Children)
            {
                if (borderChild is System.Windows.Controls.Border border && border.Child is Grid rowGrid)
                {
                    // 열 인덱스를 기반으로 TextBox 찾기 (AddExpenseRow와 일치해야 함)
                    var itemTextBox = rowGrid.Children.OfType<TextBox>().FirstOrDefault(t => Grid.GetColumn(t) == 0); // Column 0 (Span 3)
                    var amountTextBox = rowGrid.Children.OfType<TextBox>().FirstOrDefault(t => Grid.GetColumn(t) == 3); // Column 3 (Span 4)
                    var descriptionTextBox = rowGrid.Children.OfType<TextBox>().FirstOrDefault(t => Grid.GetColumn(t) == 7); // Column 7 (Span 5)

                    if (itemTextBox != null && amountTextBox != null && descriptionTextBox != null)
                    {
                        decimal amount = 0;
                        decimal.TryParse(amountTextBox.Text.Replace(",", "").Replace(" ", ""), out amount);

                        // 항목, 금액, 비고 중 하나라도 입력되면 유효한 항목으로 간주
                        if (!string.IsNullOrWhiteSpace(itemTextBox.Text) || amount > 0 || !string.IsNullOrWhiteSpace(descriptionTextBox.Text))
                        {
                            // Models.ExpenseItem 사용
                            items.Add(new ExpenseItem
                            {
                                ItemName = itemTextBox.Text.Trim(),
                                Amount = amount,
                                Description = descriptionTextBox.Text.Trim()
                            });
                        }
                    }
                }
            }
            return items;
        }
    }

    // API 서비스를 정의하지 않았을 경우를 대비한 더미 클래스 (컴파일 오류 방지용)
    public class DocumentApiResult
    {
        public string DocumentNumber { get; set; } = string.Empty;
    }

    //public class DummyApiService
    //{
    //    private int _callCount = 0;
    //    public Task<DocumentApiResult> GetNextDocumentNumberAsync(string documentType, string departmentCode)
    //    {
    //        // API 호출 시점의 날짜와 순번을 이용하여 문서 번호 생성
    //        _callCount++;
    //        string year = DateTime.Now.Year.ToString().Substring(2, 2);
    //        string month = DateTime.Now.Month.ToString("00");
    //        string number = _callCount.ToString("000");

    //        return Task.FromResult(new DocumentApiResult
    //        {
    //            DocumentNumber = $"{documentType}-{departmentCode}-{year}{month}-{number}"
    //        });
    //    }
    //}
}