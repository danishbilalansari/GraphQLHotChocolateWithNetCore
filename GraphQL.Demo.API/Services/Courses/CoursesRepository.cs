using GraphQL.Demo.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.Demo.API.Services.Courses
{
    public class CoursesRepository
    {
        private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

        public CoursesRepository(IDbContextFactory<SchoolDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<CourseDTO>> GetAll()
        {
            using (SchoolDbContext context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.Courses.Include(c => c.Instructor).Include(c => c.Students).ToListAsync();
            }
        }

        public async Task<CourseDTO> GetById(Guid courseId)
        {
            using (SchoolDbContext context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.Courses.Include(c => c.Instructor).Include(c => c.Students).FirstOrDefaultAsync(c => c.Id == courseId);
            }
        }

        public async Task<CourseDTO> Create(CourseDTO course)
        {
            using (SchoolDbContext context = await _contextFactory.CreateDbContextAsync())
            {
                context.Courses.Add(course);
                await context.SaveChangesAsync();

                return course;
            }
        }

        public async Task<CourseDTO> Update(CourseDTO course)
        {
            using (SchoolDbContext context = await _contextFactory.CreateDbContextAsync())
            {
                context.Courses.Update(course);
                await context.SaveChangesAsync();

                return course;
            }
        }

        public async Task<bool> Delete(Guid id)
        {
            using (SchoolDbContext context = await _contextFactory.CreateDbContextAsync())
            {
                CourseDTO course = new CourseDTO()
                {
                    Id = id
                };
                context.Courses.Remove(course);

                return await context.SaveChangesAsync() > 0;
            }
        }
    }
}
