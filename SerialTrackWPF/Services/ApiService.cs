﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using numeryseryjneWPF.Models;
using SerialTrackWPF;

namespace numeryseryjneWPF.Services
{
    internal class ApiService
    {
        private readonly HttpClient _client;
        public ApiService(string baseUrl)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(baseUrl);
        }
        public async Task<List<SerialNumber>> GetSerialsAsync()
        {
            var responce = await _client.GetAsync("/api/SerialNumber");
            responce.EnsureSuccessStatusCode();
            var json = await responce.Content.ReadAsStringAsync();
            Debug.WriteLine(json);
            return JsonSerializer.Deserialize<List<SerialNumber>>(json);
        }


        public async Task<SerialNumber> GenerateSerialNumberAsync(string product)
        {
            try {
                var responce = await _client.PostAsync($"/api/SerialNumber/generate/{product}", null);
                responce.EnsureSuccessStatusCode();
                var json = await responce.Content.ReadAsStringAsync();
                Debug.WriteLine(json);
                return JsonSerializer.Deserialize<SerialNumber>(json);
            } catch (Exception ex)
            {
                Debug.WriteLine($"Error podczas generowania numeru seryjnego: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteSerialNumberAsync(string serialnumber)
        {
            try
            {
                var response = await _client.DeleteAsync($"/api/SerialNumber/{serialnumber}");
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error podczas usuwania numeru seryjnego: {ex.Message}");
                return false;
            }
        }


        public async Task<bool> UpdateSerialNumberAsync(SerialItem item)
        {
            try
            {
                string url = $"api/SerialNumber/{item.SerialNumber}";
                var content = new StringContent(
                    JsonSerializer.Serialize(item),
                    Encoding.UTF8,
                    "application/json");
                
                var response = await _client.PutAsync(url, content);
                Debug.WriteLine(response);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd w UpdateSerialItemAsync: {ex.Message}");
                return false;
            }
        }


    }
}
