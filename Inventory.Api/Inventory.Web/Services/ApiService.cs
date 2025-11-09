using Inventory.Web.Models.ViewModels;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Inventory.Web.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        #region Authentication

        public async Task<ApiResponse<AuthResponseViewModel>> RegisterAsync(RegisterViewModel model)
        {
            return await PostAsync<RegisterViewModel, AuthResponseViewModel>("/api/auth/register", model);
        }

        public async Task<ApiResponse<AuthResponseViewModel>> LoginAsync(LoginViewModel model)
        {
            return await PostAsync<LoginViewModel, AuthResponseViewModel>("/api/auth/login", model);
        }

        #endregion

        #region Productos

        public async Task<ApiResponse<List<ProductoViewModel>>> GetProductosAsync(string? token)
        {
            return await GetAsync<List<ProductoViewModel>>("/api/productos", token);
        }

        public async Task<ApiResponse<ProductoViewModel>> GetProductoByIdAsync(int id, string? token)
        {
            return await GetAsync<ProductoViewModel>($"/api/productos/{id}", token);
        }

        public async Task<ApiResponse<ProductoViewModel>> CreateProductoAsync(CreateProductoViewModel model, string? token)
        {
            return await PostAsync<CreateProductoViewModel, ProductoViewModel>("/api/productos", model, token);
        }

        public async Task<ApiResponse<ProductoViewModel>> UpdateProductoAsync(int id, UpdateProductoViewModel model, string? token)
        {
            return await PutAsync<UpdateProductoViewModel, ProductoViewModel>($"/api/productos/{id}", model, token);
        }

        public async Task<ApiResponse<object>> DeleteProductoAsync(int id, string? token)
        {
            return await DeleteAsync<object>($"/api/productos/{id}", token);
        }

        #endregion

        #region Ventas

        public async Task<ApiResponse<List<VentaViewModel>>> GetVentasAsync(string? token)
        {
            return await GetAsync<List<VentaViewModel>>("/api/ventas", token);
        }

        public async Task<ApiResponse<VentaViewModel>> GetVentaByIdAsync(int id, string? token)
        {
            return await GetAsync<VentaViewModel>($"/api/ventas/{id}", token);
        }

        public async Task<ApiResponse<VentaViewModel>> CreateVentaAsync(CreateVentaViewModel model, string? token)
        {
            return await PostAsync<CreateVentaViewModel, VentaViewModel>("/api/ventas", model, token);
        }

        #endregion

        #region HTTP Methods

        private async Task<ApiResponse<TResponse>> GetAsync<TResponse>(string endpoint, string? token = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                AddAuthorizationHeader(request, token);

                var response = await _httpClient.SendAsync(request);
                return await ProcessResponse<TResponse>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<TResponse>
                {
                    Success = false,
                    ErrorMessage = $"Error de conexión: {ex.Message}"
                };
            }
        }

        private async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, string? token = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json")
                };
                AddAuthorizationHeader(request, token);

                var response = await _httpClient.SendAsync(request);
                return await ProcessResponse<TResponse>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<TResponse>
                {
                    Success = false,
                    ErrorMessage = $"Error de conexión: {ex.Message}"
                };
            }
        }

        private async Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, string? token = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
                {
                    Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json")
                };
                AddAuthorizationHeader(request, token);

                var response = await _httpClient.SendAsync(request);
                return await ProcessResponse<TResponse>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<TResponse>
                {
                    Success = false,
                    ErrorMessage = $"Error de conexión: {ex.Message}"
                };
            }
        }

        private async Task<ApiResponse<TResponse>> DeleteAsync<TResponse>(string endpoint, string? token = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
                AddAuthorizationHeader(request, token);

                var response = await _httpClient.SendAsync(request);
                return await ProcessResponse<TResponse>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<TResponse>
                {
                    Success = false,
                    ErrorMessage = $"Error de conexión: {ex.Message}"
                };
            }
        }

        private void AddAuthorizationHeader(HttpRequestMessage request, string? token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private async Task<ApiResponse<T>> ProcessResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                return new ApiResponse<T>
                {
                    Success = true,
                    Data = data,
                    StatusCode = (int)response.StatusCode
                };
            }
            else
            {
                // Intentar extraer mensaje de error de la respuesta
                string errorMessage;
                try
                {
                    var errorObj = JsonSerializer.Deserialize<Dictionary<string, object>>(content, _jsonOptions);
                    errorMessage = errorObj?.ContainsKey("message") == true
                        ? errorObj["message"].ToString() ?? "Error desconocido"
                        : $"Error: {response.StatusCode}";
                }
                catch
                {
                    errorMessage = $"Error: {response.StatusCode}";
                }

                return new ApiResponse<T>
                {
                    Success = false,
                    ErrorMessage = errorMessage,
                    StatusCode = (int)response.StatusCode
                };
            }
        }

        #endregion
    }
}

