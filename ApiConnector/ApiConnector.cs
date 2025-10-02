using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;

namespace SharpForm.ApiConnector
{

    public partial class ApiConnector : Form
    {
        private static readonly HttpClient client = new HttpClient();
        public ApiConnector()
        {
            InitializeComponent();
        }

        public async Task<T?> GetDataAsync<T>(string url) where T : class
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // 2xx 상태 코드가 아닌 경우 예외 발생
                string responseBody = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // JSON 속성 이름의 대소문자를 구분하지 않음
                };

                return JsonSerializer.Deserialize<T>(responseBody, options);
            }
            catch (HttpRequestException e)
            {
                // 네트워크 또는 서버 오류 처리
                MessageBox.Show($"HTTP 요청 오류: {e.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (JsonException e)
            {
                // JSON 파싱 오류 처리
                MessageBox.Show($"JSON 파싱 오류: {e.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private  void button1_Click(object sender, EventArgs e)
        {
            
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            string apiUrl = "";
            try
            {
                // 1. JSON 파일 읽기
                string executablePath = Application.StartupPath;
                string jsonContent = File.ReadAllText(Path.Combine(executablePath, "UrlList.json"));

                // 2. JSON 파싱 및 'echo' 키의 값 추출
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("echo", out JsonElement echoElement))
                    {
                        apiUrl = echoElement.GetString();
                    }
                }

                if (string.IsNullOrEmpty(apiUrl))
                {
                    MessageBox.Show("UrlList.json 파일에서 'echo' URL을 찾을 수 없습니다.", "구성 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode(); // 2xx가 아닌 상태 코드일 경우 예외를 발생시킵니다.

                // 응답 본문을 문자열로 읽어옵니다.
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // 원본 JSON 문자열을 MessageBox에 표시합니다.
                MessageBox.Show(jsonResponse, "API 응답 (Raw JSON)");
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("URL 설정 파일(ApiConnector/UrlList.json)을 찾을 수 없습니다.", "파일 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (JsonException jsonEx)
            {
                MessageBox.Show($"JSON 파일 파싱 오류: {jsonEx.Message}", "파싱 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (HttpRequestException ex)
            {
                // 네트워크 또는 서버 오류를 처리합니다.
                MessageBox.Show($"HTTP 요청 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
