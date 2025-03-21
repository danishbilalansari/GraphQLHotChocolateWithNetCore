using GraphQL.Demo.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.Demo.API.Services.Instructors
{
    public class InstructorsRepository
    {
        private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

        public InstructorsRepository(IDbContextFactory<SchoolDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<InstructorDTO> GetById(Guid instructorId)
        {
            using (SchoolDbContext context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.Instructors.FirstOrDefaultAsync(c => c.Id == instructorId);
            }
        }

        public async Task<IEnumerable<InstructorDTO>> GetManyByIds(IReadOnlyList<Guid> instructorIds)
        {
            using (SchoolDbContext context = await _contextFactory.CreateDbContextAsync())
            {
                return await context.Instructors
                    .Where(x => instructorIds.Contains(x.Id))
                    .ToListAsync();
            }
        }
    }
}
