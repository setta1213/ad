using Microsoft.AspNetCore.Mvc;
using StudentAdWindowsApp.Api.Models;
using StudentAdWindowsApp.Models;
using StudentAdWindowsApp.Services;


namespace StudentAdWindowsApp.Api.Controllers
{
    [ApiController]
    [Route("api/main")]
    public class StudentsController : ControllerBase
    {
        private readonly AdService _adService;
        private readonly AdConfig _config;

        public StudentsController(AdService adService, AdConfig config)
        {
            _adService = adService;
            _config = config;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("API IS ALIVE");
        }
        [HttpPost("create")]
        public IActionResult Create([FromBody] CreateStudentDto dto)
        {
            try
            {
                _adService.CreateStudent(
                    _config,
                    dto.StudentId,
                    dto.Password,
                    dto.FirstName,
                    dto.LastName
                );

                return Ok(new { success = true, message = "สร้างผู้ใช้สำเร็จ" });
            }
            catch (InvalidOperationException ex) when (ex.Message == "DUPLICATE_USER")
            {
                // ✅ user ซ้ำ -> 409 Conflict
                return Conflict(new
                {
                    success = false,
                    errorCode = "DUPLICATE_USER",
                    message = "มี Student ID นี้อยู่แล้ว"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    errorCode = "CREATE_FAILED",
                    message = ex.Message
                });
            }
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.StudentId))
                    return BadRequest(new { success = false, message = "กรุณาระบุ StudentId" });

                _adService.ResetPasswordAndForceChange(
                    _config,
                    dto.StudentId
                );

                return Ok(new
                {
                    success = true,
                    message = "รีเซ็ตรหัสผ่านเป็น 12345678 และบังคับเปลี่ยนรหัสผ่านเมื่อ login"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        [HttpPost("delete")]
        public IActionResult DeleteStudent([FromBody] DeleteStudentDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.StudentId))
                    return BadRequest(new { success = false, message = "กรุณาระบุ StudentId" });

                _adService.DeleteStudent(
                    _config,
                    dto.StudentId
                );

                return Ok(new
                {
                    success = true,
                    message = "ลบนักศึกษาออกจากระบบเรียบร้อย"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        [HttpGet("info/{studentId}")]
        public IActionResult GetStudent(string studentId)
        {
            try
            {
                var info = _adService.GetStudentInfo(_config, studentId);

                return Ok(new
                {
                    success = true,
                    data = info
                });
            }
            catch (Exception ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        [HttpPut("update")]
        public IActionResult Update([FromBody] UpdateStudentDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.StudentId))
                    return BadRequest(new { success = false, message = "กรุณาระบุ StudentId" });

                _adService.UpdateStudent(_config, dto);

                return Ok(new
                {
                    success = true,
                    message = "อัปเดตข้อมูลนักศึกษาสำเร็จ"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }






        //------------------------------------------//
    }
}
