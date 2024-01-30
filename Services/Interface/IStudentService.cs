using Practiseproject.DTO;

namespace Practiseproject.Services.Interface
{
    public interface IStudentService
    {
        List<StudentDTO> GetAllStudents();
        StudentDTO GetStudentById(int id);

        StudentDTO CreateStudent(StudentDTO studentDTO);
        StudentDTO UpdateStudent(StudentDTO studentDTO);

        void DeleteStudent(int id);
    }
}
