using Library.API.Entities;
using Library.API.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Library.API.Controllers
{
    using AutoMapper;
    using Microsoft.AspNetCore.Mvc;
    using Services;
    using System;
    using System.Collections.Generic;

    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public BooksController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet()]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntities = _libraryRepository.GetBooksForAuthor(authorId);

            var bookDtos = Mapper.Map<IEnumerable<Models.BookDto>>(bookEntities);

            return Ok(bookDtos);
        }

        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = _libraryRepository.GetBookForAuthor(authorId, id);

            if (bookEntity == null)
            {
                return NotFound();
            }

            var bookDto = Mapper.Map<Models.BookDto>(bookEntity);

            return Ok(bookDto);
        }

        [HttpPost()]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody]BookForCreationDto bookForCreationDto)
        {
            if (bookForCreationDto == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = Mapper.Map<Entities.Book>(bookForCreationDto);

            _libraryRepository.AddBookForAuthor(authorId, bookEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Creating a book for author {authorId} failed on save.");
            }

            var bookToReturn = Mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute("GetBookForAuthor", new { authorId =  authorId, id = bookToReturn.Id }, bookToReturn);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorToDelete = _libraryRepository.GetBookForAuthor(authorId, id);

            if (bookForAuthorToDelete == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteBook(bookForAuthorToDelete);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting book {id} for author {authorId} failed on save.");
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id, [FromBody]BookForUpdateDto bookToUpdate)
        {
            if (bookToUpdate == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = _libraryRepository.GetBookForAuthor(authorId, id);
            if (bookEntity == null)
            {
                var bookToAdd = Mapper.Map<Book>(bookToUpdate);
                bookToAdd.Id = id;

                _libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!_libraryRepository.Save())
                {
                    throw new Exception($"Upserting book {id} for author {authorId} failed on save.");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute("GetBookForAuthor", new {authorId = authorId, id = bookToReturn.Id}, bookToReturn);
            }

            Mapper.Map(bookToUpdate, bookEntity);

            _libraryRepository.UpdateBookForAuthor(bookEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Updating book {id} for author {authorId} failed on save.");
            }

            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid id,
            [FromBody] JsonPatchDocument<BookForUpdateDto> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);

            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            var bookForPatch = Mapper.Map<BookForUpdateDto>(bookForAuthorFromRepo);

            patchDocument.ApplyTo(bookForPatch);

            // Add Validation

            Mapper.Map(bookForPatch, bookForAuthorFromRepo);

            _libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Patching book {id} for author {authorId} failed on save.");
            }

            return NoContent();

        }
    }
}
