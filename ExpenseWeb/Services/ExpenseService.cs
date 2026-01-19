using ExpenseWeb.Models;
using ExpenseWeb.Services.Contracts;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ExpenseWeb.Services
{ 
    public class ExpenseService : IExpenseService
    {
        private readonly IHttpClientFactory _clientFactory;
        private const string apiEndpoint = "/api/expense/";
        private readonly JsonSerializerOptions _options;

        public ExpenseService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<PagedResult<ExpenseViewModel>> GetExpensesPaged(ExpenseQueryParameters parameters)
        {
            var client = _clientFactory.CreateClient("ExpenseApi");

            var query = BuildQueryString(parameters);
            var response = await client.GetAsync($"{apiEndpoint}?{query}");
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadAsStreamAsync();
                var pagedDto = await JsonSerializer.DeserializeAsync<PagedResult<ExpenseApiDto>>(apiResponse, _options);

                if (pagedDto == null)
                {
                    return new PagedResult<ExpenseViewModel>
                    {
                        Page = parameters.Page,
                        PageSize = parameters.PageSize,
                        TotalCount = 0,
                        Items = Enumerable.Empty<ExpenseViewModel>()
                    };
                }

                var items = pagedDto.Items.Select(dto => new ExpenseViewModel
                {
                    ExpenseId = dto.ExpenseId,
                    Date = dto.StartDate,
                    Name = dto.Name ?? string.Empty,
                    Amount = dto.TotalAmount,
                    Installments = dto.Installments,
                    Validity = dto.Validity ?? default(DateTime),
                    Status = dto.Status ?? string.Empty,
                    NoteImageUrl = dto.NoteImageUrl,
                    ProofImageUrl = dto.ProofImageUrl,
                    CategoryId = dto.CategoryId,
                    CategoryName = dto.CategoryName,
                    // Preencher ReferenceMonth e ReferenceYear baseado na data
                    ReferenceMonth = dto.StartDate.Month,
                    ReferenceYear = dto.StartDate.Year
                }).ToList();

                return new PagedResult<ExpenseViewModel>
                {
                    Page = pagedDto.Page,
                    PageSize = pagedDto.PageSize,
                    TotalCount = pagedDto.TotalCount,
                    Items = items
                };
            }
            
            return new PagedResult<ExpenseViewModel>
            {
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalCount = 0,
                Items = Enumerable.Empty<ExpenseViewModel>()
            };
        }

        public async Task<ExpenseViewModel?> GetExpenseById(int id)
        {
            var client = _clientFactory.CreateClient("ExpenseApi");

            var response = await client.GetAsync(apiEndpoint + id);
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadAsStreamAsync();
                var expenseDto = await JsonSerializer.DeserializeAsync<ExpenseApiDto>(apiResponse, _options);
                
                if (expenseDto == null)
                    return null;

                // Mapear ExpenseApiDto para ExpenseViewModel
                return new ExpenseViewModel
                {
                    ExpenseId = expenseDto.ExpenseId,
                    Date = expenseDto.StartDate,
                    Name = expenseDto.Name ?? string.Empty,
                    Amount = expenseDto.TotalAmount,
                    Installments = expenseDto.Installments,
                    Validity = expenseDto.Validity ?? default(DateTime),
                    Status = expenseDto.Status ?? string.Empty,
                    NoteImageUrl = expenseDto.NoteImageUrl,
                    ProofImageUrl = expenseDto.ProofImageUrl,
                    CategoryId = expenseDto.CategoryId,
                    CategoryName = expenseDto.CategoryName,
                    // Preencher ReferenceMonth e ReferenceYear baseado na data
                    ReferenceMonth = expenseDto.StartDate.Month,
                    ReferenceYear = expenseDto.StartDate.Year
                };
            }
            
            return null;
        }

        public async Task<ExpenseViewModel?> CreateExpense(ExpenseViewModel expenseVM)
        {
            var client = _clientFactory.CreateClient("ExpenseApi");

            // Garantir que Installments seja pelo menos 1
            var installments = expenseVM.Installments > 0 ? expenseVM.Installments : 1;

            // Mapear ExpenseViewModel para o formato esperado pela API (ExpenseDTO)
            var expenseDto = new
            {
                ExpenseId = expenseVM.ExpenseId,
                StartDate = expenseVM.Date,
                Name = expenseVM.Name,
                TotalAmount = expenseVM.Amount,
                Installments = installments,
                InstallmentAmount = expenseVM.Amount / installments,
                Validity = expenseVM.Validity == default(DateTime) ? (DateTime?)null : expenseVM.Validity,
                Status = expenseVM.Status,
                NoteImageUrl = expenseVM.NoteImageUrl,
                ProofImageUrl = expenseVM.ProofImageUrl,
                CategoryId = expenseVM.CategoryId
            };

            StringContent content = new StringContent(JsonSerializer.Serialize(expenseDto), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiEndpoint, content);
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadAsStreamAsync();
                var createdExpenseDto = await JsonSerializer.DeserializeAsync<ExpenseApiDto>(apiResponse, _options);
                
                if (createdExpenseDto == null)
                    return null;

                // Mapear ExpenseApiDto para ExpenseViewModel
                return new ExpenseViewModel
                {
                    ExpenseId = createdExpenseDto.ExpenseId,
                    Date = createdExpenseDto.StartDate,
                    Name = createdExpenseDto.Name ?? string.Empty,
                    Amount = createdExpenseDto.TotalAmount,
                    Installments = createdExpenseDto.Installments,
                    Validity = createdExpenseDto.Validity ?? default(DateTime),
                    Status = createdExpenseDto.Status ?? string.Empty,
                    NoteImageUrl = createdExpenseDto.NoteImageUrl,
                    ProofImageUrl = createdExpenseDto.ProofImageUrl,
                    CategoryId = createdExpenseDto.CategoryId,
                    CategoryName = createdExpenseDto.CategoryName,
                    // Preencher ReferenceMonth e ReferenceYear baseado na data
                    ReferenceMonth = createdExpenseDto.StartDate.Month,
                    ReferenceYear = createdExpenseDto.StartDate.Year
                };
            }
            
            // Capturar mensagem de erro da API
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Erro ao criar despesa: {response.StatusCode} - {errorContent}"
            );
        }

        public async Task<ExpenseViewModel> UpdateExpense(ExpenseViewModel expenseVM)
        {
            var client = _clientFactory.CreateClient("ExpenseApi");

            // Garantir que Installments seja pelo menos 1
            var installments = expenseVM.Installments > 0 ? expenseVM.Installments : 1;

            // Mapear ExpenseViewModel para o formato esperado pela API (ExpenseDTO)
            var expenseDto = new
            {
                ExpenseId = expenseVM.ExpenseId,
                StartDate = expenseVM.Date,
                Name = expenseVM.Name,
                TotalAmount = expenseVM.Amount,
                Installments = installments,
                InstallmentAmount = expenseVM.Amount / installments,
                Validity = expenseVM.Validity == default(DateTime) ? (DateTime?)null : expenseVM.Validity,
                Status = expenseVM.Status,
                NoteImageUrl = expenseVM.NoteImageUrl,
                ProofImageUrl = expenseVM.ProofImageUrl,
                CategoryId = expenseVM.CategoryId
            };

            var json = JsonSerializer.Serialize(expenseDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(apiEndpoint + expenseVM.ExpenseId, content);
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadAsStreamAsync();
                var expenseUpdatedDto = await JsonSerializer.DeserializeAsync<ExpenseApiDto>(apiResponse, _options);
                
                if (expenseUpdatedDto == null)
                    return null;

                // Mapear ExpenseApiDto para ExpenseViewModel
                return new ExpenseViewModel
                {
                    ExpenseId = expenseUpdatedDto.ExpenseId,
                    Date = expenseUpdatedDto.StartDate,
                    Name = expenseUpdatedDto.Name ?? string.Empty,
                    Amount = expenseUpdatedDto.TotalAmount,
                    Installments = expenseUpdatedDto.Installments,
                    Validity = expenseUpdatedDto.Validity ?? default(DateTime),
                    Status = expenseUpdatedDto.Status ?? string.Empty,
                    NoteImageUrl = expenseUpdatedDto.NoteImageUrl,
                    ProofImageUrl = expenseUpdatedDto.ProofImageUrl,
                    CategoryId = expenseUpdatedDto.CategoryId,
                    CategoryName = expenseUpdatedDto.CategoryName,
                    // Preencher ReferenceMonth e ReferenceYear baseado na data
                    ReferenceMonth = expenseUpdatedDto.StartDate.Month,
                    ReferenceYear = expenseUpdatedDto.StartDate.Year
                };
            }
            
            return null;
        }

        public async Task<bool> DeleteExpense(int id)
        {
            var client = _clientFactory.CreateClient("ExpenseApi");

            using (var response = await  client.DeleteAsync(apiEndpoint + id))
            {
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private static string BuildQueryString(ExpenseQueryParameters parameters)
        {
            var query = new List<string>
            {
                $"page={parameters.Page}",
                $"pageSize={parameters.PageSize}"
            };

            if (parameters.From.HasValue)
                query.Add($"from={Uri.EscapeDataString(parameters.From.Value.ToString("yyyy-MM-dd"))}");

            if (parameters.To.HasValue)
                query.Add($"to={Uri.EscapeDataString(parameters.To.Value.ToString("yyyy-MM-dd"))}");

            if (parameters.CategoryId.HasValue)
                query.Add($"categoryId={parameters.CategoryId.Value}");

            if (!string.IsNullOrWhiteSpace(parameters.Status))
                query.Add($"status={Uri.EscapeDataString(parameters.Status)}");

            if (!string.IsNullOrWhiteSpace(parameters.Search))
                query.Add($"search={Uri.EscapeDataString(parameters.Search)}");

            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                query.Add($"sortBy={Uri.EscapeDataString(parameters.SortBy)}");

            if (!string.IsNullOrWhiteSpace(parameters.SortDir))
                query.Add($"sortDir={Uri.EscapeDataString(parameters.SortDir)}");

            return string.Join("&", query);
        }
    }
}
