using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProductApp.Entity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ProductApp.Controllers
{
    public class TasksController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TasksController> _logger;

        public TasksController(IHttpClientFactory httpClientFactory, ILogger<TasksController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("TasksApi");
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Attempting to get tasks from API");
                
                // BaseAddress already has a trailing slash, so no need to add anything
                var response = await _httpClient.GetAsync("");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API returned non-success status code: {response.StatusCode}, Content: {errorContent}");
                    ViewBag.ErrorMessage = $"API Error: {response.StatusCode} - {errorContent}";
                    return View("Error");
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Received tasks from API: {json}");
                var tasks = JsonConvert.DeserializeObject<List<TodoTask>>(json);
                
                // Current month for the calendar view
                ViewBag.CurrentMonth = DateTime.Now.Month;
                ViewBag.CurrentYear = DateTime.Now.Year;
                
                return View(tasks ?? new List<TodoTask>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting tasks");
                
                // Get detailed inner exception information
                var detailedError = GetDetailedExceptionMessage(ex);
                ViewBag.ErrorMessage = detailedError;
                
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(string name, DateTime? dueDate)
        {
            try
            {
                var task = new TodoTask { 
                    Name = name,
                    Title = name, // Use name for title if no separate field
                    Description = "Task created from web application", // Provide a default description
                    DueDate = dueDate
                };
                
                // Format due date for logging
                string dueDateFormatted = dueDate.HasValue 
                    ? dueDate.Value.ToString("yyyy-MM-dd") 
                    : "No due date";
                
                _logger.LogInformation($"Adding new task: {name} with due date: {dueDateFormatted}");
                
                // BaseAddress already has trailing slash
                var response = await _httpClient.PostAsJsonAsync("", task);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API returned non-success status code: {response.StatusCode}, Content: {errorContent}");
                    ViewBag.ErrorMessage = $"API Error: {response.StatusCode} - {errorContent}";
                    return View("Error");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding task");
                ViewBag.ErrorMessage = GetDetailedExceptionMessage(ex);
                return View("Error");
            }
        }

        public async Task<IActionResult> Calendar(int month, int year)
        {
            try
            {
                // Validate month and year
                if (month < 1 || month > 12)
                    month = DateTime.Now.Month;
                    
                if (year < 2000 || year > 2100)
                    year = DateTime.Now.Year;
                
                _logger.LogInformation($"Viewing calendar for {year}-{month}");

                // Get all tasks from API
                var response = await _httpClient.GetAsync("");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API returned non-success status code: {response.StatusCode}, Content: {errorContent}");
                    ViewBag.ErrorMessage = $"API Error: {response.StatusCode} - {errorContent}";
                    return View("Error");
                }

                var json = await response.Content.ReadAsStringAsync();
                var allTasks = JsonConvert.DeserializeObject<List<TodoTask>>(json) ?? new List<TodoTask>();
                
                // Filter tasks for the selected month/year
                var tasksForMonth = allTasks.Where(t => t.DueDate.HasValue && 
                                                     t.DueDate.Value.Month == month && 
                                                     t.DueDate.Value.Year == year)
                                          .ToList();
                
                // Setup calendar data for the view
                ViewBag.Month = month;
                ViewBag.Year = year;
                ViewBag.MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
                
                // Get first day of month and total days
                var firstDayOfMonth = new DateTime(year, month, 1);
                ViewBag.FirstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
                ViewBag.DaysInMonth = DateTime.DaysInMonth(year, month);
                
                // Previous/Next month navigation
                int prevMonth = month == 1 ? 12 : month - 1;
                int prevYear = month == 1 ? year - 1 : year;
                int nextMonth = month == 12 ? 1 : month + 1;
                int nextYear = month == 12 ? year + 1 : year;
                
                ViewBag.PrevMonth = prevMonth;
                ViewBag.PrevYear = prevYear;
                ViewBag.NextMonth = nextMonth;
                ViewBag.NextYear = nextYear;
                
                return View(tasksForMonth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting calendar");
                ViewBag.ErrorMessage = GetDetailedExceptionMessage(ex);
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                _logger.LogInformation($"Completing task with ID: {id}");
                
                // Log the ID we're trying to complete
                _logger.LogInformation($"Attempting to complete task ID: {id}");
                
                // Creating proper HttpContent (empty but valid)
                var content = new StringContent(string.Empty);
                
                // Direct POST request with URL and content
                var response = await _httpClient.PostAsync($"{id}/complete", content);
                
                // Log the complete response details
                _logger.LogInformation($"Complete API response: {response.StatusCode}, Request URI: {_httpClient.BaseAddress}{id}/complete");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API Error: {response.StatusCode}, Content: {errorContent}");
                    ViewBag.ErrorMessage = $"API Error: {response.StatusCode} - {errorContent}";
                    return View("Error");
                }
                
                _logger.LogInformation("Task completed successfully");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while completing task");
                ViewBag.ErrorMessage = GetDetailedExceptionMessage(ex);
                return View("Error");
            }
        }

        private string GetDetailedExceptionMessage(Exception ex)
        {
            var message = $"Error: {ex.Message}";
            
            if (ex is HttpRequestException httpEx)
            {
                message += $" - Status: {httpEx.StatusCode}";
            }
            
            if (ex.InnerException != null)
            {
                message += $"\nInner Exception: {ex.InnerException.Message}";
                
                if (ex.InnerException.InnerException != null)
                {
                    message += $"\nDeeper Inner Exception: {ex.InnerException.InnerException.Message}";
                }
            }
            
            message += $"\nStack Trace: {ex.StackTrace}";
            
            return message;
        }
    }
}