using Users.API.Interfaces.Services;
using Users.API.Dto.Sistema;
using Users.API.Interfaces.Repositories;
using AutoMapper;
using System.Threading.Tasks;
using System.Collections.Generic;
using Users.API.Entities;

namespace Users.API.Services
{
  public class SistemaService : ISistemaService
  {
    private readonly ISistemaRepository _sistemaRepository;
    //Automapper
    private readonly IMapper _mapper;

    public SistemaService(ISistemaRepository sistemaRepository,
                          IMapper mapper)
    {
      _sistemaRepository = sistemaRepository;
      _mapper = mapper;
    }


    public async Task<IEnumerable<SistemaDto>> GetAll()
    {
      return _mapper.Map<IEnumerable<SistemaDto>>(await _sistemaRepository.GetAll());
    }

    public async Task<SistemaDto> Get(long id)
    {
      return _mapper.Map<SistemaDto>(await _sistemaRepository.Get(id));
    }

    public async Task<SistemaDto> Add(SistemaAddDto sistema)
    {
      var entity = _mapper.Map<Sistema>(sistema);
      await _sistemaRepository.Add(entity);
      return _mapper.Map<SistemaDto>(entity);
    }

    public async Task<SistemaDto> Update(SistemaUpdateDto sistema)
    {
            var entity = _mapper.Map<Sistema>(sistema);
      await _sistemaRepository.Update(entity);
      return _mapper.Map<SistemaDto>(entity);
    }

    public async Task Delete(long id)
    {
      await _sistemaRepository.Delete(id);
    }

  }
}