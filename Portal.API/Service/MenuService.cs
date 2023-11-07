using Portal.API.Dto.Menu;
using Portal.API.Interfaces.Repositories;
using Portal.API.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Portal.API.Common.Dto;
using Portal.API.Entities;

namespace Portal.API.Service
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IMapper _mapper;

        public MenuService(IMenuRepository menuRepository,
            IMapper mapper)
        {
            _menuRepository = menuRepository;
            _mapper = mapper;
        }

        public async Task<MenuDto> Create(MenuAddDto model)
        {
            var menu = _mapper.Map<Menu>(model);
            await _menuRepository.Create(menu);
            return _mapper.Map<MenuDto>(menu);
        }

        public async Task Delete(long id)
        {
            await _menuRepository.Delete(id);
        }

        public async Task<MenuDto> Get(long id)
        {
            return _mapper.Map<MenuDto>(await _menuRepository.Get(id));
        }

        public async Task<ResponseDto<MenuDto>> GetAllPaged(int limit, int skip, string order, MenuFilterDto filter)
        {
            return _mapper.Map<ResponseDto<MenuDto>>(await _menuRepository.GetAllPaged(limit, skip, order, filter));
        }
        public async Task<List<MenuDto>> GetGroupsBySistemaId(long sistema_id)
        {
            return _mapper.Map<List<MenuDto>>((await _menuRepository.GetBySistemaId(sistema_id)).Where(m => m.Type == "GRUPO").OrderBy(m => m.ParentId).ThenBy(m => m.Order).ToList());
        }
        public async Task<List<GroupMenuDto>> GetBySistemaId(long id)
        {
            List<GroupMenuDto> groupMenuDto = null;
            var menus = await _menuRepository.GetBySistemaId(id);
            if (menus != null && menus.Count > 0)
            {
                groupMenuDto = new List<GroupMenuDto>();
                var groups = menus.Where(m => m.Type == "GRUPO" && m.ParentId == null).OrderBy(m => m.Order);
                foreach (var group in groups)
                {
                    var g = new GroupMenuDto()
                    {
                        Id = group.Id,
                        Name = group.Name,
                        Type = group.Type,
                        ParentId = group.ParentId,
                        Icon = group.Icon,
                        Order = group.Order,
                        SistemaId = group.SistemaId,
                        Url = group.Url,
                        Children = _mapper.Map<List<GroupMenuDto>>(menus.Where(m => m.ParentId == group.Id).OrderBy(o => o.Order).ToList())
                    };
                    g.Children.ForEach(c =>
                    {
                        c.Children = _mapper.Map<List<GroupMenuDto>>(menus.Where(m => m.ParentId == c.Id).OrderBy(o => o.Order).ToList());
                    });
                    groupMenuDto.Add(g);
                }

            }
            return groupMenuDto;
        }
        public async Task<List<GroupMenuDto>> GetByUserAndConvenio(long user_id, long sistema_id, long profile_id, long? convenio_id)
        {
            List<GroupMenuDto> groupMenuDto = null;
            var menus = await _menuRepository.GetByUserAndConvenio(user_id, sistema_id, profile_id, convenio_id);
            if (menus != null && menus.Count > 0)
            {
                groupMenuDto = new List<GroupMenuDto>();
                var groups = menus.Where(m => m.Type == "GRUPO" && m.ParentId == null).OrderBy(m => m.Order);
                foreach (var group in groups)
                {
                    var g = new GroupMenuDto()
                    {
                        Id = group.Id,
                        Name = group.Name,
                        Type = group.Type,
                        ParentId = group.ParentId,
                        Icon = group.Icon,
                        Order = group.Order,
                        SistemaId = group.SistemaId,
                        Url = group.Url,
                        Children = _mapper.Map<List<GroupMenuDto>>(menus.Where(m => m.ParentId == group.Id).OrderBy(o => o.Order).ToList())
                    };
                    g.Children.ForEach(c =>
                    {
                        c.Children = _mapper.Map<List<GroupMenuDto>>(menus.Where(m => m.ParentId == c.Id).OrderBy(o => o.Order).ToList());
                    });

                    groupMenuDto.Add(g);
                }

            }
            return groupMenuDto;
        }
        public async Task<MenuDto> Update(MenuUpdateDto menu)
        {
            var entity = _mapper.Map<Menu>(menu);
            await _menuRepository.Update(entity);
            return _mapper.Map<MenuDto>(entity);
        }
    }
}
