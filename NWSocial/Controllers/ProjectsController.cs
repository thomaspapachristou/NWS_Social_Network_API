﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NWSocial.Data;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NWSocial.Models;
using NWSocial.Dtos.ProjectDtos;
using NWSocial.Dtos.ProjectMemberDtos;
using NWSocial.Dtos.ProjectSlotDtos;
using NWSocial.Dtos.ProjectRequestDtos;
using Microsoft.AspNetCore.JsonPatch;

namespace NWSocial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly INWSRepo _repo;
        private readonly IMapper _mapper;

        public ProjectsController(INWSRepo repository, IMapper mapper)
        {
            _repo = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ProjectReadDto>> GetProjects(string filter, string role, int? guildId, int? indexPage, int? numberPerPage)
        {
            var list = _repo.GetProjects(filter, role, guildId, indexPage, numberPerPage);
            return Ok(_mapper.Map<IEnumerable<ProjectReadDto>>(list));
        }

        [HttpGet("{projectId}")]
        public ActionResult<ProjectReadDto> GetProject(int projectId)
        {
            var project = _repo.GetProject(projectId);
            if(project == null)
            {
                return NotFound();
            }
            return (_mapper.Map<ProjectReadDto>(project));
        }

        [HttpPost]  
        public ActionResult<ProjectReadDto> AddProject(ProjectCreateDto newProject)
        {
            var project = _mapper.Map<Project>(newProject);
            _repo.AddProject(project);
            _repo.SaveChanges();
            var newSlot = new ProjectSlot { ProjectId = project.Id, Role = "Chef de projet" };
            _repo.AddProjectSlot(newSlot);
            _repo.SaveChanges();
            _repo.AddProjectMember(new ProjectMember { ProjectId = project.Id, SlotId = newSlot.Id, UserId = newProject.UserId});
            return Ok(_mapper.Map<ProjectReadDto>(project));
        }

        [HttpDelete("{id}")]
        public ActionResult RemoveProject(int id)
        {
            var project = _repo.GetProject(id);
            if(project == null)
            {
                return NotFound();
            }
            _repo.RemoveProject(project);
            _repo.SaveChanges();
            return NoContent();
        }

        [HttpGet("{id}/members")]
        public ActionResult<IEnumerable<ProjectMemberReadDto>> GetProjectMembers(int id)
        {
            var list = _repo.GetProjectMembers(id);
            return Ok(_mapper.Map<IEnumerable<ProjectMemberReadDto>>(list));
        }

        /*
         * DEPRECATED
         * DONT USE
        [HttpPost("{id}/members")]
        public ActionResult<ProjectMemberReadDto> AddProjectMember(int id, ProjectMemberCreateDto newPm)
        {
            var pm = _mapper.Map<ProjectMember>(newPm);
            pm.ProjectId = id;
            _repo.AddProjectMember(pm);
            _repo.SaveChanges();
            return Ok(_mapper.Map<ProjectMemberReadDto>(pm));
        }
        */

        [HttpDelete("{projectId}/members/{userId}")]
        public ActionResult RemoveProjectMember(int projectId, int userId )
        {
            var pm = _repo.GetProjectMember(projectId, userId);
            if(pm == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        // BETA ROUTE TO TEST

        [HttpGet("{projectId}/slots")]
        public ActionResult<IEnumerable<ProjectSlotReadDto>> GetProjectSlots(int projectId)
        {
            var list = _repo.GetProjectSlots(projectId);
            return Ok(_mapper.Map<IEnumerable<ProjectSlotReadDto>>(list));
        }

        [HttpPost("{projectId}/slots")]
        public ActionResult<ProjectSlotReadDto> AddProjectSlot(int projectId,[FromBody]  IEnumerable<ProjectSlotCreateDto> newSlots)
        {
            var slots = _mapper.Map<IEnumerable<ProjectSlot>>(newSlots);
            slots.ToList().ForEach(s => s.ProjectId = projectId);
            _repo.AddProjectSlots(slots);
            _repo.SaveChanges();
            return Ok(_mapper.Map<IEnumerable<ProjectSlotReadDto>>(slots));
        }

        [HttpPost("{projectId}/slots/{slotId}/requests/{userId}")]
        public ActionResult<ProjectRequestAfterCreationReadDto> AddProjectRequest(int slotId, int userId)
        {
            var pr = new ProjectRequest
            {
                UserId = userId,
                SlotId = slotId,
                Status = "En attente"
            };
            _repo.AddProjectRequest(pr);
            _repo.SaveChanges();
            return Ok( _mapper.Map<ProjectRequestAfterCreationReadDto>(pr));
        }

        [HttpPatch("{projectId}/slots/{slotId}/requests/{userId}")]
        public ActionResult UpdateRequestStatus(int projectId,int slotId, int userId, JsonPatchDocument<ProjectRequestUpdateDto> patchDocument)
        {
            var modelFromRepo = _repo.GetProjectRequest(userId,slotId);
            if (modelFromRepo == null)
            {
                return NotFound();
            }
            var prToPatch = _mapper.Map<ProjectRequestUpdateDto>(modelFromRepo);
            patchDocument.ApplyTo(prToPatch, ModelState);
            if (!TryValidateModel(prToPatch))
            {
                return ValidationProblem(ModelState);
            }
            _mapper.Map(prToPatch, modelFromRepo);
            _repo.UpdateProjectRequest(modelFromRepo,projectId);
            _repo.SaveChanges();
            return NoContent();
        }
    }
}
