using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Models.SchoolViewModels;
using ContosoUniversity.Services;

namespace ContosoUniversity.Controllers
{
    public class InstructorsController : BaseController
    {
        public InstructorsController(SchoolContext context, INotificationService notification) 
            : base(context, notification)
        {
        }
        // GET: Instructors - All roles can view
        public async Task<IActionResult> Index(int? id, int? courseID)
        {
            var viewModel = new InstructorIndexData();
            viewModel.Instructors = await db.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(c => c.Course)
                        .ThenInclude(d => d.Department)
                .OrderBy(i => i.LastName)
                .ToListAsync();

            if (id != null)
            {
                ViewBag.InstructorID = id.Value;
                viewModel.Courses = viewModel.Instructors.Where(
                    i => i.ID == id.Value).Single().CourseAssignments.Select(s => s.Course);
            }

            if (courseID != null)
            {
                ViewBag.CourseID = courseID.Value;
                viewModel.Enrollments = viewModel.Courses.Where(
                    x => x.CourseID == courseID).Single().Enrollments;
            }

            return View(viewModel);
        }

        // GET: Instructors/Details/5 - All roles can view details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Instructor? instructor = await db.Instructors.FindAsync(id);
            if (instructor == null)
            {
                return NotFound();
            }
            return View(instructor);
        }

        // GET: Instructors/Create
        public async Task<IActionResult> Create()
        {
            var instructor = new Instructor();
            instructor.CourseAssignments = new List<CourseAssignment>();
            await PopulateAssignedCourseDataAsync(instructor);
            return View(instructor);
        }

        // POST: Instructors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LastName,FirstMidName,HireDate,OfficeAssignment")] Instructor instructor, string[]? selectedCourses)
        {
            if (selectedCourses != null)
            {
                instructor.CourseAssignments = new List<CourseAssignment>();
                foreach (var course in selectedCourses)
                {
                    var courseToAdd = new CourseAssignment { InstructorID = instructor.ID, CourseID = int.Parse(course) };
                    instructor.CourseAssignments.Add(courseToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                db.Instructors.Add(instructor);
                await db.SaveChangesAsync();
                
                // Send notification for instructor creation
                SendEntityNotification("Instructor", instructor.ID.ToString(), EntityOperation.CREATE);
                
                return RedirectToAction("Index");
            }
            await PopulateAssignedCourseDataAsync(instructor);
            return View(instructor);
        }

        // GET: Instructors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Instructor? instructor = await db.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(c => c.Course)
                .Where(i => i.ID == id)
                .FirstOrDefaultAsync();
            if (instructor == null)
            {
                return NotFound();
            }
            await PopulateAssignedCourseDataAsync(instructor);
            return View(instructor);
        }

        private async Task PopulateAssignedCourseDataAsync(Instructor instructor)
        {
            var allCourses = await db.Courses.ToListAsync();
            var instructorCourses = new HashSet<int>(instructor.CourseAssignments.Select(c => c.CourseID));
            var viewModel = new List<AssignedCourseData>();
            foreach (var course in allCourses)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            ViewBag.Courses = viewModel;
        }

        // POST: Instructors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, string[]? selectedCourses)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var instructorToUpdate = await db.Instructors
               .Include(i => i.OfficeAssignment)
               .Include(i => i.CourseAssignments)
                   .ThenInclude(c => c.Course)
               .Where(i => i.ID == id)
               .FirstOrDefaultAsync();

            if (instructorToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync(instructorToUpdate, "",
               i => i.LastName, i => i.FirstMidName, i => i.HireDate, i => i.OfficeAssignment))
            {
                try
                {
                    if (instructorToUpdate.OfficeAssignment != null && String.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment.Location))
                    {
                        instructorToUpdate.OfficeAssignment = null;
                    }

                    await UpdateInstructorCoursesAsync(selectedCourses, instructorToUpdate);

                    await db.SaveChangesAsync();
                    
                    // Send notification for instructor update
                    SendEntityNotification("Instructor", instructorToUpdate.ID.ToString(), EntityOperation.UPDATE);

                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            await PopulateAssignedCourseDataAsync(instructorToUpdate);
            return View(instructorToUpdate);
        }

        private async Task UpdateInstructorCoursesAsync(string[]? selectedCourses, Instructor instructorToUpdate)
        {
            if (selectedCourses == null)
            {
                instructorToUpdate.CourseAssignments = new List<CourseAssignment>();
                return;
            }

            var selectedCoursesHS = new HashSet<string>(selectedCourses);
            var instructorCourses = new HashSet<int>
                (instructorToUpdate.CourseAssignments.Select(c => c.Course.CourseID));
            var allCourses = await db.Courses.ToListAsync();
            foreach (var course in allCourses)
            {
                if (selectedCoursesHS.Contains(course.CourseID.ToString()))
                {
                    if (!instructorCourses.Contains(course.CourseID))
                    {
                        instructorToUpdate.CourseAssignments.Add(new CourseAssignment { InstructorID = instructorToUpdate.ID, CourseID = course.CourseID });
                    }
                }
                else
                {

                    if (instructorCourses.Contains(course.CourseID))
                    {
                        CourseAssignment? courseToRemove = instructorToUpdate.CourseAssignments.SingleOrDefault(i => i.CourseID == course.CourseID);
                        if (courseToRemove != null)
                        {
                            db.Entry(courseToRemove).State = EntityState.Deleted;
                        }
                    }
                }
            }
        }

        // GET: Instructors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Instructor? instructor = await db.Instructors.FindAsync(id);
            if (instructor == null)
            {
                return NotFound();
            }
            return View(instructor);
        }

        // POST: Instructors/Delete/5 - Only admins can delete instructors
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Instructor? instructor = await db.Instructors
              .Include(i => i.OfficeAssignment)
              .Where(i => i.ID == id)
              .FirstOrDefaultAsync();

            if (instructor == null)
            {
                return NotFound();
            }

            db.Instructors.Remove(instructor);

            var department = await db.Departments
                .Where(d => d.InstructorID == id)
                .FirstOrDefaultAsync();
            if (department != null)
            {
                department.InstructorID = null;
            }

            await db.SaveChangesAsync();
            
            // Send notification for instructor deletion
            SendEntityNotification("Instructor", id.ToString(), EntityOperation.DELETE);
            
            return RedirectToAction("Index");
        }
    }
}
