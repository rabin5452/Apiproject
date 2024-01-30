using AutoMapper;
using Practiseproject.DTO;
using Practiseproject.GenericeRepository.Interface;
using Practiseproject.Model;
using Practiseproject.Services.Interface;

namespace Practiseproject.Services.Implementation
{
    public class StudentService:IStudentService
    {
        private readonly IGenericRepository<Student> _genericRepository;
        private IMapper _mapper;
        public StudentService(IGenericRepository<Student> genericRepository ,IMapper mapper)
        {
            _genericRepository = genericRepository;
            _mapper = mapper;
        }
        public List<StudentDTO> GetAllStudents() 
        {
            List<Student> students = _genericRepository.GetAll();
            List<StudentDTO> studentDTOs=_mapper.Map<List<StudentDTO>>(students);
            return studentDTOs;
        }
        public StudentDTO GetStudentById(int id) 
        {
            Student student= _genericRepository.GetById(id);
            StudentDTO studentDTO=_mapper.Map<StudentDTO>(student);
            return studentDTO;
        }
        public StudentDTO CreateStudent(StudentDTO studentDTO)
        {
            Student student= _mapper.Map<Student>(studentDTO);  
            student=_genericRepository.Add(student);
            studentDTO = _mapper.Map<StudentDTO>(student);
            return studentDTO;
        }
        public StudentDTO UpdateStudent(StudentDTO studentDTO)
        {
            Student student = _mapper.Map<Student>(studentDTO);
            student = _genericRepository.Update(student);
            studentDTO=_mapper.Map<StudentDTO>(student);
            return studentDTO;
        }
        public void DeleteStudent(int id)
        {
            _genericRepository.Delete(id);
        }
    }
}
