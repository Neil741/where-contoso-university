using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Services;

namespace ContosoUniversity.Controllers
{
    public class ToDosController : BaseController
    {
        public ToDosController(SchoolContext context, INotificationService notification) 
            : base(context, notification)
        {
        }
        // GET: ToDos
        public async Task<IActionResult> Index()
        {
            var todos = new List<ToDo>();
            
            try
            {
                var connection = db.Database.GetDbConnection();
                var command = connection.CreateCommand();
                command.CommandText = "sp_GetAllToDos";
                command.CommandType = CommandType.StoredProcedure;
                
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        todos.Add(new ToDo
                        {
                            ID = reader.GetInt32(reader.GetOrdinal("ID")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                            IsCompleted = reader.GetBoolean(reader.GetOrdinal("IsCompleted")),
                            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                            CompletedDate = reader.IsDBNull(reader.GetOrdinal("CompletedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("CompletedDate"))
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error loading todos: {ex.Message} | Stack: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Unable to load todos. Try again, and if the problem persists see your system administrator.";
            }
            
            return View(todos);
        }

        // GET: ToDos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            ToDo? todo = null;
            
            try
            {
                var connection = db.Database.GetDbConnection();
                var command = connection.CreateCommand();
                command.CommandText = "sp_GetToDoById";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@ID", id));
                
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        todo = new ToDo
                        {
                            ID = reader.GetInt32(reader.GetOrdinal("ID")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                            IsCompleted = reader.GetBoolean(reader.GetOrdinal("IsCompleted")),
                            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                            CompletedDate = reader.IsDBNull(reader.GetOrdinal("CompletedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("CompletedDate"))
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error loading todo details: {ex.Message} | ID: {id} | Stack: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Unable to load todo details. Try again, and if the problem persists see your system administrator.";
                return RedirectToAction("Index");
            }

            if (todo == null)
            {
                return NotFound();
            }
            
            return View(todo);
        }

        // GET: ToDos/Create
        public IActionResult Create()
        {
            var todo = new ToDo
            {
                IsCompleted = false
            };
            return View(todo);
        }

        // POST: ToDos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,IsCompleted,CompletedDate")] ToDo todo)
        {
            try
            {
                // Auto-set CreatedDate to current date
                todo.CreatedDate = DateTime.Today;

                if (ModelState.IsValid)
                {
                    int newId = 0;
                    var connection = db.Database.GetDbConnection();
                    var command = connection.CreateCommand();
                    command.CommandText = "sp_CreateToDo";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@Title", todo.Title));
                    command.Parameters.Add(new SqlParameter("@Description", (object?)todo.Description ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@IsCompleted", todo.IsCompleted));
                    command.Parameters.Add(new SqlParameter("@CreatedDate", todo.CreatedDate));
                    command.Parameters.Add(new SqlParameter("@CompletedDate", (object?)todo.CompletedDate ?? DBNull.Value));
                    
                    await connection.OpenAsync();
                    var result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        newId = Convert.ToInt32(result);
                    }
                    
                    Trace.TraceInformation($"Created ToDo with ID: {newId}");
                    
                    // Send notification for todo creation
                    SendEntityNotification("ToDo", newId.ToString(), todo.Title, EntityOperation.CREATE);
                    
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error creating todo: {ex.Message} | Title: {todo?.Title} | Stack: {ex.StackTrace}");
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            
            return View(todo);
        }

        // GET: ToDos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            ToDo? todo = null;
            
            try
            {
                var connection = db.Database.GetDbConnection();
                var command = connection.CreateCommand();
                command.CommandText = "sp_GetToDoById";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@ID", id));
                
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        todo = new ToDo
                        {
                            ID = reader.GetInt32(reader.GetOrdinal("ID")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                            IsCompleted = reader.GetBoolean(reader.GetOrdinal("IsCompleted")),
                            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                            CompletedDate = reader.IsDBNull(reader.GetOrdinal("CompletedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("CompletedDate"))
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error loading todo for edit: {ex.Message} | ID: {id} | Stack: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Unable to load todo. Try again, and if the problem persists see your system administrator.";
                return RedirectToAction("Index");
            }

            if (todo == null)
            {
                return NotFound();
            }
            
            return View(todo);
        }

        // POST: ToDos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("ID,Title,Description,IsCompleted,CreatedDate,CompletedDate")] ToDo todo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var connection = db.Database.GetDbConnection();
                    var command = connection.CreateCommand();
                    command.CommandText = "sp_UpdateToDo";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ID", todo.ID));
                    command.Parameters.Add(new SqlParameter("@Title", todo.Title));
                    command.Parameters.Add(new SqlParameter("@Description", (object?)todo.Description ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@IsCompleted", todo.IsCompleted));
                    command.Parameters.Add(new SqlParameter("@CreatedDate", todo.CreatedDate));
                    command.Parameters.Add(new SqlParameter("@CompletedDate", (object?)todo.CompletedDate ?? DBNull.Value));
                    
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    
                    Trace.TraceInformation($"Updated ToDo with ID: {todo.ID}");
                    
                    // Send notification for todo update
                    SendEntityNotification("ToDo", todo.ID.ToString(), todo.Title, EntityOperation.UPDATE);
                    
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error editing todo: {ex.Message} | ID: {todo?.ID} | Title: {todo?.Title} | Stack: {ex.StackTrace}");
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            
            return View(todo);
        }

        // GET: ToDos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            ToDo? todo = null;
            
            try
            {
                var connection = db.Database.GetDbConnection();
                var command = connection.CreateCommand();
                command.CommandText = "sp_GetToDoById";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@ID", id));
                
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        todo = new ToDo
                        {
                            ID = reader.GetInt32(reader.GetOrdinal("ID")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                            IsCompleted = reader.GetBoolean(reader.GetOrdinal("IsCompleted")),
                            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                            CompletedDate = reader.IsDBNull(reader.GetOrdinal("CompletedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("CompletedDate"))
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error loading todo for delete: {ex.Message} | ID: {id} | Stack: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Unable to load todo. Try again, and if the problem persists see your system administrator.";
                return RedirectToAction("Index");
            }

            if (todo == null)
            {
                return NotFound();
            }
            
            return View(todo);
        }

        // POST: ToDos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                string todoTitle = string.Empty;
                
                var connection = db.Database.GetDbConnection();
                await connection.OpenAsync();
                
                // Get the todo title before deleting for notification
                var getCommand = connection.CreateCommand();
                getCommand.CommandText = "sp_GetToDoById";
                getCommand.CommandType = CommandType.StoredProcedure;
                getCommand.Parameters.Add(new SqlParameter("@ID", id));
                
                using (var reader = await getCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        todoTitle = reader.GetString(reader.GetOrdinal("Title"));
                    }
                }
                
                // Delete the todo
                var deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText = "sp_DeleteToDo";
                deleteCommand.CommandType = CommandType.StoredProcedure;
                deleteCommand.Parameters.Add(new SqlParameter("@ID", id));
                await deleteCommand.ExecuteNonQueryAsync();
                
                Trace.TraceInformation($"Deleted ToDo with ID: {id}");
                
                // Send notification for todo deletion
                SendEntityNotification("ToDo", id.ToString(), todoTitle, EntityOperation.DELETE);
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error deleting todo: {ex.Message} | ID: {id} | Stack: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Unable to delete the todo. Try again, and if the problem persists see your system administrator.";
                return RedirectToAction("Index");
            }
        }
    }
}
