using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ContosoUniversity.Data;
using ContosoUniversity.Services;
using ContosoUniversity.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Controllers
{
    public class ToDosController : BaseController
    {
        public ToDosController(SchoolContext context, NotificationService notificationService) 
            : base(context, notificationService)
        {
        }
        // GET: ToDos
        public ActionResult Index()
        {
            var todos = new List<ToDo>();
            
            try
            {
                using (var connection = db.Database.GetDbConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "sp_GetAllToDos";
                        command.CommandType = CommandType.StoredProcedure;
                        
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
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
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            ToDo todo = null;
            
            try
            {
                using (var connection = db.Database.GetDbConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "sp_GetToDoById";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ID", id));
                        
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
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
        public ActionResult Create()
        {
            var todo = new ToDo
            {
                Title = "",
                IsCompleted = false
            };
            return View(todo);
        }

        // POST: ToDos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Title,Description,IsCompleted,CompletedDate")] ToDo todo)
        {
            try
            {
                // Auto-set CreatedDate to current date
                todo.CreatedDate = DateTime.Today;

                if (ModelState.IsValid)
                {
                    int newId = 0;
                    using (var connection = db.Database.GetDbConnection())
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "sp_CreateToDo";
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@Title", todo.Title));
                            command.Parameters.Add(new SqlParameter("@Description", (object)todo.Description ?? DBNull.Value));
                            command.Parameters.Add(new SqlParameter("@IsCompleted", todo.IsCompleted));
                            command.Parameters.Add(new SqlParameter("@CreatedDate", todo.CreatedDate));
                            command.Parameters.Add(new SqlParameter("@CompletedDate", (object)todo.CompletedDate ?? DBNull.Value));
                            
                            connection.Open();
                            var result = command.ExecuteScalar();
                            if (result != null)
                            {
                                newId = Convert.ToInt32(result);
                            }
                        }
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
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            ToDo todo = null;
            
            try
            {
                using (var connection = db.Database.GetDbConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "sp_GetToDoById";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ID", id));
                        
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
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
        public ActionResult Edit([Bind("ID,Title,Description,IsCompleted,CreatedDate,CompletedDate")] ToDo todo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (var connection = db.Database.GetDbConnection())
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "sp_UpdateToDo";
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.Add(new SqlParameter("@ID", todo.ID));
                            command.Parameters.Add(new SqlParameter("@Title", todo.Title));
                            command.Parameters.Add(new SqlParameter("@Description", (object)todo.Description ?? DBNull.Value));
                            command.Parameters.Add(new SqlParameter("@IsCompleted", todo.IsCompleted));
                            command.Parameters.Add(new SqlParameter("@CreatedDate", todo.CreatedDate));
                            command.Parameters.Add(new SqlParameter("@CompletedDate", (object)todo.CompletedDate ?? DBNull.Value));
                            
                            connection.Open();
                            command.ExecuteNonQuery();
                        }
                    }
                    
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
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            ToDo todo = null;
            
            try
            {
                using (var connection = db.Database.GetDbConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "sp_GetToDoById";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ID", id));
                        
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
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
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                string todoTitle = string.Empty;
                
                using (var connection = db.Database.GetDbConnection())
                {
                    connection.Open();
                    
                    // Get the todo title before deleting for notification
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "sp_GetToDoById";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ID", id));
                        
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                todoTitle = reader.GetString(reader.GetOrdinal("Title"));
                            }
                        }
                    }
                    
                    // Delete the todo
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "sp_DeleteToDo";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ID", id));
                        command.ExecuteNonQuery();
                    }
                }
                
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
