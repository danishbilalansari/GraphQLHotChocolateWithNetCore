using GraphQL.Demo.API.DTOs;
using GraphQL.Demo.API.Services.Instructors;

namespace GraphQL.Demo.API.DataLoaders
{
    public class InstructorDataLoader : BatchDataLoader<Guid, InstructorDTO>
    {
        private readonly InstructorsRepository _instructorsRepository;

        public InstructorDataLoader(
            InstructorsRepository instructorsRepository, 
            IBatchScheduler batchScheduler, 
            DataLoaderOptions? options = null) 
            : base(batchScheduler, options)
        {
            _instructorsRepository = instructorsRepository;
        }

        /// <summary>
        /// The Instructor DataLoader is used to load the instructor data by only querying to db one time.
        /// </summary>
        /// <param name="keys">The list of instructor id(s)</param>
        /// <param name="cancellationToken">The Cancellation Token.</param>
        /// <returns>Retuns the list of instructors.</returns>
        protected override async Task<IReadOnlyDictionary<Guid, InstructorDTO>> LoadBatchAsync(IReadOnlyList<Guid> keys, CancellationToken cancellationToken)
        {
            IEnumerable<InstructorDTO> instructors = await _instructorsRepository.GetManyByIds(keys);
            return instructors.ToDictionary(i => i.Id);
        }
    }
}
