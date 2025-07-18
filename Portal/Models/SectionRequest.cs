﻿using Microsoft.Extensions.Options;
using Portal.Interfaces;
using Portal.Shared.Models.DTOs.Shared;
using Portal.Shared.Models.ViewModel;

namespace Portal.Models
{
    public class SectionRequest : BaseRequest, ISectionRequest
    {
        public SectionRequest(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
            : base(httpClient, apiSettings) { }

        public async Task<IEnumerable<SectionViewModel>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(_apiSettings.SectionAll);
            var apiResponse = await HandleResponse<IEnumerable<SectionViewModel>>(response);
            return apiResponse.Data ?? Enumerable.Empty<SectionViewModel>();
        }

        public async Task<SectionViewModel> GetByIdAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.SectionSearch, id);
            var response = await _httpClient.GetAsync(endpoint);
            var apiResponse = await HandleResponse<SectionViewModel>(response);
            return apiResponse.Data;
        }

        public async Task<IEnumerable<SectionViewModel>> GetByDepartmentIdAsync(int departmentId)
        {
            var endpoint = string.Format(_apiSettings.SectionsByDepartment, departmentId);
            var response = await _httpClient.GetAsync(endpoint);
            var apiResponse = await HandleResponse<IEnumerable<SectionViewModel>>(response);
            return apiResponse.Data ?? Enumerable.Empty<SectionViewModel>();
        }

        public async Task<ApiResponse<object>> CreateAsync(SectionViewModel viewModel)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiSettings.SectionSave, viewModel);
            return await HandleResponse<object>(response);
        }

        public async Task<ApiResponse<object>> UpdateAsync(int id, SectionViewModel viewModel)
        {
            var response = await _httpClient.PutAsJsonAsync(_apiSettings.SectionSave, viewModel);
            return await HandleResponse<object>(response);
        }

        public async Task<ApiResponse<object>> DeleteAsync(int id)
        {
            var endpoint = string.Format(_apiSettings.SectionDelete, id);
            var response = await _httpClient.DeleteAsync(endpoint);
            return await HandleResponse<object>(response);
        }
    }
}