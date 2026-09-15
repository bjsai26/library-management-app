using LibraryManagement.AppServices.DTOs.Book;
using LibraryManagement.AppServices.DTOs.Common;
using LibraryManagement.AppServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

/// <summary>
/// The book catalogue. Any signed-in user can read it; only an Admin can change it.
/// </summary>
[Route("api/books")]
[Authorize]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
public class BooksController : BaseApiController
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>Returns one page of books, ordered by title.</summary>
    /// <param name="query">Paging and search options. Search matches title or ISBN.</param>
    /// <param name="cancellationToken">Cancels the request if the caller disconnects.</param>
    /// <response code="200">A page of books, with the total count and page metadata.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BookDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] QueryParameters query,
        CancellationToken cancellationToken)
        => HandleResult(await _bookService.GetAllAsync(query, cancellationToken));

    /// <summary>Returns a single book, including its author and category names.</summary>
    /// <param name="id">Identifier of the book.</param>
    /// <param name="cancellationToken">Cancels the request if the caller disconnects.</param>
    /// <response code="200">The requested book.</response>
    /// <response code="404">No book exists with that id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => HandleResult(await _bookService.GetByIdAsync(id, cancellationToken));

    /// <summary>Adds a book to the catalogue. Admin only.</summary>
    /// <param name="request">The book to create. The author and category must already exist.</param>
    /// <param name="cancellationToken">Cancels the request if the caller disconnects.</param>
    /// <response code="200">The created book, including its new id.</response>
    /// <response code="400">A field failed validation, or the author or category does not exist.</response>
    /// <response code="403">The signed-in user is not an Admin.</response>
    /// <response code="409">Another book already uses that ISBN.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] BookRequest request,
        CancellationToken cancellationToken)
        => HandleResult(await _bookService.CreateAsync(request, cancellationToken));

    /// <summary>Replaces every field of an existing book. Admin only.</summary>
    /// <param name="id">Identifier of the book to update.</param>
    /// <param name="request">The new values. All fields are required.</param>
    /// <param name="cancellationToken">Cancels the request if the caller disconnects.</param>
    /// <response code="200">The updated book.</response>
    /// <response code="400">A field failed validation, or the author or category does not exist.</response>
    /// <response code="403">The signed-in user is not an Admin.</response>
    /// <response code="404">No book exists with that id.</response>
    /// <response code="409">Another book already uses that ISBN.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] BookRequest request,
        CancellationToken cancellationToken)
        => HandleResult(await _bookService.UpdateAsync(id, request, cancellationToken));

    /// <summary>Permanently removes a book. Admin only.</summary>
    /// <param name="id">Identifier of the book to delete.</param>
    /// <param name="cancellationToken">Cancels the request if the caller disconnects.</param>
    /// <response code="200">The book was deleted.</response>
    /// <response code="403">The signed-in user is not an Admin.</response>
    /// <response code="404">No book exists with that id.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => HandleResult(await _bookService.DeleteAsync(id, cancellationToken));
}
