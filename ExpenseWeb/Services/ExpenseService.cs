using ExpenseWeb.Models;
using ExpenseWeb.Services.Contracts;
using System.Text;
using System.Text.Json;

namespace ExpenseWeb.Services
{ 
    public class ExpenseService : IExpenseService
    {
        private readonly IHttpClientFactory _clientFactory;
        private const string apiEndpoint = "/api/expense/";
        private readonly JsonSerializerOptions _options;
        private ExpenseViewModel expenseVM;
        private IEnumerable<ExpenseViewModel> expensesVM;

        public ExpenseService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        }

        

        public async Task<IEnumerable<ExpenseViewModel>> GetAllExpenses()
        {
            var client = _clientFactory.CreateClient("ExpenseApi");

            using (var response = await client.GetAsync(apiEndpoint)) {
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadAsStreamAsync();
                    expensesVM = JsonSerializer.Deserialize<IEnumerable<ExpenseViewModel>>(apiResponse, _options);
                }
                else
                {
                    return null;
                }
            }
            return expensesVM;
        }

        public async Task<ExpenseViewModel> GetExpenseById(int id)
        {
            var client = _clientFactory.CreateClient("ExpenseApi");

            using (var response = await client.GetAsync(apiEndpoint + id))
            {
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadAsStreamAsync();
                    expensesVM = JsonSerializer.Deserialize<IEnumerable<ExpenseViewModel>>(apiResponse, _options);
                }
                else
                {
                    return null;
                }
            }
            return expenseVM;
        }

        public async Task<ExpenseViewModel> CreateExpense(ExpenseViewModel expenseVM)
        {
            var client = _clientFactory.CreateClient("ExpenseApi");

            StringContent content = new StringContent(JsonSerializer.Serialize(expenseVM), Encoding.UTF8, "application/json");

            using (var response = await client.PostAsync(apiEndpoint, content))
            {
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadAsStreamAsync();
                    expenseVM = JsonSerializer.Deserialize<ExpenseViewModel>(apiResponse, _options);
                }
                else
                {
                    return null;
                }
            }
            return expenseVM;
        }

        public async Task<ExpenseViewModel> UpdateExpense(ExpenseViewModel expenseVM)
        {
            var client = _clientFactory.CreateClient("ExpenseApi");

            ExpenseViewModel expenseUpdated = new ExpenseViewModel();

            using (var response = await client.PutAsJsonAsync(apiEndpoint, expenseVM))
            {
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadAsStreamAsync();
                    expenseUpdated = JsonSerializer.Deserialize<ExpenseViewModel>(apiResponse, _options);
                }
                else
                {
                    return null;
                }
            }
            return expenseUpdated;

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

    }
}
