using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Practiseproject.Data;
using Practiseproject.DTO;
using Practiseproject.Model;
using Practiseproject.Services.Interface;

namespace Practiseproject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
       
        private readonly IStudentService _studentService;
        public StudentController(IStudentService studentService)
        {
            _studentService=studentService;
            
        }
        [HttpGet]
        public IActionResult GetAll()
        {
           List<StudentDTO> studentDTOs=_studentService.GetAllStudents();
           return Ok(studentDTOs);

        }
        [HttpGet("{id}")]
        public IActionResult GetStudent(int id)
        {
            StudentDTO studentDTO=_studentService.GetStudentById(id);
            return Ok(studentDTO);
        }
        [HttpPost]
        public IActionResult PostStudent(StudentDTO studentDTO) 
        {

            studentDTO = _studentService.CreateStudent(studentDTO);
            return Ok(studentDTO);

        }
        [HttpPost("{id}")]
        public IActionResult UpdateStudent(StudentDTO studentDTO)
        {
            studentDTO=_studentService.UpdateStudent(studentDTO);
            return Ok(studentDTO);
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id) 
        {
            _studentService.DeleteStudent(id);
            return Ok("Deleted");
        }

    }
}
