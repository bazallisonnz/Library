using Library.API.Models;

namespace Library.API.Controllers
{
    using AutoMapper;
    using Microsoft.AspNetCore.Mvc;
    using Services;
    using System;
    using System.Collections.Generic;

    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public AuthorsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet()]
        public IActionResult GetAuthors()
        {
            var authorEntities = _libraryRepository.GetAuthors();

            var authorDtos = Mapper.Map<IEnumerable<Models.AuthorDto>>(authorEntities);

            return Ok(authorDtos);
        }

        [HttpGet("{id}", Name="GetAuthor")]
        public IActionResult GetAuthor(Guid id)
        {
            var authorEntity = _libraryRepository.GetAuthor(id);

            if (authorEntity == null)
            {
                return NotFound();
            }

            var authorDto = Mapper.Map<Models.AuthorDto>(authorEntity);

            return Ok(authorDto);
        }

        [HttpPost()]
        public IActionResult CreateAuthor(Guid id, [FromBody]AuthorForCreateDto authorForCreateDto)
        {
            if (authorForCreateDto == null)
            {
                return BadRequest();
            }

            var authorEntity = Mapper.Map<Entities.Author>(authorForCreateDto);

            _libraryRepository.AddAuthor(authorEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an author failed on save.");
            }

            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);

            return CreatedAtRoute("GetAuthor", new {id = authorToReturn.Id}, authorToReturn);
        }
    }
}
