using Google.Cloud.Firestore;
using TasksPage.Firebase_Helper;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TasksPage.Models;

namespace TasksPage.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            return await GetTasks();
        }

        private async Task<IActionResult> GetTasks()
        {
            try
            {
                List<Tasks> tasksList = new List<Tasks>();
                Query query = FirestoreDb.Create("tasksdb-bc128").Collection("Tasks");
                QuerySnapshot querySnaphot = await query.GetSnapshotAsync();

                foreach (var item in querySnaphot)
                {
                    Dictionary<string, object> data = item.ToDictionary();

                    tasksList.Add(new Tasks
                    {
                        Id = item.Id,
                        Title = data["Title"].ToString(),
                        Description = data["Description"].ToString(),
                        DueDate = data["DueDate"].ToString(),
                        IsCompleted = Convert.ToInt16(data["IsCompleted"]),
                        Priority = data["Priority"].ToString()
                    });
                }

                ViewBag.Tasks = tasksList;
                return View();
            }
            catch (Exception ex)
            {
                // Manejar la excepción aquí, por ejemplo, loguearla o mostrar un mensaje de error
                Console.WriteLine("Error getting tasks: " + ex.Message);
                return View();
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string description, string duedate, int iscompleted, string priority)
        {
            try
            {
                DocumentReference addedDocRef =
                    await FirestoreDb.Create("tasksdb-bc128")
                        .Collection("Tasks").AddAsync(new Dictionary<string, object>
                            {
                                { "Title", title },
                                { "Description",  description },
                                { "DueDate", duedate },
                                { "IsCompleted", iscompleted },
                                { "Priority", priority },
                            });

                await GetTasks();

                return View("~/Views/Home/Index.cshtml");
            }
            catch (Exception ex)
            {
                // Manejar la excepción aquí
                Console.WriteLine("Error creating task: " + ex.Message);
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string taskId, string title, string description, string duedate, int iscompleted, string priority)
        {
            try
            {
                var cardDocRef = FirestoreDb.Create("tasksdb-bc128")
                    .Collection("Tasks")
                    .Document(taskId);

                var updateData = new Dictionary<string, object>
                {
                    { "Title", title },
                    { "Description", description },
                    { "DueDate", duedate },
                    { "IsCompleted", iscompleted },
                    { "Priority", priority }
                };

                await cardDocRef.UpdateAsync(updateData);

                await GetTasks();

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Manejar la excepción aquí
                Console.WriteLine("Error editing the task: " + ex.Message);
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string taskId)
        {
            try
            {
                var cardDocRef = FirestoreDb.Create("tasksdb-bc128")
                    .Collection("Tasks")
                    .Document(taskId);

                await cardDocRef.DeleteAsync();

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting card: " + ex.Message);
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCompletion(string taskId)
        {
            try
            {
                var taskDocRef = FirestoreDb.Create("tasksdb-bc128")
                    .Collection("Tasks")
                    .Document(taskId);

                var taskSnapshot = await taskDocRef.GetSnapshotAsync();

                if (taskSnapshot.Exists)
                {
                    var taskData = taskSnapshot.ToDictionary();
                    bool isCompleted = taskData.ContainsKey("IsCompleted") ? Convert.ToBoolean(taskData["IsCompleted"]) : false;

                    await taskDocRef.UpdateAsync("IsCompleted", !isCompleted);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating task status: " + ex.Message);
                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}