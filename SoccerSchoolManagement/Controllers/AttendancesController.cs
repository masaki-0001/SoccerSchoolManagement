using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerSchoolManagement.Data;
using SoccerSchoolManagement.Models;
using SoccerSchoolManagement.ViewModels.Attendances;

public class AttendancesController : Controller
{
    private readonly AppDbContext _context;

        private static readonly string[] ValidStatuses =
        {
        "未確認",
        "出席",
        "欠席"
        };

        private static readonly string[] ValidStatusFilters =
        {
        "すべて",
        "未確認",
        "出席",
        "欠席"
        };

        public AttendancesController(AppDbContext context)
        {
            _context = context;
        }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id, string? keyword, string statusFilter = "すべて")
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var lesson = await _context.Lessons
            .Include(x => x.SoccerClass)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id.Value && !x.IsDeleted);

        if (lesson == null)
        {
            return NotFound();
        }
        if (!ValidStatusFilters.Contains(statusFilter))
        {
            statusFilter = "すべて";
        }

        var membershipQuery = _context.StudentClasses
            .Include(x => x.Student)
            .Where(x => x.ClassId == lesson.ClassId
                && !x.IsDeleted
                && !x.Student.IsDeleted
                && x.StartDate <= lesson.LessonDate
                && (!x.EndDate.HasValue || x.EndDate >= lesson.LessonDate));

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.Trim();

            membershipQuery = membershipQuery
                .Where(x => x.Student.Name.Contains(keyword) || x.Student.Kana.Contains(keyword));
        }

        var memberships = await membershipQuery
            .OrderBy(x => x.Student.Kana)
            .ThenBy(x => x.Student.Name)
            .AsNoTracking()
            .ToListAsync();

        var attendances = await _context.Attendances
            .Where(x => x.LessonId == lesson.Id && !x.IsDeleted)
            .AsNoTracking()
            .ToDictionaryAsync(x => x.StudentId);

        var model = new AttendanceEditViewModel
        {
            LessonId = lesson.Id,
            ClassId = lesson.ClassId,
            ClassName = lesson.SoccerClass.Name,
            LessonDate = lesson.LessonDate,
            StartTime = lesson.StartTime,
            EndTime = lesson.EndTime,
            VenueName = lesson.VenueName,
            CoachName = lesson.CoachName,
            LessonStatus = lesson.Status,
            Keyword = keyword,
            StatusFilter = statusFilter
        };

        foreach (var membership in memberships)
        {
            var studentModel = new AttendanceStudentViewModel
            {
                StudentId = membership.StudentId,
                StudentName = membership.Student.Name,
                Kana = membership.Student.Kana,
                Grade = membership.Student.Grade,
                JerseyNumber = membership.Student.JerseyNumber
            };

            if (attendances.TryGetValue(membership.StudentId, out var attendance))
            {
                studentModel.Status = attendance.Status;
                studentModel.Reason = attendance.Reason;
                studentModel.Note = attendance.Note;
            }

            if (statusFilter != "すべて" && studentModel.Status != statusFilter)
            {
                continue;
            }

            model.Students.Add(studentModel);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AttendanceEditViewModel model)
    {
        var lesson = await _context.Lessons
            .Include(x => x.SoccerClass)
            .FirstOrDefaultAsync(x => x.Id == model.LessonId && !x.IsDeleted);

        if (lesson == null)
        {
            return NotFound();
        }

        if (!ValidStatusFilters.Contains(model.StatusFilter))
        {
            model.StatusFilter = "すべて";
        }

        var membershipQuery = _context.StudentClasses
            .Include(x => x.Student)
            .Where(x => x.ClassId == lesson.ClassId
                && !x.IsDeleted
                && !x.Student.IsDeleted
                && x.StartDate <= lesson.LessonDate
                && (!x.EndDate.HasValue
                    || x.EndDate >= lesson.LessonDate));

        if (!string.IsNullOrWhiteSpace(model.Keyword))
        {
            model.Keyword = model.Keyword.Trim();

            membershipQuery = membershipQuery
                .Where(x => x.Student.Name.Contains(model.Keyword)
                    || x.Student.Kana.Contains(model.Keyword));
        }

        var memberships = await membershipQuery
            .OrderBy(x => x.Student.Kana)
            .ThenBy(x => x.Student.Name)
            .ToListAsync();

        var existingAttendances = await _context.Attendances
            .Where(x => x.LessonId == lesson.Id && !x.IsDeleted)
            .ToDictionaryAsync(x => x.StudentId);

        var attendances = await _context.Attendances
            .Where(x => x.LessonId == lesson.Id && !x.IsDeleted)
            .ToDictionaryAsync(x => x.StudentId);

        if (model.StatusFilter != "すべて")
        {
            memberships = memberships
                .Where(x =>
                {
                    if (existingAttendances.TryGetValue(
                        x.StudentId,
                        out var attendance))
                    {
                        return attendance.Status
                            == model.StatusFilter;
                    }

                    return model.StatusFilter == "未確認";
                })
                .ToList();
        }

        SetDisplayValues(model, lesson, memberships);

        if (lesson.Status == "中止" || lesson.Status == "休講")
        {
            ModelState.AddModelError(string.Empty,"中止または休講の練習予定には出欠を登録できません。");
        }

        var targetStudentIds = memberships
            .Select(x => x.StudentId)
            .ToHashSet();

        var submittedStudentIds = model.Students
            .Select(x => x.StudentId)
            .ToList();

        if (submittedStudentIds.Count !=
            submittedStudentIds.Distinct().Count())
        {
            ModelState.AddModelError(string.Empty,"同じ生徒の出欠情報が重複しています。");
        }

        for (var i = 0; i < model.Students.Count; i++)
        {
            var student = model.Students[i];

            if (!targetStudentIds.Contains(student.StudentId))
            {
                ModelState.AddModelError($"Students[{i}].StudentId","出欠対象ではない生徒が含まれています。");
            }

            if (!ValidStatuses.Contains(student.Status))
            {
                ModelState.AddModelError($"Students[{i}].Status","出欠状況を正しく選択してください。");
            }
        }

        if (model.Students.Count != memberships.Count)
        {
            ModelState.AddModelError(string.Empty,"対象生徒の情報が不足しているため、登録できませんでした。");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var now = DateTime.Now;

        foreach (var student in model.Students)
        {
            var reason = student.Status == "欠席"
                ? student.Reason?.Trim()
                : null;

            var note = student.Note?.Trim();

            if (existingAttendances.TryGetValue(
                student.StudentId,
                out var attendance))
            {
                attendance.Status = student.Status;
                attendance.Reason = reason;
                attendance.Note = note;
                attendance.UpdatedAt = now;
            }
            else
            {
                var newAttendance = new Attendance
                {
                    LessonId = lesson.Id,
                    StudentId = student.StudentId,
                    Status = student.Status,
                    Reason = reason,
                    Note = note,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsDeleted = false
                };

                _context.Attendances.Add(newAttendance);
            }
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "出欠情報を登録しました。";

        return RedirectToAction(nameof(Edit),
            new
            {
                id = lesson.Id,
                keyword = model.Keyword
            });
    }

    private static void SetDisplayValues(
    AttendanceEditViewModel model,
    Lesson lesson,
    List<StudentClass> memberships)
    {
        model.ClassId = lesson.ClassId;
        model.ClassName = lesson.SoccerClass.Name;
        model.LessonDate = lesson.LessonDate;
        model.StartTime = lesson.StartTime;
        model.EndTime = lesson.EndTime;
        model.VenueName = lesson.VenueName;
        model.CoachName = lesson.CoachName;
        model.LessonStatus = lesson.Status;

        var membershipDictionary = memberships
            .ToDictionary(x => x.StudentId);

        foreach (var student in model.Students)
        {
            if (!membershipDictionary.TryGetValue(student.StudentId, out var membership))
            {
                continue;
            }

            student.StudentName = membership.Student.Name;
            student.Kana = membership.Student.Kana;
            student.Grade = membership.Student.Grade;
            student.JerseyNumber = membership.Student.JerseyNumber;
        }
    }
}